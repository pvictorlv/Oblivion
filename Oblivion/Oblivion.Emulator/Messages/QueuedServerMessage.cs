using System.Collections.Generic;
using Oblivion.Connection.Connection;

namespace Oblivion.Messages
{
    /// <summary>
    /// Class QueuedServerMessage.
    /// </summary>
    public class QueuedServerMessage
    {
        /// <summary>
        /// The _packet
        /// </summary>
        private List<byte[]> _packets;

        /// <summary>
        /// The _user connection
        /// </summary>
        private ConnectionInformation _userConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedServerMessage"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public QueuedServerMessage(ConnectionInformation connection)
        {
            _userConnection = connection;
            _packets = new List<byte[]>();
        }

        /// <summary>
        /// Gets the get packet.
        /// </summary>
        /// <value>The get packet.</value>
        internal List<byte[]> GetPacket => _packets;

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            _packets.Clear();
            _userConnection = null;
        }

        /// <summary>
        /// Appends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void AppendResponse(ServerMessage message)
        {
            if (message == null) return;
            AppendBytes(message.GetReversedBytes());
        }

        
        /// <summary>
        /// Adds the bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal void AddBytes(List<byte[]> bytes)
        {
            foreach (byte[] byteArray in bytes)
                AppendBytes(byteArray);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        internal void SendResponse()
        {
            foreach (var packet in _packets)
                _userConnection?.SendData(packet);

            Dispose();
        }

        /// <summary>
        /// Appends the bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        private void AppendBytes(byte[] bytes)
        {
            _packets.Add(bytes);
        }
    }
}