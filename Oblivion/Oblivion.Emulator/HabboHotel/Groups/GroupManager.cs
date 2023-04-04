using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.HabboHotel.Users;

namespace Oblivion.HabboHotel.Groups
{
    /// <summary>
    ///     Class GroupManager.
    /// </summary>
    internal class GroupManager
    {
        /// <summary>
        ///     The back ground colours
        /// </summary>
        internal HybridDictionary BackGroundColours;

        /// <summary>
        ///     The base colours
        /// </summary>
        internal HashSet<GroupBaseColours> BaseColours;

        /// <summary>
        ///     The bases
        /// </summary>
        internal HashSet<GroupBases> Bases;

        /// <summary>
        ///     The groups
        /// </summary>
        internal Dictionary<uint, Guild> Groups;

        /// <summary>
        ///     The symbol colours
        /// </summary>
        internal HybridDictionary SymbolColours;

        /// <summary>
        ///     The symbols
        /// </summary>
        internal HashSet<GroupSymbols> Symbols;

        /// <summary>
        ///     Initializes the groups.
        /// </summary>
        internal void InitGroups()
        {
            Bases = new HashSet<GroupBases>();
            Symbols = new HashSet<GroupSymbols>();
            BaseColours = new HashSet<GroupBaseColours>();
            SymbolColours = new HybridDictionary();
            BackGroundColours = new HybridDictionary();
            Groups = new Dictionary<uint, Guild>();

            ClearInfo();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM groups_badges_parts ORDER BY id");

                var table = queryReactor.GetTable();

                if (table == null)
                    return;

                /* TODO CHECK */
                foreach (DataRow row in table.Rows)
                {
                    switch (row["type"].ToString().ToLower())
                    {
                        case "base":
                            Bases.Add(new GroupBases(int.Parse(row["id"].ToString()), row["code"].ToString(),
                                row["code2"].ToString()));
                            break;
                        case "symbol":
                            Symbols.Add(new GroupSymbols(int.Parse(row["id"].ToString()), row["code"].ToString(),
                                row["code2"].ToString()));
                            break;
                        case "base_color":
                            BaseColours.Add(new GroupBaseColours(int.Parse(row["id"].ToString()),
                                row["code"].ToString()));
                            break;
                        case "symbol_color":
                            SymbolColours.Add(int.Parse(row["id"].ToString()),
                                new GroupSymbolColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                            break;
                        case "other_color":
                            BackGroundColours.Add(int.Parse(row["id"].ToString()),
                                new GroupBackGroundColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Clears the information.
        /// </summary>
        internal void ClearInfo()
        {
            Bases.Clear();
            Symbols.Clear();
            BaseColours.Clear();
            SymbolColours.Clear();
            BackGroundColours.Clear();
        }

        /// <summary>
        ///     Creates the theGroup.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="badge">The badge.</param>
        /// <param name="session">The session.</param>
        /// <param name="colour1">The colour1.</param>
        /// <param name="colour2">The colour2.</param>
        /// <param name="group">The theGroup.</param>
        internal async Task<Guild> CreateGroup(string name, string desc, uint roomId, string badge, GameClient session, int colour1,
            int colour2)
        {
            Habbo user = session.GetHabbo();
            Dictionary<uint, GroupMember> emptyDictionary = new Dictionary<uint, GroupMember>();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"INSERT INTO groups_data (`name`, `desc`,`badge`,`owner_id`,`created`,`room_id`,`colour1`,`colour2`) VALUES(@name,@desc,@badge,'{session.GetHabbo().Id}',UNIX_TIMESTAMP(),'{roomId}','{colour1}','{colour2}')");
                queryReactor.AddParameter("name", name);
                queryReactor.AddParameter("desc", desc);
                queryReactor.AddParameter("badge", badge);

                var id = (uint)await queryReactor.InsertQueryAsync();

                await queryReactor.RunFastQueryAsync($"UPDATE rooms_data SET group_id='{id}' WHERE id='{roomId}' LIMIT 1");

                var memberGroup = new GroupMember(user.Id, user.UserName, user.Look, id, 2,
                    Oblivion.GetUnixTimeStamp(), true);
                Dictionary<uint, GroupMember> dictionary =
                    new Dictionary<uint, GroupMember> { { session.GetHabbo().Id, memberGroup } };

                var  group = new Guild(id, name, desc, roomId, badge, Oblivion.GetUnixTimeStamp(), user.Id, colour1, colour2,
                    dictionary, emptyDictionary, emptyDictionary, 0, 1, false, name, desc, 0, 0.0, 0, string.Empty, 0,
                    0, 1, 1, 2, true);

                Groups.Add(id, group);

                await queryReactor.RunFastQueryAsync(
                    $"INSERT INTO groups_members (group_id, user_id, rank, date_join) VALUES ('{id}','{session.GetHabbo().Id}','2','{Oblivion.GetUnixTimeStamp()}')");

                var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);

                if (room != null)
                {
                    room.RoomData.Group = group;
                    room.RoomData.GroupId = id;
                }

                user.UserGroups.Add(memberGroup);
                group.Admins.Add(user.Id, memberGroup);

                await queryReactor.RunFastQueryAsync(
                    $"UPDATE users_stats SET favourite_group='{id}' WHERE id='{user.Id}' LIMIT 1");
                await queryReactor.RunFastQueryAsync($"DELETE FROM rooms_rights WHERE room_id='{roomId}'");

                return group;
            }

        }

        /// <summary>
        ///     Gets the theGroup.
        /// </summary>
        /// <param name="groupId">The theGroup identifier.</param>
        /// <returns>Guild.</returns>
        internal Guild GetGroup(uint groupId)
        {
            if (Groups == null)
                return null;

            if (groupId <= 0) return null;

            if (Groups.TryGetValue(groupId, out Guild grp))
                return grp;

            var members = new Dictionary<uint, GroupMember>();
            var admins = new Dictionary<uint, GroupMember>();
            var requests = new Dictionary<uint, GroupMember>();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT * FROM groups_data WHERE id={groupId} LIMIT 1");

                var row = queryReactor.GetRow();

                if (row == null)
                    return null;

                queryReactor.SetQuery(
                    "SELECT g.user_id, u.username, u.look, g.rank, g.date_join FROM groups_members g " +
                    $"INNER JOIN users u ON (g.user_id = u.id) WHERE g.group_id={groupId}");

                var groupMembersTable = queryReactor.GetTable();

                queryReactor.SetQuery("SELECT g.user_id, u.username, u.look FROM groups_requests g " +
                                      $"INNER JOIN users u ON (g.user_id = u.id) WHERE group_id={groupId}");

                var groupRequestsTable = queryReactor.GetTable();

                uint userId;

                /* TODO CHECK */
                foreach (DataRow dataRow in groupMembersTable.Rows)
                {
                    userId = (uint)dataRow["user_id"];

                    var rank = int.Parse(dataRow["rank"].ToString());

                    var membGroup = new GroupMember(userId, dataRow["username"].ToString(), dataRow["look"].ToString(),
                        groupId, rank, (int)dataRow["date_join"], true);

                    members[userId] = membGroup;

                    if (rank >= 1)
                    {
                        admins[userId] = membGroup;
                    }
                }

                /* TODO CHECK */
                foreach (DataRow dataRow in groupRequestsTable.Rows)
                {
                    userId = (uint)dataRow["user_id"];

                    var membGroup = new GroupMember(userId, dataRow["username"].ToString(), dataRow["look"].ToString(),
                        groupId, 0, Oblivion.GetUnixTimeStamp(), true);

                    requests[userId] = membGroup;
                }

                var group = new Guild((uint)row["id"], row["name"].ToString(), row["desc"].ToString(),
                    (uint)row["room_id"],
                    row["badge"].ToString(), (int)row["created"], (uint)row["owner_id"], (int)row["colour1"],
                    (int)row["colour2"], members, requests,
                    admins, Convert.ToUInt16(row["state"]), Convert.ToUInt16(row["admindeco"]),
                    row["has_forum"].ToString() == "1",
                    row["forum_name"].ToString(), row["forum_description"].ToString(),
                    uint.Parse(row["forum_messages_count"].ToString()), double.Parse(row["forum_score"].ToString()),
                    uint.Parse(row["forum_lastposter_id"].ToString()), row["forum_lastposter_name"].ToString(),
                    int.Parse(row["forum_lastposter_timestamp"].ToString()),
                    (int)row["who_can_read"], (int)row["who_can_post"], (int)row["who_can_thread"],
                    (int)row["who_can_mod"], Oblivion.EnumToBool(row["has_chat"].ToString()));

                Groups[(uint)row["id"]] = group;

                return group;
            }
        }

        /// <summary>
        ///     Gets the user groups.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>HashSet&lt;GroupUser&gt;.</returns>
        internal HashSet<GroupMember> GetUserGroups(uint userId)
        {
            var list = new HashSet<GroupMember>();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"SELECT u.username, u.look, g.group_id, g.rank, g.date_join FROM groups_members g INNER JOIN users u ON (g.user_id = u.id) WHERE g.user_id={userId}");

                var table = queryReactor.GetTable();

                /* TODO CHECK */
                foreach (DataRow dataRow in table.Rows)
                    list.Add(new GroupMember(userId, dataRow["username"].ToString(), dataRow["look"].ToString(),
                        (uint)dataRow["group_id"], Convert.ToInt16(dataRow["rank"]), (int)dataRow["date_join"],
                        false));
            }

