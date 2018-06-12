﻿using SuperSocket.SocketBase.Protocol;

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

        public RequestInfo(byte[] body, bool isFlashRequest = false)
            : base("__MESSAGE__", body)
        {
            IsFlashRequest = isFlashRequest;
        }

        #endregion Constructors
    }

}