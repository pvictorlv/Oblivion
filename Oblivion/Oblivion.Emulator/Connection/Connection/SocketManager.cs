﻿using System;
using System.Net;
using System.Net.Sockets;
using Oblivion.Configuration;
using Oblivion.Messages.Parsers;

namespace Oblivion.Connection.Connection
{
    /// <summary>
    ///     Class SocketManager.
    /// </summary>
    public class SocketManager
    {
        /// <summary>
        /// The port to open socket
        /// </summary>
        private int _portInformation;

        /// <summary>
        /// Count of accepeted connections
        /// </summary>
        public uint AcceptedConnections;

        /// <summary>
        /// The _connection listener
        /// </summary>
        private Socket _listener;

        /// <summary>
        /// The _disableNagleAlgorithm in connectios
        /// </summary>
        private bool _disableNagleAlgorithm;

        /// <summary>
        /// The _parser
        /// </summary>
        private IDataParser _parser;

        /// <summary>
        ///     A client has connected (nothing has been sent or received yet)
        /// </summary>
        public delegate void OnClientConnectedEvent(ConnectionInformation connection);

        public event OnClientConnectedEvent OnClientConnected = delegate { };

        /// <summary>
        ///     A client has disconnected
        /// </summary>
        public delegate void OnClientDisconnectedEvent(ConnectionInformation connection, Exception exception);

        public event OnClientDisconnectedEvent OnClientDisconnected = delegate { };

        /// <summary>
        /// Gets or sets the maximum connections.
        /// </summary>
        /// <value>The maximum connections.</value>
        public int MaximumConnections { get; set; }

        /// <summary>
        /// Gets or sets the maximum ip connection count.
        /// </summary>
        /// <value>The maximum ip connection count.</value>
        public int MaxIpConnectionCount { get; set; }

        /// <summary>
        /// Gets or sets the AntiDDoS Status.
        /// </summary>
        public bool AntiDDosStatus { get; set; }

        /// <summary>
        /// Initializes the specified port identifier.
        /// </summary>
        /// <param name="portId">The port identifier.</param>
        /// <param name="maxConnections">The maximum connections.</param>
        /// <param name="connectionsPerIp">The connections per ip.</param>
        /// <param name="antiDdoS">The antiDDoS status</param>
        /// <param name="parser">The parser.</param>
        /// <param name="disableNaglesAlgorithm">if set to <c>true</c> [disable nagles algorithm].</param>
        public void Init(int portId, int maxConnections, int connectionsPerIp, bool antiDdoS, IDataParser parser,
            bool disableNaglesAlgorithm)
        {
            _parser = parser;
            _disableNagleAlgorithm = disableNaglesAlgorithm;
            MaximumConnections = maxConnections;
            _portInformation = portId;
            AcceptedConnections = 0;
            MaxIpConnectionCount = connectionsPerIp;
            AntiDDosStatus = antiDdoS;
            if (_portInformation < 0)
                throw new ArgumentOutOfRangeException("port", _portInformation, "Port must be 0 or more.");
            if (_listener != null)
                throw new InvalidOperationException("Already listening.");
            PrepareConnectionDetails();
        }

        /// <summary>
        /// Prepares the connection details.
        /// </summary>
        /// <exception cref="SocketInitializationException"></exception>
        private void PrepareConnectionDetails()
        {
            //                _socketSystem = new SocketSystem<SessionBase>("0.0.0.0", _portInformation, 150, 30000, _parser);

            SocketConnectionCheck.SetupTcpAuthorization(20000);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _portInformation);

            _listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endPoint);
            _listener.Listen(150);

            //                _listener = new TcpListener(IPAddress.Any, _portInformation);
            //                _listener.Start();
            _listener.BeginAccept(OnAcceptSocket, _listener);
        }

        private void OnChannelDisconnect(ConnectionInformation connection, Exception exception)
        {
            connection.Disconnected = null;
            OnClientDisconnected(connection, exception);
            connection.Cleanup();
        }

        private void OnAcceptSocket(IAsyncResult ar)
        {
            if (ar == null || _listener == null) return;
            try
            {
                var socket = _listener.EndAccept(ar);

                if (socket.Connected)
                {
                    if (SocketConnectionCheck.CheckConnection(socket, MaxIpConnectionCount, AntiDDosStatus))
                    {
                        socket.NoDelay = _disableNagleAlgorithm;
                        AcceptedConnections++;
                        var connectionInfo =
                            new ConnectionInformation(socket, _parser.Clone() as IDataParser, AcceptedConnections)
                            {
                                Disconnected = OnChannelDisconnect
                            };
                        OnClientConnected(connectionInfo);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "OnAccept");
            }

            _listener.BeginAccept(OnAcceptSocket, _listener);
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            _listener = null;
        }
    }
}