            return list;
        }

        internal async Task AddGroupMemberIntoResponse(ServerMessage response, GroupMember member)
        {
            await response.AppendIntegerAsync(member.Rank == 2 ? 0 : member.Rank == 1 ? 1 : 2);
            await response.AppendIntegerAsync(member.Id);
            await response.AppendStringAsync(member.Name);
            await response.AppendStringAsync(member.Look);
            await response.AppendStringAsync(Oblivion.GetGroupDateJoinString(member.DateJoin));
        }

        /// <summary>
        ///     Serializes the theGroup members.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="theGroup">The theGroup.</param>
        /// <param name="reqType">Type of the req.</param>
        /// <param name="session">The session.</param>
        /// <param name="searchVal">The search value.</param>
        /// <param name="page">The page.</param>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializeGroupMembers(ServerMessage response, Guild theGroup, uint reqType,
            GameClient session, string searchVal = "", int page = 0)
        {
            if (theGroup == null || session == null)
                return null;

            response.AppendInteger(theGroup.Id);
            response.AppendString(theGroup.Name);
            response.AppendInteger(theGroup.RoomId);
            response.AppendString(theGroup.Badge);

            var list = (GetGroupUsersByString(theGroup, searchVal, reqType));

            int startIndex = (page - 1) * 14 + 14;
            int finishIndex = list.Count;
            var members = list.Skip(startIndex).Take(finishIndex - startIndex).ToList();
            if (reqType == 0)
            {
                response.AppendInteger(list.Count);

                if (theGroup.Members.Count > 0)
                {
                    response.AppendInteger(members.Count);

                    using (var enumerator = members.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;

                            await AddGroupMemberIntoResponse(response, current);
                        }
                    }
                }
                else
                    response.AppendInteger(0);
            }
            else if (reqType == 1)
            {
                response.AppendInteger(theGroup.Admins.Count);

                var paging = (page <= list.Count - 1) ? members : null;

                if ((theGroup.Admins.Count > 0) && (list.Count - 1 >= page) && paging != null)
                {
                    response.AppendInteger(members.Count);

                    using (var enumerator = members.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;

                            await AddGroupMemberIntoResponse(response, current);
                        }
                    }
                }
                else
                    response.AppendInteger(0);
            }
            else if (reqType == 2)
            {
                response.AppendInteger(theGroup.Requests.Count);

                if (theGroup.Requests.Count > 0 && list.Count - 1 >= page && members != null)
                {
                    response.AppendInteger(members.Count);

                    using (var enumerator = members.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;

                            response.AppendInteger(3);

                            if (current != null)
                            {
                                response.AppendInteger(current.Id);
                                response.AppendString(current.Name);
                                response.AppendString(current.Look);
                            }

                            response.AppendString(string.Empty);
                        }
                    }
                }
                else
                    response.AppendInteger(0);
            }

            response.AppendBool(session.GetHabbo().Id == theGroup.CreatorId);
            response.AppendInteger(14);
            response.AppendInteger(page);
            response.AppendInteger(reqType);
            response.AppendString(searchVal);

            return response;
        }

        /// <summary>
        ///     Gets the theGroup users by string.
        /// </summary>
        /// <param name="theGroup">The theGroup.</param>
        /// <param name="searchVal">The search value.</param>
        /// <param name="req">The req.</param>
        /// <returns>List&lt;GroupUser&gt;.</returns>
        internal List<GroupMember> GetGroupUsersByString(Guild theGroup, string searchVal, uint req)
        {
            List<GroupMember> list = null;

            switch (req)
            {
                case 0:
                    list = theGroup.Members.Values.ToList();
                    break;

                case 1:
                    list = theGroup.Admins.Values.ToList();
                    break;

                case 2:
                    list = GetGroupRequestsByString(theGroup, searchVal);
                    break;
            }

            if (list == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(searchVal))
                list = list.Where(member => member.Name.ToLower().Contains(searchVal.ToLower())).ToList();

            return list;
        }

        /// <summary>
        ///     Gets the theGroup requests by string.
        /// </summary>
        /// <param name="theGroup">The theGroup.</param>
        /// <param name="searchVal">The search value.</param>
        /// <returns>List&lt;System.UInt32&gt;.</returns>
        internal List<GroupMember> GetGroupRequestsByString(Guild theGroup, string searchVal) =>
            string.IsNullOrWhiteSpace(searchVal)
                ? theGroup.Requests.Values.ToList()
                : theGroup.Requests.Values.Where(request => request.Name.ToLower().Contains(searchVal.ToLower()))
                    .ToList();

        /// <summary>
        ///     Serializes the theGroup information.
        /// </summary>
        /// <param name="group">The theGroup.</param>
        /// <param name="response">The response.</param>
        /// <param name="session">The session.</param>
        /// <param name="newWindow">if set to <c>true</c> [new window].</param>
        internal async Task SerializeGroupInfo(Guild group, ServerMessage response, GameClient session,
            bool newWindow = false)
        {
            if (group == null || session == null)
                return;

            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var dateTime2 = dateTime.AddSeconds(group.CreateTime);

            await response.InitAsync(LibraryParser.OutgoingRequest("GroupDataMessageComposer"));

            var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(group.RoomId);
            await response.AppendIntegerAsync(group.Id);
            response.AppendBool(true);
            await response.AppendIntegerAsync(group.State);
            await response.AppendStringAsync(group.Name);
            await response.AppendStringAsync(group.Description);
            await response.AppendStringAsync(group.Badge);
            await response.AppendIntegerAsync(group.RoomId);
            await response.AppendStringAsync((roomData == null)
                ? "No room found.."
                : roomData.Name);
            await response.AppendIntegerAsync((group.CreatorId == session.GetHabbo().Id)
                ? 3
                : (group.Requests.ContainsKey(session.GetHabbo().Id)
                    ? 2
                    : (group.Members.ContainsKey(session.GetHabbo().Id) ? 1 : 0)));
            await response.AppendIntegerAsync(group.Members.Count);
            response.AppendBool(session.GetHabbo().FavouriteGroup == group.Id);
            await response.AppendStringAsync($"{dateTime2.Day:00}-{dateTime2.Month:00}-{dateTime2.Year}");
            response.AppendBool(group.CreatorId == session.GetHabbo().Id);
            response.AppendBool(group.Admins.ContainsKey(session.GetHabbo().Id));
            var habbo = Oblivion.GetHabboById(group.CreatorId);
            await response.AppendStringAsync((habbo == null)
                ? string.Empty
                : habbo.UserName);
            response.AppendBool(newWindow);
            response.AppendBool(group.AdminOnlyDeco == 0u);
            await response.AppendIntegerAsync(group.Requests.Count);
            response.AppendBool(group.HasForum);
            await session.SendMessage(response);
        }

        /// <summary>
        ///     Serializes the theGroup information.
        /// </summary>
        /// <param name="group">The theGroup.</param>
        /// <param name="response">The response.</param>
        /// <param name="session">The session.</param>
        /// <param name="room">The room.</param>
        /// <param name="newWindow">if set to <c>true</c> [new window].</param>
        internal async Task SerializeGroupInfo(Guild group, ServerMessage response, GameClient session, Room room,
            bool newWindow = false)
        {
            if (room == null || group == null)
                return;

            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var dateTime2 = dateTime.AddSeconds(group.CreateTime);

            await response.InitAsync(LibraryParser.OutgoingRequest("GroupDataMessageComposer"));
            var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(group.RoomId);

            await response.AppendIntegerAsync(group.Id);
            response.AppendBool(true);
            await response.AppendIntegerAsync(group.State);
            await response.AppendStringAsync(group.Name);
            await response.AppendStringAsync(group.Description);
            await response.AppendStringAsync(group.Badge);
            await response.AppendIntegerAsync(group.RoomId);
            await response.AppendStringAsync((roomData == null)
                ? "No room found.."
                : roomData.Name);
            await response.AppendIntegerAsync((group.CreatorId == session.GetHabbo().Id)
                ? 3
                : (group.Requests.ContainsKey(session.GetHabbo().Id)
                    ? 2
                    : (group.Members.ContainsKey(session.GetHabbo().Id) ? 1 : 0)));
            await response.AppendIntegerAsync(group.Members.Count);
            response.AppendBool(session.GetHabbo().FavouriteGroup == group.Id);
            await response.AppendStringAsync($"{dateTime2.Day:00}-{dateTime2.Month:00}-{dateTime2.Year}");
            response.AppendBool(group.CreatorId == session.GetHabbo().Id);
            response.AppendBool(group.Admins.ContainsKey(session.GetHabbo().Id));
            await response.AppendStringAsync((Oblivion.GetHabboById(group.CreatorId) == null)
                ? string.Empty
                : Oblivion.GetHabboById(group.CreatorId).UserName);
            response.AppendBool(newWindow);
            response.AppendBool(group.AdminOnlyDeco == 0u);
            await response.AppendIntegerAsync(group.Requests.Count);
            response.AppendBool(group.HasForum);
            await room.SendMessage(response);
        }

        /// <summary>
        ///     Generates the guild image.
        /// </summary>
        /// <param name="guildBase">The guild base.</param>
        /// <param name="guildBaseColor">Color of the guild base.</param>
        /// <param name="states">The states.</param>
        /// <returns>System.String.</returns>
        internal string GenerateGuildImage(int guildBase, int guildBaseColor, List<int> states)
        {
            var image = new StringBuilder($"b{guildBase:00}{guildBaseColor:00}");

            for (var i = 0; i < 3 * 4; i += 3)
                image.Append(i >= states.Count ? "s" : $"s{states[i]:00}{states[i + 1]:00}{states[i + 2]}");

            return image.ToString();
        }

        /// <summary>
        ///     Gets the theGroup colour.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="colour1">if set to <c>true</c> [colour1].</param>
        /// <returns>System.String.</returns>
        internal string GetGroupColour(int index, bool colour1)
        {
            if (colour1)
            {
                if (SymbolColours.Contains(index))
                    return ((GroupSymbolColours)SymbolColours[index]).Colour;

                return BackGroundColours.Contains(index)
                    ? ((GroupBackGroundColours)BackGroundColours[index]).Colour
                    : "4f8a00";
            }

            return BackGroundColours.Contains(index)
                ? ((GroupBackGroundColours)BackGroundColours[index]).Colour
                : "4f8a00";
        }

        /// <summary>
        ///     Deletes the theGroup.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal async Task DeleteGroup(uint id)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"DELETE FROM groups_data WHERE id = {id};");
                await queryReactor.RunQueryAsync();

                Groups.Remove(id);
            }
        }

        /// <summary>
        ///     Gets the message count for thread.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>System.Int32.</returns>
        internal int GetMessageCountForThread(uint id)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT COUNT(*) FROM groups_forums_posts WHERE parent_id='{id}'");
                return int.Parse(queryReactor.GetString());
            }
        }

        /// <summary>
        ///     Splits the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>List&lt;List&lt;GroupUser&gt;&gt;.</returns>
        private static List<List<GroupMember>> Split(IEnumerable<GroupMember> source)
        {
            return (from x in source.Select((x, i) => new { Index = i, Value = x })
                group x by x.Index / 14
                into x
                select (from v in x
                    select v.Value).ToList()).ToList();
        }
    }
}