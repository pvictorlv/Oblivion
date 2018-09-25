﻿using System;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using Oblivion.Messages.Parsers;
using SuperSocket.SocketBase;

namespace Oblivion.Connection.SuperSocket
{
    public class Session<T> : AppSession<Session<T>, RequestInfo>
    {
        #region Properties

        public bool IsAir;
        

        public ARC4 ServerRc4 { get; set; }

        public IDataParser Parser { get; set; }

        #endregion Properties

        #region Methods

        public void Disconnect()
        {
//            SocketConnectionCheck.FreeConnection(GetIp());
            Close();
            Dispose();
        }

        public void Send(byte[] data)
        {
            if (_disposed) return;
            try
            {
                Send(data, 0, data.Length);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public void SendAsync(byte[] data)
        {
            if (_disposed) return;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Send(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
            });
        }

        public void SendArray(ArraySegment<byte> data)
        {
            if (_disposed) return;
            try
            {
                Send(data);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public bool Disposed() => _disposed;
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            
            Oblivion.GetGame().GetClientManager().DisposeConnection(VirtualId);

            _disposed = true;
            UserData = default(T);
            if (Parser != null)
            {
                Parser.Dispose();
                Parser = null;
            }

        }

        public uint VirtualId;
        public T UserData { get; set; }

        protected override void HandleException(Exception e)
        {
//            Logging.HandleException(e, "Connection - Session.cs");
            Disconnect();
        }

        #endregion Methods
    }
}