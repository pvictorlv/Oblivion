using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.Messages.Parsers;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using Oblivion.Messages;

namespace Oblivion.Connection.Connection
{
    /// <summary>
    /// Class ConnectionInformation.
    /// </summary>
    public class ConnectionInformation : IDisposable
    {
        /// <summary>
        /// The _socket
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// The _remote end point
        /// </summary>
        private EndPoint _remoteEndPoint;

        /// <summary>
        /// Delegate OnClientDisconnectedEvent
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="exception">The exception.</param>
        public delegate void OnClientDisconnectedEvent(ConnectionInformation connection, Exception exception);

        /// <summary>
        /// Occurs when [disconnect action].
        /// </summary>
        public event OnClientDisconnectedEvent DisconnectAction = delegate { };

        /// <summary>
        /// Identity of this channel
        /// </summary>
        /// <value>The channel identifier.</value>
        /// <remarks>Must be unique within a server.</remarks>
        public int ChannelId { get; }

        /// <summary>
        /// The _is connected
        /// </summary>
        private bool _connected;


        /// <summary>
        /// Gets or sets the parser.
        /// </summary>
        /// <value>The parser.</value>
        public IDataParser Parser { get; set; }


        public bool IsAir { get; set; }

        public SocketAsyncEventArgs Args { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInformation" /> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="channelId">The channel identifier.</param>
        public ConnectionInformation(Socket sock, SocketAsyncEventArgs e, IDataParser parser, int channelId)
        {
            Args = e;
            var socket = sock;
            if (socket != null)
            {
                Socket = socket;
                socket.SendBufferSize = GameSocketManagerStatics.BufferSize;
                _remoteEndPoint = socket.RemoteEndPoint;
            }

            Parser = parser;
            _connected = true;
            ChannelId = channelId;
        }

        /// <summary>
        /// Gets or sets the disconnected.
        /// </summary>
        /// <value>The disconnected.</value>
        public OnClientDisconnectedEvent Disconnected
        {
            get => DisconnectAction;

            set
            {
                if (value == null)
                    DisconnectAction = (x, e) => { };
                else
                    DisconnectAction = value;
            }
        }

        internal ARC4 Arc4ServerSide;

        public bool Initialized;

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        private void ReadAsync()
        {
            try
            {
                //todo?
                //                Socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReadCompleted, Socket);
//                ProcessReceive(Args);
//                Oblivion.GetConnectionManager().Manager.HandleReceive(Args);
            }
            catch (Exception e)
            {
                HandleDisconnect(e);
            }
        }

        /// <summary>
        /// Handles the disconnect.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void HandleDisconnect(Exception exception, bool socks = true)
        {
            try
            {
                if (Socket != null && Socket.Connected && socks)
                {
                    try
                    {
                        Socket.Shutdown(SocketShutdown.Both);
                        Socket.Close();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                _connected = false;
                Parser.Dispose();

                SocketConnectionCheck.FreeConnection(GetIp());

                DisconnectAction(this, exception);
                Cleanup();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
                Logging.HandleException(ex, "Oblivion.Connection.Connection.ConnectionInformation");
            }
        }


        /// <summary>
        /// Cleanup everything so that the channel can be reused.
        /// </summary>
        public void Cleanup()
        {
            Socket = null;
            _remoteEndPoint = null;
            Args?.Dispose();
            Args = null;
            Parser?.Dispose();
            Parser = null;
            _connected = false;
        }

        /// <summary>
        /// Starts the packet processing.
        /// </summary>
        public void StartPacketProcessing()
        {
            if (_connected && Socket.Connected)
                ReadAsync();
            else
                Dispose();
        }

        public int receivedPrefixBytesDoneCount;


        /// <summary>
        /// Gets the ip.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetIp() => _remoteEndPoint.ToString().Split(':')[0];

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        public int GetConnectionId() => ChannelId;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        internal void Disconnect(bool socks = true)
        {
            if (_connected)
                HandleDisconnect(new SocketException((int) SocketError.ConnectionReset), socks);
        }

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="bytesReceived"></param>
        public void HandlePacketData(byte[] packet, int bytesReceived)
        {
            Arc4ServerSide?.Parse(ref packet);
            Parser?.HandlePacketData(packet, bytesReceived);
        }

        private void OnSendCompleted(IAsyncResult async)
        {
            try
            {
                if (Socket != null && Socket.Connected && _connected)
                    Socket.EndSend(async);
                else
                    Disconnect();
            }
            catch (Exception exception)
            {
                HandleDisconnect(exception);
            }
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void SendData(byte[] packet)
        {
            if (Args.SocketError != SocketError.Success)
            {
                return;
            }
            if (Socket == null || !Socket.Connected) return;
            byte[] newHeader = null;
            if (IsAir)
            {
                newHeader = AirPacketTranslator.ReplaceOutgoingHeader(packet, out var oldHeader);
                string packetName = LibraryParser.TryGetOutgoingName(oldHeader);
                if (newHeader == null)
                {
                    if (Oblivion.DebugMode)
                        Console.WriteLine("Header *production* " + oldHeader + " (" + packetName +
                                          ") wasn't translated to packet air.");
                    return;
                }
                if (Oblivion.DebugMode)

                    Console.WriteLine("Header *production* " + oldHeader + " (" + packetName +
                                      ") has been translated to packet air.");
            }

            try
            {
                //                Args.SetBuffer(packet, 0, packet.Length);


//                bool useAir = newHeader != null;
//                Args.SetBuffer(useAir ? newHeader : packet, 0, useAir ? newHeader.Length : packet.Length);
//                new Task(() =>
//                {
//                    Socket.Send(packet);

//                                        Socket.SendAsync(Args);
//                                        Socket.ReceiveAsync(Args);
//                }).Start();

                Socket.Send(packet, 0, packet.Length, SocketFlags.None);
//                                                Socket.BeginSend(newHeader ?? packet, 0, packet.Length, 0, OnSendCompleted, null);
            }
            catch (Exception e)
            {
//                Logging.HandleException(e, "SendData - ConnectionInformation.cs");
//                HandleDisconnect(e);
            }
        }

        private int sendBytesRemainingCount;
    }
}