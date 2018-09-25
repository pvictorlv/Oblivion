using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private int _userIdCounter = 1;


        private ConcurrentDictionary<uint, ulong> _usersByVirtualId;
        private ConcurrentDictionary<ulong, uint> _usersByRealId;


        public uint GetVirtualId(GameClient session)
        {
            if (session.GetHabbo() != null)
            {
                return GetVirtualId(session.VirtualId);
            }

            Interlocked.Increment(ref _userIdCounter);

            var newId = Convert.ToUInt32(_userIdCounter);
            return newId;
        }

        public void StoreVirtualId(uint virtualId, ulong realId)
        {
            _usersByRealId.TryAdd(realId, virtualId);
            _usersByVirtualId.TryAdd(virtualId, realId);
        }

        public uint GetVirtualId(ulong realId)
        {
            if (_usersByRealId.TryGetValue(realId, out var virtualId))
            {
                return virtualId;
            }

            Interlocked.Increment(ref _userIdCounter);

            var newId = Convert.ToUInt32(_userIdCounter);
            _usersByRealId.TryAdd(realId, newId);
            _usersByVirtualId.TryAdd(newId, realId);

            return newId;
        }

        public ulong GetRealId(uint virtualId)
        {
            if (_usersByVirtualId.TryGetValue(virtualId, out var realId))
            {
                return realId;
            }


            return 0L;
        }

        /// <summary>
        ///     The _user name register
        /// </summary>
        private readonly ConcurrentDictionary<string, GameClient> _userNameRegister;

        /// <summary>
        ///     The clients
        /// </summary>
        internal ConcurrentDictionary<ulong, GameClient> Clients;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClientManager" /> class.
        /// </summary>
        internal GameClientManager()
        {
            Clients = new ConcurrentDictionary<ulong, GameClient>();
            _usersByVirtualId = new ConcurrentDictionary<uint, ulong>();
            _usersByRealId = new ConcurrentDictionary<ulong, uint>();
            _userNameRegister = new ConcurrentDictionary<string, GameClient>();
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
        internal GameClient GetClientByUserId(uint userId)
        {
            var realId = GetRealId(userId);

            return Clients.TryGetValue(realId, out var client) ? client : null;
        }

        /// <summary>
        ///     Gets the client by user identifier.
        /// </summary>
        /// <param name="realId">The user identifier.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClientByUserId(ulong realId)
        {
            return Clients.TryGetValue(realId, out var client) ? client : null;
        }

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
        internal string GetNameById(ulong id)
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
                x.GetHabbo() != null && x.GetHabbo().Rank >= Oblivion.StaffAlertMinRank && x.VirtualId != exclude);

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
        internal void CreateAndStartClient(Session<GameClient> connection)
        {
            var gameClient = new GameClient(connection);
            connection.UserData = gameClient;
            var clientId = GetVirtualId(gameClient);
            gameClient.VirtualId = clientId;
            connection.VirtualId = clientId;

            gameClient.StartConnection();
        }

        /// <summary>
        ///     Disposes the connection.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        internal void DisposeConnection(uint clientId)
        {
            var realId = GetRealId(clientId);

            if (!Clients.TryRemove(realId, out var client))
                return;
            client.Dispose();
        }


        /// <summary>
        /// Send message for all users
        /// </summary>
        /// <param name="packet"></param>
        public void SendMessage(ServerMessage packet)
        {
            var bytes = packet.GetReversedBytes();

            foreach (var client in Clients.Values)
            {
                client?.GetConnection()?.Send(bytes);
            }
        }

        public void SendMessageAsync(ServerMessage packet)
        {
            var bytes = packet.GetReversedBytes();

            foreach (var client in Clients.Values)
            {
                client?.GetConnection()?.SendAsync(bytes);
            }
        }

        /// <summary>
        ///     Logs the clones out.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void LogClonesOut(uint userId)
        {
            //todo
            var clientByUserId = GetClientByUserId(userId);
            clientByUserId?.Disconnect("user null LogClonesOut");
        }

        /// <summary>
        ///     Registers the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userName">Name of the user.</param>
        internal void RegisterClient(GameClient client, ulong userId, string userName)
        {
            _userNameRegister[userName.ToLower()] = client;

            StoreVirtualId(client.VirtualId, userId);

            Clients[userId] = client;
        }

        /// <summary>
        ///     Unregisters the client.
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="userName">The username.</param>
        internal void UnregisterClient(ulong userid, string userName)
        {
            Clients.TryRemove(userid, out _);
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