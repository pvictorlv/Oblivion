using System;
using System.Linq;
using Oblivion.Configuration;
using Oblivion.Connection.Connection;
using Oblivion.Connection.Net;
using Oblivion.Connection.SuperSocket;
using Oblivion.HabboHotel.Misc;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.UserDataManagement;
using Oblivion.Messages;
using Oblivion.Messages.Enums;
using Oblivion.Messages.Handlers;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.GameClients.Interfaces
{
    /// <summary>
    ///     Class GameClient.
    /// </summary>
    public class GameClient
    {
        /// <summary>
        ///     The _connection
        /// </summary>
        private Session<GameClient> _connection;

        /// <summary>
        ///     The _disconnected
        /// </summary>
        private bool _disconnected;

        /// <summary>
        ///     The _habbo
        /// </summary>
        private Habbo _habbo;

        /// <summary>
        ///     The _message handler
        /// </summary>
        private GameClientMessageHandler _messageHandler;

        /// <summary>
        ///     The current room user identifier
        /// </summary>
        internal int CurrentRoomUserId;

        /// <summary>
        ///     The designed handler
        /// </summary>
        internal int DesignedHandler = 1;

        /// <summary>
        ///     The machine identifier
        /// </summary>
        internal string MachineId;

        /// <summary>
        ///     The packet parser
        /// </summary>
        internal GamePacketParser PacketParser;


        /// <summary>
        ///     The time pinged received
        /// </summary>
        internal DateTime TimePingedReceived;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClient" /> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="connection">The connection.</param>
        internal GameClient(uint clientId, Session<GameClient> connection)
        {
            ConnectionId = clientId;
            _connection = connection;
            CurrentRoomUserId = -1;
            PacketParser = new GamePacketParser();
        }

        /// <summary>
        ///     Gets the connection identifier.
        /// </summary>
        /// <value>The connection identifier.</value>
        internal uint ConnectionId { get; }

        public bool IsAir
        {
            get => _connection != null && _connection.IsAir;
            set
            {
                if (_connection != null) _connection.IsAir = value;
            }
        }

        /// <summary>
        ///     Gets the connection.
        /// </summary>
        /// <returns>ConnectionInformation.</returns>
        internal Session<GameClient> GetConnection() => _connection;

        /// <summary>
        ///     Gets the message handler.
        /// </summary>
        /// <returns>GameClientMessageHandler.</returns>
        internal GameClientMessageHandler GetMessageHandler() => _messageHandler;

        /// <summary>
        ///     Gets the habbo.
        /// </summary>
        /// <returns>Habbo.</returns>
        internal Habbo GetHabbo() => _habbo;


        /// <summary>
        ///     Starts the connection.
        /// </summary>
        internal void StartConnection()
        {
            if (_messageHandler == null)
                InitHandler();

            PacketParser.SetConnection(this);

            TimePingedReceived = DateTime.Now;

//            if (_connection.Parser is InitialPacketParser packetParser)
//                packetParser.PolicyRequest += PolicyRequest;

//            if (_connection.Parser is InitialPacketParser initialPacketParser)
//                initialPacketParser.SwitchParserRequest += SwitchParserRequest;
//            _connection.StartPacketProcessing();
        }

        /// <summary>
        ///     Initializes the handler.
        /// </summary>
        internal void InitHandler()
        {
            _messageHandler = new GameClientMessageHandler(this);
        }

        /// <summary>
        ///     Tries the login.
        /// </summary>
        /// <param name="authTicket">The authentication ticket.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryLogin(string authTicket)
        {
            try
            {
                var ip = GetConnection()?.RemoteEndPoint.Address.ToString();
                if (ip == null) return false;
                var userData = UserDataFactory.GetUserData(authTicket, out var errorCode);

                if (errorCode == 1 || errorCode == 2 || userData?.User == null)
                    return false;

                var text = userData.User.UserName;

                if (string.IsNullOrWhiteSpace(text) || text.Length < 3 || text.Length > 15)
                    return false;

                var lower = text.ToLower();
                var array = lower.ToCharArray();
                const string source = "abcdefghijklmnopqrstuvwxyz1234567890.,_-;:?!@";
                if (array.Any(c => !source.Contains(char.ToLower(c))))
                {
                    return false;
                }

                Oblivion.GetGame().GetClientManager().RegisterClient(this, userData.UserId, userData.User.UserName);
                _habbo = userData.User;
                _habbo.LoadData(userData);

                if (string.IsNullOrEmpty(_habbo.UserName) ||
                    Oblivion.GetGame().GetBanManager().CheckBan(_habbo.UserName, ip, MachineId))
                {
                    var banReason = Oblivion.GetGame().GetBanManager()
                        .GetBanReason(_habbo.UserName, ip, MachineId);
                    SendNotifWithScroll(banReason);
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery($"SELECT ip_last FROM users WHERE id={_habbo.Id} LIMIT 1");

                        var supaString = queryReactor.GetString();

                        queryReactor.SetQuery(
                            $"SELECT COUNT(user_id) FROM users_bans_access WHERE user_id={_habbo.Id} LIMIT 1");
                        var integer = queryReactor.GetInteger();

                        if (integer > 0)
                            queryReactor.RunFastQuery("UPDATE users_bans_access SET attempts = attempts + 1, ip='" +
                                                      supaString + "' WHERE user_id=" + _habbo.Id + " LIMIT 1");
                        else
                            queryReactor.RunFastQuery("INSERT INTO users_bans_access (user_id, ip) VALUES (" +
                                                      GetHabbo().Id + ", '" + supaString + "')");
                    }

                    return false;
                }

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunFastQuery(
                        $"UPDATE users SET ip_last='{ip}', online = '1' WHERE id={GetHabbo().Id}");

                _habbo.Init(this, userData);

//                    var queuedServerMessage = new QueuedServerMessage(_connection);

                var msg =
                    new ServerMessage(LibraryParser.OutgoingRequest("AuthenticationOKMessageComposer"));
                SendMessage(msg);
                var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));

                serverMessage2.AppendInteger(_habbo.HomeRoom);
                serverMessage2.AppendInteger(_habbo.HomeRoom);
                SendMessage(serverMessage2);

                var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("FavouriteRoomsMessageComposer"));

                serverMessage.AppendInteger(30);

                if (_habbo.Data.FavouritedRooms == null || _habbo.Data.FavouritedRooms.Count <= 0)
                    serverMessage.AppendInteger(0);
                else
                {
                    serverMessage.AppendInteger(_habbo.Data.FavouritedRooms.Count);

                    /* TODO CHECK */
                    foreach (var i in _habbo.Data.FavouritedRooms)
                        serverMessage.AppendInteger(i);
                }

                SendMessage(serverMessage);

                var rightsMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("UserClubRightsMessageComposer"));

                rightsMessage.AppendInteger(_habbo.GetSubscriptionManager().HasSubscription ? 2 : 0);
                rightsMessage.AppendInteger(_habbo.Rank);
                rightsMessage.AppendInteger(0);
                SendMessage(rightsMessage);


                serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("EnableNotificationsMessageComposer"));
                serverMessage.AppendBool(true);
                serverMessage.AppendBool(false);
                serverMessage.AppendBool(true);
                SendMessage(serverMessage);

                /*var xmasGift =
                    new ServerMessage(LibraryParser.OutgoingRequest("CampaignCalendarDataMessageComposer"));

                xmasGift.AppendString("xmas16"); //eventTrigger
//                    xmasGift.AppendString(""); //idk? same as habbo ;P
                xmasGift.AppendBool(false);
                xmasGift.AppendBool(false);
                xmasGift.AppendInteger(DateTime.Now.Day - 1); //currentDate
                xmasGift.AppendInteger(25); //totalAmountOfDays

                xmasGift.AppendInteger(_habbo.Data.OpenedGifts.Count); //countOpenGifts

                foreach (var opened in _habbo.Data.OpenedGifts)
                {
                    xmasGift.AppendInteger(opened);
                }


                var MissedGifts = new List<int>();
                foreach (var day in Enumerable.Range(0, DateTime.Now.Day - 3))
                {
                    if (!_habbo.Data.OpenedGifts.Contains(day))
                        MissedGifts.Add(day);
                }


                xmasGift.AppendInteger(MissedGifts.Count);
                foreach (int Missed in MissedGifts)
                {
                    xmasGift.AppendInteger(Missed); //giftDay

                    queuedServerMessage.AppendResponse(xmasGift);
                }*/

                serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("CfhTopicsInitMessageComposer"));
                serverMessage.AppendInteger(6);
                serverMessage.AppendString("sexual_content");
                serverMessage.AppendInteger(8);
                serverMessage.AppendString("pii_meeting_irl");
                serverMessage.AppendInteger(1);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("sexual_webcam_images_auto");
                serverMessage.AppendInteger(2);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("explicit_sexual_talk");
                serverMessage.AppendInteger(3);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("cybersex");
                serverMessage.AppendInteger(4);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("cybersex_auto");
                serverMessage.AppendInteger(5);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("meet_some");
                serverMessage.AppendInteger(6);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("meet_irl");
                serverMessage.AppendInteger(7);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("email_or_phone");
                serverMessage.AppendInteger(8);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("scamming");
                serverMessage.AppendInteger(3);
                serverMessage.AppendString("stealing");
                serverMessage.AppendInteger(9);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("scamsites");
                serverMessage.AppendInteger(10);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("selling_buying_accounts_or_furni");
                serverMessage.AppendInteger(11);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("trolling_bad_behavior");
                serverMessage.AppendInteger(11);
                serverMessage.AppendString("hate_speech");
                serverMessage.AppendInteger(12);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("violent_roleplay");
                serverMessage.AppendInteger(13);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("swearing");
                serverMessage.AppendInteger(14);
                serverMessage.AppendString("auto_reply");
                serverMessage.AppendString("drugs");
                serverMessage.AppendInteger(15);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("gambling");
                serverMessage.AppendInteger(16);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("self_threatening");
                serverMessage.AppendInteger(17);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("mild_staff_impersonation");
                serverMessage.AppendInteger(18);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("severe_staff_impersonation");
                serverMessage.AppendInteger(19);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("habbo_name");
                serverMessage.AppendInteger(20);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("minors_access");
                serverMessage.AppendInteger(21);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("bullying");
                serverMessage.AppendInteger(22);
                serverMessage.AppendString("guardians");
                serverMessage.AppendString("game_interruption");
                serverMessage.AppendInteger(2);
                serverMessage.AppendString("flooding");
                serverMessage.AppendInteger(23);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("doors");
                serverMessage.AppendInteger(24);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("room");
                serverMessage.AppendInteger(1);
                serverMessage.AppendString("room_report");
                serverMessage.AppendInteger(25);
                serverMessage.AppendString("mods");
                serverMessage.AppendString("help");
                serverMessage.AppendInteger(2);
                serverMessage.AppendString("help_habbo");
                serverMessage.AppendInteger(26);
                serverMessage.AppendString("auto_reply");
                serverMessage.AppendString("help_payments");
                serverMessage.AppendInteger(27);
                serverMessage.AppendString("auto_reply");
                SendMessage(serverMessage);

                _habbo.UpdateCreditsBalance();

                serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
                serverMessage.AppendInteger(2);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(_habbo.ActivityPoints);
                serverMessage.AppendInteger(5);
                serverMessage.AppendInteger(_habbo.Diamonds);

                serverMessage.AppendInteger(102);
                serverMessage.AppendInteger(_habbo.Emeralds);

                SendMessage(serverMessage);


                if (_habbo.HasFuse("fuse_mod"))
                    SendMessage(Oblivion.GetGame().GetModerationTool()
                        .SerializeTool(this));

                SendMessage(Oblivion.GetGame().GetAchievementManager()
                    .AchievementDataCached);

                if (!GetHabbo().Vip && ExtraSettings.NewUsersGiftsEnabled)
                    SendMessage(
                        new ServerMessage(LibraryParser.OutgoingRequest("NuxSuggestFreeGiftsMessageComposer")));

                SendMessage(GetHabbo().GetAvatarEffectsInventoryComponent().GetPacket());
