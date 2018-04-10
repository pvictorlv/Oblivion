using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Security;

namespace Oblivion.HabboHotel.Users.Messenger
{
    /// <summary>
    ///     Class HabboMessenger.
    /// </summary>
    internal class HabboMessenger
    {
        /// <summary>
        ///     The _user identifier
        /// </summary>
        private readonly uint _userId;

        /// <summary>
        ///     The appear offline
        /// </summary>
        internal bool AppearOffline;

        /// <summary>
        ///     The friends
        /// </summary>
        internal Dictionary<uint, MessengerBuddy> Friends;

        /// <summary>
        ///     The requests
        /// </summary>
        internal Dictionary<uint, MessengerRequest> Requests;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HabboMessenger" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal HabboMessenger(uint userId)
        {
            Requests = new Dictionary<uint, MessengerRequest>();
            Friends = new Dictionary<uint, MessengerBuddy>();
            _userId = userId;
        }

        internal void Init()
        {
            DataTable friendsTable;
            DataTable friendsRequestsTable;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    string.Format(
                        "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_one_id WHERE messenger_friendships.user_two_id = {0} UNION ALL SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE messenger_friendships.user_one_id = {0} LIMIT 1100",
                        _userId));
                friendsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT messenger_requests.from_id,messenger_requests.to_id,users.Username, users.Look FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = {_userId}");
                friendsRequestsTable = queryReactor.GetTable();
            }


            foreach (DataRow row in friendsTable.Rows)
            {
                var num4 = Convert.ToUInt32(row["id"]);
                var pUsername = (string) row["username"];
                var pLook = (string) row["look"];
                var pMotto = (string) row["motto"];
                var pAppearOffline = Oblivion.EnumToBool(row["hide_online"].ToString());
                var pHideInroom = Oblivion.EnumToBool(row["hide_inroom"].ToString());

                if (num4 != _userId && !Friends.ContainsKey(num4))
                    Friends.Add(num4,
                        new MessengerBuddy(num4, pUsername, pLook, pMotto, pAppearOffline, pHideInroom));
            }


