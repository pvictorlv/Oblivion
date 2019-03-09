using System;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Fleck;
using Oblivion.Util;

namespace Oblivion.Connection.WebSocket
{
    public class WebSocketManager
    {
        private ConcurrentDictionary<Guid, IWebSocketConnection> _connections;

        private WebSocketServer _server;

        public WebSocketManager(string socketUrl)
        {
            FleckLog.Level = LogLevel.Error;
            _connections = new ConcurrentDictionary<Guid, IWebSocketConnection>();

            _server = new WebSocketServer(socketUrl);
            if (socketUrl.StartsWith("wss://"))
            {
                _server.Certificate = new X509Certificate2(Application.StartupPath + "/ca.pfx");
                
            }
            _server.Start(socket =>
            {
                socket.OnClose = () => { _connections.TryRemove(socket.ConnectionInfo.Id, out var _); };
                socket.OnMessage = message =>
                {
                    var msg = message.Split('|');

                    if (msg.Length == 0 || msg.Length > 2048) return;

                    if (!int.TryParse(msg[0], out var pId))
                        return;


                    switch (pId)
                    {
                        case 1:
                        {
                            uint userId;
                            if (msg.Length < 2) return;

                            var sso = msg[1];
                            if (sso.Length < 3) return;

                            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery(
                                    "SELECT id FROM users WHERE auth_ticket= @auth LIMIT 1");
                                dbClient.AddParameter("auth", sso);
                                dbClient.RunQuery();
                                var drow = dbClient.GetRow();
                                if (drow == null)
                                {
                                    return;
                                }

                                userId = (uint) drow["id"];
                            }

                            if (userId == 0) return;

                            var client = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId)
                                ?.GetHabbo();
                            if (client == null) return;

                            if (client.WebSocketConnId != Guid.Empty) return;

                            socket.Send("1");
                            client.WebSocketConnId = socket.ConnectionInfo.Id;
                            _connections[socket.ConnectionInfo.Id] = socket;
                        }
                            break;
                    }
                };
            });

            Out.WriteLine($"Loaded WebSocket Manager at {socketUrl}", "Server.AsyncWebSocketListener");
        }


        public bool IsValidConnection(Guid connGuid) =>
            _connections.TryGetValue(connGuid, out var conn) && conn.IsAvailable;

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