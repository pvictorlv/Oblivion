using System;
using Oblivion.Configuration;
using Oblivion.Connection.Connection;
using Oblivion.Connection.SuperSocket;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Factorys;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.Connection.Net
{
    /// <summary>
    /// Class GamePacketParser.
    /// </summary>
    public class GamePacketParser : IDataParser
    {
        /// <summary>
        /// The _current client
        /// </summary>
        private GameClient _currentClient;

        /// <summary>
        /// The int size
        /// </summary>
        private const int IntSize = sizeof(int);
        /// <summary>
        /// The memory container
        /// </summary>
        private static readonly MemoryContainer MemoryContainer = new MemoryContainer(10, 4072);
        /// <summary>
        /// The _buffered data
        /// </summary>
        private readonly byte[] _bufferedData;
        /// <summary>
        /// The _buffer position
        /// </summary>
        private int _bufferPos;
        /// <summary>
        /// The _current packet length
        /// </summary>
        private int _currentPacketLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePacketParser" /> class.
        /// </summary>
        internal GamePacketParser()
        {
            _bufferPos = 0;
            _currentPacketLength = -1;
            _bufferedData = MemoryContainer.TakeBuffer();
        }

        /// <summary>
        /// Delegate HandlePacket
        /// </summary>
        /// <param name="message">The message.</param>
        public delegate void HandlePacket(ClientMessage message);

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="me">Me.</param>
        public void SetConnection( GameClient me)
        {
            Console.WriteLine("\n\rsetted!");
            _currentClient = me;
        }

        

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length">The length.</param>
        public void HandlePacketData(byte[] data, int length)
        {
            short messageId = 0;

            try
            {
                int pos;

                for (pos = 0; pos < length;)
                {
                    if (_currentPacketLength == -1)
                    {
                        if (length < IntSize)
                        {
                            BufferCopy(data, length); 
                            break;
                        }

                        _currentPacketLength = HabboEncoding.DecodeInt32(data, ref pos);
                    }

                    if (_currentPacketLength < 2 || _currentPacketLength > 4096)
                    {
                        _currentPacketLength = -1;

                        break; 
                    }

                    if (_currentPacketLength == ((length - pos) + _bufferPos)) 
                    {
                        if (_bufferPos != 0) 
                        {
                            BufferCopy(data, length, pos);

                            pos = 0;

                            messageId = HabboEncoding.DecodeInt16(_bufferedData, ref pos);

                            HandleMessage(messageId, _bufferedData, 2, _currentPacketLength);
                        }
                        else
                        {
                            messageId = HabboEncoding.DecodeInt16(data, ref pos); 
                            HandleMessage(messageId, data, pos, _currentPacketLength);
                        }

                        pos = length;
                        _currentPacketLength = -1;
                    }
                    else 
                    {
                        int remainder = ((length - pos)) - (_currentPacketLength - _bufferPos);

                        if (_bufferPos != 0)
                        {
                            int toCopy = remainder - _bufferPos;

                            BufferCopy(data, toCopy, pos);

                            int zero = 0;

                            messageId = HabboEncoding.DecodeInt16(_bufferedData, ref zero);
 
                            HandleMessage(messageId, _bufferedData, 2, _currentPacketLength); 
                        }
                        else
                        {
                            messageId = HabboEncoding.DecodeInt16(data, ref pos);
                                
                            HandleMessage(messageId, data, pos, _currentPacketLength);

//                            pos -= 2; 
                        }

                        _currentPacketLength = -1;

                        pos = (length - remainder);
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.HandleException(exception, $"packet handling ----> {messageId}");
            }
        }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="packetContent">Content of the packet.</param>
        /// <param name="position">The position.</param>
        /// <param name="packetLength">Length of the packet.</param>
        private void HandleMessage(int messageId, byte[] packetContent, int position, int packetLength)
        {
            try
            {
                int oldHeader = messageId;
                if (_currentClient.IsAir)
                    messageId = AirPacketTranslator.ReplaceIncomingHeader((short) messageId);

                using (var clientMessage = ClientMessageFactory.GetClientMessage(messageId, packetContent, position, packetLength))
                {
                    if (messageId == 0 && _currentClient.IsAir)
                    {
                        if (Oblivion.DebugMode)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("[INCOMING][AIR][REFUSED]");
                            Console.ResetColor();
                            Console.WriteLine($"{oldHeader} => " + clientMessage);
                        }
                        return;
                    }

                    if (clientMessage != null && _currentClient?.GetMessageHandler() != null)
                        _currentClient.GetMessageHandler().HandleRequest(clientMessage);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "HandleMessage");
            }
        }

        public void SuperHandle(ClientMessage message, Session<GameClient> userSocket)
        {
            Console.WriteLine("\n\rcalled!");

            var client = userSocket.UserData;
            if (client == null)
                return;
            if (client.GetMessageHandler() == null)
            {
                client.StartConnection();
                
            }
            if (message != null) 
                client.GetMessageHandler().HandleRequest(message);
        }

        /// <summary>
        /// Buffers the copy.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        private void BufferCopy(byte[] data, int bytes, int offset)
        {
            for (int i = 0; i < (bytes - offset); i++)
                _bufferedData[_bufferPos++] = data[i + offset];
        }

        /// <summary>
        /// Buffers the copy.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="bytes">The bytes.</param>
        private void BufferCopy(byte[] data, int bytes)
        {
            for (int i = 0; i < bytes; i++)
                _bufferedData[_bufferPos++] = data[i];
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            MemoryContainer.GiveBuffer(_bufferedData);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone() => new GamePacketParser();
    }
}