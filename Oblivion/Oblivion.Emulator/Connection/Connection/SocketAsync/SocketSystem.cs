using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Oblivion.Messages.Parsers;

namespace Oblivion.Connection.Connection.SocketAsync
{
    public class SocketSystem<T> where T : SessionBase
    {
        /// <summary>
        /// Occurs when a connection is accepted.
        /// </summary>
        public event Action<T> ConnectionAccepted;

        /// <summary>
        /// Occurs when a connection receives data from the client.
        /// </summary>
        public event Action<SocketReceivedEventArgs<T>> DataReceived;

        /// <summary>
        /// Occurs when a connection sent data to the client.
        /// </summary>
        public event Action<SocketSentEventArgs<T>> DataSent;

        /// <summary>
        /// The <see cref="Socket"/> class of the connection.
        /// </summary>
        public Socket Socket { get; private set; }

        internal Pool<SocketAsyncEventArgs> AcceptPool { get; private set; }

        internal Pool<SocketAsyncEventArgs> ReceivePool { get; private set; }

        internal Pool<SocketAsyncEventArgs> SendPool { get; private set; }

        internal BufferPool BufferPool { get; private set; }

        internal SemaphoreSlim PoolEnforcer { get; private set; }


        /// <summary>
        /// Creates a new instance of the SocketSystem class and starts the socket.
        /// </summary>
        /// <param name="ip">The IP of the socket, use 0.0.0.0 for all IPs and 127.0.0.1 for only localhost.</param>
        /// <param name="port">The port of the socket.</param>
        /// <param name="backlog">The backlog; 'maximum amount of connections' in the queue which aren't accepted yet.</param>
        /// <param name="supportedAmount">The maximum amount of connections.</param>
        public SocketSystem(string ip, int port, int backlog, int supportedAmount, IDataParser parser)
        {
            _parser = parser;

            this.ConstructSocket(IPAddress.Parse(ip), port, backlog);
            this.ConstructPooling(supportedAmount);
        }

        internal void ConstructSocket(IPAddress ip, int port, int backlog)
        {
            IPEndPoint endPoint = new IPEndPoint(ip, port);

            Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(endPoint);
            Socket.Blocking = false;

            Socket.Listen(backlog);
        }

        private int _counter;

        internal void ConstructPooling(int SupportedAmount)
        {
            this.AcceptPool = new Pool<SocketAsyncEventArgs>(SupportedAmount);
            this.ReceivePool = new Pool<SocketAsyncEventArgs>(SupportedAmount);
            this.SendPool = new Pool<SocketAsyncEventArgs>(SupportedAmount);
            this.PoolEnforcer = new SemaphoreSlim(SupportedAmount, SupportedAmount);
            this.BufferPool = new BufferPool(SupportedAmount);
            this.BufferPool.PushAllReceivers(this, this.ReceivePool.PushAndHandleAll());
            this.BufferPool.PushAllSenders(this, this.SendPool.PushAndHandleAll());

            foreach (SocketAsyncEventArgs AcceptArgs in AcceptPool.PushAndHandleAll())
            {
                AcceptArgs.Completed += AcceptArgs_Completed;
            }

            StartAccept();
        }

        #region Accepting

        internal void StartAccept()
        {
            SocketAsyncEventArgs AcceptArgs;

            if (AcceptPool.TryPop(out AcceptArgs))
            {
                PoolEnforcer.Wait();

                bool Trigger = Socket.AcceptAsync(AcceptArgs);

                if (!Trigger)
                {
                    HandleAccept(AcceptArgs);
                }
            }
        }

        internal void AcceptArgs_Completed(object sender, SocketAsyncEventArgs AcceptArgs)
        {
            HandleAccept(AcceptArgs);
        }

        internal void HandleAccept(SocketAsyncEventArgs AcceptArgs)
        {
            if (AcceptArgs.SocketError != SocketError.Success)
            {
                HandleBadAccept(AcceptArgs);
                return;
            }

            StartAccept();

            SocketAsyncEventArgs ReceiveArgs;

            if (ReceivePool.TryPop(out ReceiveArgs))
            {
                Interlocked.Increment(ref _counter);


                ReceiveArgs.UserToken = (T) Activator.CreateInstance(typeof(T));
                ((T) ReceiveArgs.UserToken).Socket = AcceptArgs.AcceptSocket;
                ((T) ReceiveArgs.UserToken).Id = (uint) _counter;
                ((T) ReceiveArgs.UserToken).Parser = (IDataParser) _parser.Clone();
                ((T) ReceiveArgs.UserToken).ReceiveEventArgs = ReceiveArgs;
//                Oblivion.GetGame().GetClientManager().CreateAndStartClient(((T)ReceiveArgs.UserToken).Id, ((T)ReceiveArgs.UserToken));

                AcceptArgs.AcceptSocket = null;
                AcceptPool.Push(AcceptArgs);

                if (ConnectionAccepted != null)
                {
                    ConnectionAccepted.Invoke((T) ReceiveArgs.UserToken);
                }

                StartReceive(ReceiveArgs);
            }
            else
            {
                HandleAccept(AcceptArgs);
            }
        }

