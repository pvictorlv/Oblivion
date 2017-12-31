using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Oblivion.Connection.Connection.AsyncSocket;
using Oblivion.Messages.Parsers;

namespace Oblivion.Connection.Connection
{
    /// <summary>
    ///     Class SocketManager.
    /// </summary>
    public class SocketManager
    {
        /// <summary>
        ///     A client has connected (nothing has been sent or received yet)
        /// </summary>
        public delegate void OnClientConnectedEvent(ConnectionInformation connection);

        /// <summary>
        ///     A client has disconnected
        /// </summary>
        public delegate void OnClientDisconnectedEvent(ConnectionInformation connection, Exception exception);

        private const int opsToPreAlloc = 2; // read, write (don't alloc buffer space for accepts)

        /// <summary>
        ///     The _parser
        /// </summary>
        private static IDataParser _parser;


        private static SocketAsyncEventArgsPool SocketPool;

//        private static Socket BaseSocket;
        private static BufferManager BufferManager;

        private static int ConnectedAmount;
        private static int Counter;

        /// <summary>
        ///     The _disableNagleAlgorithm in connectios
        /// </summary>
        private bool _disableNagleAlgorithm;

        /// <summary>
        ///     The _connection listener
        /// </summary>
        private Socket _listener;

        /// <summary>
        ///     The port to open socket
        /// </summary>
        private int _portInformation;

        /// <summary>
        ///     Count of accepeted connections
        /// </summary>
        public int AcceptedConnections;

        /// <summary>
        ///     Gets or sets the maximum connections.
        /// </summary>
        /// <value>The maximum connections.</value>
        public int MaximumConnections { get; set; }

        /// <summary>
        ///     Gets or sets the maximum ip connection count.
        /// </summary>
        /// <value>The maximum ip connection count.</value>
        public int MaxIpConnectionCount { get; set; }

        /// <summary>
        ///     Gets or sets the AntiDDoS Status.
        /// </summary>
        public bool AntiDDosStatus { get; set; }

        public event OnClientConnectedEvent OnClientConnected = delegate { };

        public event OnClientDisconnectedEvent OnClientDisconnected = delegate { };

        /// <summary>
        ///     Initializes the specified port identifier.
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

            // allocate buffers such that the maximum number of sockets can have one outstanding read and 
            //write posted to the socket simultaneously  
            BufferManager = new BufferManager(GameSocketManagerStatics.BufferSize * maxConnections * opsToPreAlloc,
                8124);

            PrepareConnectionDetails();
        }

