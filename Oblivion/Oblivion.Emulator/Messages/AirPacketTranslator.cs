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
        private IDictionary<short, short> _incomingTranslator;
        private IDictionary<short, short> _outgoingTranslator;

        private AirPacketTranslator()
        {
            _incomingTranslator = new Dictionary<short, short>();
            _outgoingTranslator = new Dictionary<short, short>();

            RegisterIncoming();
            RegisterOutgoing();
        }

        private void RegisterIncoming()
        {
            // ORDER: Habbo Air > Production

            // HANDSHAKE
            _incomingTranslator.Add(4000, 4000);
            _incomingTranslator.Add(1415, 1415); // INITCRYPTO
            _incomingTranslator.Add(1767, 1767); // GENERATE_SECRET_KEY
            _incomingTranslator.Add(2278, 921); // UNIQUE_KEY

            // LOGIN
            _incomingTranslator.Add(1497, 286); // SSO TICKET
            _incomingTranslator.Add(1681, 2401); // INFO_RETRIEVE

            // CATALOG
            _incomingTranslator.Add(445, 2069);
        }

        private void RegisterOutgoing()
        {
            // ORDER: Production > HabboAir
            // HANDSHAKE
            _outgoingTranslator.Add(884, 884);
            _outgoingTranslator.Add(1838, 1838);

            // LOGIN
            _outgoingTranslator.Add(3054, 1748); // AUTH_OK
            _outgoingTranslator.Add(1513, 2776); // USER_OBJECT

            // CATALOGUE
            _outgoingTranslator.Add(808, 2200);
        }

        public int ReplaceIncomingHeader(short messageId)
        {
            if (_incomingTranslator.TryGetValue(messageId, out short newMessageId))
            {
#if DEBUG
                Console.WriteLine($"[AIR][INCOMING] Changed header {messageId} to {newMessageId}");
#endif
                return newMessageId;
            }

            return 0;
        }

        public short ReplaceOutgoingHeader(ref byte[] packet, out short oldHeader)
        {
            byte[] oldHeaderBytes = new byte[] { packet[4], packet[5] };
            Array.Reverse(oldHeaderBytes);
            oldHeader = BitConverter.ToInt16(oldHeaderBytes, 0);

            if (_outgoingTranslator.TryGetValue(oldHeader, out short newHeader))
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
        

        private static AirPacketTranslator Instance { get; set; }

        public static AirPacketTranslator GetInstance()
        {
            if (Instance == null)
                Instance = new AirPacketTranslator();

            return Instance;
        }
    }
}