        internal void HandleBadAccept(SocketAsyncEventArgs AcceptArgs)
        {
            AcceptArgs.AcceptSocket.Close();
            AcceptPool.Push(AcceptArgs);
        }

        #endregion

        #region Receive

        internal void StartReceive(SocketAsyncEventArgs Args)
        {
            bool Trigger = ((T) Args.UserToken).Socket.ReceiveAsync(Args);

            if (!Trigger)
            {
                ProcessReceive(Args);
            }
        }

        private IDataParser _parser;


        internal void ProcessReceive(SocketAsyncEventArgs Args)
        {
            if (Args.BytesTransferred > 0 && Args.SocketError == SocketError.Success)
            {
//                byte[] Buffer = BufferPool.Buffer;
                byte[] Data = new byte[Args.BytesTransferred];

                Array.Copy(BufferPool.Buffer, Data, Args.BytesTransferred);

                T session = (T) Args.UserToken;

                DataReceived?.Invoke(new SocketReceivedEventArgs<T>(ref session, Data, Args.BytesTransferred));
                session.HandlePacketData(Data, Args.BytesTransferred);
//                Oblivion.GetConnectionManager().Manager.OnClientConnected(session);
                if (!session.Started)
                {
//                    Oblivion.GetGame().GetClientManager().CreateAndStartClient(session.Id, session);
                    session.Started = true;
                }
                StartReceive(Args);
            }
            else
            {
                CloseClientSocket(Args);
                this.ReceivePool.Push(Args);
                this.PoolEnforcer.Release();
            }
        }

        #endregion

        #region Send

        /// <summary>
        /// Attempts to send data to the client of a connection.
        /// </summary>
        /// <param name="session">The session sending data.</param>
        /// <param name="bytes">The data to be sent.</param>
        public void SendBytes(T session, byte[] bytes)
        {
            SocketAsyncEventArgs args;

            if (SendPool.TryPop(out args))
            {
                args.UserToken = session;
                session.DataToSend = bytes;
                session.SendBytesRemainingCount = bytes.Length;
                args.AcceptSocket = session.Socket;

                StartSend(args);
            }
        }

        internal void StartSend(SocketAsyncEventArgs Args)
        {
            T Session = (T) Args.UserToken;

            if (Session.SendBytesRemainingCount <= BufferPool.BUF_SIZE)
            {
                Args.SetBuffer(Args.Offset, Session.SendBytesRemainingCount);
                Buffer.BlockCopy(Session.DataToSend, Session.BytesSentAlreadyCount, Args.Buffer, Args.Offset,
                    Session.SendBytesRemainingCount);
            }
            else
            {
                Args.SetBuffer(Args.Offset, BufferPool.BUF_SIZE);
                Buffer.BlockCopy(Session.DataToSend, Session.BytesSentAlreadyCount, Args.Buffer, Args.Offset,
                    BufferPool.BUF_SIZE);
            }

            bool Trigger = Args.AcceptSocket.SendAsync(Args);

            if (!Trigger)
            {
                ProcessSend(Args);
            }
        }

        internal void ProcessSend(SocketAsyncEventArgs Args)
        {
            T Session = (T) Args.UserToken;

            if (Args.SocketError == SocketError.Success)
            {
                Session.SendBytesRemainingCount = Session.SendBytesRemainingCount - Args.BytesTransferred;

                if (Session.SendBytesRemainingCount == 0)
                {
                    SendPool.Push(Args);
                }
                else
                {
                    Session.BytesSentAlreadyCount += Args.BytesTransferred;
                    StartSend(Args);
                }

                if (DataSent != null)
                {
                    DataSent.Invoke(new SocketSentEventArgs<T>(Session, Session.DataToSend, Args.BytesTransferred));
                }
            }
            else
            {
                CloseClientSocket(Args);
                SendPool.Push(Args);
            }
        }

        #endregion

        internal void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
            }
        }

        internal void CloseClientSocket(SocketAsyncEventArgs Args)
        {
            T Session = (T) Args.UserToken;

            try
            {
                Session.Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }

            Session.Socket.Close();
            Session.OnConnectionClose();
        }
    }
}