﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Oblivion.Util;

namespace Oblivion.Connection.Connection
{
    /// <summary>
    /// Class SocketConnectionCheck.
    /// </summary>
    internal class SocketConnectionCheck
    {
        /// <summary>
        /// The _m connection storage
        /// </summary>
        private static string[] _mConnectionStorage;

        /// <summary>
        /// The banned ip list
        /// </summary>
        private static readonly List<string> BannedList = new List<string>();

        /// <summary>
        /// the last banned ip
        /// </summary>
        private static string _lastBanned;

        /// <summary>
        /// Checks the connection.
        /// </summary>
        /// <param name="sock">The sock.</param>
        /// <param name="maxIpConnectionCount">The maximum ip connection count.</param>
        /// <param name="antiDDosStatus">if set to <c>true</c> [anti d dos status].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool CheckConnection(Socket sock, int maxIpConnectionCount, bool antiDDosStatus)
        {
            if (!antiDDosStatus)
                return true;

            var iP = sock.RemoteEndPoint.ToString().Split(':')[0];

            if (iP == null)
                return false;

            if (iP == _lastBanned)
            {
                return false;
            }

            if (BannedList.Contains(iP))
                return false;
         

            if ((GetConnectionAmount(iP) > maxIpConnectionCount))
            {
                Out.WriteLine(iP + " was banned by Anti-DDoS system.", "Oblivion.TcpAntiDDoS", ConsoleColor.Blue);
                _lastBanned = iP;

                BannedList.Add(iP);

                return false;
            }

            int freeConnectionId = GetFreeConnectionId();

            if (freeConnectionId < 0)
                return false;

            _mConnectionStorage[freeConnectionId] = iP;

            return true;
        }

        /// <summary>
        /// Frees the connection.
        /// </summary>
        /// <param name="ip">The ip.</param>
        internal static void FreeConnection(string ip)
        {
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == ip)
                    _mConnectionStorage[i] = null;
        }

        private static int GetConnectionAmount(string ip) => _mConnectionStorage.Count(t => t == ip);

        private static int GetFreeConnectionId()
        {
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == null)
                    return i;

            return -1;
        }

        internal static void ClearCache()
        {
            BannedList.Clear();
        }

        internal static void SetupTcpAuthorization(int connectionCount)
        {
            _mConnectionStorage = new string[connectionCount];
        }
    }
}