using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Users.Authenticator;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Relationships;
using Oblivion.HabboHotel.Users.Subscriptions;

namespace Oblivion.HabboHotel.Users.UserDataManagement
{
    /// <summary>
    ///     Class UserDataFactory.
    /// </summary>
    internal class UserDataFactory
    {
        /// <summary>
        ///     Gets the user data.
        /// </summary>
        /// <param name="sessionTicket">The session ticket.</param>
        /// <param name="errorCode">The error code.</param>
        /// <returns>UserData.</returns>
        /// <exception cref="UserDataNotFoundException"></exception>
        internal static UserData GetUserData(string sessionTicket, out uint errorCode)
        {
            const uint miniMailCount = 0;
            errorCode = 1;

            DataTable groupsTable;
            DataRow dataRow;
            DataTable achievementsTable;
            DataTable talentsTable;
            DataRow statsTable;
            DataTable favoritesTable;
            DataTable ignoresTable;
            DataTable tagsTable;
            DataRow subscriptionsRow;
            DataTable pollsTable;

            DataTable relationShipsTable;
            DataTable questsTable;
            DataTable dBlockedCommands;

            DataTable myRoomsTable;

            uint userId;
            string userName;
            string look;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                
                queryReactor.SetQuery($"SELECT id,username,look,rank,builders_expire,navilogs,disabled_alert,DutyLevel,OnDuty,builders_items_max,builders_items_used,motto,gender,last_online,credits,activity_points,is_muted,home_room,hide_online,hide_inroom,block_newfriends,vip,account_created,talent_status,diamonds,last_name_change,trade_lock,trade_lock_expire,{Oblivion.GetDbConfig().DbData["emerald.column"]},teamRank,prefixes FROM users WHERE auth_ticket = @ticket");
                queryReactor.AddParameter("ticket", sessionTicket);
                dataRow = queryReactor.GetRow();
                if (dataRow == null)
                    return null;

                userId = Convert.ToUInt32(dataRow["id"]);
                userName = dataRow["username"].ToString();
                look = dataRow["look"].ToString();


                if (Oblivion.GetGame().GetClientManager().GetClientByUserId(userId) != null)
                    Oblivion.GetGame()
                        .GetClientManager()
                        .GetClientByUserId(userId)
                        .Disconnect("User connected in other place");

                queryReactor.SetQuery(
                    $"SELECT `group`, `level`, progress FROM users_achievements WHERE userId = {userId}");
                achievementsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT talent_id, talent_state FROM users_talents WHERE userId = {userId}");
                talentsTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT COUNT(id) FROM users_stats WHERE id = {userId}");

                if (int.Parse(queryReactor.GetString()) == 0)
                    queryReactor.RunFastQuery($"INSERT INTO users_stats (id) VALUES ({userId})");

                queryReactor.SetQuery($"SELECT room_id FROM users_favorites WHERE user_id = {userId}");
                favoritesTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT ignore_id FROM users_ignores WHERE user_id = {userId}");
                ignoresTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT tag FROM users_tags WHERE user_id = {userId} LIMIT 15");
                tagsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT subscription_id, timestamp_activated, timestamp_expire, timestamp_lastgift FROM users_subscriptions WHERE user_id = {userId} AND timestamp_expire > UNIX_TIMESTAMP() ORDER BY subscription_id DESC LIMIT 1");
                subscriptionsRow = queryReactor.GetRow();



                queryReactor.SetQuery(
                    $"SELECT poll_id FROM users_polls WHERE user_id = {userId} GROUP BY poll_id;");
                pollsTable = queryReactor.GetTable();


                queryReactor.SetQuery($"SELECT * FROM users_stats WHERE id = {userId}");
                statsTable = queryReactor.GetRow();

               

                queryReactor.SetQuery("SELECT * FROM rooms_data WHERE owner = @name LIMIT 150");
                queryReactor.AddParameter("name", userName);
                myRoomsTable = queryReactor.GetTable();

               

                queryReactor.SetQuery(
                    $"SELECT quest_id, progress FROM users_quests_data WHERE user_id = {userId}");
                questsTable = queryReactor.GetTable();
                

                queryReactor.SetQuery(
                    $"SELECT group_id, rank, date_join, has_chat FROM groups_members WHERE user_id = {userId}");
                groupsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    "REPLACE INTO users_info(user_id, login_timestamp) VALUES(@userId, @login_timestamp)");
                queryReactor.AddParameter("userId", userId);
                queryReactor.AddParameter("login_timestamp", Oblivion.GetUnixTimeStamp());
                queryReactor.RunQuery();

                queryReactor.SetQuery($"SELECT * FROM users_relationships WHERE user_id = {userId}");
                relationShipsTable = queryReactor.GetTable();

                queryReactor.SetQuery("SELECT command_name FROM user_blockcmd WHERE user_id = '" + userId + "'");
                dBlockedCommands = queryReactor.GetTable();

//                queryReactor.RunFastQuery($"UPDATE users SET online='1' WHERE id = {userId} LIMIT 1");
            }

            var achievements = new Dictionary<string, UserAchievement>();

