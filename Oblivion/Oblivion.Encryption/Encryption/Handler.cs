#region

using System.Globalization;
using System.Numerics;
using System.Text;
using Oblivion.Encryption.Encryption.Crypto.KeyExchange;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Rsa;
using Oblivion.Encryption.Encryption.Utils;

#endregion

namespace Oblivion.Encryption.Encryption
{
    public class Handler
    {
        public static RsaKey Rsa;
        public static DiffieHellman DiffieHellman;

        public static void Initialize(string n, string d, string e)
        {
            Rsa = RsaKey.ParsePrivateKey('0' + n, '0' + e, '0' + d);
            DiffieHellman = new DiffieHellman();
        }

        public static string GetRsaDiffieHellmanPrimeKey()
        {
            return GetRsaStringEncrypted(DiffieHellman.Prime.ToString());
        }

        public static string GetRsaDiffieHellmanGeneratorKey()
        {
            return GetRsaStringEncrypted(DiffieHellman.Generator.ToString());
        }

        public static string GetRsaDiffieHellmanPublicKey()
        {
            return GetRsaStringEncrypted(DiffieHellman.PublicKey.ToString());
        }

        public static BigInteger CalculateDiffieHellmanSharedKey(string publicKey)
        {
            try
            {
                var bytes = BigInteger.Parse('0' + publicKey, NumberStyles.HexNumber).ToByteArray();
                var keyBytes = Rsa.Verify(bytes);
                var keyString = Encoding.Default.GetString(keyBytes);

                return DiffieHellman.CalculateSharedKey(BigInteger.Parse(keyString));
            }
            catch
            {
                return 0;
            }
        }

        private static string GetRsaStringEncrypted(string message)
        {
            try
            {
                var m = Encoding.Default.GetBytes(message);
                var c = Rsa.Sign(m);

                return Converter.BytesToHexString(c);
            }
            catch
            {
                return null;
            }
        }
    }
}