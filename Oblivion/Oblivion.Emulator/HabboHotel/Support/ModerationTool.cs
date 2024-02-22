using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms.Chat;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Support
{
    /// <summary>
    ///     Class ModerationTool.
    /// </summary>
    public class ModerationTool
    {
        /// <summary>
        ///     Abusive suppot ticket cooldown
        /// </summary>
        internal Dictionary<uint, double> AbusiveCooldown;

        /// <summary>
        ///     The moderation templates
        /// </summary>
        internal Dictionary<uint, ModerationTemplate> ModerationTemplates;

        /// <summary>
        ///     The room message presets
        /// </summary>
        internal List<string> RoomMessagePresets;

        /// <summary>
        ///     The support ticket hints
        /// </summary>
        internal StringDictionary SupportTicketHints;

        /// <summary>
        ///     The tickets
        /// </summary>
        internal List<SupportTicket> Tickets;

        /// <summary>
        ///     The user message presets
        /// </summary>
        internal List<string> UserMessagePresets;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModerationTool" /> class.
        /// </summary>
        internal ModerationTool()
        {
            Tickets = new List<SupportTicket>();
            UserMessagePresets = new List<string>();
            RoomMessagePresets = new List<string>();
            SupportTicketHints = new StringDictionary();
            ModerationTemplates = new Dictionary<uint, ModerationTemplate>();
            AbusiveCooldown = new Dictionary<uint, double>();
        }


        /// <summary>
        ///     Sends the ticket to moderators.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        internal static async Task SendTicketToModerators(SupportTicket ticket)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolIssueMessageComposer"));
            message = ticket.Serialize(message);

            await Oblivion.GetGame().GetClientManager().StaffAlert(message);
        }

        /// <summary>
        ///     Performs the room action.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="kickUsers">if set to <c>true</c> [kick users].</param>
        /// <param name="lockRoom">if set to <c>true</c> [lock room].</param>
        /// <param name="inappropriateRoom">if set to <c>true</c> [inappropriate room].</param>
        /// <param name="message">The message.</param>
        internal static async Task PerformRoomAction(GameClient modSession, uint roomId, bool kickUsers, bool lockRoom,
            bool inappropriateRoom, ServerMessage message)
        {
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);

            if (room == null)
                return;

            if (lockRoom)
            {
                room.RoomData.State = 1;

                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    await queryReactor.RunFastQueryAsync($"UPDATE rooms_data SET state = 'locked' WHERE id = {room.RoomId}");
            }

            if (inappropriateRoom)
            {
                room.RoomData.Name = "Inapropriado para a Gerência do Hotel";
                room.RoomData.Description = "A descrição do quarto não é permitida.";
                room.ClearTags();
                await room.RoomData.SerializeRoomData(message, modSession, false, true);
            }

            if (kickUsers)
                await room.OnRoomKick();
        }

        /// <summary>
        ///     Mods the action result.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="result">if set to <c>true</c> [result].</param>
        internal static async Task ModActionResult(uint userId, bool result)
        {
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
            await clientByUserId.GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("ModerationActionResultMessageComposer"));
            await clientByUserId.GetMessageHandler().GetResponse().AppendIntegerAsync(userId);
            clientByUserId.GetMessageHandler().GetResponse().AppendBool(false);
            await clientByUserId.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Serializes the room tool.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>ServerMessage.</returns>
        internal static async Task<ServerMessage> SerializeRoomTool(RoomData data)
        {
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(data.Id);

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ModerationRoomToolMessageComposer"));
            await serverMessage.AppendIntegerAsync(data.Id);
            await serverMessage.AppendIntegerAsync(data.UsersNow);

            if (room != null)
                serverMessage.AppendBool(room.GetRoomUserManager().GetRoomUserByHabbo(data.Owner) != null);
            else
                serverMessage.AppendBool(false);

            await serverMessage.AppendIntegerAsync(room?.RoomData.OwnerId ?? 0);
            await serverMessage.AppendStringAsync(data.Owner);
            serverMessage.AppendBool(room != null);
            await serverMessage.AppendStringAsync(data.Name);
            await serverMessage.AppendStringAsync(data.Description);
            await serverMessage.AppendIntegerAsync(data.TagCount);

            /* TODO CHECK */
            foreach (var current in data.Tags)
                await serverMessage.AppendStringAsync(current);

            serverMessage.AppendBool(false);

            return serverMessage;
        }

        /// <summary>
        ///     Kicks the user.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="soft">if set to <c>true</c> [soft].</param>
        internal static async Task KickUser(GameClient modSession, uint userId, string message, bool soft)
        {
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            if (clientByUserId == null || clientByUserId.GetHabbo().CurrentRoomId < 1 ||
                clientByUserId.GetHabbo().Id == modSession.GetHabbo().Id)
            {
                await ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }

            if (clientByUserId.GetHabbo().Rank >= modSession.GetHabbo().Rank)
            {
                await ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }

            var room = clientByUserId.GetHabbo().CurrentRoom;

            if (room == null)
                return;

            await room.GetRoomUserManager().RemoveUserFromRoom(clientByUserId, true, false);
            clientByUserId.CurrentRoomUserId = -1;

            await clientByUserId.SendNotif(message);

            if (soft)
                return;

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                await queryReactor.RunFastQueryAsync($"UPDATE users_info SET cautions = cautions + 1 WHERE user_id = {userId}");
        }

        /// <summary>
        ///     Alerts the user.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="caution">if set to <c>true</c> [caution].</param>
        internal static async Task AlertUser(GameClient modSession, uint userId, string message, bool caution)
        {
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
            if (clientByUserId != null)
                await clientByUserId.SendModeratorMessage(message);
        }

        /// <summary>
        ///     Locks the trade.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="length">The length.</param>
        internal static async Task LockTrade(GameClient modSession, uint userId, string message, int length)
        {
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            if (clientByUserId == null)
                return;

            if (!clientByUserId.GetHabbo().CheckTrading())
                length += Oblivion.GetUnixTimeStamp() - clientByUserId.GetHabbo().TradeLockExpire;

            clientByUserId.GetHabbo().TradeLocked = true;
            clientByUserId.GetHabbo().TradeLockExpire = Oblivion.GetUnixTimeStamp() + length;
            await clientByUserId.SendNotifyAsync(message).ConfigureAwait(false);

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                await queryReactor.RunFastQueryAsync(
                    $"UPDATE users SET trade_lock_expire = '{clientByUserId.GetHabbo().TradeLockExpire}' WHERE id = '{clientByUserId.GetHabbo().Id}'");
        }

        /// <summary>
        ///     Bans the user.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="length">The length.</param>
        /// <param name="message">The message.</param>
        internal static async Task BanUser(GameClient modSession, uint userId, int length, string message)
        {
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            if (clientByUserId == null || clientByUserId.GetHabbo().Id == modSession.GetHabbo().Id)
            {
                await ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }

            if (clientByUserId.GetHabbo().Rank >= modSession.GetHabbo().Rank)
            {
                await ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }


            await Oblivion.GetGame()
                .GetBanManager()
                .BanUser(clientByUserId, modSession.GetHabbo().UserName, length, message, false, false);
        }

        /// <summary>
        ///     Serializes the user information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>ServerMessage.</returns>
        /// <exception cref="System.NullReferenceException">User not found in database.</exception>
        internal static ServerMessage SerializeUserInfo(uint userId)
        {
            var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolUserToolMessageComposer"));
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                if (queryReactor != null)
                {
                    queryReactor.SetQuery(
                        "SELECT id, username, mail, look, trade_lock_expire, rank, ip_last, " +
                        "IFNULL(cfhs, 0) cfhs, IFNULL(cfhs_abusive, 0) cfhs_abusive, IFNULL(cautions, 0) cautions, IFNULL(bans, 0) bans, " +
                        "IFNULL(reg_timestamp, 0) reg_timestamp, IFNULL(login_timestamp, 0) login_timestamp " +
                        $"FROM users left join users_info on (users.id = users_info.user_id) WHERE id = '{userId}' LIMIT 1"
                    );

                    var row = queryReactor.GetRow();

                    var id = Convert.ToUInt32(row["id"]);
                    serverMessage.AppendInteger(id);
                    serverMessage.AppendString(row["username"].ToString());
                    serverMessage.AppendString(row["look"].ToString());
                    var regTimestamp = (double)row["reg_timestamp"];
                    var loginTimestamp = (double)row["login_timestamp"];
                    var unixTimestamp = Oblivion.GetUnixTimeStamp();
                    serverMessage.AppendInteger(
                        (int)(regTimestamp > 0 ? Math.Ceiling((unixTimestamp - regTimestamp) / 60.0) : regTimestamp));
                    serverMessage.AppendInteger(
                        (int)
                        (loginTimestamp > 0 ? Math.Ceiling((unixTimestamp - loginTimestamp) / 60.0) : loginTimestamp));
                    serverMessage.AppendBool(true);
                    serverMessage.AppendInteger(Convert.ToInt32(row["cfhs"]));
                    serverMessage.AppendInteger(Convert.ToInt32(row["cfhs_abusive"]));
                    serverMessage.AppendInteger(Convert.ToInt32(row["cautions"]));
                    serverMessage.AppendInteger(Convert.ToInt32(row["bans"]));

                    serverMessage.AppendInteger(0);
                    var rank = (uint)row["rank"];
                    var expire = int.Parse(row["trade_lock_expire"].ToString());
                    serverMessage.AppendString(expire >= Oblivion.GetUnixTimeStamp()
                        ? Oblivion.UnixToDateTime(expire).ToLongDateString()
                        : "Not trade-locked");
                    serverMessage.AppendString(rank < 6 ? row["ip_last"].ToString() : "127.0.0.1");
                    serverMessage.AppendInteger(id);
                    serverMessage.AppendInteger(0);

                    serverMessage.AppendString($"E-Mail:         {row["mail"]}");
                    serverMessage.AppendString($"Rank ID:        {rank}");
                }
            }

            return serverMessage;
        }

        /// <summary>
        ///     Serializes the room visits.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>ServerMessage.</returns>
        internal static async Task<ServerMessage> SerializeRoomVisits(uint userId)
        {
            var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolRoomVisitsMessageComposer"));
            await serverMessage.AppendIntegerAsync(userId);

            var user = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            if (user?.GetHabbo() == null)
            {
                await serverMessage.AppendStringAsync("Not online");
                await serverMessage.AppendIntegerAsync(0);
                return serverMessage;
            }

            await serverMessage.AppendStringAsync(user.GetHabbo().UserName);
            serverMessage.StartArray();

            /* TODO CHECK */
            foreach (var roomId in user.GetHabbo()
                         .RecentlyVisitedRooms)
            {
                var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
                if (roomData != null)
                {
                    await serverMessage.AppendIntegerAsync(roomData.Id);
                    await serverMessage.AppendStringAsync(roomData.Name);

                    await serverMessage.AppendIntegerAsync(0); //hour
                    await serverMessage.AppendIntegerAsync(0); //min

                    serverMessage.SaveArray();
                }
            }

            serverMessage.EndArray();
            return serverMessage;
        }

        /// <summary>
        ///     Serializes the user chatlog.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>ServerMessage.</returns>
        internal static async Task<ServerMessage> SerializeUserChatlog(uint userId)
        {
            ServerMessage result;

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery(
                    $"SELECT DISTINCT room_id FROM users_chatlogs WHERE user_id = '{userId}' ORDER BY timestamp DESC LIMIT 4");
                var table = queryReactor.GetTable();
                var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolUserChatlogMessageComposer"));
                await serverMessage.AppendIntegerAsync(userId);
                await serverMessage.AppendStringAsync(Oblivion.GetGame().GetClientManager().GetNameById(userId));

                if (table != null)
                {
                    await serverMessage.AppendIntegerAsync(table.Rows.Count);
                    var enumerator = table.Rows.GetEnumerator();

                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            var dataRow = (DataRow)enumerator.Current;

                            queryReactor.SetQuery(
                                $"SELECT user_id,timestamp,message FROM users_chatlogs WHERE room_id = {dataRow["room_id"]} AND user_id = {userId} ORDER BY timestamp DESC LIMIT 30");

                            var table2 = queryReactor.GetTable();
                            var roomData = await Oblivion.GetGame().GetRoomManager()
                                .GenerateRoomData((uint)dataRow["room_id"]);

                            if (table2 != null)
                            {
                                serverMessage.AppendByte(1);
                                await serverMessage.AppendShortAsync(2);
                                await serverMessage.AppendStringAsync("roomName");
                                serverMessage.AppendByte(2);
                                await serverMessage.AppendStringAsync(roomData == null ? "This room was deleted" : roomData.Name);
                                await serverMessage.AppendStringAsync("roomId");
                                serverMessage.AppendByte(1);
                                await serverMessage.AppendIntegerAsync((uint)dataRow["room_id"]);
                                await serverMessage.AppendShortAsync(table2.Rows.Count);
                                var enumerator2 = table2.Rows.GetEnumerator();
                                try
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        var dataRow2 = (DataRow)enumerator2.Current;

                                        var habboForId = Oblivion.GetHabboById((uint)dataRow2["user_id"]);
                                        Oblivion.UnixToDateTime((double)dataRow2["timestamp"]);

                                        if (habboForId == null)
                                            return null;

                                        await serverMessage.AppendIntegerAsync(
                                            ((int)(Oblivion.GetUnixTimeStamp() - (double)dataRow2["timestamp"])));

                                        await serverMessage.AppendIntegerAsync(habboForId.Id);
                                        await serverMessage.AppendStringAsync(habboForId.UserName);
                                        await serverMessage.AppendStringAsync(dataRow2["message"].ToString());
                                        serverMessage.AppendBool(false);
                                    }

                                    continue;
                                }
                                finally
                                {
                                    var disposable = enumerator2 as IDisposable;

                                    disposable?.Dispose();
                                }
                            }

                            serverMessage.AppendByte(1);
                            await serverMessage.AppendShortAsync(0);
                            await serverMessage.AppendShortAsync(0);
                        }

                        result = serverMessage;
                        return result;
                    }
                    finally
                    {
                        var disposable2 = enumerator as IDisposable;

                        disposable2?.Dispose();
                    }
                }

                await serverMessage.AppendIntegerAsync(0);
                result = serverMessage;
            }

            return result;
        }

        /// <summary>
        ///     Serializes the ticket chatlog.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="roomData">The room data.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>ServerMessage.</returns>
        /// <exception cref="System.NullReferenceException">No room found.</exception>
        internal static async Task<ServerMessage> SerializeTicketChatlog(SupportTicket ticket, RoomData roomData, double timestamp)
        {
            var message = new ServerMessage();

            var room = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(ticket.RoomId);

            if (room != null)
            {
                await message.InitAsync(LibraryParser.OutgoingRequest("ModerationToolIssueChatlogMessageComposer"));

                await message.AppendIntegerAsync(ticket.TicketId);
                await message.AppendIntegerAsync(ticket.SenderId);
                await message.AppendIntegerAsync(ticket.ReportedId);
                await message.AppendIntegerAsync(ticket.RoomId);

                message.AppendByte(1);
                await message.AppendShortAsync(2);
                await message.AppendStringAsync("roomName");
                message.AppendByte(2);
                await message.AppendStringAsync(ticket.RoomName);
                await message.AppendStringAsync("roomId");
                message.AppendByte(1);
                await message.AppendIntegerAsync(ticket.RoomId);

                var tempChatlogs =
                    room.RoomChat.Skip(Math.Max(0, room.RoomChat.Count - 60)).Take(60).ToList();

                await message.AppendShortAsync(tempChatlogs.Count);

                /* TODO CHECK */
                foreach (var chatLog in tempChatlogs)
                    message = await chatLog.Serialize(message);

                return message;
            }

            return null;
        }

        /// <summary>
        ///     Serializes the room chatlog.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>ServerMessage.</returns>
        /// <exception cref="System.NullReferenceException">No room found.</exception>
        internal static async Task<ServerMessage> SerializeRoomChatlog(uint roomId)
        {
            var message = new ServerMessage();

            var room = await Oblivion.GetGame().GetRoomManager().LoadRoom(roomId);

            if (room?.RoomData != null)
            {
                await message.InitAsync(LibraryParser.OutgoingRequest("ModerationToolRoomChatlogMessageComposer"));
                message.AppendByte(1);
                await message.AppendShortAsync(2);
                await message.AppendStringAsync("roomName");
                message.AppendByte(2);
                await message.AppendStringAsync(room.RoomData.Name);
                await message.AppendStringAsync("roomId");
                message.AppendByte(1);
                await message.AppendIntegerAsync(room.RoomData.Id);

                var tempChatlogs = new List<Chatlog>();
                var i = 0;
                foreach (var chatlog in room.RoomData.RoomChat.TakeWhile(chatlog => i < 150))
                {
                    tempChatlogs.Add(chatlog);
                    i++;
                }

                DataTable table = null;
                if (i < 150)
                {
                    using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        dbClient.RunQuery(
                            $"SELECT user_id,timestamp,message FROM users_chatlogs WHERE room_id = '{room.RoomId}' LIMIT 50");
                        table = dbClient.GetTable();
                    }
                }

                if (table != null)
                {
                    i += table.Rows.Count;
                }

                await message.AppendShortAsync(i);

                if (table != null)
                    foreach (DataRow row in table.Rows)
                    {
                        var timeStamp = Oblivion.UnixToDateTime((double)row["timestamp"]);
                        var habbo = Oblivion.GetHabboById(Convert.ToUInt32(row["user_id"]));
                        await message.AppendStringAsync(timeStamp.ToString("h:mm:ss"));
                        await message.AppendIntegerAsync(Convert.ToInt32(row["user_id"]));
                        await message.AppendStringAsync(habbo == null ? "*User not found*" : habbo.UserName);
                        await message.AppendStringAsync(row["message"].ToString());
                        message.AppendBool(false);
                    }

                foreach (var chatLog in tempChatlogs)
                {
                    message = await chatLog.Serialize(message);
                }

                return message;
            }

            return null;
        }

        /// <summary>
        ///     Serializes the tool.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeTool(GameClient session)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadModerationToolMessageComposer"));

            serverMessage.AppendInteger(Tickets.Count);

            foreach (var current in Tickets)
                current.Serialize(serverMessage);


            serverMessage.AppendInteger(UserMessagePresets.Count);

            foreach (var current2 in UserMessagePresets)
                serverMessage.AppendString(current2);


            /* TODO CHECK */

            var enumerable =
                (from x in ModerationTemplates.Values where x.Category == -1 select x).ToList();

            serverMessage.AppendInteger(enumerable.Count);

            foreach (var entry in enumerable)
            {
                serverMessage.AppendString(entry.Caption);
            }

            // but = button
            serverMessage.AppendBool(true); //ticket_queue_but
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_chatlogs")); //chatlog_but
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_alert")); //message_but
            serverMessage.AppendBool(true); //modaction_but
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_ban")); //ban_but
            serverMessage.AppendBool(true);
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_kick")); //kick_but

            serverMessage.AppendInteger(RoomMessagePresets.Count);

            /* TODO CHECK */
            foreach (var current4 in RoomMessagePresets)
                serverMessage.AppendString(current4);

            return serverMessage;
        }

        /// <summary>
        ///     Loads the message presets.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal Task LoadMessagePresets(IQueryAdapter dbClient)
        {
            UserMessagePresets.Clear();
            RoomMessagePresets.Clear();
            SupportTicketHints.Clear();
            ModerationTemplates.Clear();
            dbClient.SetQuery("SELECT type,message FROM moderation_presets WHERE enabled = 2");
            var table = dbClient.GetTable();
            dbClient.SetQuery("SELECT word,hint FROM moderation_tickethints");
            var table2 = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM moderation_templates");
            var table3 = dbClient.GetTable();

            if (table == null || table2 == null)
                return Task.CompletedTask;

            foreach (DataRow dataRow in table.Rows)
            {
                var item = (string)dataRow["message"];
                var a = dataRow["type"].ToString().ToLower();

                if (a != "message")
                {
                    switch (a)
                    {
                        case "roommessage":
                            RoomMessagePresets.Add(item);
                            break;
                    }
                }
                else
                    UserMessagePresets.Add(item);
            }

            foreach (DataRow dataRow2 in table2.Rows)
                SupportTicketHints.Add((string)dataRow2[0], (string)dataRow2[1]);

            foreach (DataRow dataRow3 in table3.Rows)
                ModerationTemplates.Add(uint.Parse(dataRow3["id"].ToString()),
                    new ModerationTemplate(uint.Parse(dataRow3["id"].ToString()),
                        short.Parse(dataRow3["category"].ToString()), dataRow3["cName"].ToString(),
                        dataRow3["caption"].ToString(), dataRow3["warning_message"].ToString(),
                        dataRow3["ban_message"].ToString(), short.Parse(dataRow3["ban_hours"].ToString()),
                        dataRow3["avatar_ban"].ToString() == "1", dataRow3["mute"].ToString() == "1",
                        dataRow3["trade_lock"].ToString() == "1"));
            return Task.CompletedTask;
        }


        /// <summary>
        ///     Sends the new ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="category">The category.</param>
        /// <param name="type">The type.</param>
        /// <param name="reportedUser">The reported user.</param>
        /// <param name="message">The message.</param>
        /// <param name="messages">The messages.</param>
        internal async Task SendNewTicket(GameClient session, int category, int type, uint reportedUser, string message,
            List<string> messages)
        {
            uint id;

            if (session.GetHabbo().CurrentRoomId <= 0)
            {
                using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    dbClient.SetQuery(
                        string.Concat(
                            "INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'",
                            category, "','open','", session.GetHabbo().Id, "','", reportedUser,
                            "','0',@message,'0','','", Oblivion.GetUnixTimeStamp(), "')"));
                    dbClient.AddParameter("message", message);
                    id = (uint)await dbClient.InsertQueryAsync();
                    await dbClient.RunFastQueryAsync(
                        $"UPDATE users_info SET cfhs = cfhs + 1 WHERE user_id = {session.GetHabbo().Id}");
                }

                var ticket = new SupportTicket(id, 1, category, type, session.GetHabbo().Id, reportedUser, message, 0u,
                    "", Oblivion.GetUnixTimeStamp(), messages);

                Tickets.Add(ticket);
                await SendTicketToModerators(ticket);
            }
            else
            {
                var data = await Oblivion.GetGame().GetRoomManager()
                    .GenerateNullableRoomData(session.GetHabbo().CurrentRoomId);

                using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    dbClient.SetQuery(
                        string.Concat(
                            "INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'",
                            category, "','open','", session.GetHabbo().Id, "','", reportedUser, "','0',@message,'",
                            data.Id, "',@name,'", Oblivion.GetUnixTimeStamp(), "')"));
                    dbClient.AddParameter("message", message);
                    dbClient.AddParameter("name", data.Name);
                    id = (uint)await dbClient.InsertQueryAsync();
                    await dbClient.RunFastQueryAsync(
                        $"UPDATE users_info SET cfhs = cfhs + 1 WHERE user_id = {session.GetHabbo().Id}");
                }

                var ticket2 = new SupportTicket(id, 1, category, type, session.GetHabbo().Id, reportedUser, message,
                    data.Id, data.Name, Oblivion.GetUnixTimeStamp(), messages);

                Tickets.Add(ticket2);
                await SendTicketToModerators(ticket2);
            }
        }

        /// <summary>
        ///     Gets the ticket.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <returns>SupportTicket.</returns>
        internal SupportTicket GetTicket(uint ticketId)
        {
            return Tickets.FirstOrDefault(current => current.TicketId == ticketId);
        }

        /// <summary>
        ///     Picks the ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        internal async Task PickTicket(GameClient session, uint ticketId)
        {
            var ticket = GetTicket(ticketId);

            if (ticket == null || ticket.Status != TicketStatus.Open)
                return;

            await ticket.Pick(session.GetHabbo().Id, true);
            await SendTicketToModerators(ticket);
        }

        /// <summary>
        ///     Releases the ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        internal async Task ReleaseTicket(GameClient session, uint ticketId)
        {
            var ticket = GetTicket(ticketId);

            if (ticket == null || ticket.Status != TicketStatus.Picked || ticket.ModeratorId != session.GetHabbo().Id)
                return;

            await ticket.Release(true);
            await SendTicketToModerators(ticket);
        }

        /// <summary>
        ///     Closes the ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="result">The result.</param>
        internal async Task CloseTicket(GameClient session, uint ticketId, int result)
        {
            var ticket = GetTicket(ticketId);

            if (ticket == null || ticket.Status != TicketStatus.Picked || ticket.ModeratorId != session.GetHabbo().Id)
                return;

            var senderUser = Oblivion.GetHabboById(ticket.SenderId);

            if (senderUser == null)
                return;

            uint statusCode;

            TicketStatus newStatus;

            switch (result)
            {
                case 1:
                    statusCode = 1;
                    newStatus = TicketStatus.Invalid;
                    break;

                case 2:
                    statusCode = 2;
                    newStatus = TicketStatus.Abusive;
                    break;

                default:
                    statusCode = 0;
                    newStatus = TicketStatus.Resolved;
                    break;
            }

            if (statusCode == 2)
            {
                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    AbusiveCooldown.Add(ticket.SenderId, Oblivion.GetUnixTimeStamp() + 600);
                    await queryReactor.RunFastQueryAsync(
                        $"UPDATE users_info SET cfhs_abusive = cfhs_abusive + 1 WHERE user_id = {ticket.SenderId}");
                }
            }

            var senderClient = Oblivion.GetGame().GetClientManager().GetClientByUserId(senderUser.Id);

            if (senderClient != null)
            {
                /* TODO CHECK */
                foreach (
                    var current2 in
                    Tickets.FindAll(
                        current => current.ReportedId == ticket.ReportedId && current.Status == TicketStatus.Picked)
                )
                {
                    await current2.Delete(true);
                    await SendTicketToModerators(current2);
                    await current2.Close(newStatus, true);
                }

                await senderClient.GetMessageHandler()
                    .GetResponse()
                    .InitAsync(LibraryParser.OutgoingRequest("ModerationToolUpdateIssueMessageComposer"));
                await senderClient.GetMessageHandler().GetResponse().AppendIntegerAsync(1);
                await senderClient.GetMessageHandler().GetResponse().AppendIntegerAsync(ticket.TicketId);
                await senderClient.GetMessageHandler().GetResponse().AppendIntegerAsync(ticket.ModeratorId);
                await senderClient.GetMessageHandler()
                    .GetResponse()
                    .AppendStringAsync((Oblivion.GetHabboById(ticket.ModeratorId) != null)
                        ? Oblivion.GetHabboById(ticket.ModeratorId).UserName
                        : "Undefined");
                senderClient.GetMessageHandler().GetResponse().AppendBool(false);
                await senderClient.GetMessageHandler().GetResponse().AppendIntegerAsync(0);
                await senderClient.GetMessageHandler()
                    .GetResponse()
                    .InitAsync(LibraryParser.OutgoingRequest("ModerationTicketResponseMessageComposer"));
                await senderClient.GetMessageHandler().GetResponse().AppendIntegerAsync(statusCode);
                await senderClient.GetMessageHandler().SendResponse();
            }
            else
            {
                /* TODO CHECK */
                foreach (
                    var current2 in
                    Tickets.FindAll(
                        current => current.ReportedId == ticket.ReportedId && current.Status == TicketStatus.Picked)
                )
                {
                    await current2.Delete(true);
                    await SendTicketToModerators(current2);
                    await current2.Close(newStatus, true);
                }
            }

            using (var queryreactor2 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                await queryreactor2.RunFastQueryAsync(
                    $"UPDATE users_stats SET tickets_answered = tickets_answered+1 WHERE id={session.GetHabbo().Id} LIMIT 1");
        }

        /// <summary>
        ///     Check if the user has an pending issue
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool UsersHasPendingTicket(uint id)
        {
            return Tickets.Any(current => current.SenderId == id && current.Status == TicketStatus.Open);
        }

        /// <summary>
        ///     Check if the previous issue of an user was abusive
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool UsersHasAbusiveCooldown(uint id)
        {
            if (!AbusiveCooldown.TryGetValue(id, out var time)) return false;
            if (time - Oblivion.GetUnixTimeStamp() > 0) return true;

            AbusiveCooldown.Remove(id);
            return false;
        }

        /// <summary>
        ///     Deletes the pending ticket for user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal async Task DeletePendingTicketForUser(uint id)
        {
            /* TODO CHECK */
            foreach (var current in Tickets)
            {
                if (current.SenderId != id) continue;
                await current.Delete(true);
                await SendTicketToModerators(current);
                break;
            }
        }

        /// <summary>
        ///     Gets the pending ticket for user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>SupportTicket.</returns>
        internal SupportTicket GetPendingTicketForUser(uint id)
        {
            return Tickets.FirstOrDefault(current => current.SenderId == id && current.Status == TicketStatus.Open);
        }

        /// <summary>
        ///     Logs the staff entry.
        /// </summary>
        /// <param name="modName">Name of the mod.</param>
        /// <param name="target">The target.</param>
        /// <param name="type">The type.</param>
        /// <param name="description">The description.</param>
        internal async Task LogStaffEntry(string modName, string target, string type, string description)
        {
            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery(
                    "INSERT INTO server_stafflogs (staffuser,target,action_type,description) VALUES (@Username,@target,@type,@desc)");
                queryReactor.AddParameter("Username", modName);
                queryReactor.AddParameter("target", target);
                queryReactor.AddParameter("type", type);
                queryReactor.AddParameter("desc", description);
                await queryReactor.RunQueryAsync();
            }
        }
    }
}