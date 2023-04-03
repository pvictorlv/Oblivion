using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Users;
using Oblivion.Messages.Parsers;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        internal readonly ushort TotalPerPage = 20;

        /// <summary>
        /// Serializes the group purchase page.
        /// </summary>
        internal async Task SerializeGroupPurchasePage()
        {
            var list = new List<RoomData>();
            foreach (var x in Session.GetHabbo().Data.Rooms)
            {
                var current = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(x);
                if (current.Group == null) list.Add(current);
            }

            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupPurchasePageMessageComposer"));
            await Response.AppendIntegerAsync(10);
            await Response.AppendIntegerAsync(list.Count);

            /* TODO CHECK */
            foreach (RoomData current2 in list)
            {
                await Response.AppendIntegerAsync(current2.Id);
                await Response.AppendStringAsync(current2.Name);
                Response.AppendBool(false);
            }

            await Response.AppendIntegerAsync(5);
            await Response.AppendIntegerAsync(10);
            await Response.AppendIntegerAsync(3);
            await Response.AppendIntegerAsync(4);
            await Response.AppendIntegerAsync(25);
            await Response.AppendIntegerAsync(17);
            await Response.AppendIntegerAsync(5);
            await Response.AppendIntegerAsync(25);
            await Response.AppendIntegerAsync(17);
            await Response.AppendIntegerAsync(3);
            await Response.AppendIntegerAsync(29);
            await Response.AppendIntegerAsync(11);
            await Response.AppendIntegerAsync(4);
            await Response.AppendIntegerAsync(0);
            await Response.AppendIntegerAsync(0);
            await Response.AppendIntegerAsync(0);
            await SendResponse();
        }

        /// <summary>
        /// Serializes the group purchase parts.
        /// </summary>
        internal async Task SerializeGroupPurchaseParts()
        {
            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupPurchasePartsMessageComposer"));
            await Response.AppendIntegerAsync(Oblivion.GetGame().GetGroupManager().Bases.Count);
            /* TODO CHECK */
            foreach (GroupBases current in Oblivion.GetGame().GetGroupManager().Bases)
            {
                await Response.AppendIntegerAsync(current.Id);
                await Response.AppendStringAsync(current.Value1);
                await Response.AppendStringAsync(current.Value2);
            }

            await Response.AppendIntegerAsync(Oblivion.GetGame().GetGroupManager().Symbols.Count);
            /* TODO CHECK */
            foreach (GroupSymbols current2 in Oblivion.GetGame().GetGroupManager().Symbols)
            {
                await Response.AppendIntegerAsync(current2.Id);
                await Response.AppendStringAsync(current2.Value1);
                await Response.AppendStringAsync(current2.Value2);
            }

            await Response.AppendIntegerAsync(Oblivion.GetGame().GetGroupManager().BaseColours.Count);
            /* TODO CHECK */
            foreach (GroupBaseColours current3 in Oblivion.GetGame().GetGroupManager().BaseColours)
            {
                await Response.AppendIntegerAsync(current3.Id);
                await Response.AppendStringAsync(current3.Colour);
            }

            await Response.AppendIntegerAsync(Oblivion.GetGame().GetGroupManager().SymbolColours.Count);
            /* TODO CHECK */
            foreach (GroupSymbolColours current4 in Oblivion.GetGame().GetGroupManager().SymbolColours.Values)
            {
                await Response.AppendIntegerAsync(current4.Id);
                await Response.AppendStringAsync(current4.Colour);
            }

            await Response.AppendIntegerAsync(Oblivion.GetGame().GetGroupManager().BackGroundColours.Count);
            /* TODO CHECK */
            foreach (GroupBackGroundColours current5 in Oblivion.GetGame().GetGroupManager().BackGroundColours.Values)
            {
                await Response.AppendIntegerAsync(current5.Id);
                await Response.AppendStringAsync(current5.Colour);
            }

            await SendResponse();
        }

        /// <summary>
        /// Purchases the group.
        /// </summary>
        internal async Task PurchaseGroup()
        {
            if (Session == null || Session.GetHabbo().Credits < 10)
                return;

            var gStates = new List<int>();
            var name = Request.GetString();
            var description = Request.GetString();
            var roomid = Request.GetUInteger();
            var color = Request.GetInteger();
            var num3 = Request.GetInteger();

            Request.GetInteger();

            var guildBase = Request.GetInteger();
            var guildBaseColor = Request.GetInteger();
            var num6 = Request.GetInteger();
            var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomid);

            if (roomData.Owner != Session.GetHabbo().UserName)
                return;

            for (var i = 0; i < (num6 * 3); i++)
                gStates.Add(Request.GetInteger());

            var image = Oblivion.GetGame().GetGroupManager().GenerateGuildImage(guildBase, guildBaseColor, gStates);

            await Oblivion.GetGame().GetGroupManager().CreateGroup(name, description, roomid, image, Session,
                (!Oblivion.GetGame().GetGroupManager().SymbolColours.Contains(color)) ? 1 : color,
                (!Oblivion.GetGame().GetGroupManager().BackGroundColours.Contains(num3)) ? 1 : num3, out var theGroup);

            await Session.SendMessageAsync(CatalogPageComposer.PurchaseOk(0u, "CREATE_GUILD", 10));
            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupRoomMessageComposer"));
            await Response.AppendIntegerAsync(roomid);
            await Response.AppendIntegerAsync(theGroup.Id);
            await SendResponse();
            roomData.Group = theGroup;
            roomData.GroupId = theGroup.Id;
            await roomData.SerializeRoomData(Response, Session, true);

            if (!Session.GetHabbo().InRoom || Session.GetHabbo().CurrentRoom.RoomId != roomData.Id)
            {
                await Session.GetMessageHandler().PrepareRoomForUser(roomData.Id, roomData.PassWord);
                Session.GetHabbo().CurrentRoomId = roomData.Id;
            }

            if (Session.GetHabbo().CurrentRoom != null && !Session.GetHabbo().CurrentRoom.LoadedGroups
                    .ContainsKey(theGroup.Id))
                Session.GetHabbo().CurrentRoom.LoadedGroups.Add(theGroup.Id, theGroup.Badge);

            if (CurrentLoadingRoom != null && !CurrentLoadingRoom.LoadedGroups.ContainsKey(theGroup.Id))
                CurrentLoadingRoom.LoadedGroups.Add(theGroup.Id, theGroup.Badge);

            if (CurrentLoadingRoom != null)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));

                await serverMessage.AppendIntegerAsync(CurrentLoadingRoom.LoadedGroups.Count);

                /* TODO CHECK */
                foreach (var current in CurrentLoadingRoom.LoadedGroups)
                {
                    await serverMessage.AppendIntegerAsync(current.Key);
                    await serverMessage.AppendStringAsync(current.Value);
                }

                await CurrentLoadingRoom.SendMessage(serverMessage);
            }

            if (CurrentLoadingRoom == null || Session.GetHabbo().FavouriteGroup != theGroup.Id)
                return;

            var serverMessage2 =
                new ServerMessage(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));

            await serverMessage2.AppendIntegerAsync(CurrentLoadingRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
            await serverMessage2.AppendIntegerAsync(theGroup.Id);
            await serverMessage2.AppendIntegerAsync(3);
            await serverMessage2.AppendStringAsync(theGroup.Name);

            await CurrentLoadingRoom.SendMessage(serverMessage2);
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        internal async Task SerializeGroupInfo()
        {
            uint groupId = Request.GetUInteger();
            bool newWindow = Request.GetBool();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (group == null)
                return;

            Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, newWindow);
        }

        /// <summary>
        /// Serializes the group members.
        /// </summary>
        internal async Task SerializeGroupMembers()
        {
            uint groupId = Request.GetUInteger();
            int page = Request.GetInteger();
            string searchVal = Request.GetString();
            uint reqType = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (group == null) return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));

            Oblivion.GetGame().GetGroupManager()
                .SerializeGroupMembers(Response, group, reqType, Session, searchVal, page);

            await SendResponse();
        }

        /// <summary>
        /// Makes the group admin.
        /// </summary>
        internal async Task MakeGroupAdmin()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(num);

            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) ||
                group.Admins.ContainsKey(num2))
                return;

            group.Members[num2].Rank = 1;

            group.Admins.Add(num2, group.Members[num2]);

            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 1u, Session);

            await SendResponse();

            Room room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);

            RoomUser roomUserByHabbo = room?.GetRoomUserManager()
                .GetRoomUserByHabbo(Oblivion.GetHabboById(num2).UserName);

            if (roomUserByHabbo != null)
            {
//                if (!roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
                roomUserByHabbo.AddStatus("flatctrl 1", "");

                await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                await Response.AppendIntegerAsync(1);
                await roomUserByHabbo.GetClient().SendMessageAsync(GetResponse());
                roomUserByHabbo.UpdateNeeded = true;
            }

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(string.Concat("UPDATE groups_members SET rank='1' WHERE group_id=", num,
                    " AND user_id=", num2, " LIMIT 1;"));
        }

        /// <summary>
        /// Removes the group admin.
        /// </summary>
        internal async Task RemoveGroupAdmin()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(num);

            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) ||
                !group.Admins.ContainsKey(num2))
                return;

            group.Members[num2].Rank = 0;
            group.Admins.Remove(num2);

            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            await SendResponse();

            Room room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);
            RoomUser roomUserByHabbo = room?.GetRoomUserManager()
                .GetRoomUserByHabbo(Oblivion.GetHabboById(num2).UserName);

            if (roomUserByHabbo != null)
            {
//                if (roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
                roomUserByHabbo.RemoveStatus("flatctrl 1");

                await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                await Response.AppendIntegerAsync(0);
                await roomUserByHabbo.GetClient().SendMessageAsync(GetResponse());
                roomUserByHabbo.UpdateNeeded = true;
            }

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(string.Concat("UPDATE groups_members SET rank='0' WHERE group_id=", num,
                    " AND user_id=", num2, " LIMIT 1;"));
        }

        /// <summary>
        /// Accepts the membership.
        /// </summary>
        internal async Task AcceptMembership()
        {
            uint groupId = Request.GetUInteger();
            uint userId = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(Session.GetHabbo().Id))
                return;

            if (!group.Requests.TryGetValue(userId, out var member))
                return;

            if (group.Members.ContainsKey(userId))
            {
                group.Requests.Remove(userId);

                using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    await queryReactor.RunFastQueryAsync(
                        $"DELETE FROM groups_requests WHERE group_id = '{groupId}' AND user_id = '{userId}' LIMIT 1");
                return;
            }


            member.DateJoin = Oblivion.GetUnixTimeStamp();
            group.Members.Add(userId, member);
            group.Requests.Remove(userId);
            group.Admins.Add(userId, member);

            Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            await SendResponse();

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(
                    $"DELETE FROM groups_requests WHERE group_id = '{groupId}' AND user_id = '{userId}' LIMIT 1");

            using (IQueryAdapter queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryreactor2.RunFastQueryAsync(
                    $"INSERT INTO groups_members (group_id, user_id, rank, date_join) VALUES ('{groupId}','{userId}','0','{Oblivion.GetUnixTimeStamp()}')");
        }

        /// <summary>
        /// Declines the membership.
        /// </summary>
        internal async Task DeclineMembership()
        {
            var groupId = Request.GetUInteger();
            var userId = Request.GetUInteger();

            var group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(Session.GetHabbo().Id) &&
                !group.Requests.ContainsKey(userId))
                return;

            group.Requests.Remove(userId);

            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 2u, Session);
            await SendResponse();

            var room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);

            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Oblivion.GetHabboById(userId).UserName);

            if (roomUserByHabbo != null)
            {
//                if (roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
                roomUserByHabbo.RemoveStatus("flatctrl 1");

                roomUserByHabbo.UpdateNeeded = true;
            }

            Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync("DELETE FROM groups_requests WHERE group_id=" + groupId + " AND user_id=" +
                                                     userId);
        }

        /// <summary>
        /// Joins the group.
        /// </summary>
        internal async Task JoinGroup()
        {
            uint groupId = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);
            Habbo user = Session?.GetHabbo();
            if (user == null) return;

            if (!group.Members.ContainsKey(user.Id))
            {
                if (group.State == 0)
                {
                    using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        await queryReactor.RunFastQueryAsync(
                            string.Concat("INSERT INTO groups_members (user_id, group_id, date_join) VALUES (", user.Id,
                                ",", groupId, ",", Oblivion.GetUnixTimeStamp(), ")"));
                        await queryReactor.RunFastQueryAsync(string.Concat("UPDATE users_stats SET favourite_group=", groupId,
                            " WHERE id= ", user.Id, " LIMIT 1"));
                    }

                    var member = new GroupMember(user.Id, user.UserName, user.Look, group.Id, 0,
                        Oblivion.GetUnixTimeStamp(),
                        true);
                    group.Members.Add(user.Id, member);
                    user.UserGroups.Add(member);
                    user.GetMessenger().SerializeUpdate(group);
                }
                else
                {
                    if (!group.Requests.ContainsKey(user.Id))
                    {
                        using (IQueryAdapter queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                            await queryreactor2.RunFastQueryAsync(
                                string.Concat("INSERT INTO groups_requests (user_id, group_id) VALUES (",
                                    Session.GetHabbo().Id, ",", groupId, ")"));

                        var groupRequest = new GroupMember(user.Id, user.UserName, user.Look, group.Id, 0,
                            Oblivion.GetUnixTimeStamp(), true);

                        group.Requests.Add(user.Id, groupRequest);
                    }
                }

                Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            }
        }

        /// <summary>
        /// Removes the member.
        /// </summary>
        internal async Task RemoveMember()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(num);

            if (num2 == Session.GetHabbo().Id)
            {
                if (group.Members.ContainsKey(num2))
                    group.Members.Remove(num2);

                if (group.Admins.ContainsKey(num2))
                    group.Admins.Remove(num2);

                using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    await queryReactor.RunFastQueryAsync(string.Concat("DELETE FROM groups_members WHERE user_id=", num2,
                        " AND group_id=", num, " LIMIT 1"));

                await Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);

                if (Session.GetHabbo().FavouriteGroup == num)
                {
                    Session.GetHabbo().FavouriteGroup = 0u;

                    using (IQueryAdapter queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                        await queryreactor2.RunFastQueryAsync($"UPDATE users_stats SET favourite_group=0 WHERE id={num2} LIMIT 1");

                    await Response.InitAsync(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
                    await Response.AppendIntegerAsync(Session.GetHabbo().Id);
                    await Session.GetHabbo().CurrentRoom.SendMessage(Response);
                    await Response.InitAsync(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
                    await Response.AppendIntegerAsync(0);
                    await Response.AppendIntegerAsync(-1);
                    await Response.AppendIntegerAsync(-1);
                    await Response.AppendStringAsync("");
                    await Session.GetHabbo().CurrentRoom.SendMessage(Response);

                    if (group.AdminOnlyDeco == 0u)
                    {
                        RoomUser roomUserByHabbo = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId)
                            .GetRoomUserManager().GetRoomUserByHabbo(Oblivion.GetHabboById(num2).UserName);

                        if (roomUserByHabbo == null)
                            return;

                        roomUserByHabbo.RemoveStatus("flatctrl 1");
                        await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));

                        await Response.AppendIntegerAsync(0);

                        await roomUserByHabbo.GetClient().SendMessageAsync(GetResponse());
                    }
                }

                return;
            }

            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2))
                return;

            group.Members.Remove(num2);

            if (group.Admins.ContainsKey(num2))
                group.Admins.Remove(num2);

            await Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            await SendResponse();

            using (IQueryAdapter queryreactor3 = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryreactor3.RunFastQueryAsync(string.Concat("DELETE FROM groups_members WHERE group_id=", num,
                    " AND user_id=", num2, " LIMIT 1;"));
        }

        /// <summary>
        /// Makes the fav.
        /// </summary>
        internal async Task MakeFav()
        {
            uint groupId = Request.GetUInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup == null)
                return;

            if (!theGroup.Members.ContainsKey(Session.GetHabbo().Id))
                return;

            Session.GetHabbo().FavouriteGroup = theGroup.Id;
            await Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(theGroup, Response, Session);

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(string.Concat("UPDATE users_stats SET favourite_group =", theGroup.Id,
                    " WHERE id=", Session.GetHabbo().Id, " LIMIT 1;"));

            await Response.InitAsync(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
            await Response.AppendIntegerAsync(Session.GetHabbo().Id);
            await Session.SendMessageAsync(Response);

            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (!Session.GetHabbo().CurrentRoom.LoadedGroups.ContainsKey(theGroup.Id))
                {
                    Session.GetHabbo().CurrentRoom.LoadedGroups.Add(theGroup.Id, theGroup.Badge);
                    await Response.InitAsync(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                    await Response.AppendIntegerAsync(Session.GetHabbo().CurrentRoom.LoadedGroups.Count);

                    /* TODO CHECK */
                    foreach (KeyValuePair<uint, string> current in Session.GetHabbo().CurrentRoom.LoadedGroups)
                    {
                        await Response.AppendIntegerAsync(current.Key);
                        await Response.AppendStringAsync(current.Value);
                    }

                    await Session.GetHabbo().CurrentRoom.SendMessage(Response);
                }
            }

            await Response.InitAsync(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
            await Response.AppendIntegerAsync(0);
            await Response.AppendIntegerAsync(theGroup.Id);
            await Response.AppendIntegerAsync(3);
            await Response.AppendStringAsync(theGroup.Name);

            await Session.SendMessageAsync(Response);
        }

        /// <summary>
        /// Removes the fav.
        /// </summary>
        internal async Task RemoveFav()
        {
            Request.GetUInteger();
            Session.GetHabbo().FavouriteGroup = 0u;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(
                    $"UPDATE users_stats SET favourite_group=0 WHERE id={Session.GetHabbo().Id} LIMIT 1;");

            await Response.InitAsync(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
            await Response.AppendIntegerAsync(Session.GetHabbo().Id);
            await Session.SendMessageAsync(Response);
            await Response.InitAsync(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
            await Response.AppendIntegerAsync(0);
            await Response.AppendIntegerAsync(-1);
            await Response.AppendIntegerAsync(-1);
            await Response.AppendStringAsync(string.Empty);

            await Session.SendMessageAsync(Response);
        }

        /// <summary>
        /// Publishes the forum thread.
        /// </summary>
        internal async Task PublishForumThread()
        {
            if ((Oblivion.GetUnixTimeStamp() - Session.GetHabbo().LastSqlQuery) < 20)
                return;

            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            string subject = Request.GetString();
            string content = Request.GetString();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (group == null || !group.HasForum)
                return;

            int timestamp = Oblivion.GetUnixTimeStamp();

            using (IQueryAdapter dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                if (threadId != 0)
                {
                    dbClient.SetQuery($"SELECT locked,hidden FROM groups_forums_posts WHERE id = {threadId}");

                    DataRow row = dbClient.GetRow();
                    if (row["locked"].ToString() == "1" || row["hidden"].ToString() == "1")
                    {
                        await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("forums_cancel"));
                        return;
                    }
                }

                Session.GetHabbo().LastSqlQuery = Oblivion.GetUnixTimeStamp();
                dbClient.SetQuery(
                    "INSERT INTO groups_forums_posts (group_id, parent_id, timestamp, poster_id, subject, post_content) VALUES (@gid, @pard, @ts, @pid, @pnm, @plk, @subjc, @content)");
                dbClient.AddParameter("gid", groupId);
                dbClient.AddParameter("pard", threadId);
                dbClient.AddParameter("ts", timestamp);
                dbClient.AddParameter("pid", Session.GetHabbo().Id);
                dbClient.AddParameter("subjc", subject);
                dbClient.AddParameter("content", content);

                threadId = (uint) dbClient.GetInteger();
            }

            group.ForumScore += 0.25;
            group.ForumLastPosterName = Session.GetHabbo().UserName;
            group.ForumLastPosterId = Session.GetHabbo().Id;
            group.ForumLastPosterTimestamp = timestamp;
            group.ForumMessagesCount++;
            group.UpdateForum();

            if (threadId == 0)
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumNewThreadMessageComposer"));
                await message.AppendIntegerAsync(groupId);
                await message.AppendIntegerAsync(threadId);
                await message.AppendIntegerAsync(Session.GetHabbo().Id);
                await message.AppendStringAsync(subject);
                await message.AppendStringAsync(content);
                message.AppendBool(false);
                message.AppendBool(false);
                await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - timestamp));
                await message.AppendIntegerAsync(1);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(1);
                await message.AppendStringAsync("");
                await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - timestamp));
                message.AppendByte(1);
                await message.AppendIntegerAsync(1);
                await message.AppendStringAsync("");
                await message.AppendIntegerAsync(42);
                await Session.SendMessageAsync(message);
            }
            else
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumNewResponseMessageComposer"));
                await message.AppendIntegerAsync(groupId);
                await message.AppendIntegerAsync(threadId);
                await message.AppendIntegerAsync(group.ForumMessagesCount);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(Session.GetHabbo().Id);
                await message.AppendStringAsync(Session.GetHabbo().UserName);
                await message.AppendStringAsync(Session.GetHabbo().Look);
                await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - timestamp));
                await message.AppendStringAsync(content);
                message.AppendByte(0);
                await message.AppendIntegerAsync(0);
                await message.AppendStringAsync("");
                await message.AppendIntegerAsync(0);
                await Session.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Updates the state of the thread.
        /// </summary>
        internal async Task UpdateThreadState()
        {
            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            bool pin = Request.GetBool();
            bool Lock = Request.GetBool();

            using (IQueryAdapter dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    $"SELECT * FROM groups_forums_posts WHERE group_id = '{groupId}' AND id = '{threadId}' LIMIT 1;");
                DataRow row = dbClient.GetRow();

                Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

                if (row != null)
                {
                    if ((uint) row["poster_id"] == Session.GetHabbo().Id ||
                        theGroup.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.SetQuery(
                            $"UPDATE groups_forums_posts SET pinned = @pin , locked = @lock WHERE id = {threadId};");
                        dbClient.AddParameter("pin", (pin) ? "1" : "0");
                        dbClient.AddParameter("lock", (Lock) ? "1" : "0");
                        await dbClient.RunQueryAsync();
                    }
                }

                var thread = new GroupForumPost(row);

                if (thread.Pinned != pin)
                {
                    var notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

                    await notif.AppendStringAsync((pin) ? "forums.thread.pinned" : "forums.thread.unpinned");
                    await notif.AppendIntegerAsync(0);
                    await Session.SendMessageAsync(notif);
                }

                if (thread.Locked != Lock)
                {
                    var notif2 = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

                    await notif2.AppendStringAsync((Lock) ? "forums.thread.locked" : "forums.thread.unlocked");
                    await notif2.AppendIntegerAsync(0);
                    await Session.SendMessageAsync(notif2);
                }

                if (thread.ParentId != 0)
                    return;

                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                await message.AppendIntegerAsync(groupId);
                await message.AppendIntegerAsync(thread.Id);
                await message.AppendIntegerAsync(thread.PosterId);
                await message.AppendStringAsync(thread.PosterName);
                await message.AppendStringAsync(thread.Subject);
                message.AppendBool(pin);
                message.AppendBool(Lock);
                await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                await message.AppendIntegerAsync(thread.MessageCount + 1);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(1);
                await message.AppendStringAsync("");
                await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendByte((thread.Hidden) ? 10 : 1);
                await message.AppendIntegerAsync(1);
                await message.AppendStringAsync(thread.Hider);
                await message.AppendIntegerAsync(0);

                await Session.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Alters the state of the forum thread.
        /// </summary>
        internal async Task AlterForumThreadState()
        {
            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            int stateToSet = Request.GetInteger();

            using (IQueryAdapter dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    $"SELECT * FROM groups_forums_posts WHERE group_id = '{groupId}' AND id = '{threadId}' LIMIT 1;");

                DataRow row = dbClient.GetRow();
                Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

                if (row != null)
                {
                    if ((uint) row["poster_id"] == Session.GetHabbo().Id ||
                        theGroup.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.SetQuery($"UPDATE groups_forums_posts SET hidden = @hid WHERE id = {threadId};");
                        dbClient.AddParameter("hid", (stateToSet == 20) ? "1" : "0");
                        await dbClient.RunQueryAsync();
                    }
                }

                var thread = new GroupForumPost(row);
                var notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

                await notif.AppendStringAsync((stateToSet == 20) ? "forums.thread.hidden" : "forums.thread.restored");
                await notif.AppendIntegerAsync(0);
                await Session.SendMessageAsync(notif);

                if (thread.ParentId != 0)
                    return;

                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                await message.AppendIntegerAsync(groupId);
                await message.AppendIntegerAsync(thread.Id);
                await message.AppendIntegerAsync(thread.PosterId);
                await message.AppendStringAsync(thread.PosterName);
                await message.AppendStringAsync(thread.Subject);
                message.AppendBool(thread.Pinned);
                message.AppendBool(thread.Locked);
                await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                await message.AppendIntegerAsync(thread.MessageCount + 1);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(0);
                await message.AppendStringAsync(string.Empty);
                await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendByte(stateToSet);
                await message.AppendIntegerAsync(0);
                await message.AppendStringAsync(thread.Hider);
                await message.AppendIntegerAsync(0);

                await Session.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Reads the forum thread.
        /// </summary>
        internal async Task ReadForumThread()
        {
            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            int startIndex = Request.GetInteger();

            Request.GetInteger();

            var theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup == null || !theGroup.HasForum)
                return;

            using (IQueryAdapter dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    $"SELECT * FROM groups_forums_posts WHERE group_id = '{groupId}' AND parent_id = '{threadId}' OR id = '{threadId}' ORDER BY timestamp ASC;");

                DataTable table = dbClient.GetTable();

                if (table == null)
                    return;

                int b = table.Rows.Count <= 20 ? table.Rows.Count : 20;

                List<GroupForumPost> posts = new List<GroupForumPost>();

                int i = 1;

                while (i <= b)
                {
                    DataRow row = table.Rows[i - 1];

                    if (row == null)
                    {
                        b--;
                        continue;
                    }

                    GroupForumPost thread = new GroupForumPost(row);

                    if (thread.ParentId == 0 && thread.Hidden)
                        return;

                    posts.Add(thread);

                    i++;
                }

                var messageBuffer =
                    new ServerMessage(LibraryParser.OutgoingRequest("GroupForumReadThreadMessageComposer"));

                await messageBuffer.AppendIntegerAsync(groupId);
                await messageBuffer.AppendIntegerAsync(threadId);
                await messageBuffer.AppendIntegerAsync(startIndex);
                await messageBuffer.AppendIntegerAsync(b);

                int indx = 0;

                /* TODO CHECK */
                foreach (GroupForumPost post in posts)
                {
                    await messageBuffer.AppendIntegerAsync(indx++ - 1);
                    await messageBuffer.AppendIntegerAsync(indx - 1);
                    await messageBuffer.AppendIntegerAsync(post.PosterId);
                    await messageBuffer.AppendStringAsync(post.PosterName);
                    await messageBuffer.AppendStringAsync(post.PosterLook);
                    await messageBuffer.AppendIntegerAsync(Oblivion.GetUnixTimeStamp() - post.Timestamp);
                    await messageBuffer.AppendStringAsync(post.PostContent);
                    messageBuffer.AppendByte(0);
                    await messageBuffer.AppendIntegerAsync(0);
                    await messageBuffer.AppendStringAsync(post.Hider);
                    await messageBuffer.AppendIntegerAsync(0);
                    await messageBuffer.AppendIntegerAsync(0);
                }

                await Session.SendMessageAsync(messageBuffer);
            }
        }


        /// <summary>
        /// Gets the group forum thread root.
        /// </summary>
        internal async Task GetGroupForumThreadRoot()
        {
            uint groupId = Request.GetUInteger();
            int startIndex = Request.GetInteger();

            using (IQueryAdapter dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    $"SELECT count(id) FROM groups_forums_posts WHERE group_id = '{groupId}' AND parent_id = 0");

                dbClient.GetInteger();

                dbClient.SetQuery(
                    $"SELECT * FROM groups_forums_posts WHERE group_id = '{groupId}' AND parent_id = 0 ORDER BY timestamp DESC, pinned DESC LIMIT @startIndex, @totalPerPage;");

                dbClient.AddParameter("startIndex", startIndex);
                dbClient.AddParameter("totalPerPage", TotalPerPage);

                DataTable table = dbClient.GetTable();
                int threadCount = (table.Rows.Count <= TotalPerPage) ? table.Rows.Count : TotalPerPage;

                var threads = (from DataRow row in table.Rows select new GroupForumPost(row)).ToList();

                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadRootMessageComposer"));
                await message.AppendIntegerAsync(groupId);
                await message.AppendIntegerAsync(startIndex);
                await message.AppendIntegerAsync(threadCount);

                /* TODO CHECK */
                foreach (GroupForumPost thread in threads)
                {
                    await message.AppendIntegerAsync(thread.Id);
                    await message.AppendIntegerAsync(thread.PosterId);
                    await message.AppendStringAsync(thread.PosterName);
                    await message.AppendStringAsync(thread.Subject);
                    message.AppendBool(thread.Pinned);
                    message.AppendBool(thread.Locked);
                    await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                    await message.AppendIntegerAsync(thread.MessageCount + 1);
                    await message.AppendIntegerAsync(0);
                    await message.AppendIntegerAsync(0);
                    await message.AppendIntegerAsync(0);
                    await message.AppendStringAsync(string.Empty);
                    await message.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                    message.AppendByte((thread.Hidden) ? 10 : 1);
                    await message.AppendIntegerAsync(0);
                    await message.AppendStringAsync(thread.Hider);
                    await message.AppendIntegerAsync(0);
                }

                await Session.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Gets the group forum data.
        /// </summary>
        internal async Task GetGroupForumData()
        {
            uint groupId = Request.GetUInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup != null && theGroup.HasForum)
                await Session.SendMessageAsync(theGroup.ForumDataMessage(Session.GetHabbo().Id));
        }

        /// <summary>
        /// Gets the group forums.
        /// </summary>
        internal async Task GetGroupForums()
        {
            int selectType = Request.GetInteger();
            int startIndex = Request.GetInteger();

            var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumListingsMessageComposer"));
            await message.AppendIntegerAsync(selectType);
            var groupList = new List<Guild>();

            switch (selectType)
            {
                case 0:
                case 1:
                    using (IQueryAdapter dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery(
                            "SELECT count(id) FROM groups_data WHERE has_forum = '1' AND forum_Messages_count > 0");

                        int qtdForums = dbClient.GetInteger();

                        dbClient.SetQuery(
                            "SELECT id FROM groups_data WHERE has_forum = '1' AND forum_Messages_count > 0 ORDER BY forum_Messages_count DESC LIMIT @startIndex, @totalPerPage;");

                        dbClient.AddParameter("startIndex", startIndex);
                        dbClient.AddParameter("totalPerPage", TotalPerPage);

                        DataTable table = dbClient.GetTable();

                        await message.AppendIntegerAsync(qtdForums == 0 ? 1 : qtdForums);
                        await message.AppendIntegerAsync(startIndex);

                        groupList.AddRange(from DataRow rowGroupData in table.Rows
                            select uint.Parse(rowGroupData["id"].ToString())
                            into groupId
                            select Oblivion.GetGame().GetGroupManager().GetGroup(groupId));

                        await message.AppendIntegerAsync(table.Rows.Count);

                        /* TODO CHECK */
                        foreach (Guild theGroup in groupList)
                            theGroup.SerializeForumRoot(message);

                        await Session.SendMessageAsync(message);
                    }

                    break;

                case 2:
                    groupList.AddRange(Session.GetHabbo().UserGroups
                        .Select(groupUser => Oblivion.GetGame().GetGroupManager().GetGroup(groupUser.GroupId))
                        .Where(aGroup => aGroup != null && aGroup.HasForum));

                    await message.AppendIntegerAsync(groupList.Count == 0 ? 1 : groupList.Count);

                    groupList = groupList.OrderByDescending(x => x.ForumMessagesCount).Skip(startIndex).Take(20)
                        .ToList();

                    await message.AppendIntegerAsync(startIndex);
                    await message.AppendIntegerAsync(groupList.Count);

                    /* TODO CHECK */
                    foreach (Guild theGroup in groupList)
                        theGroup.SerializeForumRoot(message);

                    await Session.SendMessageAsync(message);
                    break;

                default:
                    await message.AppendIntegerAsync(1);
                    await message.AppendIntegerAsync(startIndex);
                    await message.AppendIntegerAsync(0);
                    await Session.SendMessageAsync(message);
                    break;
            }
        }

        /// <summary>
        /// Manages the group.
        /// </summary>
        internal async Task ManageGroup()
        {
            var groupId = Request.GetUInteger();
            var theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup == null)
                return;

            if (!theGroup.Admins.ContainsKey(Session.GetHabbo().Id) && theGroup.CreatorId != Session.GetHabbo().Id &&
                Session.GetHabbo().Rank < 7)
                return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupDataEditMessageComposer"));
            await Response.AppendIntegerAsync(0);
            Response.AppendBool(true);
            await Response.AppendIntegerAsync(theGroup.Id);
            await Response.AppendStringAsync(theGroup.Name);
            await Response.AppendStringAsync(theGroup.Description);
            await Response.AppendIntegerAsync(theGroup.RoomId);
            await Response.AppendIntegerAsync(theGroup.Colour1);
            await Response.AppendIntegerAsync(theGroup.Colour2);
            await Response.AppendIntegerAsync(theGroup.State);
            await Response.AppendIntegerAsync(theGroup.AdminOnlyDeco);
            Response.AppendBool(false);
            await Response.AppendStringAsync(string.Empty);

            var array = theGroup.Badge.Replace("b", string.Empty).Split('s');

            await Response.AppendIntegerAsync(5);

            var num = (5 - array.Length);

            var num2 = 0;
            var array2 = array;

            /* TODO CHECK */

            foreach (var text in array2)
            {
                try
                {
                    await Response.AppendIntegerAsync(text.Length >= 6
                        ? uint.Parse(text.Substring(0, 3))
                        : uint.Parse(text.Substring(0, 2)));

                    await Response.AppendIntegerAsync((text.Length >= 6)
                        ? uint.Parse(text.Substring(3, 2))
                        : uint.Parse(text.Substring(2, 2)));

                    if (text.Length < 5)
                        await Response.AppendIntegerAsync(0);
                    else if (text.Length >= 6)
                        await Response.AppendIntegerAsync(uint.Parse(text.Substring(5, 1)));
                    else
                        await Response.AppendIntegerAsync(uint.Parse(text.Substring(4, 1)));
                }
                catch
                {
                    //ignored
                }
            }


            while (num2 != num)
            {
                await Response.AppendIntegerAsync(0);
                await Response.AppendIntegerAsync(0);
                await Response.AppendIntegerAsync(0);
                num2++;
            }

            await Response.AppendStringAsync(theGroup.Badge);
            await Response.AppendIntegerAsync(theGroup.Members.Count);

            await SendResponse();
        }

        /// <summary>
        /// Updates the name of the group.
        /// </summary>
        internal async Task UpdateGroupName()
        {
            uint num = Request.GetUInteger();
            string text = Request.GetString();
            string text2 = Request.GetString();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(num);

            if (theGroup?.CreatorId != Session.GetHabbo().Id)
                return;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"UPDATE groups_data SET `name`=@name, `desc`=@desc WHERE id={num} LIMIT 1");
                queryReactor.AddParameter("name", text);
                queryReactor.AddParameter("desc", text2);

                await queryReactor.RunQueryAsync();
            }

            theGroup.Name = text;
            theGroup.Description = text2;

            Oblivion.GetGame().GetGroupManager()
                .SerializeGroupInfo(theGroup, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Updates the group badge.
        /// </summary>
        internal async Task UpdateGroupBadge()
        {
            uint guildId = Request.GetUInteger();

            Guild guild = Oblivion.GetGame().GetGroupManager().GetGroup(guildId);

            if (guild != null)
            {
                Room room = Oblivion.GetGame().GetRoomManager().GetRoom(guild.RoomId);

                if (room != null)
                {
                    Request.GetInteger();

                    int Base = Request.GetInteger();
                    int baseColor = Request.GetInteger();

                    Request.GetInteger();

                    var guildStates = new List<int>();

                    for (int i = 0; i < 12; i++)
                        guildStates.Add(Request.GetInteger());

                    string badge = Oblivion.GetGame().GetGroupManager()
                        .GenerateGuildImage(Base, baseColor, guildStates);

                    guild.Badge = badge;

                    await Response.InitAsync(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                    await Response.AppendIntegerAsync(room.LoadedGroups.Count);

                    foreach (KeyValuePair<uint, string> current2 in room.LoadedGroups)
                    {
                        await Response.AppendIntegerAsync(current2.Key);
                        await Response.AppendStringAsync(current2.Value);
                    }

                    await room.SendMessage(Response);

                    Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(guild, Response, Session, room);

                    using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery($"UPDATE groups_data SET badge = @badgi WHERE id = {guildId}");
                        queryReactor.AddParameter("badgi", badge);
                        await queryReactor.RunQueryAsync();
                    }

                    if (Session.GetHabbo().CurrentRoom != null)
                    {
                        Session.GetHabbo().CurrentRoom.LoadedGroups[guildId] = guild.Badge;

                        await Response.InitAsync(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                        await Response.AppendIntegerAsync(Session.GetHabbo().CurrentRoom.LoadedGroups.Count);

                        /* TODO CHECK */
                        foreach (KeyValuePair<uint, string> current in Session.GetHabbo().CurrentRoom.LoadedGroups)
                        {
                            await Response.AppendIntegerAsync(current.Key);
                            await Response.AppendStringAsync(current.Value);
                        }

                        Session.GetHabbo().CurrentRoom.SendMessage(Response);

                        Oblivion.GetGame().GetGroupManager()
                            .SerializeGroupInfo(guild, Response, Session, Session.GetHabbo().CurrentRoom);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the group colours.
        /// </summary>
        internal async Task UpdateGroupColours()
        {
            uint groupId = Request.GetUInteger();
            int num = Request.GetInteger();
            int num2 = Request.GetInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup?.CreatorId != Session.GetHabbo().Id)
                return;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(string.Concat("UPDATE groups_data SET colour1= ", num, ", colour2=", num2,
                    " WHERE id=", theGroup.Id, " LIMIT 1"));

            theGroup.Colour1 = num;
            theGroup.Colour2 = num2;

            Oblivion.GetGame().GetGroupManager()
                .SerializeGroupInfo(theGroup, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Updates the group settings.
        /// </summary>
        internal async Task UpdateGroupSettings()
        {
            uint groupId = Request.GetUInteger();
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup?.CreatorId != Session.GetHabbo().Id)
                return;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(string.Concat("UPDATE groups_data SET state='", num, "', admindeco='", num2,
                    "' WHERE id=", theGroup.Id, " LIMIT 1"));

            theGroup.State = num;
            theGroup.AdminOnlyDeco = num2;

            Room room = Oblivion.GetGame().GetRoomManager().GetRoom(theGroup.RoomId);

            if (room != null)
            {
                /* TODO CHECK */
                foreach (RoomUser current in room.GetRoomUserManager().GetRoomUsers())
                {
                    if (room.RoomData.OwnerId != current.UserId && !theGroup.Admins.ContainsKey(current.UserId) &&
                        theGroup.Members.ContainsKey(current.UserId))
                    {
                        if (num2 == 1u)
                        {
                            current.RemoveStatus("flatctrl 1");
                            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                            await Response.AppendIntegerAsync(0);
                            await current.GetClient().SendMessageAsync(GetResponse());
                        }
                        else
                        {
                            current.AddStatus("flatctrl 1", "");
                            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                            await Response.AppendIntegerAsync(1);
                            await current.GetClient().SendMessageAsync(GetResponse());
                        }

                        current.UpdateNeeded = true;
                    }
                }
            }

            await Oblivion.GetGame().GetGroupManager()
                .SerializeGroupInfo(theGroup, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Requests the leave group.
        /// </summary>
        internal async Task RequestLeaveGroup()
        {
            uint groupId = Request.GetUInteger();
            uint userId = Request.GetUInteger();

            Guild guild = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (guild == null || guild.CreatorId == userId)
                return;

            if (userId == Session.GetHabbo().Id || guild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("GroupAreYouSureMessageComposer"));
                await Response.AppendIntegerAsync(userId);
                await Response.AppendIntegerAsync(0);
                await SendResponse();
            }
        }

        /// <summary>
        /// Confirms the leave group.
        /// </summary>
        internal async Task ConfirmLeaveGroup()
        {
            uint guild = Request.GetUInteger();
            uint userId = Request.GetUInteger();

            Guild byeGuild = Oblivion.GetGame().GetGroupManager().GetGroup(guild);

            if (byeGuild == null)
                return;

            if (byeGuild.CreatorId == userId)
            {
                await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("user_room_video_true"));
                return;
            }

            if (userId == Session.GetHabbo().Id || byeGuild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                int type;

                if (byeGuild.Members.TryGetValue(userId, out var memberShip))
                {
                    type = 3;
                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Members.Remove(userId);
                }
                else if (byeGuild.Admins.TryGetValue(userId, out memberShip))
                {
                    type = 1;
                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Admins.Remove(userId);
                }
                else
                    return;

                using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    await queryReactor.RunFastQueryAsync(string.Concat("DELETE FROM groups_members WHERE user_id=", userId,
                        " AND group_id=", guild, " LIMIT 1"));

                Habbo byeUser = Oblivion.GetHabboById(userId);

                if (byeUser != null)
                {
                    await Response.InitAsync(LibraryParser.OutgoingRequest("GroupConfirmLeaveMessageComposer"));
                    await Response.AppendIntegerAsync(guild);
                    await Response.AppendIntegerAsync(type);
                    await Response.AppendIntegerAsync(byeUser.Id);
                    await Response.AppendStringAsync(byeUser.UserName);
                    await Response.AppendStringAsync(byeUser.Look);
                    await Response.AppendStringAsync(string.Empty);

                    await SendResponse();
                }

                if (byeUser != null && byeUser.FavouriteGroup == guild)
                {
                    byeUser.FavouriteGroup = 0;

                    using (IQueryAdapter queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                        await queryreactor2.RunFastQueryAsync(
                            $"UPDATE users_stats SET favourite_group=0 WHERE id={userId} LIMIT 1");

                    Room room = Session.GetHabbo().CurrentRoom;

                    await Response.InitAsync(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
                    await Response.AppendIntegerAsync(byeUser.Id);

                    if (room != null)
                        await room.SendMessage(Response);
                    else
                        await SendResponse();
                }

                await Response.InitAsync(LibraryParser.OutgoingRequest("GroupRequestReloadMessageComposer"));
                await Response.AppendIntegerAsync(guild);

                await SendResponse();
            }
        }


        internal async Task UpdateForumSettings()
        {
            if (Session?.GetHabbo() == null) return;


            uint guild = Request.GetUInteger();
            int whoCanRead = Request.GetInteger();
            int whoCanPost = Request.GetInteger();
            int whoCanThread = Request.GetInteger();
            int whoCanMod = Request.GetInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(guild);

            if (group == null)
                return;

            var room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);

            if (!room.CheckRights(Session, false, true, true))
                return;

            group.WhoCanRead = whoCanRead;
            group.WhoCanPost = whoCanPost;
            group.WhoCanThread = whoCanThread;
            group.WhoCanMod = whoCanMod;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "UPDATE groups_data SET who_can_read = @who_can_read, who_can_post = @who_can_post, who_can_thread = @who_can_thread, who_can_mod = @who_can_mod WHERE id = @group_id");
                queryReactor.AddParameter("group_id", group.Id);
                queryReactor.AddParameter("who_can_read", whoCanRead);
                queryReactor.AddParameter("who_can_post", whoCanPost);
                queryReactor.AddParameter("who_can_thread", whoCanThread);
                queryReactor.AddParameter("who_can_mod", whoCanMod);
                await queryReactor.RunQueryAsync();
            }

            await Session.SendMessageAsync(group.ForumDataMessage(Session.GetHabbo().Id));

            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModForumCanReadSeen", 1);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModForumCanPostSeen", 1);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModForumCanPostThrdSeen", 1);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModForumCanModerateSeen", 1);
        }

        internal async Task DeleteGroup()
        {
            uint groupId = Request.GetUInteger();
            var group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);
            if (group == null) return;
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);

            if (Session == null) return;

            if (room?.RoomData?.Group == null)
            {
                await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("command_group_has_no_room"));
            }
            else
            {
                if (!room.CheckRights(Session, true))
                    return;

                /* TODO CHECK */
                foreach (var user in group.Members.Values)
                {
                    var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(user.Id);

                    if (clientByUserId == null)
                        continue;

                    clientByUserId.GetHabbo().UserGroups.Remove(user);

                    if (clientByUserId.GetHabbo().FavouriteGroup == group.Id)
                        clientByUserId.GetHabbo().FavouriteGroup = 0;
                }

                room.RoomData.Group = null;
                room.RoomData.GroupId = 0;

                Oblivion.GetGame().GetGroupManager().DeleteGroup(group.Id);

                var deleteGroup = new ServerMessage(LibraryParser.OutgoingRequest("GroupDeletedMessageComposer"));

                await deleteGroup.AppendIntegerAsync(groupId);
                await room.SendMessage(deleteGroup);

                room.GetRoomItemHandler().RemoveAllFurniture(Session);

                var roomId = room.RoomData.Id;

                Oblivion.GetGame().GetRoomManager().UnloadRoom(room, "Delete room");

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    await queryReactor.RunFastQueryAsync($"DELETE FROM users_favorites WHERE room_id = {roomId}");
                    await queryReactor.RunFastQueryAsync($"DELETE FROM items_rooms WHERE room_id = {roomId}");
                    await queryReactor.RunFastQueryAsync($"DELETE FROM rooms_rights WHERE room_id = {roomId}");
                    await queryReactor.RunFastQueryAsync($"UPDATE users SET home_room = '0' WHERE home_room = {roomId}");
                }

//                var roomData2 =
//                    (from p in Session.GetHabbo().Data.Rooms where p.Id == roomId select p).SingleOrDefault();

//                if (roomData2 != null)
                Session.GetHabbo().Data.Rooms.Remove(roomId);
            }
        }
    }
}