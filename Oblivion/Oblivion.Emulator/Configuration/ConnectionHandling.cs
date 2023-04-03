using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.Connection;
using Oblivion.Connection.Connection;
using Oblivion.Connection.Net;
using Oblivion.Connection.Netty;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Users;
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
        public ConnectionManager<GameClient> Manager;

        private int _connectionCounter;

        internal int ConnectionsPerIp;

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

            CrossDomainSettings flashPolicy = new CrossDomainSettings("*", port);

            
            Manager = new ConnectionManager<GameClient>(new ServerSettings()
            {
                TcpNoDelay = enabeNagles,
                MaxWorkingThreads = 0, // Let the OS decide
                MinWorkingThreads = 0,
                MinIOThreads = 0,
                MaxIOThreads = 0,
                MaxConnections = 10000,
                Port = port,
                IP = "localhost"
            }, flashPolicy);
            ConnectionsPerIp = connectionsPerIp;

            SocketConnectionCheck.SetupTcpAuthorization(maxConnections);
            //            Manager.OnConnectionOpened += OnClientConnected;

//            Manager.Init(port, maxConnections, connectionsPerIp, antiDdoS, new InitialPacketParser(), !enabeNagles);
        }

        public async Task StartServer()
        {

            Manager.OnConnectionClosed += (session) => OnClientDisconnected(session, null);

            Manager.OnMessageReceived += async (session, body) =>
            {
                if (session?.UserData?.PacketParser == null) return;

                using (var clientMessage = new ClientMessage(body))
                {
                    session.UserData.PacketParser.SuperHandle(clientMessage, session);
                }
            };

            Manager.OnConnectionOpened += async session =>
            {
                //                if (SocketConnectionCheck.CheckConnection(session.GetIp(), connectionsPerIp, true))
                {

                    await Oblivion.GetGame().GetClientManager().CreateAndStartClient(session);
                }
                //                else
                {
                    //                    session.Disconnect();

                }
            };



            await Manager.Start();
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
        private static void OnClientDisconnected(ISession<GameClient> connection, Exception exception)
        {
            try
            {
                SocketConnectionCheck.FreeConnection(connection.Channel.RemoteAddress.ToString());
                Oblivion.GetGame().GetClientManager().DisposeConnection(connection.Channel.Id);
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