            /* TODO CHECK */ foreach (DataRow row in achievementsTable.Rows)
            {
                var text = (string) row["group"];
                var level = (int) row["level"];
                var progress = (int) row["progress"];
                var value = new UserAchievement(text, level, progress);
                achievements.Add(text, value);
            }

            var talents = new Dictionary<int, UserTalent>();

            /* TODO CHECK */ foreach (DataRow row in talentsTable.Rows)
            {
                var num2 = (int) row["talent_id"];
                var state = (int) row["talent_state"];
                var value2 = new UserTalent(num2, state);
                talents.Add(num2, value2);
            }

            var favorites = (from DataRow row in favoritesTable.Rows select (uint) row["room_id"]).ToList();
            var ignoreUsers = (from DataRow row in ignoresTable.Rows select (uint) row["ignore_id"]).ToList();
            var tags = (from DataRow row in tagsTable.Rows select row["tag"].ToString().Replace(" ", "")).ToList();


          

            Subscription subscriptions = null;

            if (subscriptionsRow != null)
                subscriptions = new Subscription((int) subscriptionsRow["subscription_id"],
                    (int) subscriptionsRow["timestamp_activated"], (int) subscriptionsRow["timestamp_expire"],
                    (int) subscriptionsRow["timestamp_lastgift"]);

            var pollSuggested = new HashSet<uint>();

            /* TODO CHECK */ foreach (var pId in from DataRow row in pollsTable.Rows select (uint) row["poll_id"])
                pollSuggested.Add(pId);



            var quests = new Dictionary<uint, int>();
            /* TODO CHECK */ foreach (DataRow row in questsTable.Rows)
            {
                var key = Convert.ToUInt32(row["quest_id"]);
                var value3 = (int) row["progress"];

                if (quests.ContainsKey(key))
                    quests.Remove(key);

                quests.Add(key, value3);
            }

            var groups = (from DataRow row in groupsTable.Rows select new GroupMember(userId, userName, look, (uint) row["group_id"], Convert.ToInt16(row["rank"]), (int) row["date_join"], Oblivion.EnumToBool(row["has_chat"].ToString()))).ToList();

            /* TODO CHECK */

            var relationShips = relationShipsTable.Rows.Cast<DataRow>()
                .ToDictionary(row => (int) row[0],
                    row => new Relationship((int) row[0], (int) row[2], Convert.ToInt32(row[3].ToString())));

            var user = HabboFactory.GenerateHabbo(dataRow, statsTable, groups);
            errorCode = 0;

          
            var blockedCommands = (from DataRow r in dBlockedCommands.Rows select r["command_name"].ToString())
                .ToList();

            var myRooms = (from DataRow row in myRoomsTable.Rows let roomId = Convert.ToUInt32(row["id"].ToString()) select Oblivion.GetGame().GetRoomManager().FetchRoomData(roomId, row, userId)).ToList();

            var openedGifts = new List<int>();

            var opened = statsTable["opened_gifts"].ToString();
            if (!string.IsNullOrEmpty(opened))
            {
                openedGifts = opened.Split(',').Select(int.Parse).ToList();
            }

            var data = new UserData(userId, achievements, talents, favorites, ignoreUsers, tags, subscriptions, myRooms,
                quests, user, relationShips,
                pollSuggested, miniMailCount, blockedCommands, openedGifts) {LoadedRelations = true};

            return data;
        }

        /// <summary>
        ///     Gets the user data.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>UserData.</returns>
        internal static UserData GetUserData(int userId)
        {
            //todo: improve it
            DataRow dataRow;
            uint num;
//            DataRow row;
//            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT u.id,s.favourite_group,s.achievement_score,username,block_newfriends,look,motto,gender,last_online,account_created FROM users AS u, users_stats AS s WHERE u.id = '{userId}' AND u.id = s.id");

                dataRow = queryReactor.GetRow();

                if (dataRow == null)
                    return null;
                
                num = Convert.ToUInt32(dataRow["id"]);
//                Oblivion.GetGame().GetClientManager().LogClonesOut(num);



                if (Oblivion.GetGame().GetClientManager().GetClientByUserId(num) != null)
                    return null;
                
//                queryReactor.SetQuery($"SELECT * FROM users_stats WHERE id={num} LIMIT 1");
//                row = queryReactor.GetRow();

//                queryReactor.SetQuery("SELECT * FROM users_relationships WHERE user_id=@id");
//                queryReactor.AddParameter("id", num);
//                table = queryReactor.GetTable();
            }

            var achievements = new Dictionary<string, UserAchievement>();
            var talents = new Dictionary<int, UserTalent>();
            var favouritedRooms = new List<uint>();
            var ignores = new List<uint>();
            var tags = new List<string>();
            var rooms = new List<RoomData>();
            var quests = new Dictionary<uint, int>();
            var group = new List<GroupMember>();
            var pollData = new HashSet<uint>();

            var user = HabboFactory.GenerateCachedHabbo(dataRow, group);

            return new UserData(num, achievements, talents, favouritedRooms, ignores, tags, null, rooms, quests, user, new Dictionary<int, Relationship>(), pollData, 0, new List<string>(), new List<int>());
        }
    }
}