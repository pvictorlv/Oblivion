using System;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using Oblivion.Messages.Parsers;
using SuperSocket.SocketBase;

namespace Oblivion.Connection.SuperSocket
{
    public class Session<T> : AppSession<Session<T>, RequestInfo>
    {
        #region Properties

        public uint ConnId { get; set; }

        public bool IsAir;
        

        public ARC4 ServerRc4 { get; set; }

        public IDataParser Parser { get; set; }

        #endregion Properties

        #region Methods

        public void Disconnect()
        {
            Close();
            Dispose();
        }

        public void Send(byte[] data)
        {
            if (_disposed) return;
            Send(new ArraySegment<byte>(data));
        }


        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            UserData = default(T);
            if (Parser != null)
            {
                Parser.Dispose();
                Parser = null;
            }

        }
        public T UserData { get; set; }

        protected override void HandleException(Exception e)
        {
            Logger.Warn("A networking error occured", e);
            Disconnect();
        }

        #endregion Methods
    }
}