            foreach (DataRow row in friendsRequestsTable.Rows)
            {
                var num5 = Convert.ToUInt32(row["from_id"]);
                var num6 = Convert.ToUInt32(row["to_id"]);
                var pUsername2 = row["username"].ToString();
                var pLook = row["look"].ToString();

                if (num5 != _userId)
                {
                    if (!Requests.ContainsKey(num5))
                        Requests.Add(num5, new MessengerRequest(_userId, num5, pUsername2, pLook));
                    else if (!Requests.ContainsKey(num6))
                        Requests.Add(num6, new MessengerRequest(_userId, num6, pUsername2, pLook));
                }
            }
        }

        /// <summary>
        ///     Clears the requests.
        /// </summary>
        internal void ClearRequests()
        {
            Requests.Clear();
        }

        /// <summary>
        ///     Gets the request.
        /// </summary>
        /// <param name="senderId">The sender identifier.</param>
        /// <returns>MessengerRequest.</returns>
        internal MessengerRequest GetRequest(uint senderId) =>
            Requests.TryGetValue(senderId, out var sender) ? sender : null;

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            foreach (var current in Friends.Values.ToList())
            {
                var user = current?.Client;
                user?.GetHabbo()?.GetMessenger()?.UpdateFriend(_userId, null, true);
            }

            Friends?.Clear();
            Requests?.Clear();
            Friends = null;
            Requests = null;
        }

        /// <summary>
        ///     Called when [status changed].
        /// </summary>
        /// <param name="notification">if set to <c>true</c> [notification].</param>
        internal void OnStatusChanged(bool notification)
        {
            if (Friends == null)
                return;

            foreach (var current in Friends.Values.ToList())
            {
                var user = current?.Client;
                var messenger = user?.GetHabbo()?.GetMessenger();
                if (messenger == null) continue;

                messenger.UpdateFriend(_userId, user, true);
                UpdateFriend(user.GetHabbo().Id, user, notification);
            }
        }

        /// <summary>
        ///     Updates the friend.
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="client">The client.</param>
        /// <param name="notification">if set to <c>true</c> [notification].</param>
        internal void UpdateFriend(uint userid, GameClient client, bool notification)
        {
            if (Friends == null) return;

            if (!Friends.TryGetValue(userid, out var friend))
                return;

            friend.UpdateUser();

            if (!notification)
                return;
            var client2 = GetClient();

            client2?.SendMessage(SerializeUpdate(friend));
        }

        /// <summary>
        ///     Serializes the messenger action.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        internal void SerializeMessengerAction(int type, string name)
        {
            if (GetClient() == null)
                return;

            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("ConsoleMessengerActionMessageComposer"));
            serverMessage.AppendString(GetClient().GetHabbo().Id.ToString());
            serverMessage.AppendInteger(type);
            serverMessage.AppendString(name);

            foreach (var current in Friends.Values)
            {
                current?.Client?.SendMessage(serverMessage);
            }
        }

        /// <summary>
        ///     Handles all requests.
        /// </summary>
        internal void HandleAllRequests()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery("DELETE FROM messenger_requests WHERE from_id = " + _userId + " OR to_id = " +
                                          _userId);

            ClearRequests();
        }

        /// <summary>
        ///     Handles the request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        internal void HandleRequest(uint sender)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Concat("DELETE FROM messenger_requests WHERE (from_id = ", _userId,
                    " AND to_id = ", sender, ") OR (to_id = ", _userId, " AND from_id = ", sender, ")"));

            Requests.Remove(sender);
        }

        /// <summary>
        ///     Creates the friendship.
        /// </summary>
        /// <param name="friendId">The friend identifier.</param>
        internal void CreateFriendship(uint friendId)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(
                    string.Concat("REPLACE INTO messenger_friendships (user_one_id,user_two_id) VALUES (", _userId, ",",
                        friendId, ")"));

            OnNewFriendship(friendId);

            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(friendId);
            var myClient = Oblivion.GetGame().GetClientManager().GetClientByUserId(_userId);

            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(clientByUserId, "ACH_FriendListSize", 1, true);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(myClient, "ACH_FriendListSize", 1, true);

            if (clientByUserId?.GetHabbo().GetMessenger() != null)
                clientByUserId.GetHabbo().GetMessenger().OnNewFriendship(_userId);
        }

        /// <summary>
        ///     Destroys the friendship.
        /// </summary>
        /// <param name="friendId">The friend identifier.</param>
        internal void DestroyFriendship(uint friendId)
        {
            var habbo = GetClient().GetHabbo();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("DELETE FROM messenger_friendships WHERE (user_one_id = ",
                    _userId, " AND user_two_id = ", friendId, ") OR (user_two_id = ", _userId, " AND user_one_id = ",
                    friendId, ")"));

                queryReactor.RunFastQuery(string.Concat("SELECT id FROM users_relationships WHERE user_id=", habbo.Id,
                    " AND target = ", friendId, " LIMIT 1"));
                var id = queryReactor.GetInteger();
                if (id > 0)
                {
                    queryReactor.RunFastQuery(string.Concat("DELETE FROM users_relationships WHERE (user_id = ",
                        habbo.Id,
                        " AND target = ", friendId, ")"));

                    queryReactor.RunFastQuery(string.Concat("DELETE FROM users_relationships WHERE (user_id = ",
                        friendId,
                        " AND target = ", habbo.Id, ")"));


                    habbo.Data.Relations.Remove(id);
                }

                var clientRelationship = Oblivion.GetGame().GetClientManager().GetClientByUserId(friendId);

                if (clientRelationship?.GetHabbo().GetMessenger() != null)
                {
                    clientRelationship.GetHabbo().GetMessenger().OnDestroyFriendship(_userId);

                    clientRelationship.GetHabbo().Data.Relations.Remove(id);
                }
            }

            OnDestroyFriendship(friendId);
        }

        /// <summary>
        ///     Called when [new friendship].
        /// </summary>
        /// <param name="friendId">The friend identifier.</param>
        internal void OnNewFriendship(uint friendId)
        {
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(friendId);
            MessengerBuddy messengerBuddy;

            if (clientByUserId?.GetHabbo() == null)
            {
                DataRow row;

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(
                        $"SELECT id,username,motto,look,last_online,hide_inroom,hide_online FROM users WHERE id = {friendId}");
                    row = queryReactor.GetRow();
                }

                messengerBuddy = new MessengerBuddy(friendId, (string) row["Username"], (string) row["look"],
                    (string) row["motto"], Oblivion.EnumToBool(row["hide_online"].ToString()),
                    Oblivion.EnumToBool(row["hide_inroom"].ToString()));
            }
            else
            {
                var habbo = clientByUserId.GetHabbo();

                messengerBuddy = new MessengerBuddy(friendId, habbo.UserName, habbo.Look, habbo.Motto,
                    habbo.AppearOffline, habbo.HideInRoom);
                messengerBuddy.UpdateUser();
            }

            if (!Friends.ContainsKey(friendId))
                Friends.Add(friendId, messengerBuddy);

            GetClient().SendMessage(SerializeUpdate(messengerBuddy));
        }


        public void DisposeRoom(uint roomId)
        {
            foreach (var friend in Friends.Values)
            {
                if (friend?.CurrentRoom?.RoomId == roomId)
                {
                    friend.CurrentRoom = null;
                }
            }
        }

        /// <summary>
        ///     Requests the exists.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RequestExists(uint requestId) => Requests != null && Requests.ContainsKey(requestId);

        /// <summary>
        ///     Friendships the exists.
        /// </summary>
        /// <param name="friendId">The friend identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool FriendshipExists(uint friendId) => Friends.ContainsKey(friendId);

        /// <summary>
        ///     Called when [destroy friendship].
        /// </summary>
        /// <param name="friend">The friend.</param>
        internal void OnDestroyFriendship(uint friend)
        {
            Friends.Remove(friend);
            GetClient()
                .GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("FriendUpdateMessageComposer"));
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1); //count
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1); //count
            GetClient().GetMessageHandler().GetResponse().AppendString("Grupos"); //count
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1);
            GetClient().GetMessageHandler().GetResponse().AppendInteger(-1);
            GetClient().GetMessageHandler().GetResponse().AppendInteger(friend);
            GetClient().GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Remove user from group chat
        /// </summary>
        /// <param name="groupId">The group.</param>
        internal void OnDisableChat(int groupId)
        {
            GetClient()
                .GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("FriendUpdateMessageComposer"));
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1); //count
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1); //count
            GetClient().GetMessageHandler().GetResponse().AppendString("Grupos"); //count
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1);
            GetClient().GetMessageHandler().GetResponse().AppendInteger(-1);
            GetClient().GetMessageHandler().GetResponse().AppendInteger(-groupId);
            GetClient().GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Called when [destroy friendship].
        /// </summary>
        /// <param name="friend">The friend.</param>
        internal void OnDestroyFriendship(int friend)
        {
            GetClient()
                .GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("FriendUpdateMessageComposer"));
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1); //count
            GetClient().GetMessageHandler().GetResponse().AppendInteger(2); //id
            GetClient().GetMessageHandler().GetResponse().AppendString("Grupos"); //id
            GetClient().GetMessageHandler().GetResponse().AppendInteger(1);
            GetClient().GetMessageHandler().GetResponse().AppendInteger(-1);
            GetClient().GetMessageHandler().GetResponse().AppendInteger(friend);
            GetClient().GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Requests the buddy.
        /// </summary>
        /// <param name="userQuery">The user query.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RequestBuddy(string userQuery)
        {
            var clientByUsername = Oblivion.GetGame().GetClientManager().GetClientByUserName(userQuery);
            uint userId;
            bool blockForNewFriends;

            if (clientByUsername == null)
            {
                DataRow dataRow;

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("SELECT id, block_newfriends FROM users WHERE username = @query");
                    queryReactor.AddParameter("query", userQuery.ToLower());
                    dataRow = queryReactor.GetRow();
                }

                if (dataRow == null)
                    return false;

                userId = Convert.ToUInt32(dataRow["id"]);
                blockForNewFriends = Oblivion.EnumToBool(dataRow["block_newfriends"].ToString());
            }
            else
            {
                var currentUser = clientByUsername.GetHabbo();
                if (currentUser == null) return false;
                userId = currentUser.Id;
                blockForNewFriends = currentUser.HasFriendRequestsDisabled;
            }

            var client = GetClient();

            if (blockForNewFriends && client.GetHabbo().Rank < 4)
            {
                client
                    .GetMessageHandler()
                    .GetResponse()
                    .Init(LibraryParser.OutgoingRequest("NotAcceptingRequestsMessageComposer"));
                client.GetMessageHandler().GetResponse().AppendInteger(39);
                client.GetMessageHandler().GetResponse().AppendInteger(3);
                client.GetMessageHandler().SendResponse();
                return false;
            }

            if (RequestExists(userId))
            {
                client.SendNotif("Ya le has enviado una petición anteriormente.");
                return true;
            }

            using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryreactor2.RunFastQuery(string.Concat("REPLACE INTO messenger_requests (from_id,to_id) VALUES (",
                    _userId, ",", userId, ")"));

            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(client, QuestType.AddFriends);
            var fromUser = client.GetHabbo();

            if (clientByUsername?.GetHabbo()?.GetMessenger() != null && fromUser != null)
            {
                var messengerRequest = new MessengerRequest(userId, _userId, fromUser.UserName, fromUser.Look);

                clientByUsername.GetHabbo().GetMessenger().OnNewRequest(_userId, messengerRequest);

                var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("ConsoleSendFriendRequestMessageComposer"));

                messengerRequest.Serialize(serverMessage);
                clientByUsername.SendMessage(serverMessage);
            }

            return true;
        }

        /// <summary>
        ///     Called when [new request].
        /// </summary>
        /// <param name="friendId">The friend identifier.</param>
        /// <param name="friendRequest"></param>
        internal void OnNewRequest(uint friendId, MessengerRequest friendRequest)
        {
            if (!Requests.ContainsKey(friendId))
                Requests.Add(friendId, friendRequest);
        }

        /// <summary>
        ///     Sends the instant message.
        /// </summary>
        /// <param name="gp">To identifier.</param>
        /// <param name="message">The message.</param>
        internal void SendInstantMessage(Guild gp, string message)
        {
            if (!BobbaFilter.CanTalk(GetClient(), message))
            {
                return;
            }

            if (!gp.Members.ContainsKey(GetClient().GetHabbo().Id))
            {
                DeliverInstantMessageError(6, gp.Id);
                return;
            }

            var sender = GetClient().GetHabbo();

            /* TODO CHECK */
            foreach (var usr in gp.Members.Values.ToList())
            {
                if (!usr.HasChat) continue;
                GameClient client = Oblivion.GetGame().GetClientManager().GetClientByUserId(usr.Id);
                if (client?.GetHabbo()?.GetMessenger() != null && client.GetHabbo().Id != sender.Id)
                    client.GetHabbo()
                        .GetMessenger()
                        .DeliverInstantMessage((int) gp.Id, message, (int) sender.Id, sender.UserName, sender.Look);
            }
        }

        /// <summary>
        ///     Sends the instant message.
        /// </summary>
        /// <param name="toId">To identifier.</param>
        /// <param name="message">The message.</param>
        internal void SendInstantMessage(uint toId, string message)
        {
            if (!BobbaFilter.CanTalk(GetClient(), message))
            {
                return;
            }


            if (!FriendshipExists(toId))
            {
                DeliverInstantMessageError(6, toId);
                return;
            }

            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(toId);
            if (clientByUserId?.GetHabbo().GetMessenger() == null)
            {
                if (Oblivion.OfflineMessages.TryGetValue(toId, out var msgs))
                {
                    msgs.Add(new OfflineMessage(GetClient().GetHabbo().Id, message,
                        Oblivion.GetUnixTimeStamp()));
                }
                else
                {
                    msgs = new List<OfflineMessage>
                    {
                        new OfflineMessage(GetClient().GetHabbo().Id, message,
                            Oblivion.GetUnixTimeStamp())
                    };
                    Oblivion.OfflineMessages.Add(toId, msgs);
                }

                OfflineMessage.SaveMessage(Oblivion.GetDatabaseManager().GetQueryReactor(), toId,
                    GetClient().GetHabbo().Id, message);

                return;
            }

            if (GetClient().GetHabbo().Muted)
            {
                DeliverInstantMessageError(4, toId);
                return;
            }

            if (clientByUserId.GetHabbo().Muted) DeliverInstantMessageError(3, toId);

            if (message == "")
                return;

            clientByUserId.GetHabbo().GetMessenger().DeliverInstantMessage(message, _userId);
        }

        /// <summary>
        ///     Delivers the instant message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="convoId">The convo identifier.</param>
        internal void DeliverInstantMessage(string message, uint convoId)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ConsoleChatMessageComposer"));
            serverMessage.AppendInteger(convoId);
            serverMessage.AppendString(message);
            serverMessage.AppendInteger(0);
            GetClient().SendMessage(serverMessage);
        }

        /// <summary>
        ///     Delivers the instant message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="GroupId">the group id.</param>
        internal void DeliverInstantMessage(int GroupId, string message, int UserId, string Username,
            string figure)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ConsoleChatMessageComposer"));
            serverMessage.AppendInteger(-GroupId);
            serverMessage.AppendString(message);
            serverMessage.AppendInteger(0);
            serverMessage.AppendString(Username + "/" + figure + "/" + UserId);
            GetClient().SendMessage(serverMessage);
        }

        /// <summary>
        ///     Delivers the instant message error.
        /// </summary>
        /// <param name="errorId">The error identifier.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        internal void DeliverInstantMessageError(int errorId, uint conversationId)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ConsoleChatErrorMessageComposer"));
            serverMessage.AppendInteger(errorId);
            serverMessage.AppendInteger(conversationId);
            serverMessage.AppendString("");
            GetClient().SendMessage(serverMessage);
        }

        /// <summary>
        ///     Serializes the categories.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeCategories()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadFriendsCategories"));
            serverMessage.AppendInteger(1100);
            serverMessage.AppendInteger(600);
            serverMessage.AppendInteger(800);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendString("Grupos");
            return serverMessage;
        }

        /// <summary>
        ///     Serializes the friends.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeFriends()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadFriendsMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(0);

            var client = GetClient();
            if (client?.GetHabbo() == null) return null;
            var groups = new List<Guild>();
            foreach (var gp in Oblivion.GetGame().GetGroupManager().Groups.Values.ToList())
            {
                if (gp?.Members == null) continue;
                if (gp.HasChat && gp.Members.TryGetValue(client.GetHabbo().Id, out var memb))
                {
                    if (memb != null && memb.HasChat)
                        groups.Add(gp);
                }
            }

            serverMessage.AppendInteger(Friends.Count + groups.Count);

            foreach (var current in Friends.Values)
            {
                current.UpdateUser();
                current.Serialize(serverMessage, client);
            }

            foreach (var group in groups)
            {
                serverMessage.AppendInteger(-Convert.ToInt32(group.Id));
                serverMessage.AppendString(group.Name);
                serverMessage.AppendInteger(0);
                serverMessage.AppendBool(group.HasChat);
                serverMessage.AppendBool(false);
                serverMessage.AppendString(group.Badge);
                serverMessage.AppendInteger(1);
                serverMessage.AppendString(group.Description);
                serverMessage.AppendString("");
                serverMessage.AppendString("");
                serverMessage.AppendBool(false);
                serverMessage.AppendBool(false);
                serverMessage.AppendBool(false);
                serverMessage.AppendShort(0);
            }

            return serverMessage;
        }

        /// <summary>
        ///     Serializes the offline messages.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeOfflineMessages(OfflineMessage message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ConsoleChatMessageComposer"));
            serverMessage.AppendInteger(message.FromId);
            serverMessage.AppendString(message.Message);
            serverMessage.AppendInteger(((int) (Oblivion.GetUnixTimeStamp() - message.Timestamp)));

            return serverMessage;
        }

        /// <summary>
        ///     Serializes the update.
        /// </summary>
        /// <param name="friend">The friend.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeUpdate(MessengerBuddy friend)
        {
            var client = GetClient();
            if (client == null) return null;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FriendUpdateMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendString("Grupos");
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(0);
            friend.Serialize(serverMessage, client);
//            serverMessage.AppendBool(false);
            return serverMessage;
        }

        /// <summary>
        ///     Serializes the update.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeUpdate(Guild group)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FriendUpdateMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendString("Grupos");
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(-Convert.ToInt32(group.Id));
            serverMessage.AppendString(group.Name);
            serverMessage.AppendInteger(1);
            serverMessage.AppendBool(true);
            serverMessage.AppendBool(false);
            serverMessage.AppendString(string.Empty);
            serverMessage.AppendInteger(1); //category
            serverMessage.AppendString(group.Name);
            serverMessage.AppendString(string.Empty);
            serverMessage.AppendString(string.Empty);
            serverMessage.AppendBool(false);
            serverMessage.AppendBool(false);
            serverMessage.AppendBool(false);
            serverMessage.AppendShort(0);
            return serverMessage;
        }

        /// <summary>
        ///     Serializes the requests.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeRequests()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FriendRequestsMessageComposer"));
            serverMessage.AppendInteger(Requests.Count > Oblivion.FriendRequestLimit
                ? (int) Oblivion.FriendRequestLimit
                : Requests.Count);
            serverMessage.AppendInteger(Requests.Count > Oblivion.FriendRequestLimit
                ? (int) Oblivion.FriendRequestLimit
                : Requests.Count);

            var requests = Requests.Values.Take((int) Oblivion.FriendRequestLimit);

            foreach (var current in requests)
                current.Serialize(serverMessage);

            return serverMessage;
        }

        /// <summary>
        ///     Performs the search.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage PerformSearch(string query)
        {
            var searchResult = SearchResultFactory.GetSearchResult(query);

            var list = new List<SearchResult>();
            var list2 = new List<SearchResult>();

            foreach (var current in searchResult)
            {
                if (FriendshipExists(current.UserId))
                    list.Add(current);
                else
                    list2.Add(current);
            }

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ConsoleSearchFriendMessageComposer"));

            serverMessage.AppendInteger(list.Count);

            foreach (var current2 in list)
                current2.Searialize(serverMessage);

            serverMessage.AppendInteger(list2.Count);

            foreach (var current3 in list2)
                current3.Searialize(serverMessage);

            return serverMessage;
        }

        /// <summary>
        ///     Gets the active friends rooms.
        /// </summary>
        /// <returns>HashSet&lt;RoomData&gt;.</returns>
        internal HashSet<RoomData> GetActiveFriendsRooms()
        {
            var toReturn = new HashSet<RoomData>();

            /* TODO CHECK */
            foreach (
                var current in
                from p in Friends.Values where p != null && p.InRoom && p.CurrentRoom?.RoomData != null select p)
                toReturn.Add(current.CurrentRoom.RoomData);

            return toReturn;
        }

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient GetClient() => Oblivion.GetGame().GetClientManager().GetClientByUserId(_userId);
    }
}