using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Configuration;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

namespace Oblivion.HabboHotel.Roles
{
    /// <summary>
    ///     Class RoleManager.
    /// </summary>
    internal class RoleManager
    {
        /// <summary>
        ///     The _CMD rights
        /// </summary>
        private readonly Dictionary<string, string> _cmdRights;

        /// <summary>
        ///     The _rights
        /// </summary>
        private readonly Dictionary<string, uint> _rights;

        /// <summary>
        ///     The _sub rights
        /// </summary>
        private readonly Dictionary<string, int> _subRights;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleManager" /> class.
        /// </summary>
        internal RoleManager()
        {
            _rights = new Dictionary<string, uint>();
            _subRights = new Dictionary<string, int>();
            _cmdRights = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Loads the rights.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadRights(IQueryAdapter dbClient)
        {
            ClearRights();
            dbClient.SetQuery("SELECT command,rank FROM server_fuses;");
            var table = dbClient.GetTable();
            if (table != null)
            {
                /* TODO CHECK */ foreach (DataRow dataRow in table.Rows)
                {
                    if (!_cmdRights.ContainsKey((string) dataRow[0]))
                    {
                        _cmdRights.Add((string) dataRow[0], (string) dataRow[1]);
                    }
                    else
                    {
                        Logging.LogException($"Duplicate Fuse Command \"{dataRow[0]}\" found");
                    }
                }
            }
            dbClient.SetQuery("SELECT * FROM server_fuserights");
            var table2 = dbClient.GetTable();
            if (table2 == null)
            {
                return;
            }
            /* TODO CHECK */ foreach (DataRow dataRow2 in table2.Rows)
            {
                if ((int) dataRow2[3] == 0)
                {
                    if (!_rights.ContainsKey((string) dataRow2[0]))
                    {
                        _rights.Add((string) dataRow2[0], Convert.ToUInt32(dataRow2[1]));
                    }
                    else
                    {
                        Logging.LogException($"Unknown Subscription Fuse \"{dataRow2[0]}\" found");
                    }
                }
                else
                {
                    if ((int) dataRow2[3] > 0)
                    {
                        _subRights.Add((string) dataRow2[0], (int) dataRow2[3]);
                    }
                    else
                    {
                        Logging.LogException($"Unknown fuse type \"{dataRow2[3]}\" found");
                    }
                }
            }
        }

        /// <summary>
        ///     Ranks the got command.
        /// </summary>
        /// <param name="rankId">The rank identifier.</param>
        /// <param name="cmd">The command.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RankGotCommand(uint rankId, string cmd)
        {
            if (!_cmdRights.TryGetValue(cmd, out var role))
            {
                return false;
            }
            if (!role.Contains(";"))
            {
                return rankId >= uint.Parse(role);
            }

            var cmdranks = role.Split(';');
            return cmdranks.Any(rank => rank.Contains(Convert.ToString(rankId))) ||
                   role.Contains(Convert.ToString(rankId));
        }

        /// <summary>
        ///     Ranks the has right.
        /// </summary>
        /// <param name="rankId">The rank identifier.</param>
        /// <param name="fuse">The fuse.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RankHasRight(uint rankId, string fuse) => _rights.TryGetValue(fuse, out var right) && rankId >= right;

        /// <summary>
        ///     Determines whether the specified sub has vip.
        /// </summary>
        /// <param name="sub">The sub.</param>
        /// <param name="fuse">The fuse.</param>
        /// <returns><c>true</c> if the specified sub has vip; otherwise, <c>false</c>.</returns>
        internal bool HasVip(int sub, string fuse)
        {
            return _subRights.TryGetValue(fuse, out var right) && right == sub;
        }

        /// <summary>
        ///     Gets the rights for rank.
        /// </summary>
        /// <param name="rankId">The rank identifier.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        internal List<string> GetRightsForRank(uint rankId)
        {
            var list = new List<string>();
            /* TODO CHECK */
            foreach (var current in _rights)
            {
                if (rankId >= current.Value && !list.Contains(current.Key))
                {
                    list.Add(current.Key);
                }
            }

            return list;
        }
        
        /// <summary>
        ///     Clears the rights.
        /// </summary>
        internal void ClearRights()
        {
            _rights.Clear();
            _cmdRights.Clear();
            _subRights.Clear();
        }
    }
}