//                    queuedServerMessage.SendResponse();

                if (GetHabbo().GetMessenger() != null)
                    GetHabbo().GetMessenger().OnStatusChanged(true);


                Oblivion.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(this);
                Oblivion.GetGame().GetAchievementManager().TryProgressRegistrationAchievements(this);
                Oblivion.GetGame().GetAchievementManager().TryProgressLoginAchievements(this);


                return true;
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException($"Bug during user login: {ex}");
            }

            return false;
        }

        /// <summary>
        ///     Sends the notif with scroll.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendNotifWithScroll(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("MOTDNotificationMessageComposer"));

            serverMessage.AppendInteger(1);
            serverMessage.AppendString(message);
            SendMessage(serverMessage);
        }

        /// <summary>
        ///     Sends the broadcast message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendBroadcastMessage(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));

            serverMessage.AppendString(message);
            serverMessage.AppendString(string.Empty);
            SendMessage(serverMessage);
        }

        /// <summary>
        ///     Sends the moderator message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendModeratorMessage(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AlertNotificationMessageComposer"));

            serverMessage.AppendString(message);
            serverMessage.AppendString(string.Empty);
            SendMessage(serverMessage);
        }

        /// <summary>
        ///     Sends the whisper.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fromWired"></param>
        internal void SendWhisper(string message, bool fromWired = false)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            var roomUserByHabbo = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);

            if (roomUserByHabbo == null)
                return;

            var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));

            whisp.AppendInteger(roomUserByHabbo.VirtualId);
            whisp.AppendString(message);
            whisp.AppendInteger(0);
            whisp.AppendInteger(fromWired ? 34 : roomUserByHabbo.LastBubble);
            whisp.AppendInteger(0);
            whisp.AppendInteger(fromWired);

            SendMessage(whisp);
        }

        /// <summary>
        ///     Sends the notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        internal void SendNotif(string message, string title = "Aviso", string picture = "")
        {
            SendMessage(GetBytesNotif(message, title, picture));
        }

        internal ServerMessage GetBubble(string message, string title, string picture = "", bool isBubble = false)
        {
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("RoomNotificationMessageComposer"));
            msg.AppendString(picture);
            msg.AppendInteger(5);
            msg.AppendString("title");
            msg.AppendString(title);
            msg.AppendString("message");
            msg.AppendString(message);
            msg.AppendString("linkUrl");
            msg.AppendString("");
            msg.AppendString("linkTitle");
            msg.AppendString("");
            msg.AppendString("display");
            msg.AppendString(isBubble ? "BUBBLE" : "POP_UP");
            return msg;
        }

        /// <summary>
        ///     Gets the bytes notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        /// <returns>System.Byte[].</returns>
        public static byte[] GetBytesNotif(string message, string title = "Aviso", string picture = "")
        {
            using (var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer")))
            {
                serverMessage.AppendString(picture);
                serverMessage.AppendInteger(4);
                serverMessage.AppendString("title");
                serverMessage.AppendString(title);
                serverMessage.AppendString("message");
                serverMessage.AppendString(message);
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");

                return serverMessage.GetReversedBytes();
            }
        }

        /// <summary>
        ///     Stops this instance.
        /// </summary>
        internal void Stop()
        {
            _habbo?.OnDisconnect("disconnect");

            if (GetMessageHandler() != null)
                GetMessageHandler().Destroy();

            CurrentRoomUserId = -1;
            _messageHandler?.Dispose();
            _messageHandler = null;
            _habbo = null;

            _connection?.Dispose();

            _connection = null;
            PacketParser?.Dispose();
            PacketParser = null;
            _disconnected = true;
        }


        /// <summary>
        ///     Disconnects the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal void Disconnect(string reason)
        {
            try
            {
                _habbo?.OnDisconnect(reason);

                if (_disconnected)
                    return;

                _connection?.Dispose();
                _connection = null;
                _disconnected = true;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "user disconnect");
            }
        }


        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(ServerMessage message)
        {
            if (message == null)
                return;

            if (_connection == null)
                return;

            var bytes = message.GetReversedBytes();

            _connection.Send(bytes);
        }
         internal void SendMessageAsync(ServerMessage message)
        {
            if (message == null)
                return;

            if (_connection == null)
                return;

            var bytes = message.GetReversedBytes();

            _connection.SendAsync(bytes);
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal void SendMessage(byte[] bytes)
        {
            _connection?.Send(bytes);
        }

        internal void SendMessage(ArraySegment<byte> bytes)
        {
            _connection?.SendArray(bytes);
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        internal void SendMessage(StaticMessage type)
        {
            _connection?.Send(StaticMessagesManager.Get(type));
        }

        /// <summary>
        ///     Switches the parser request.
        /// </summary>
        private void SwitchParserRequest(byte[] data, int amountOfBytes)
        {
            try
            {
                if (_connection == null)
                    return;


                _connection.Parser.Dispose();
                _connection.Parser = PacketParser;
                _connection.Parser.HandlePacketData(data, amountOfBytes);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Handle packet");
            }
        }

        /// <summary>
        ///     Policies the request.
        /// </summary>
        private void PolicyRequest()
        {
            _connection.Send(CrossDomainPolicy.XmlPolicyBytes);
        }
    }
}