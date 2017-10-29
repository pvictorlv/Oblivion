using Oblivion.Messages.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oblivion.Messages
{
    internal class AirPacketTranslator
    {
        public static int ReplaceIncomingHeader(short messageId)
        {
            if (LibraryParser.IncomingAir.TryGetValue(messageId, out short newMessageId))
            {
#if DEBUG
                Console.WriteLine($"[AIR][INCOMING] Changed header {messageId} to {newMessageId}");
#endif
                return newMessageId;
            }

            return 0;
        }

        public static short ReplaceOutgoingHeader(ref byte[] packet, out short oldHeader)
        {
            byte[] oldHeaderBytes = new byte[] { packet[4], packet[5] };
            Array.Reverse(oldHeaderBytes);
            oldHeader = BitConverter.ToInt16(oldHeaderBytes, 0);

            if (LibraryParser.OutgoingAir.TryGetValue(oldHeader, out short newHeader))
            {
#if DEBUG
                Console.WriteLine($"[AIR][INCOMING] Changed header {oldHeader} to {newHeader}");
#endif
                byte[] newHeaderReverse = BitConverter.GetBytes(newHeader);
                packet[4] = newHeaderReverse[1];
                packet[5] = newHeaderReverse[0];

                return newHeader;
            }

            return 0;
        }
    }
}
