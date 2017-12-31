using System.Net.Sockets;

namespace Oblivion.Connection.Connection.AsyncSocket
{
    public class AsyncUserToken
    {
        public bool Redused;

        public ConnectionInformation Session;
        public Socket Socket;

        public AsyncUserToken() : this(null)
        {
        }

        public AsyncUserToken(Socket Socket)
        {
            this.Socket = Socket;
            Redused = false;
        }
    }
}