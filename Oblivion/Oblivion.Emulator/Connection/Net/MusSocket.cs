using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Oblivion.Util;

namespace Oblivion.Connection.Net
{
    public class MusSocket
    {
        private static Socket _handler;
        private static List<string> _allowedIps;
        internal MusSocket(int port, string allowedIps)
        {
            _handler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _allowedIps = allowedIps.Split(';').ToList();
           
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
                var ip = nSocket.RemoteEndPoint.ToString().Split(':')[0];
                if (!_allowedIps.Contains(ip))
                {
                    Out.WriteLine("The ip " + ip + " are trying to send mus request");
                    nSocket.Close();
                    return;
                }
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