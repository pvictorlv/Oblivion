﻿using SuperSocket.SocketBase.Protocol;

namespace Oblivion.Connection.SuperSocket
{
    public class FlashReceiveFilter : IReceiveFilter<RequestInfo>
    {
        #region Properties

        public int LeftBufferSize
        {
            get; private set;
        }

        public IReceiveFilter<RequestInfo> NextReceiveFilter
        {
            get; private set;
        }

        public FilterState State
        {
            get { return FilterState.Normal; }
        }

        #endregion Properties

        #region Constructors

        public FlashReceiveFilter()
        {
            this.NextReceiveFilter = new ReceiveFilter();
        }

        #endregion Constructors

        #region Methods

        public virtual RequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            // did not read anything
            rest = length;

            if (length < 2)
            {
                return null;
            }

            if (readBuffer[offset] == '<' && readBuffer[offset + 1] == 'p')
            {
                return new RequestInfo(null, true);
            }
            else
            {
                return null;
            }
        }

        public void Reset()
        {
            // Do nothing
        }

        #endregion Methods
    }

}