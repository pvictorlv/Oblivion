using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Support
{
    /// <summary>
    ///     Class ModerationBanManager.
    /// </summary>
    internal class ModerationBanManager
    {
        /// <summary>
        ///     The _banned i ps
        /// </summary>
        private readonly HybridDictionary _bannedIPs;

        /// <summary>
        ///     The _banned machines
        /// </summary>
        private readonly Dictionary<string, ModerationBan> _bannedMachines;

        /// <summary>
        ///     The _banned usernames
        /// </summary>
        private readonly HybridDictionary _bannedUsernames;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModerationBanManager" /> class.
        /// </summary>
        internal ModerationBanManager()
        {
            _bannedUsernames = new HybridDictionary();
            _bannedIPs = new HybridDictionary();
            _bannedMachines = new Dictionary<string, ModerationBan>();
        }

        /// <summary>
        ///     Loads the bans.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadBans(IQueryAdapter dbClient)
        {
            _bannedUsernames.Clear();
            _bannedIPs.Clear();
            _bannedMachines.Clear();
            

            dbClient.SetQuery($"SELECT ban_type,banned_value,message,banned_until FROM users_bans WHERE banned_until > NOW() AND is_active");
            var table = dbClient.GetTable();

            foreach (DataRow dataRow in table.Rows)
            {
                var text = (string) dataRow["banned_value"];
                var reasonMessage = (string) dataRow["message"];
                var num2 = (DateTime)dataRow["banned_until"];
                var a = (string) dataRow["ban_type"];

                ModerationBanType type;

                switch (a)
                {
                    case "USER_ID":
                        type = ModerationBanType.UserName;
                        break;

                    case "IP_ADDRESS":
                        type = ModerationBanType.Ip;
                        break;

                    default:
                        type = ModerationBanType.Machine;
                        break;
                }

                var moderationBan = new ModerationBan(type, text, reasonMessage, num2);
                

                switch (moderationBan.Type)
                {
                    case ModerationBanType.UserName:
                        if (!_bannedUsernames.Contains(text)) _bannedUsernames.Add(text, moderationBan);
                        break;

                    case ModerationBanType.Ip:
                        if (!_bannedIPs.Contains(text)) _bannedIPs.Add(text, moderationBan);
                        break;

                    default:
                        if (!_bannedMachines.ContainsKey(text)) _bannedMachines.Add(text, moderationBan);
                        break;
                }
            }
        }

        /// <summary>
        ///     Gets the ban reason.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="ip">The ip.</param>
        /// <param name="machineid">The machineid.</param>
        /// <returns>System.String.</returns>
        internal string GetBanReason(string userName, string ip, string machineid)
        {
            if (_bannedUsernames.Contains(userName))
            {
                var moderationBan = (ModerationBan) _bannedUsernames[userName];

                if (!moderationBan.Expired)
                    return moderationBan.ReasonMessage;
            }
            else
            {
                if (_bannedIPs.Contains(ip))
                {
                    var moderationBan2 = (ModerationBan) _bannedIPs[ip];

                    if (!moderationBan2.Expired)
                        return moderationBan2.ReasonMessage;
                }
                else
                {
                    if (!_bannedMachines.ContainsKey(machineid))
                        return string.Empty;

                    var moderationBan3 = _bannedMachines[machineid];

                    if (!moderationBan3.Expired)
                        return moderationBan3.ReasonMessage;
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///     Checks the machine ban.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="machineId">The machine identifier.</param>
        /// <param name="userName"></param>
        /// <returns>System.String.</returns>
        internal bool CheckBan(string userName, string ip, string machineId) =>
            _bannedMachines.ContainsKey(machineId) || _bannedIPs.Contains(ip) || _bannedUsernames.Contains(userName);

        /// <summary>
        ///     Bans the user.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="moderator">The moderator.</param>
        /// <param name="lengthSeconds">The length seconds.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="ipBan">if set to <c>true</c> [ip ban].</param>
        /// <param name="machine">if set to <c>true</c> [machine].</param>
        internal void BanUser(GameClient client, string moderator, double lengthSeconds, string reason, bool ipBan,
            bool machine)
        {
            var type = ModerationBanType.UserName;
            var text = client.GetHabbo().Id.ToString();
            var typeStr = "user";
            var num = DateTime.Now.AddSeconds(lengthSeconds);

            if (ipBan)
            {
                type = ModerationBanType.Ip;

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("SELECT ip_last FROM users WHERE username = @name LIMIT 1");
                    queryReactor.AddParameter("name", text);
                    text = queryReactor.GetString();
                }

                typeStr = "ip";
            }

            if (machine)
            {
                type = ModerationBanType.Machine;
                typeStr = "machine";
                text = client.MachineId;
            }

            var moderationBan = new ModerationBan(type, text, reason, num);

            switch (moderationBan.Type)
            {
                case ModerationBanType.Ip:
                    if (_bannedIPs.Contains(text)) _bannedIPs[text] = moderationBan;
                    else _bannedIPs.Add(text, moderationBan);
                    break;

                case ModerationBanType.Machine:
                    if (_bannedMachines.ContainsKey(text)) _bannedMachines[text] = moderationBan;
                    else _bannedMachines.Add(text, moderationBan);
                    break;

                default:
                    if (_bannedUsernames.Contains(text)) _bannedUsernames[text] = moderationBan;
                    else _bannedUsernames.Add(text, moderationBan);
                    break;
            }

            using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.SetQuery(
                    "INSERT INTO users_bans (bantype,value,reason,expire,added_by,added_date) VALUES (@rawvar,@var,@reason,@num,@mod,@time)");
                queryreactor2.AddParameter("rawvar", typeStr);
                queryreactor2.AddParameter("var", text);
                queryreactor2.AddParameter("reason", reason);
                queryreactor2.AddParameter("num", num);
                queryreactor2.AddParameter("mod", moderator);
                queryreactor2.AddParameter("time", DateTime.Now.ToLongDateString());
                queryreactor2.RunQuery();
            }

            if (ipBan)
            {
                DataTable dataTable;

                using (var queryreactor3 = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor3.SetQuery("SELECT id FROM users WHERE ip_last = @var");
                    queryreactor3.AddParameter("var", text);
                    dataTable = queryreactor3.GetTable();
                }

                if (dataTable != null)
                {
                    using (var queryreactor4 = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        /* TODO CHECK */
                        foreach (DataRow dataRow in dataTable.Rows)
                            queryreactor4.RunFastQuery(
                                $"UPDATE users_info SET bans = bans + 1 WHERE user_id = {Convert.ToUInt32(dataRow["id"])}");
                    }
                }

                BanUser(client, moderator, lengthSeconds, reason, false, false);
                return;
            }

            using (var queryreactor5 = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryreactor5.RunFastQuery(
                    $"UPDATE users_info SET bans = bans + 1 WHERE user_id = {client.GetHabbo().Id}");

            client.Disconnect("banned");
        }

        /// <summary>
        ///     Unbans the user.
        /// </summary>
        /// <param name="userNameOrIp">The username or ip.</param>
        internal void UnbanUser(string userNameOrIp)
        {
            _bannedUsernames.Remove(userNameOrIp);
            _bannedIPs.Remove(userNameOrIp);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("DELETE FROM users_bans WHERE value = @userorip");
                queryReactor.AddParameter("userorip", userNameOrIp);
                queryReactor.RunQuery();
            }
        }
    }
}