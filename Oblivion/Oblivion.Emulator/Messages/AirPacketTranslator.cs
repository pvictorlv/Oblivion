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

        public static byte[] ReplaceOutgoingHeader(byte[] packet, out short oldHeader)
        {
            byte[] newArr = new byte[packet.Length];
            Array.Copy(packet, newArr, packet.Length);
            byte[] oldHeaderBytes =
            {
                newArr[4], newArr[5]
            };
            Array.Reverse(oldHeaderBytes);
            oldHeader = BitConverter.ToInt16(oldHeaderBytes, 0);

            if (LibraryParser.OutgoingAir.TryGetValue(oldHeader, out short newHeader))
            {
                #if DEBUG
                Console.WriteLine($"[AIR][INCOMING] Changed header {oldHeader} to {newHeader}");
                #endif
                byte[] newHeaderReverse = BitConverter.GetBytes(newHeader);
                newArr[4] = newHeaderReverse[1];
                newArr[5] = newHeaderReverse[0];

                return newArr;
            }

            return null;
        }
    }
}