        /// <summary>
        ///     Prepares the connection details.
        /// </summary>
        /// <exception cref="SocketInitializationException"></exception>
        private void PrepareConnectionDetails()
        {
            Counter = 0;
            SocketPool = new SocketAsyncEventArgsPool(GameSocketManagerStatics.MaxConnections);
            ConnectedAmount = new int();


            for (var i = 0; i < GameSocketManagerStatics.MaxConnections; i++)
            {
                var Async = new SocketAsyncEventArgs();
                Async.Completed += Async_Completed;
                Async.UserToken = new AsyncUserToken();
                BufferManager.SetBuffer(Async);

                SocketPool.Push(Async);
            }


            try
            {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = _disableNagleAlgorithm
                };
                _listener.Bind(new IPEndPoint(IPAddress.Any, _portInformation));
                // start the server with a listen backlog of 100 connections
                _listener.Listen(100);
                WaitForAsync(null);
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        public static void CloseClientSocket(SocketAsyncEventArgs Args)
        {
            var Token = Args.UserToken as AsyncUserToken;

            if (Token.Redused)
                return;

            Token.Redused = true;


            try
            {
                if (Token.Socket.Connected)
                    Token.Socket.Shutdown(SocketShutdown.Both);
                Token.Socket.Close();
            }
            catch (Exception)
            {
            }


            Interlocked.Decrement(ref ConnectedAmount);

            SocketPool.Push(Args);
        }


        private void WaitForAsync(SocketAsyncEventArgs Args)
        {
            if (Args == null)
            {
                Args = new SocketAsyncEventArgs();
                Args.Completed += AcceptAsync_Completed;
            }
            else
            {
                Args.AcceptSocket = null;
            }


            _listener.AcceptAsync(Args);
        }


        private void HandleAsync(SocketAsyncEventArgs Args)
        {
            try
            {
                Interlocked.Increment(ref ConnectedAmount);

                var SingleSocketAsync = SocketPool.Pop();
                var Token = (AsyncUserToken) SingleSocketAsync.UserToken;

                Token.Socket = Args.AcceptSocket;

                var Session = new ConnectionInformation(Token.Socket, SingleSocketAsync, _parser.Clone() as IDataParser,
                    Interlocked.Increment(ref Counter))
                {
                    Disconnected = OnChannelDisconnect
                };
                Token.Session = Session;

                if (!Args.AcceptSocket.ReceiveAsync(SingleSocketAsync))
                    HandleReceive(SingleSocketAsync);
            }
            catch
            {
            }
            finally
            {
                WaitForAsync(Args);
            }
        }

        private void AcceptAsync_Completed(object sender, SocketAsyncEventArgs Args)
        {
            HandleAsync(Args);
        }

        private void Async_Completed(object sender, SocketAsyncEventArgs Args)
        {
            switch (Args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    HandleReceive(Args);
                    break;
                case SocketAsyncOperation.Send:
                    HandleSend(Args);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }


        private void OnChannelDisconnect(ConnectionInformation connection, Exception exception)
        {
            connection.Disconnected = null;
            OnClientDisconnected(connection, exception);
            connection.Cleanup();
        }

        private void OnAcceptSocket(IAsyncResult ar)
        {
            /* if (ar == null || _listener == null) return;
             try
             {
                 var replyFromComputer = ((Socket) ar.AsyncState).EndAccept(ar);
 
                 if (replyFromComputer.Connected)
                 {
                     //                    if (SocketConnectionCheck.CheckConnection(replyFromComputer, MaxIpConnectionCount, AntiDDosStatus))
                     {
                         replyFromComputer.NoDelay = _disableNagleAlgorithm;
                         AcceptedConnections++;
                         var connectionInfo =
                             new ConnectionInformation(replyFromComputer, _parser.Clone() as IDataParser,
                                 AcceptedConnections)
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
 
             _listener.BeginAccept(OnAcceptSocket, _listener);*/
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            _listener.Close();
            _listener = null;
        }


        #region Traffic

        public void HandleReceive(SocketAsyncEventArgs Args)
        {
            var Token = Args.UserToken as AsyncUserToken;

            try
            {
                if (Args.BytesTransferred > 0 && Args.SocketError == SocketError.Success)
                {
                    var Received = new byte[Args.BytesTransferred];

                    Array.Copy(BufferManager.Buffer, Args.Offset, Received, 0, Args.BytesTransferred);
                    if (!Token.Session.Initialized)
                    {
                        OnClientConnected(Token.Session);
                        Token.Session.Initialized = true;
                    }
                    Token.Session.Args = Args;
                    Token.Session.HandlePacketData(Received, Args.BytesTransferred);
                    var willRaiseEvent = Token.Socket.ReceiveAsync(Args);
                    if (!willRaiseEvent)
                    {
                        Token.Session?.Disconnect(false);
                        CloseClientSocket(Args);
                    }
                }
                else
                {
                    Token.Session?.Disconnect(false);

                    CloseClientSocket(Args);
                }
            }
            catch
            {
                Token.Session?.Disconnect(false);

                CloseClientSocket(Args);
            }
        }


        private static void HandleSend(SocketAsyncEventArgs Args)
        {
            if (Args.SocketError != SocketError.Success)
                CloseClientSocket(Args);
        }

        #endregion
    }
}