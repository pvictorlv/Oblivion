using System;
using Oblivion.Configuration;
using Oblivion.Messages;
using Oblivion.Util;
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

        protected override RequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] data, int offset,
            int length)
        {
      
            var d = new RequestInfo(data.CloneRange(offset, length), false, length);
            
            return d;

        }
        
        #endregion Methods
    }
}