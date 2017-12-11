using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Users.Authenticator;
using Oblivion.HabboHotel.Users.Badges;
using Oblivion.HabboHotel.Users.Inventory;
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
            DataTable badgesTable;
            DataTable itemsTable;
            DataTable effectsTable;
            DataTable pollsTable;
            DataTable friendsTable;
            DataTable friendsRequestsTable;

            DataTable relationShipsTable;
            DataTable questsTable;
            DataTable petsTable;
            DataTable dBlockedCommands;

            DataTable myRoomsTable;

            uint userId;
            string userName;
            string look;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                
                queryReactor.SetQuery("SELECT id,username,look,rank,builders_expire,navilogs,disabled_alert,DutyLevel,OnDuty,builders_items_max,builders_items_used,motto,gender,last_online,credits,activity_points,is_muted,home_room,hide_online,hide_inroom,block_newfriends,vip,account_created,talent_status,diamonds,last_name_change,trade_lock,trade_lock_expire FROM users WHERE auth_ticket = @ticket");
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

                queryReactor.SetQuery($"SELECT badge_id,badge_slot FROM users_badges WHERE user_id = {userId}");
                badgesTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT id,base_item,group_id,extra_data,songcode FROM `items_rooms` WHERE room_id='0' AND user_id={userId} LIMIT 5000");
                itemsTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT effect_id,total_duration,is_activated,activated_stamp,type FROM users_effects WHERE user_id = {userId}");
                effectsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT poll_id FROM users_polls WHERE user_id = {userId} GROUP BY poll_id;");
                pollsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    string.Format(
                        "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_one_id WHERE messenger_friendships.user_two_id = {0} UNION ALL SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE messenger_friendships.user_one_id = {0}",
                        userId));
                friendsTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT * FROM users_stats WHERE id = {userId}");
                statsTable = queryReactor.GetRow();

                queryReactor.SetQuery(
                    $"SELECT messenger_requests.from_id,messenger_requests.to_id,users.Username, users.Look FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = {userId}");
                friendsRequestsTable = queryReactor.GetTable();

                queryReactor.SetQuery("SELECT * FROM rooms_data WHERE owner = @name LIMIT 150");
                queryReactor.AddParameter("name", userName);
                myRoomsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT * FROM bots WHERE user_id = {userId} AND room_id = 0");
                petsTable = queryReactor.GetTable();

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


          
            var badges =
                (badgesTable.Rows.Cast<DataRow>()
                    .Select(dataRow8 => new Badge((string) dataRow8["badge_id"], (int) dataRow8["badge_slot"]))).ToList();

            Subscription subscriptions = null;

            if (subscriptionsRow != null)
                subscriptions = new Subscription((int) subscriptionsRow["subscription_id"],
                    (int) subscriptionsRow["timestamp_activated"], (int) subscriptionsRow["timestamp_expire"],
                    (int) subscriptionsRow["timestamp_lastgift"]);

            var items = (from DataRow row in itemsTable.Rows
                let id = Convert.ToUInt32(row["id"])
                let itemId = Convert.ToUInt32(row["base_item"])
                where Oblivion.GetGame().GetItemManager().ContainsItem(itemId)
                let extraData = !DBNull.Value.Equals(row["extra_data"]) ? (string) row["extra_data"] : string.Empty
                let @group = Convert.ToUInt32(row["group_id"])
                let songCode = (string) row["songcode"]
                select new UserItem(id, itemId, extraData, @group, songCode)).ToList();

            var effects = (from DataRow row in effectsTable.Rows
                let effectId = (int) row["effect_id"]
                let totalDuration = (int) row["total_duration"]
                let activated = Oblivion.EnumToBool((string) row["is_activated"])
                let activateTimestamp = (double) row["activated_stamp"]
                let type = Convert.ToInt16(row["type"])
                select new AvatarEffect(effectId, totalDuration, activated, activateTimestamp, type)).ToList();

            var pollSuggested = new HashSet<uint>();

            /* TODO CHECK */ foreach (var pId in from DataRow row in pollsTable.Rows select (uint) row["poll_id"])
                pollSuggested.Add(pId);

            var friends = new Dictionary<uint, MessengerBuddy>();
            var limit = (friendsTable.Rows.Count - 700);

            if (limit > 0)
            {
                using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.RunFastQuery(string.Concat("DELETE FROM messenger_friendships WHERE user_one_id=",
                        userId, " OR user_two_id=", userId, " LIMIT ", limit));
                    queryreactor2.SetQuery(
                        string.Concat(
                            "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_one_id WHERE messenger_friendships.user_two_id = ",
                            userId,
                            " UNION ALL SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE messenger_friendships.user_one_id = ",
                            userId));
                    friendsTable = queryreactor2.GetTable();
                }
            }

            /* TODO CHECK */ foreach (DataRow row in friendsTable.Rows)
            {
                var num4 = Convert.ToUInt32(row["id"]);
                var pUsername = (string) row["username"];
                var pLook = (string) row["look"];
                var pMotto = (string) row["motto"];
                var pAppearOffline = Oblivion.EnumToBool(row["hide_online"].ToString());
                var pHideInroom = Oblivion.EnumToBool(row["hide_inroom"].ToString());

                if (num4 != userId && !friends.ContainsKey(num4))
                    friends.Add(num4,
                        new MessengerBuddy(num4, pUsername, pLook, pMotto, pAppearOffline, pHideInroom));
            }

            var friendsRequests = new Dictionary<uint, MessengerRequest>();

            /* TODO CHECK */ foreach (DataRow row in friendsRequestsTable.Rows)
            {
                var num5 = Convert.ToUInt32(row["from_id"]);
                var num6 = Convert.ToUInt32(row["to_id"]);
                var pUsername2 = row["username"].ToString();
                var pLook = row["look"].ToString();

                if (num5 != userId)
                {
                    if (!friendsRequests.ContainsKey(num5))
                        friendsRequests.Add(num5, new MessengerRequest(userId, num5, pUsername2, pLook));
                    else if (!friendsRequests.ContainsKey(num6))
                        friendsRequests.Add(num6, new MessengerRequest(userId, num6, pUsername2, pLook));
                }
            }

                


            var pets = new Dictionary<uint, Pet>();
            var inventoryBots = new Dictionary<uint, RoomBot>();

            foreach (DataRow row in petsTable.Rows)
            {
                if (row["ai_type"].ToString() == "generic")
                {
                    var bot = BotManager.GenerateBotFromRow(row);
                    inventoryBots.Add(bot.BotId, bot);
                    continue;
                }
                using (var queryreactor3 = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor3.SetQuery($"SELECT * FROM pets_data WHERE id={row[0]} LIMIT 1");
                    var row3 = queryreactor3.GetRow();
                    if (row3 == null)
                        continue;
                    var pet = CatalogManager.GeneratePetFromRow(row, row3);
                    pets.Add(pet.PetId, pet);
                }
            }

            var quests = new Dictionary<uint, int>();
            /* TODO CHECK */ foreach (DataRow row in questsTable.Rows)
            {
                var key = Convert.ToUInt32(row["quest_id"]);
                var value3 = (int) row["progress"];

                if (quests.ContainsKey(key))
                    quests.Remove(key);

                quests.Add(key, value3);
            }

            var groups = new HashSet<GroupMember>();

            /* TODO CHECK */ foreach (DataRow row in groupsTable.Rows)
                groups.Add(new GroupMember(userId, userName, look, (uint) row["group_id"], Convert.ToInt16(row["rank"]),
                    (int) row["date_join"], Oblivion.EnumToBool(row["has_chat"].ToString())));

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


            return new UserData(userId, achievements, talents, favorites, ignoreUsers, tags, subscriptions, badges,
                items, effects, friends, friendsRequests, myRooms, pets, quests, user, inventoryBots, relationShips,
                pollSuggested, miniMailCount, blockedCommands, openedGifts);
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
            DataRow row;
            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT id,username,block_newfriends,navilogs,disabled_alert,DutyLevel,OnDuty,builders_items_max,builders_items_used,builders_expire,look,rank,motto,gender,last_online,credits,activity_points,is_muted,home_room,hide_online,hide_inroom,vip,account_created,talent_status,diamonds,last_name_change,trade_lock,trade_lock_expire FROM users WHERE id = '{userId}'");

                dataRow = queryReactor.GetRow();

                if (dataRow == null)
                    return null;
                
                num = Convert.ToUInt32(dataRow["id"]);
                Oblivion.GetGame().GetClientManager().LogClonesOut(num);



                if (Oblivion.GetGame().GetClientManager().GetClientByUserId(num) != null)
                    return null;

                queryReactor.SetQuery($"SELECT group_id,rank,date_join FROM groups_members WHERE user_id={userId}");
                queryReactor.GetTable();
                queryReactor.SetQuery($"SELECT * FROM users_stats WHERE id={num} LIMIT 1");
                row = queryReactor.GetRow();

                queryReactor.SetQuery("SELECT * FROM users_relationships WHERE user_id=@id");
                queryReactor.AddParameter("id", num);
                table = queryReactor.GetTable();
            }

            var achievements = new Dictionary<string, UserAchievement>();
            var talents = new Dictionary<int, UserTalent>();
            var favouritedRooms = new List<uint>();
            var ignores = new List<uint>();
            var tags = new List<string>();
            var badges = new List<Badge>();
            var inventory = new List<UserItem>();
            var effects = new List<AvatarEffect>();
            var friends = new Dictionary<uint, MessengerBuddy>();
            var requests = new Dictionary<uint, MessengerRequest>();
            var rooms = new List<RoomData>();
            var pets = new Dictionary<uint, Pet>();
            var quests = new Dictionary<uint, int>();
            var bots = new Dictionary<uint, RoomBot>();
            var group = new HashSet<GroupMember>();
            var pollData = new HashSet<uint>();

            var dictionary = table.Rows.Cast<DataRow>()
                .ToDictionary(dataRow2 => (int) dataRow2[0],
                    dataRow2 =>
                        new Relationship((int) dataRow2[0], (int) dataRow2[2], Convert.ToInt32(dataRow2[3].ToString())));
            var user = HabboFactory.GenerateHabbo(dataRow, row, group);

            return new UserData(num, achievements, talents, favouritedRooms, ignores, tags, null, badges, inventory,
                effects, friends, requests, rooms, pets, quests, user, bots, dictionary, pollData, 0, new List<string>(), new List<int>());
        }
    }
}