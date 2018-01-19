using System.Net.Sockets;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using Oblivion.Messages.Parsers;

namespace Oblivion.Connection.Connection.SocketAsync
{
    public class SessionBase
    {

        public bool Started;
        /// <summary>
        /// The underlying <see cref="Socket"/> class of the session.
        /// </summary>
        public Socket Socket
        {
            get;
            set;
        }

        internal SocketAsyncEventArgs ReceiveEventArgs
        {
            get;
            set;
        }

        public bool IsAir { get; set; }


        internal ARC4 Arc4ServerSide;
        
        public void HandlePacketData(byte[] packet, int bytesReceived)
        {
            Arc4ServerSide?.Parse(ref packet);
            Parser?.HandlePacketData(packet, bytesReceived);
        }

        public IDataParser Parser { get; set; }


        internal int SendBytesRemainingCount
        {
            get;
            set;
        }

        internal int BytesSentAlreadyCount
        {
            get;
            set;
        }

        public uint Id { get; set; }
        internal byte[] DataToSend
        {
            get;
            set;
        }

        public string IP => Socket.RemoteEndPoint.ToString().Split(':')[0];

        /// <summary>
        /// Function to be called when the connection is closed. Can be overridden.
        /// </summary>
        public virtual void OnConnectionClose()
        {

        }
    }

}