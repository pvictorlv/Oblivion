using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Oblivion.Messages
{
    internal class ServerMessage : IDisposable
    {
        /// <summary>
        /// The buffer for the ServerMessage.
        /// </summary>
        private MemoryStream _buffer;

        /// <summary>
        /// The buffer for the Arrays.
        /// </summary>
        private MemoryStream _arrayBuffer;

        /// <summary>
        /// The current buffer for the Arrays.
        /// </summary>
        private MemoryStream _arrayCurrentBuffer;

        /// <summary>
        /// The _on array
        /// </summary>
        private bool _onArray, _disposed;

        /// <summary>
        /// The _array count
        /// </summary>
        private int _arrayCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerMessage"/> class.
        /// </summary>
        public ServerMessage()
        {
            Id = 0;
            _buffer = new MemoryStream();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerMessage"/> class.
        /// </summary>
        /// <param name="header">The header.</param>
        public ServerMessage(int header)
            : this()
        {
            Init(header);
        }

        ~ServerMessage()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; private set; }

        /// <summary>
        /// Get the current message.
        /// 
        /// When StartArray is called, it'll return _arrayCurrentBuffer. Else it will return _buffer.
        /// </summary>
        /// <value>The c message.</value>
        private MemoryStream CurrentMessage => _onArray ? _arrayCurrentBuffer : _buffer;

        /// <summary>
        /// Initializes the specified header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void Init(int header)
        {
            _buffer.SetLength(0);
            Id = header;
            AppendShort(header);
        }

        /// <summary>
        /// Sets the pointer to a Temporary Buffer
        /// </summary>
        public void StartArray()
        {
            if (_onArray)
            {
                throw new InvalidOperationException("The array has already started.");
            }

            _onArray = true;
            _arrayCount = 0;

            _arrayBuffer = new MemoryStream();
            _arrayCurrentBuffer = new MemoryStream();
        }

        /// <summary>
        /// Saves the Temporary Buffer in a Safe Buffer (not main)
        /// and cleans the Temporal Buffer.
        /// </summary>
        public void SaveArray()
        {
            if (_onArray == false || _arrayCurrentBuffer.Length == 0)
                return;

            _arrayCurrentBuffer.WriteTo(_arrayBuffer);
            _arrayCurrentBuffer.SetLength(0);
            _arrayCount++;
        }

        /// <summary>
        /// Cleans the Temporal Buffer.
        /// </summary>
        public void Clear()
        {
            if (_onArray == false)
                return;

            _arrayCurrentBuffer.SetLength(0);
        }

        /// <summary>
        /// Saves the Safe Buffer to Main Buffer
        /// After disposes the other buffers.
        /// </summary>
        public void EndArray()
        {
            if (_onArray == false)
                return;

            _onArray = false;

            AppendInteger(_arrayCount);

            _arrayBuffer.WriteTo(_buffer);
            _arrayBuffer.Dispose();
            _arrayBuffer = null;

            _arrayCurrentBuffer.Dispose();
            _arrayCurrentBuffer = null;
        }

        /// <summary>
        /// Appends the server message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void AppendServerMessage(ServerMessage message)
        {
            AppendBytes(message.GetBytes(), false);
        }

        /// <summary>
        /// Appends the server messages.
        /// </summary>
        /// <param name="messages">The messages.</param>
        public void AppendServerMessages(List<ServerMessage> messages)
        {
            /* TODO CHECK */
            foreach (ServerMessage message in messages)
            {
                AppendServerMessage(message);
            }
        }

        /// <summary>
        /// Appends the short.
        /// </summary>
        /// <param name="i">The i.</param>
        public void AppendShort(int i)
        {
            Int16 value = (short) i;

            AppendBytes(BitConverter.GetBytes(value), true);
        }

        /// <summary>
        /// Appends the integer.
        /// </summary>
        /// <param name="value">The i.</param>
        public void AppendInteger(int value)
        {
            AppendBytes(BitConverter.GetBytes(value), true);
        }

        /// <summary>
        /// Appends the integer.
        /// </summary>
        /// <param name="i">The i.</param>
        public void AppendInteger(uint i)
        {
            AppendInteger((int) i);
        }

        /// <summary>
        /// Appends the integer.
        /// </summary>
        /// <param name="i">if set to <c>true</c> [i].</param>
        public void AppendInteger(bool i)
        {
            AppendInteger(i ? 1 : 0);
        }

        public void AppendIntegersArray(string str, char delimiter, int lenght, int defaultValue = 0, int maxValue = 0)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception("String is null or empty");
            }

            string[] array = str.Split(delimiter);

            if (array.Length == 0)
            {
                return;
            }

            uint i = 0;

            /* TODO CHECK */
            foreach (string text in array.TakeWhile(text => i != lenght))
            {
                i++;

                if (!int.TryParse(text, out var value))
                    value = defaultValue;

                if (maxValue != 0 && value > maxValue)
                    value = maxValue;

                AppendInteger(value);
            }
        }

        public void AppendPacketString(string packet)
        {
            char[] pckt = packet.ToCharArray();
            for (int i = 0; i < packet.Length; i++)
            {
                char c = pckt[i];

                if (c == '[')
                {
                    int r = pckt.Skip(i).TakeWhile(s => s != ']').Count();

                    if (r <= 3)
                    {
                        string s = string.Join("", pckt.Skip(i + 1).Take(r - 1).ToArray());
                        if (int.TryParse(s, out int bts))
                        {
                            AppendByte((byte)bts);
                        }
                        i = i + r;
                        continue;
                    }
                }

                AppendByte((byte)c);
            }
        }

        /// <summary>
        /// Appends the bool.
        /// </summary>
        /// <param name="b">if set to <c>true</c> [b].</param>
        public void AppendBool(bool b)
        {
            AppendByte(b ? 1 : 0);
        }

        /// <summary>
        /// Appends the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="isUtf8">If string is UTF8</param>
        public void AppendString(string s, bool isUtf8 = false)
        {
            Encoding encoding = isUtf8 ? Encoding.UTF8 : Oblivion.GetDefaultEncoding();

            byte[] bytes = encoding.GetBytes(s);
            AppendShort(bytes.Length);
            AppendBytes(bytes, false);
        }

        /// <summary>
        /// Appends the bytes.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="isInt">if set to <c>true</c> [is int].</param>
        public void AppendBytes(byte[] b, bool isInt)
        {
            if (isInt)
            {
                Array.Reverse(b);
            }

            CurrentMessage.Write(b, 0, b.Length);
        }
        

        /// <summary>
        /// Appends the byted.
        /// </summary>
        /// <param name="number">The number.</param>
        public void AppendByte(int number)
        {
            CurrentMessage.WriteByte((byte) number);
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        public byte[] GetBytes() => CurrentMessage.ToArray();

        /// <summary>
        /// Gets the reversed bytes.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        public byte[] GetReversedBytes()
        {
            byte[] bytes;

            using (MemoryStream finalBuffer = new MemoryStream())
            {
                byte[] length = BitConverter.GetBytes((int) CurrentMessage.Length);
                Array.Reverse(length);
                finalBuffer.Write(length, 0, length.Length);

                CurrentMessage.WriteTo(finalBuffer);

                bytes = finalBuffer.ToArray();
            }

            if (Oblivion.DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine();
                Console.Write("OUTGOING ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("PREPARED ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(Id + Environment.NewLine +
                              HabboEncoding.GetCharFilter(Oblivion.GetDefaultEncoding().GetString(bytes)));
                Console.WriteLine();
            }

            return bytes;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() =>
            HabboEncoding.GetCharFilter(Oblivion.GetDefaultEncoding().GetString(GetReversedBytes()));

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _buffer.Dispose();
            _buffer = null;
            if (_onArray)
            {
                _arrayBuffer?.Dispose();
                _arrayBuffer = null;
            }

            _arrayCurrentBuffer?.Dispose();
            _arrayCurrentBuffer = null;
            _disposed = true;
        }
    }
}