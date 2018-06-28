using Oblivion.HabboHotel.Misc;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;

namespace Oblivion.Connection.SuperSocket
{
    public class SuperServer<T> : AppServer<Session<T>, RequestInfo>, IServer<T>
    {
        #region Events

        public event ConnectionClosed<T> OnConnectionClosed;

        public event ConnectionOpened<T> OnConnectionOpened;

        public event MessageReceived<T> OnMessageReceived;

        #endregion Events

        #region Constructors

        public uint AcceptedConnections;


        public SuperServer(int port, int maxConn)
            : base(new DefaultReceiveFilterFactory<FlashReceiveFilter, RequestInfo>())
        {
            OnConnectionClosed = delegate { };
            OnConnectionOpened = delegate { };
            OnMessageReceived = delegate { };

            IRootConfig rootConfig = new RootConfig();
            
            IServerConfig config = CreateServerConfig(port, maxConn);

            Setup(rootConfig, config, logFactory: new Log4NetLogFactory());

            base.NewRequestReceived += HandleRequest;
        }

        #endregion Constructors

        #region Methods

        private IServerConfig CreateServerConfig(int port, int maxConn)
        {
            var config = new ServerConfig
            {
                Port = port,
                ReceiveBufferSize = 8192,
                SendBufferSize = 8192,
                ListenBacklog = 250,
                MaxConnectionNumber = maxConn,
                KeepAliveTime = 1200,
                IdleSessionTimeOut = 900,
                SendTimeOut = 3000,
                SendingQueueSize = maxConn/5,
                MaxRequestLength = 8192

            };

            return config;
        }

        private void HandleRequest(Session<T> session, RequestInfo requestInfo)
        {
            if (requestInfo.IsFlashRequest)
            {
                session.Send(CrossDomainPolicy.XmlPolicyBytes);
                session.Disconnect();
            }
            else
            {
                OnMessageReceived(session, requestInfo.Body, requestInfo.Transfered);
            }
        }

        #endregion Methods
    }
}