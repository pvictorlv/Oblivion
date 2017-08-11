using System;
using System.Net;
using System.Net.Sockets;
using Oblivion.Configuration;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

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
        private Socket _socket;
        /// <summary>
        /// The _remote end point
        /// </summary>
        private readonly EndPoint _remoteEndPoint;

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
        public uint ChannelId { get; }

        /// <summary>
        /// The _is connected
        /// </summary>
        private bool _connected;

        /// <summary>
        /// The _buffer
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// Gets or sets the parser.
        /// </summary>
        /// <value>The parser.</value>
        public IDataParser Parser { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInformation" /> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="channelId">The channel identifier.</param>
        public ConnectionInformation(Socket socket, IDataParser parser, uint channelId)
        {
            _socket = socket;
            socket.SendBufferSize = GameSocketManagerStatics.BufferSize;
            Parser = parser;
            _buffer = new byte[GameSocketManagerStatics.BufferSize];
            _remoteEndPoint = socket.RemoteEndPoint;
            _connected = true;
            ChannelId = channelId;
        }

        /// <summary>
        /// Gets or sets the disconnected.
        /// </summary>
        /// <value>The disconnected.</value>
        public OnClientDisconnectedEvent Disconnected
        {
            get { return DisconnectAction; }

            set
            {
                if (value == null)
                    DisconnectAction = (x, e) => { };
                else
                    DisconnectAction = value;
            }
        }

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        private void ReadAsync()
        {
            try
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReadCompleted, _socket);
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
        private void HandleDisconnect(Exception exception)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    try
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                        _socket.Close();
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
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
                Logging.HandleException(ex, "Oblivion.Connection.Connection.ConnectionInformation");
            }
        }

        /// <summary>
        /// Called when [read completed].
        /// </summary>
        /// <param name="async">The asynchronous.</param>
        private void OnReadCompleted(IAsyncResult async)
        {
            try
            {
                Socket dataSocket = (Socket)async.AsyncState;

                if (_socket != null && _socket.Connected && _connected)
                {
                    int bytesReceived = dataSocket.EndReceive(async);

                    if (bytesReceived != 0)
                    {
                        byte[] array = new byte[bytesReceived];

                        Array.Copy(_buffer, array, bytesReceived);

                        HandlePacketData(array, bytesReceived);
                    }
                    else
                        Disconnect();
                }
            }
            catch (Exception exception)
            {
                HandleDisconnect(exception);
            }
            finally
            {
                try
                {
                    if (_socket != null && _socket.Connected && _connected)
                        _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReadCompleted, _socket);
                    else
                        Disconnect();
                }
                catch (Exception exception)
                {
                    HandleDisconnect(exception);
                }
            }

        }

        /// <summary>
        /// Called when [send completed].
        /// </summary>
        /// <param name="async">The asynchronous.</param>
        private void OnSendCompleted(IAsyncResult async)
        {
            try
            {
                Socket dataSocket = (Socket)async.AsyncState;

                if (_socket != null && _socket.Connected && _connected)
                    dataSocket.EndSend(async);
                else
                    Disconnect();
            }
            catch (Exception exception)
            {
                HandleDisconnect(exception);
            }
        }

        /// <summary>
        /// Cleanup everything so that the channel can be reused.
        /// </summary>
        public void Cleanup()
        {
            _socket = null;
            _connected = false;
        }

        /// <summary>
        /// Starts the packet processing.
        /// </summary>
        public void StartPacketProcessing()
        {
            if (_connected && _socket.Connected)
                ReadAsync();
            else
                Dispose();
        }

        /// <summary>
        /// Gets the ip.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetIp() => _remoteEndPoint.ToString().Split(':')[0];

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        public uint GetConnectionId() => ChannelId;

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
        internal void Disconnect()
        {
            if (_connected)
                HandleDisconnect(new SocketException((int)SocketError.ConnectionReset));
        }

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="bytesReceived"></param>
        private void HandlePacketData(byte[] packet, int bytesReceived)
        {
//                Arc4ServerSide?.Parse(ref packet);
            Parser?.HandlePacketData(packet, bytesReceived);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void SendData(byte[] packet)
        {
            if (_socket == null || !_socket.Connected) return;
            try
            {
                _socket.BeginSend(packet, 0, packet.Length, 0, OnSendCompleted, _socket);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "SendData - ConnectionInformation.cs");
                HandleDisconnect(e);
            }
        }
    }
}