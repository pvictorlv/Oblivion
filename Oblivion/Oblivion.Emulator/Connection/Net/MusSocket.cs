using System;
using System.Net;
using System.Net.Sockets;
using Oblivion.Util;

namespace Oblivion.Connection.Net
{
    public class MusSocket
    {
        private static Socket Handler;
        private static int _Port;
        private static string _MusHost;

        internal MusSocket(int Port, string musHost)
        {
            _Port = Port;
            _MusHost = musHost;
            Handler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            Out.WriteLine(
                "Starting up asynchronous sockets server for MUS connections for port " +
                _Port, "Server.AsyncSocketMusListener");
            try
            {
                Handler.Bind(new IPEndPoint(IPAddress.Any, Port));
                Handler.Listen(0);
                Handler.BeginAccept(ConnRequest, Handler);
            }
            catch
            {
                Console.WriteLine("Asynchronous socket server for MUS connections running on port " + Port);
            }

            Out.WriteLine(
                "Asynchronous sockets server for MUS connections running on port " +
                _Port + Environment.NewLine,
                "Server.AsyncSocketMusListener");
        }

        private static void ConnRequest(IAsyncResult iAr)
        {
            try
            {
                var nSocket = ((Socket) iAr.AsyncState).EndAccept(iAr);
                //if (nSocket.RemoteEndPoint.ToString().Split(':')[0] != _MusHost) // Don't allow remote IP!
               /* var ip = nSocket.RemoteEndPoint.ToString().Split(':')[0];
                if (ip != _MusHost)
                {
                    Out.WriteLine("The ip " + ip + " are trying to send mus request");
                }*/
                new MusConnection(nSocket);
            }
            catch (Exception e)
            {
                Writer.Writer.LogCriticalException(e.ToString());
            }

            Handler.BeginAccept(ConnRequest, Handler);
        }
    }
}