using System.Net;
using DotNetty.Transport.Channels;
using Oblivion.Connection.Net;
using Oblivion.Messages.Parsers;

namespace Oblivion.Connection.Connection
{
    public class ConnectionActor
    {
        /// <summary>
        ///     Data Parser
        /// </summary>
        public GamePacketParser DataParser;

        /// <summary>
        ///     Connection Id
        /// </summary>
        public string ConnectionId;

        /// <summary>
        ///     IP Address
        /// </summary>
        public string IpAddress;

        public bool IsAir = false;

        /// <summary>
        ///    Connection Channel
        /// </summary>
        public IChannel ConnectionChannel;

        /// <summary>
        ///     Is in HandShake Process
        /// </summary>
        internal bool HandShakeCompleted;

        /// <summary>
        ///     Is in HandShake Process
        /// </summary>
        internal bool HandShakePartialCompleted;

        /// <summary>
        ///     Count of Same IP Connection.
        /// </summary>
        internal int SameHandledCount;

        public ConnectionActor(GamePacketParser dataParser, IChannel context)
        {
            DataParser = dataParser;

            ConnectionChannel = context;

            ConnectionId = context.Id.ToString();

            IpAddress = (context.RemoteAddress as IPEndPoint)?.Address.ToString();

            SameHandledCount = 0;

            HandShakeCompleted = false;

            HandShakePartialCompleted = false;
        }

        public void Close()
        {
            ConnectionChannel?.Flush();

            DataParser?.Dispose();
        }

        public void CompleteClose()
        {
            ConnectionChannel?.CloseAsync();

            Close();
        }
    }
}