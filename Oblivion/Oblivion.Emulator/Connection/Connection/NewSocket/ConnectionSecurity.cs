using System.Collections.Generic;
using Oblivion.Configuration;
using Oblivion.Util;

namespace Oblivion.Connection.Connection
{
    public class ConnectionSecurity
    {
        /// <summary>
        ///     Server Connections By IP Address
        /// </summary>
        internal static Dictionary<string, uint> ServerClientConnectionsByAddress;

        /// <summary>
        ///     Blocked Client Connections By IP Address
        /// </summary>
        internal static List<string> BlockedClientConnectionsByAddress;

        /// <summary>
        ///     Initialize Security Manager
        /// </summary>
        internal static void Init()
        {
            ServerClientConnectionsByAddress = new Dictionary<string, uint>();

            BlockedClientConnectionsByAddress = new List<string>();
        }

        /// <summary>
        ///     Add new Client to List
        /// </summary>
        internal static void AddNewClient(string clientAddress)
            => ServerClientConnectionsByAddress.Add(clientAddress, 1);

        /// <summary>
        ///     Add Client Count
        /// </summary>
        internal static void AddClientCount(string clientAddress)
        {
            if (ServerClientConnectionsByAddress.ContainsKey(clientAddress))
                ServerClientConnectionsByAddress[clientAddress]++;
        }

        /// <summary>
        ///     Remove Client Count
        /// </summary>
        internal static void RemoveClientCount(string clientAddress)
        {
            if (ServerClientConnectionsByAddress.ContainsKey(clientAddress))
                ServerClientConnectionsByAddress[clientAddress]--;
        }

        /// <summary>
        ///     Get Client Count
        /// </summary>
        internal static uint GetClientCount(string clientAddress)
        {
            if (ServerClientConnectionsByAddress.ContainsKey(clientAddress))
                return ServerClientConnectionsByAddress[clientAddress];

            AddClientCount(clientAddress);

            return 1;
        }

        /// <summary>
        ///     Check Availability of New Connection
        /// </summary>
        internal static bool CheckAvailability(string clientAddress)
        {
            if (BlockedClientConnectionsByAddress.Contains(clientAddress))
                return false;
            
            if (!ServerClientConnectionsByAddress.ContainsKey(clientAddress))
            {
                AddNewClient(clientAddress);

                return true;
            }

            if (GetClientCount(clientAddress) >= 100)
            {
                if (!BlockedClientConnectionsByAddress.Contains(clientAddress))
                {
                    BlockedClientConnectionsByAddress.Add(clientAddress);

                    Logging.LogMessage($"Connection with Address {clientAddress} is Banned. Due TCP Flooding, will be unbanned after next Restart.");
                }

                return false;
            }

            AddClientCount(clientAddress);

            return true;
        }
    }
}