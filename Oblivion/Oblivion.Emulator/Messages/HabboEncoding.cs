using System;
using System.Buffers;

namespace Oblivion.Messages
{
    /// <summary>
    /// Class HabboEncoding.
    /// </summary>
    internal class HabboEncoding
    {
        private const short ShortLength = sizeof(short);
        private const int IntLength = sizeof(int);

        /// <summary>
        /// Decodes the int32.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="position"></param>
        /// <returns>System.Int32.</returns>
        internal static int DecodeInt32(byte[] v, ref int position)
        {
            if (position + IntLength > v.Length || position < 0)
                return 0;

            return (v[position++] << 24) + (v[position++] << 16) + (v[position++] << 8) + v[position++];
        }
        

        public static int ToInt(ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);

            reader.TryReadBigEndian(out int value);

            /*
            var data = reader.Sequence.ToArray();
            var offset = reader.Consumed;
            
            CheckRange(data, offset, 4);

            return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | (data[offset + 3]);
            */

            return value;
        }

        private static void CheckRange(byte[] data, int offset, int count)
        {
            if (offset + count > data.Length || offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Index was"
                                                                +
                                                                " out of range. Must be non-negative and less than the"
                                                                + " size of the collection.");
            }
        }
        /// <summary>
        /// Decodes the int16.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="position"></param>
        /// <returns>Int16.</returns>
        internal static short DecodeInt16(byte[] v, ref int position)
        {
            if (position + ShortLength > v.Length || position < 0)
                return 0;

            return (short)((v[position++] << 8) + v[position++]);
        }

        /// <summary>
        /// Gets the character filter.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.String.</returns>
        public static string GetCharFilter(string data)
        {
            for (var i = 0; i <= 13; i++)
                data = data.Replace(Convert.ToChar(i) + "", "[" + i + "]");

            return data;
        }
    }
}