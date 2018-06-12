using System;
using Oblivion.Messages;
using SuperSocket.Common;
using SuperSocket.Facility.Protocol;

namespace Oblivion.Connection.SuperSocket
{
    public class ReceiveFilter : FixedHeaderReceiveFilter<RequestInfo>
    {
        #region Constructors

        public ReceiveFilter()
            : base(4)
        {
        }

        #endregion Constructors

        #region Methods

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            var head = HabboEncoding.ToInt(header, offset);
    
            return head;
        }

        protected override RequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset,
            int length)
        {
            return new RequestInfo(bodyBuffer.CloneRange(offset, length));
        }

        #endregion Methods
    }
}