using System;
using System.Net.Sockets;
using Oblivion.Connection.Net;
using Oblivion.Connection.SuperSocket;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;

namespace Oblivion.Configuration
{
    /// <summary>
    /// Class ConnectionHandling.
    /// </summary>
    public class ConnectionHandling
    {
        /// <summary>
        /// The manager
        /// </summary>
        public SuperServer<GameClient> Manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHandling"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="maxConnections">The maximum connections.</param>
        /// <param name="connectionsPerIp">The connections per ip.</param>
        /// <param name="antiDdoS"></param>
        /// <param name="enabeNagles">if set to <c>true</c> [enabe nagles].</param>
        public ConnectionHandling(int port, int maxConnections, int connectionsPerIp, bool antiDdoS, bool enabeNagles)
        {
            Manager = new SuperServer<GameClient>(port, maxConnections);

//            Manager.OnConnectionOpened += OnClientConnected;
            Manager.OnConnectionClosed += (session) => OnClientDisconnected(session, null);

            Manager.OnMessageReceived += (session, body, bytes) =>
            {
                if (session?.Parser == null) return;

                session.ServerRc4?.Parse(ref body);

                using (var clientMessage = new ClientMessage(body))
                {
                    session.Parser.SuperHandle(clientMessage, session);
                }
            };

            Manager.NewSessionConnected += session =>
            {
                session.SocketSession.Client
                    .SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.NoDelay, !enabeNagles);

                session.Parser = new GamePacketParser();
                session.ConnId = ++Manager.AcceptedConnections;
                Oblivion.GetGame().GetClientManager().CreateAndStartClient(session.ConnId, session);
            };


            Manager.SessionClosed += (session, value) => OnClientDisconnected(session, null);

            Manager.Start();

//            Manager.Init(port, maxConnections, connectionsPerIp, antiDdoS, new InitialPacketParser(), !enabeNagles);
        }

        /// <summary>
        /// Managers the connection event.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /*private static void OnClientConnected(Session<T> connection)
        {
            try
            {
                Oblivion.GetGame().GetClientManager().CreateAndStartClient((uint)connection.ConnId, connection);

            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "Oblivion.Configuration.ConnectionHandling");
            }
        }*/
        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="exception"></param>
        private static void OnClientDisconnected(Session<GameClient> connection, Exception exception)
        {
            try
            {
                Oblivion.GetGame().GetClientManager().DisposeConnection((uint) connection.ConnId);
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "Oblivion.Configuration.ConnectionHandling");
            }
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            Manager.Stop();
        }
    }
}