#region

using System;
using System.IO;
using System.IO.Compression;

#endregion

namespace Oblivion.Encryption.Encryption.Utils
{
    public class Converter
    {
        public static string BytesToHexString(byte[] bytes)
        {
            string hexstring = BitConverter.ToString(bytes);
            return hexstring.Replace("-", string.Empty);
        }

        public static byte[] HexStringToBytes(string hexstring)
        {
            int NumberChars = hexstring.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static string Deflate(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, 2, bytes.Length - 2))
            using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(inflater))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}