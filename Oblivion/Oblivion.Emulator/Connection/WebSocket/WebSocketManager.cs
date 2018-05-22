using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Fleck;

namespace Oblivion.Connection.WebSocket
{
    public class WebSocketManager
    {
        private ConcurrentDictionary<Guid, IWebSocketConnection> _connections;

        private WebSocketServer _server;

        public WebSocketManager(string socketUrl)
        {
            new Task(() =>
            {
                FleckLog.Level = LogLevel.Error;
                _connections = new ConcurrentDictionary<Guid, IWebSocketConnection>();
                
                _server = new WebSocketServer(socketUrl);
                _server.Start(socket =>
                {
                    socket.OnClose = () => { _connections.TryRemove(socket.ConnectionInfo.Id, out var _); };
                    socket.OnMessage = message =>
                    {
                        var msg = message;
                        int pId;
                        if (!int.TryParse(msg.Split('|')[0], out pId))
                            return;
                        if (msg.Length > 1024)
                            return;

                        if (msg.StartsWith("1|"))
                        {
                            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT username FROM users WHERE auth_ticket= @auth LIMIT 1");
                                dbClient.AddParameter("auth", msg.Substring(2));
                                dbClient.RunQuery();
                                var drow = dbClient.GetRow();
                                if (drow == null)
                                {
                                    return;
                                }

                                var username = drow["username"].ToString();

                                var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(username)?.GetHabbo();
                                if (client == null) return;

                                client.WebSocketConnId = socket.ConnectionInfo.Id;
                                _connections[socket.ConnectionInfo.Id] = socket;

                            }
                        }
                    };
                });
            }, TaskCreationOptions.LongRunning).Start();
        }



        public bool IsValidConnection(Guid connGuid) => _connections.TryGetValue(connGuid, out var conn) && conn.IsAvailable;

        public void SendMessage(Guid userId, string message)
        {
            if (!_connections.TryGetValue(userId, out var conn))
                return;

            if (!conn.IsAvailable)
                return;

            conn.Send(message);
        }
        
        public void SendMessageToEveryConnection(string message)
        {
            foreach (var iwsc in _connections.Values)
                try
                {
                    iwsc.Send(message);
                }
                catch
                {
                }
        }


        public void Dispose()
        {
            foreach (var iwsc in _connections.Values)
                try
                {
                    iwsc.Close();
                }
                catch
                {
                }
            _server.Dispose();
        }

        public bool IsValidFile(string url)
            => url.StartsWith("http://") && (url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".gif"));
    }
}