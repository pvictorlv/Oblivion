using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Oblivion.Messages
{
    /// <summary>
    /// Class ClientMessage.
    /// </summary>
    public class ClientMessage : IDisposable
    {
        /// <summary>
        /// The _body
        /// </summary>
        private byte[] _body;

        /// <summary>
        /// The _position
        /// </summary>
        private int _position;

        /// <summary>
        /// The length
        /// </summary>
        private int _length;

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        internal int Id { get; private set; }

        public int Length => _length;

        public bool BytesAvailable => Length > (_position - 4);

        public void ReplaceId(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMessage"/> class.
        /// </summary>
        internal ClientMessage(int messageId, byte[] body, int position, int packetLength)
        {
            Init(messageId, body, position, packetLength);
        }

        public ClientMessage(byte[] body)
        {
            _body = body;
            Id = GetInteger16();
        }

        private IByteBuffer buffer;

        public ClientMessage(int length, IByteBuffer buf)
        {
            this._length = length;
            this.buffer = (buf != null) && (buf.ReadableBytes > 0) ? buf : Unpooled.Empty;

            if (this.buffer.ReadableBytes >= 2)
            {
                this.Id = this.buffer.ReadShort();
            }
            else
            {
                this.Id = 0;
            }
        }
        

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_body != null)
            {
                Array.Clear(_body, 0, _body.Length);
                _body = null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (this.buffer != null)
            {
                String body = this.buffer.ToString(Encoding.UTF8);

                for (int i = 0; i < 13; i++)
                {
                    body = body.Replace(((char)i).ToString(), "[" + i + "]");
                }

                return body;
            }
            string stringValue = string.Empty;

            stringValue += Encoding.Default.GetString(_body);

            for (int i = 0; i < 13; i++)
                stringValue = stringValue.Replace(char.ToString((char) (i)), $"[{i}]");

            return stringValue;
        }

        /// <summary>
        /// Initializes the specified message identifier.
        /// </summary>
        internal void Init(int messageId, byte[] body, int position, int packetLength)
        {
            Id = messageId;
            _body = body;
            _position = position;
            _length = packetLength;
        }

        internal void Init(byte[] body)
        {
            _body = body;
            Id = GetInteger16();
            _position = 0;
            _length = 0;
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <param name="len">The bytes length.</param>
        /// <returns>System.Byte[].</returns>
        internal byte[] GetBytes(int len)
        {
            if (buffer != null)
            {
                var bytes = new byte[len];

                for (int i = 0; i < len; i++)
                {
                    bytes[i] = this.buffer.ReadByte();
                }

                //        byte[] data = this.content().readBytes((length)).array();
                return bytes;
            }
            
            byte[] arrayBytes = new byte[len];
            int pos = _position;

            for (int i = 0; i < len; i++)
            {
                arrayBytes[i] = _body[pos];
                pos++;
            }

            return arrayBytes;
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string GetString()
        {
            return GetString(Encoding.UTF8);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>System.String.</returns>
        internal string GetString(Encoding encoding)
        {
            if (this.buffer != null)
            {
                int length = buffer.ReadShort();
                var bytes = GetBytes(length);
                return encoding.GetString(bytes);
            }
            
            int stringLength = GetInteger16();
            if (stringLength == 0 || _position + stringLength > _body.Length)
                return string.Empty;

            string value = encoding.GetString(_body, _position, stringLength);
            _position += stringLength;
            return value;
        }

        /// <summary>
        /// Gets the integer from string.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetIntegerFromString()
        {
            int result;

            string stringValue = GetString(Encoding.ASCII);

            int.TryParse(stringValue, out result);

            return result;
        }

        /// <summary>
        /// Gets the bool.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GetBool()
        {
            if (this.buffer != null)
            {
                return this.buffer.ReadBoolean();
            }
            
            return (_body[_position++] == 1);
        }

        /// <summary>
        /// Gets the integer16.
        /// </summary>
        /// <returns>System.Int16.</returns>
        internal Int16 GetInteger16()
        {
            if (this.buffer != null)
                return this.buffer.ReadShort();
            
            return HabboEncoding.DecodeInt16(_body, ref _position);
        }

        /// <summary>
        /// Gets the integer.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetInteger()
        {
            try
            {
                if (this.buffer != null)
                    return this.buffer.ReadInt();

                return HabboEncoding.DecodeInt32(_body, ref _position);
            }
            catch
            {
                return 0;
            }
        }

        internal bool GetIntegerAsBool()
        {
            if (this.buffer != null)
                return this.buffer.ReadInt() == 1;

            return HabboEncoding.DecodeInt32(_body, ref _position) == 1;
        }

        /// <summary>
        /// Gets the integer32.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        internal uint GetUInteger()
        {
            int value = GetInteger();
            return (value < 0 ? 0 : (uint) value);
        }

        /// <summary>
        /// Gets the integer16.
        /// </summary>
        /// <returns>System.UInt16.</returns>
        internal ushort GetUInteger16() => (ushort) GetInteger16();
    }
}