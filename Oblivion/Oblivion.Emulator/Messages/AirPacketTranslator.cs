using Oblivion.Messages.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oblivion.Messages
{
    internal static class AirPacketTranslator
    {
        public static bool ReplaceIncomingHeader(ClientMessage clientMessage)
        {
            Console.WriteLine(clientMessage.Id);
            if (LibraryParser.IncomingAir.TryGetValue((short) clientMessage.Id, out short newMessageId))
            {
                if (Oblivion.DebugMode)
                    Console.WriteLine($"[AIR][INCOMING] Changed header {clientMessage.Id} to {newMessageId}");
                clientMessage.ReplaceId(newMessageId);
                return true;
            }

            return false;
        }

        public static byte[] ReplaceOutgoingHeader(byte[] packet, out short oldHeader, out short newHeader)
        {
            byte[] newArr = new byte[packet.Length];
            Array.Copy(packet, newArr, packet.Length);
            byte[] oldHeaderBytes =
            {
                newArr[5], newArr[4]
            };
            oldHeader = BitConverter.ToInt16(oldHeaderBytes, 0);

            if (LibraryParser.OutgoingAir.TryGetValue(oldHeader, out newHeader))
            {
                if (Oblivion.DebugMode)
                    Console.WriteLine($"[AIR][OUTGOING] Changed header {oldHeader} to {newHeader}");

                byte[] newHeaderReverse = BitConverter.GetBytes(newHeader);
                newArr[4] = newHeaderReverse[1];
                newArr[5] = newHeaderReverse[0];

                return newArr;
            }

            return null;
        }
    }
}