using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oblivion.Configuration;
using Oblivion.Connection.SuperSocket;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.GameClients
{
    /// <summary>
    ///     Class GameClientManager..
    /// </summary>
    internal class GameClientManager
    {
        /// <summary>
        ///     The _badge queue
        /// </summary>
//        private readonly Queue _badgeQueue;
        /// <summary>
        ///     The _id user name register
        /// </summary>
//        private readonly HybridDictionary _idUserNameRegister;
        /// <summary>
        ///     The _user identifier register
        /// </summary>
        private readonly ConcurrentDictionary<uint, GameClient> _userIdRegister;

        /// <summary>
        ///     The _user name identifier register
        /// </summary>
//        private readonly HybridDictionary _userNameIdRegister;
        /// <summary>
        ///     The _user name register
        /// </summary>
        private readonly ConcurrentDictionary<string, GameClient> _userNameRegister;

        /// <summary>
        ///     The clients
        /// </summary>
        internal ConcurrentDictionary<uint, GameClient> Clients;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClientManager" /> class.
        /// </summary>
        internal GameClientManager()
        {
            Clients = new ConcurrentDictionary<uint, GameClient>();
            _userNameRegister = new ConcurrentDictionary<string, GameClient>();
            _userIdRegister = new ConcurrentDictionary<uint, GameClient>();
        }

        /// <summary>
        ///     Gets the client count.
        /// </summary>
        /// <value>The client count.</value>
        internal int ClientCount() => Clients.Count;

        /// <summary>
        ///     Gets the client by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClientByUserId(uint userId) =>
            _userIdRegister.TryGetValue(userId, out var client) ? client : null;

        /// <summary>
        ///     Gets the name of the client by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClientByUserName(string userName) =>
            _userNameRegister.TryGetValue(userName.ToLower(), out var client) ? client : null;

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClient(uint clientId) => Clients.TryGetValue(clientId, out var client) ? client : null;

        /// <summary>
        ///     Gets the name by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>System.String.</returns>
        internal string GetNameById(uint id)
        {
            var clientByUserId = GetClientByUserId(id);

            if (clientByUserId != null)
                return clientByUserId.GetHabbo().UserName;

            string String;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT username FROM users WHERE id = " + id);
                String = queryReactor.GetString();
            }

            return string.IsNullOrEmpty(String) ? "Unknown User" : String;
        }

        /// <summary>
        ///     Gets the clients by identifier.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns>IEnumerable&lt;GameClient&gt;.</returns>
        internal IEnumerable<GameClient> GetClientsById(List<uint> users)
        {
            return users.Select(GetClientByUserId).Where(client => client?.GetHabbo() != null);
        }

        /// <summary>
        ///     Sends the super notif.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="notice">The notice.</param>
        /// <param name="picture">The picture.</param>
        /// <param name="client">The client.</param>
        /// <param name="link">The link.</param>
        /// <param name="linkTitle">The link title.</param>
        /// <param name="broadCast">if set to <c>true</c> [broad cast].</param>
        /// <param name="Event">if set to <c>true</c> [event].</param>
        internal void SendSuperNotif(string title, string notice, string picture, GameClient client, string link,
            string linkTitle, bool broadCast, bool Event)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

            serverMessage.AppendString(picture);
            serverMessage.AppendInteger(4);
            serverMessage.AppendString("title");
            serverMessage.AppendString(title);
            serverMessage.AppendString("message");

            if (broadCast)
                if (Event)
                {
                    var text1 = Oblivion.GetLanguage().GetVar("ha_event_one");
                    var text2 = Oblivion.GetLanguage().GetVar("ha_event_two");
                    var text3 = Oblivion.GetLanguage().GetVar("ha_event_three");
                    serverMessage.AppendString(
                        $"<b>{text1} {client.GetHabbo().CurrentRoom.RoomData.Owner}!</b>\r\n {text2} .\r\n<b>{text3}</b>\r\n{notice}");
                }
                else
                {
                    var text4 = Oblivion.GetLanguage().GetVar("ha_title");
                    serverMessage.AppendString(string.Concat("<b>" + text4 + "</b>\r\n", notice, "\r\n- <i>",
                        client.GetHabbo().UserName, "</i>"));
                }
            else
                serverMessage.AppendString(notice);

            if (link != string.Empty)
            {
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString(link);
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString(linkTitle);
            }
            else
            {
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");
            }

            if (broadCast)
            {
                SendMessageAsync(serverMessage);
                return;
            }

            client.SendMessage(serverMessage);
        }

        /// <summary>
        ///     Staffs the alert.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exclude">The exclude.</param>
        internal void StaffAlert(ServerMessage message, uint exclude = 0u)
        {
            var gameClients = Clients.Values.Where(x =>
                x.GetHabbo() != null && x.GetHabbo().Rank >= Oblivion.StaffAlertMinRank && x.GetHabbo().Id != exclude);

            foreach (var current in gameClients)
                current.SendMessage(message);
        }

        /// <summary>
        ///     Mods the alert.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void ModAlert(ServerMessage message)
        {
            var bytes = message.GetReversedBytes();

            foreach (var current in Clients.Values.Where(current => current?.GetHabbo() != null).Where(current =>
                (current.GetHabbo().Rank == 4u || current.GetHabbo().Rank == 5u) || current.GetHabbo().Rank == 6u))
                current.GetConnection().Send(bytes);
        }

        /// <summary>
        ///     Creates the and start client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="connection">The connection.</param>
        internal void CreateAndStartClient(uint clientId, Session<GameClient> connection)
        {
            var gameClient = new GameClient(clientId, connection);
            connection.UserData = gameClient;
            Clients.AddOrUpdate(clientId, gameClient, (key, value) => gameClient);
            gameClient.StartConnection();
        }

        /// <summary>
        ///     Disposes the connection.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        internal void DisposeConnection(uint clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
                return;
            client.Stop();
            Clients.TryRemove(client.ConnectionId, out client);
        }


        /// <summary>
        /// Send message for all users
        /// </summary>
        /// <param name="Packet"></param>
        /// <param name="fuse"></param>
        public void SendMessage(ServerMessage Packet, string fuse = "")
        {
            var bytes = Packet.GetReversedBytes();
            var data = new ArraySegment<byte>(bytes);

            foreach (var Client in Clients.Values)
            {
                if (Client?.GetHabbo() == null) continue;
                if (!string.IsNullOrEmpty(fuse))
                    if (!Client.GetHabbo().HasFuse(fuse))
                        continue;

                Client.GetConnection().SendArray(data);
            }
        }

        public void SendMessageAsync(ServerMessage Packet, string fuse = "")
        {
            var bytes = Packet.GetReversedBytes();

            foreach (var Client in Clients.Values)
            {
                if (Client?.GetHabbo() == null) continue;
                if (!string.IsNullOrEmpty(fuse))
                    if (!Client.GetHabbo().HasFuse(fuse))
                        continue;

                Client.GetConnection().SendAsync(bytes);
            }
        }

        /// <summary>
        ///     Logs the clones out.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void LogClonesOut(uint userId)
        {
            var clientByUserId = GetClientByUserId(userId);
            clientByUserId?.Disconnect("user null LogClonesOut");
        }

        /// <summary>
        ///     Registers the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userName">Name of the user.</param>
        internal void RegisterClient(GameClient client, uint userId, string userName)
        {
            _userNameRegister[userName.ToLower()] = client;
            _userIdRegister[userId] = client;
        }

        /// <summary>
        ///     Unregisters the client.
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="userName">The username.</param>
        internal void UnregisterClient(uint userid, string userName)
        {
            _userIdRegister.TryRemove(userid, out _);
            _userNameRegister.TryRemove(userName.ToLower(), out _);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.SetQuery($"UPDATE users SET online='0' WHERE id={userid} LIMIT 1");
        }

        /// <summary>
        ///     Closes all.
        /// </summary>
        internal void CloseAll()
        {
            var stringBuilder = new StringBuilder();
            var flag = false;

            Out.WriteLine("Saving Inventary Content....", "Oblivion.Boot", ConsoleColor.DarkCyan);

            foreach (var current2 in Clients.Values.Where(current2 => current2?.GetHabbo() != null))
            {
                try
                {
                    current2.GetHabbo().GetInventoryComponent().RunDbUpdate();
                    current2.GetHabbo().RunDbUpdate();
                    stringBuilder.Append(current2.GetHabbo().GetQueryString());
                    flag = true;
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                }
                catch
                {
                    Out.WriteLine("error disponsig inventory");
                }
            }

            Out.WriteLine("Inventary Content Saved!", "Oblivion.Boot", ConsoleColor.DarkCyan);

            if (flag)
            {
                if (stringBuilder.Length > 0)
                {
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryReactor.RunFastQuery(stringBuilder.ToString());
                }
            }

            try
            {
                Out.WriteLine("Closing Connection Manager...", "Oblivion.Boot", ConsoleColor.DarkMagenta);

                foreach (var current3 in Clients.Values.Where(current3 => current3.GetConnection() != null))
                {
                    try
                    {
                        current3.GetConnection().Disconnect();

                        Console.ForegroundColor = ConsoleColor.DarkMagenta;

                        Out.WriteLine("Connection Manager Closed!", "Oblivion.Boot", ConsoleColor.DarkMagenta);
                    }
                    catch
                    {
                        Out.WriteLine("error disponsig connection");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }

            Clients.Clear();
            Out.WriteLine("Connections closed", "Oblivion.Conn", ConsoleColor.DarkYellow);
        }

        /// <summary>
        ///     Updates the client.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        internal void UpdateClient(string oldName, string newName)
        {
            if (!_userNameRegister.TryGetValue(oldName.ToLower(), out var old))
                return;

            _userNameRegister.TryRemove(oldName.ToLower(), out _);
            _userNameRegister.TryAdd(newName.ToLower(), old);
        }
    }
}