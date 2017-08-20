using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Oblivion.Collections;
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
        private static readonly List<WeakReference> BannedList = new List<WeakReference>();

        /// <summary>
        /// the last banned ip
        /// </summary>
        private static string lastBanned;
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

            var weak = new WeakReference(iP);


            if (iP == lastBanned)
            {
               return false;
            }

            foreach (var str in BannedList)
            {
                if (str?.Target == null) continue;

                if (str.Target.ToString() == iP)
                {
                    return false;
                }
            }

            if ((GetConnectionAmount(iP) > maxIpConnectionCount))
            {
                Out.WriteLine(iP + " was banned by Anti-DDoS system.", "Oblivion.TcpAntiDDoS", ConsoleColor.Blue);
                lastBanned = iP;

                BannedList.Add(weak);

                return false;
            }
            /*using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT count(0) FROM users WHERE ip_last = @ip OR ip_reg = @ip");
                dbClient.AddParameter("ip", iP);
                var res = dbClient.GetInteger();
                if (res <= 0)
                {
                    BannedList.Add(weak);
                    lastBanned = iP;
                    Out.WriteLine(iP + " was banned by Anti-DDoS system.", "Oblivion.TcpAntiDDoS", ConsoleColor.Blue);
                    return false;
                }
            }*/

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