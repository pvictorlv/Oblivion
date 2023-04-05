using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotNetty.Transport.Channels;
using Oblivion.Configuration;
using Oblivion.Connection;
using Oblivion.Connection.Net;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
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
    public class GameClient : IDisposable
    {
        /// <summary>
        ///     The _connection
        /// </summary>
        private ISession<GameClient> _connection;

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
        internal GameClient(IChannelId clientId, ISession<GameClient> connection)
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
        internal IChannelId ConnectionId { get; set; }

        public bool IsAir { get; set; }

        public ARC4 ServerRc4 { get; set; }

        /// <summary>
        ///     Gets the connection.
        /// </summary>
        /// <returns>ConnectionInformation.</returns>
        internal ISession<GameClient> GetConnection() => _connection;

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
        internal async Task<bool> TryLogin(string authTicket)
        {
            try
            {
                var ip = GetConnection()?.Channel.RemoteAddress.ToString();
                if (ip == null)
                    return false;
                var data = await UserDataFactory.GetUserData(authTicket);
                
                if (data == null)
                    return false;

                var errorCode = data.ErrorCode;
                var userData = data.Data;

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
                await _habbo.LoadData(userData);

                if (MachineId == null)
                    MachineId = "";

                if (string.IsNullOrEmpty(_habbo.UserName) ||
                    Oblivion.GetGame().GetBanManager().CheckBan(_habbo.UserName, ip, MachineId))
                {
                    var banReason = Oblivion.GetGame().GetBanManager()
                        .GetBanReason(_habbo.UserName, ip, MachineId);
                    await SendNotifWithScroll(banReason);
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery($"SELECT ip_last FROM users WHERE id={_habbo.Id} LIMIT 1");

                        var supaString = queryReactor.GetString();

                        queryReactor.SetQuery(
                            $"SELECT COUNT(user_id) FROM users_bans_access WHERE user_id={_habbo.Id} LIMIT 1");
                        var integer = queryReactor.GetInteger();

                        if (integer > 0)
                            await queryReactor.RunFastQueryAsync(
                                "UPDATE users_bans_access SET attempts = attempts + 1, ip='" +
                                supaString + "' WHERE user_id=" + _habbo.Id + " LIMIT 1");
                        else
                            await queryReactor.RunFastQueryAsync(
                                "INSERT INTO users_bans_access (user_id, ip) VALUES (" +
                                GetHabbo().Id + ", '" + supaString + "')");
                    }

                    return false;
                }

                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    await queryReactor.RunFastQueryAsync(
                        $"UPDATE users SET ip_last='{ip}', online = '1' WHERE id={GetHabbo().Id}");

                await _habbo.Init(this, userData);
                using (var msg =
                       new ServerMessage(LibraryParser.OutgoingRequest("AuthenticationOKMessageComposer")))
                {
                    await SendMessage(msg);
                }

                using (var serverMessage = new ServerMessage())
                {
                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));
                    await serverMessage.AppendIntegerAsync(_habbo.HomeRoom);
                    await serverMessage.AppendIntegerAsync(_habbo.HomeRoom);
                    await SendMessage(serverMessage);


                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("FavouriteRoomsMessageComposer"));
                    await serverMessage.AppendIntegerAsync(30);

                    if (_habbo.Data.FavouritedRooms == null || _habbo.Data.FavouritedRooms.Count <= 0)
                        await serverMessage.AppendIntegerAsync(0);
                    else
                    {
                        await serverMessage.AppendIntegerAsync(_habbo.Data.FavouritedRooms.Count);

                        /* TODO CHECK */
                        foreach (var i in _habbo.Data.FavouritedRooms)
                            await serverMessage.AppendIntegerAsync(i);
                    }

                    await SendMessage(serverMessage);
                    if (_habbo.GetSubscriptionManager() != null)
                    {
                        await serverMessage.InitAsync(LibraryParser.OutgoingRequest("UserClubRightsMessageComposer"));
                        await serverMessage.AppendIntegerAsync(_habbo.GetSubscriptionManager().HasSubscription ? 2 : 0);
                        await serverMessage.AppendIntegerAsync(_habbo.Rank);
                        await serverMessage.AppendIntegerAsync(0); //Is an ambassador
                        await SendMessage(serverMessage);
                    }

                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("UserRightsMessageComposer"));
                    serverMessage.AppendBool(true);
                    serverMessage.AppendBool(false);
                    serverMessage.AppendBool(true);
                    await SendMessage(serverMessage);

                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("EnableNotificationsMessageComposer"));
                    serverMessage.AppendBool(true);
                    await SendMessage(serverMessage);

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

                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("CfhTopicsInitMessageComposer"));
                    await serverMessage.AppendIntegerAsync(6);
                    await serverMessage.AppendStringAsync("sexual_content");
                    await serverMessage.AppendIntegerAsync(8);
                    await serverMessage.AppendStringAsync("pii_meeting_irl");
                    await serverMessage.AppendIntegerAsync(1);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("sexual_webcam_images_auto");
                    await serverMessage.AppendIntegerAsync(2);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("explicit_sexual_talk");
                    await serverMessage.AppendIntegerAsync(3);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("cybersex");
                    await serverMessage.AppendIntegerAsync(4);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("cybersex_auto");
                    await serverMessage.AppendIntegerAsync(5);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("meet_some");
                    await serverMessage.AppendIntegerAsync(6);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("meet_irl");
                    await serverMessage.AppendIntegerAsync(7);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("email_or_phone");
                    await serverMessage.AppendIntegerAsync(8);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("scamming");
                    await serverMessage.AppendIntegerAsync(3);
                    await serverMessage.AppendStringAsync("stealing");
                    await serverMessage.AppendIntegerAsync(9);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("scamsites");
                    await serverMessage.AppendIntegerAsync(10);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("selling_buying_accounts_or_furni");
                    await serverMessage.AppendIntegerAsync(11);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("trolling_bad_behavior");
                    await serverMessage.AppendIntegerAsync(11);
                    await serverMessage.AppendStringAsync("hate_speech");
                    await serverMessage.AppendIntegerAsync(12);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("violent_roleplay");
                    await serverMessage.AppendIntegerAsync(13);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("swearing");
                    await serverMessage.AppendIntegerAsync(14);
                    await serverMessage.AppendStringAsync("auto_reply");
                    await serverMessage.AppendStringAsync("drugs");
                    await serverMessage.AppendIntegerAsync(15);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("gambling");
                    await serverMessage.AppendIntegerAsync(16);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("self_threatening");
                    await serverMessage.AppendIntegerAsync(17);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("mild_staff_impersonation");
                    await serverMessage.AppendIntegerAsync(18);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("severe_staff_impersonation");
                    await serverMessage.AppendIntegerAsync(19);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("habbo_name");
                    await serverMessage.AppendIntegerAsync(20);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("minors_access");
                    await serverMessage.AppendIntegerAsync(21);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("bullying");
                    await serverMessage.AppendIntegerAsync(22);
                    await serverMessage.AppendStringAsync("guardians");
                    await serverMessage.AppendStringAsync("game_interruption");
                    await serverMessage.AppendIntegerAsync(2);
                    await serverMessage.AppendStringAsync("flooding");
                    await serverMessage.AppendIntegerAsync(23);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("doors");
                    await serverMessage.AppendIntegerAsync(24);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("room");
                    await serverMessage.AppendIntegerAsync(1);
                    await serverMessage.AppendStringAsync("room_report");
                    await serverMessage.AppendIntegerAsync(25);
                    await serverMessage.AppendStringAsync("mods");
                    await serverMessage.AppendStringAsync("help");
                    await serverMessage.AppendIntegerAsync(2);
                    await serverMessage.AppendStringAsync("help_habbo");
                    await serverMessage.AppendIntegerAsync(26);
                    await serverMessage.AppendStringAsync("auto_reply");
                    await serverMessage.AppendStringAsync("help_payments");
                    await serverMessage.AppendIntegerAsync(27);
                    await serverMessage.AppendStringAsync("auto_reply");
                    await SendMessage(serverMessage);

                    await _habbo.UpdateCreditsBalance();

                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
                    await serverMessage.AppendIntegerAsync(2);
                    await serverMessage.AppendIntegerAsync(0);
                    await serverMessage.AppendIntegerAsync(_habbo.ActivityPoints);
                    await serverMessage.AppendIntegerAsync(5);
                    await serverMessage.AppendIntegerAsync(_habbo.Diamonds);

                    await serverMessage.AppendIntegerAsync(102);
                    await serverMessage.AppendIntegerAsync(_habbo.Graffiti);

                    await SendMessage(serverMessage);


                    if (_habbo.HasFuse("fuse_mod"))
                        await SendMessage(Oblivion.GetGame().GetModerationTool()
                            .SerializeTool(this));

                    await SendMessage(Oblivion.GetGame().GetAchievementManager()
                        .AchievementDataCached);

                    if (!GetHabbo().Vip && ExtraSettings.NewUsersGiftsEnabled)
                    {
                        //todo
                        await serverMessage.InitAsync(
                            LibraryParser.OutgoingRequest("NuxSuggestFreeGiftsMessageComposer"));
                        await SendMessage(serverMessage);
                    }
                }

                await SendMessage(GetHabbo().GetAvatarEffectsInventoryComponent().GetPacket());
                //                    queuedServerMessageSendResponse();

                if (GetHabbo().GetMessenger() != null)
                    await GetHabbo().GetMessenger().OnStatusChanged(true);


                await Oblivion.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(this);
                await Oblivion.GetGame().GetAchievementManager().TryProgressRegistrationAchievements(this);
                await Oblivion.GetGame().GetAchievementManager().TryProgressLoginAchievements(this);


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
        internal async Task SendNotifWithScroll(string message)
        {
            using (var serverMessage =
                   new ServerMessage(LibraryParser.OutgoingRequest("MOTDNotificationMessageComposer")))
            {
                await serverMessage.AppendIntegerAsync(1);
                await serverMessage.AppendStringAsync(System.Net.WebUtility.HtmlDecode(message), true);
                await SendMessage(serverMessage);
            }
        }

        /// <summary>
        ///     Sends the moderator message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal async Task SendModeratorMessage(string message)
        {
            using (var serverMessage =
                   new ServerMessage(LibraryParser.OutgoingRequest("AlertNotificationMessageComposer")))
            {
                await serverMessage.AppendStringAsync(message);
                await serverMessage.AppendStringAsync(string.Empty);
                await SendMessage(serverMessage);
            }
        }

        /// <summary>
        ///     Sends the whisper.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fromWired"></param>
        internal async Task SendWhisper(string message, bool fromWired = false)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            var roomUserByHabbo = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);

            if (roomUserByHabbo == null)
                return;

            using (var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer")))
            {
                await whisp.AppendIntegerAsync(roomUserByHabbo.VirtualId);
                await whisp.AppendStringAsync(message);
                await whisp.AppendIntegerAsync(0);
                await whisp.AppendIntegerAsync(fromWired ? 34 : roomUserByHabbo.LastBubble);
                await whisp.AppendIntegerAsync(0);
                await whisp.AppendIntegerAsync(fromWired);

                await SendMessage(whisp);
            }
        }

        internal async Task SendWhisperAsync(string message, bool fromWired = false)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            var roomUserByHabbo = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);

            if (roomUserByHabbo == null)
                return;

            using (var whisp = new ServerMessage())
            {
                await whisp.InitAsync(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                await whisp.AppendIntegerAsync(roomUserByHabbo.VirtualId);
                await whisp.AppendStringAsync(message);
                await whisp.AppendIntegerAsync(0);
                await whisp.AppendIntegerAsync(fromWired ? 34 : roomUserByHabbo.LastBubble);
                await whisp.AppendIntegerAsync(0);
                await whisp.AppendIntegerAsync(fromWired);

                await SendMessageAsync(whisp);
            }
        }

        /// <summary>
        ///     Sends the notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        internal async Task SendNotif(string message, string title = "Aviso", string picture = "")
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
                await SendMessage(serverMessage);
            }
        }

        /// <summary>
        ///     Sends the notif async.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        internal async Task SendNotifyAsync(string message, string title = "Aviso", string picture = "")
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
                await SendMessage(serverMessage);
            }
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
        public static ServerMessage GetBytesNotif(string message, string title = "Aviso", string picture = "")
        {
            var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
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

            return serverMessage;
        }

        /// <summary>
        ///     Stops this instance.
        /// </summary>
        public async void Dispose()
        {
            try
            {
                if (_habbo != null)
                    await _habbo.OnDisconnect("disconnect");

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
            catch (Exception ex)
            {
                Logging.HandleException(ex, "user disconnect");
            }
        }


        /// <summary>
        ///     Disconnects the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal async Task Disconnect(string reason)
        {
            try
            {
                var habbo = _habbo;
                if (habbo != null)
                    await habbo.OnDisconnect(reason);

                if (_disconnected)
                    return;

                _connection?.Dispose();
                _connection = null;
                _disconnected = true;
                Dispose();
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
        internal async Task SendMessage(ServerMessage message)
        {
            try
            {
                if (message == null)
                    return;

                if (_connection == null || !_connection.Channel.Active)
                    return;

                await _connection.Send(message);
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "SendMessage");
            }
        }

        internal Task SendMessageAsync(ServerMessage message)
        {
            try
            {
                if (message == null)
                    return Task.CompletedTask;

                if (_connection == null || !_connection.Channel.Active)
                    return Task.CompletedTask;

                // var bytes = message.GetReversedBytes();

                return _connection.Send(message);
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "SendMessageAsync");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /* internal async Task SendMessage(byte[] bytes)
         {
             _connection?.Send(bytes); ;
         }
 
         internal async Task SendMessage(ArraySegment<byte> bytes)
         {
             _connection?.Send(bytes.Array); ;
         }
        */
        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        internal Task SendStaticMessage(StaticMessage type)
        {
            return _connection?.Send(StaticMessagesManager.Get(type));
            ;
        }
    }
}