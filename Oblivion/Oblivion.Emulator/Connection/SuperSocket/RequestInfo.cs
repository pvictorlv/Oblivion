using SuperSocket.SocketBase.Protocol;

namespace Oblivion.Connection.SuperSocket
{
    public class RequestInfo : BinaryRequestInfo
    {
        #region Properties

        public bool IsFlashRequest
        {
            get; private set;
        }

        #endregion Properties

        #region Constructors

        public int Transfered;
        public RequestInfo(byte[] body, bool isFlashRequest = false, int transfered = 0)
            : base("__MESSAGE__", body)
        {
            IsFlashRequest = isFlashRequest;

            Transfered = transfered;
        }

        #endregion Constructors
    }

}