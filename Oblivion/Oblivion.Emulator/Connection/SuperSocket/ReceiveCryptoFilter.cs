using System;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Util;
using SuperSocket.Common;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;
using System.Linq;

namespace Oblivion.Connection.SuperSocket
{
    public class ReceiveCryptoFilter : IReceiveFilter<RequestInfo>
    {
        private IAirHandler _airHandler;

        public ReceiveCryptoFilter(IAirHandler airHandler)
        {
            _airHandler = airHandler;
        }

        public RequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;

            try
            {
                _airHandler.ServerRc4?.Parse(ref readBuffer, offset, 4);
               
                int position = offset;
                int decryptedLength = HabboEncoding.DecodeInt32(readBuffer, ref position);

                _airHandler.ServerRc4?.Parse(ref readBuffer, position, decryptedLength);

                rest = length - decryptedLength - 4;
                
                return new RequestInfo(readBuffer.CloneRange(position, decryptedLength), false, decryptedLength);
            }
            catch
            {
                return null;
            }
        }


        public void Reset()
        {
        }

        public int LeftBufferSize { get; }
        public IReceiveFilter<RequestInfo> NextReceiveFilter { get; }
        public FilterState State { get; }
    }
}