using System;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.Connection.Connection;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using SuperSocket.SocketBase;

namespace Oblivion.Connection.SuperSocket
{
    public class Session<TSession> : AppSession<Session<TSession>, RequestInfo>
        where TSession: IAirHandler
    {
        #region Properties
        public uint ConnId { get; set; }
        
        public IDataParser Parser { get; set; }

        public TSession UserData { get; set; }

        public System.Net.IPAddress RemoteAddress => RemoteEndPoint.Address;

        #endregion Properties

        public string GetIp() => RemoteEndPoint.ToString().Split(':')[0];


        #region Methods

        public void ActivateRc4Filter()
        {
            this.SetNextReceiveFilter(new ReceiveCryptoFilter(UserData));
        }

        public void Disconnect()
        {
            SocketConnectionCheck.FreeConnection(GetIp());
            Close();
            Dispose();
        }

        public void Send(byte[] data)
        {
            if (_disposed || data == null) return;

            try
            {
                byte[] newHeader = null;

            if (UserData == null)
            {
                return;
            }

            if (UserData.IsAir)
            {
                newHeader = AirPacketTranslator.ReplaceOutgoingHeader(data, out var oldHeader, out var newHeaderShort);

                string packetName = "";

                if (Oblivion.DebugMode)
                    packetName = LibraryParser.TryGetOutgoingName(oldHeader);
                if (newHeader == null)
                {
                    return;
                }
                if (Oblivion.DebugMode)
                    Console.WriteLine("Header *production* " + oldHeader + " (" + packetName +
                                      ") has been translated to packet air.");
            }

                Send(newHeader ?? data, 0, data.Length);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (_disposed) return;

            await Task.Run(() =>
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
                Send(data.Array);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }


        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            UserData = default(TSession);
            if (Parser != null)
            {
                Parser.Dispose();
                Parser = null;
            }

        }

        protected override void HandleException(Exception e)
        {
//            Logging.HandleException(e, "Connection - Session.cs");
            Disconnect();
        }

        #endregion Methods
    }
}