using System;
using System.Net;
using System.Net.Sockets;
using Oblivion.Util;

namespace Oblivion.Connection.Net
{
    public class MusSocket
    {
        private static Socket _handler;

        internal MusSocket(int port)
        {
            _handler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            Out.WriteLine(
                "Starting up asynchronous sockets server for MUS connections for port " +
                port, "Server.AsyncSocketMusListener");
            try
            {
                _handler.Bind(new IPEndPoint(IPAddress.Any, port));
                _handler.Listen(0);
                _handler.BeginAccept(ConnRequest, _handler);
            }
            catch
            {
                Console.WriteLine("Asynchronous socket server for MUS connections running on port " + port);
            }

            Out.WriteLine(
                "Asynchronous sockets server for MUS connections running on port " +
                port + Environment.NewLine,
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

            _handler.BeginAccept(ConnRequest, _handler);
        }
    }
}