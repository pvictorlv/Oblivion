using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        internal void SerializeGroupPurchasePage()
        {
            var list = new List<RoomData>();
            foreach (var x in Session.GetHabbo().Data.Rooms)
            {
                var current = Oblivion.GetGame().GetRoomManager().GenerateRoomData(x);
                if (current.Group == null) list.Add(current);
            }

            Response.Init(LibraryParser.OutgoingRequest("GroupPurchasePageMessageComposer"));
            Response.AppendInteger(10);
            Response.AppendInteger(list.Count);

            /* TODO CHECK */
            foreach (RoomData current2 in list)
            {
                Response.AppendInteger(current2.Id);
                Response.AppendString(current2.Name);
                Response.AppendBool(false);
            }

            Response.AppendInteger(5);
            Response.AppendInteger(10);
            Response.AppendInteger(3);
            Response.AppendInteger(4);
            Response.AppendInteger(25);
            Response.AppendInteger(17);
            Response.AppendInteger(5);
            Response.AppendInteger(25);
            Response.AppendInteger(17);
            Response.AppendInteger(3);
            Response.AppendInteger(29);
            Response.AppendInteger(11);
            Response.AppendInteger(4);
            Response.AppendInteger(0);
            Response.AppendInteger(0);
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Serializes the group purchase parts.
        /// </summary>
        internal void SerializeGroupPurchaseParts()
        {
            Response.Init(LibraryParser.OutgoingRequest("GroupPurchasePartsMessageComposer"));
            Response.AppendInteger(Oblivion.GetGame().GetGroupManager().Bases.Count);
            /* TODO CHECK */
            foreach (GroupBases current in Oblivion.GetGame().GetGroupManager().Bases)
            {
                Response.AppendInteger(current.Id);
                Response.AppendString(current.Value1);
                Response.AppendString(current.Value2);
            }

            Response.AppendInteger(Oblivion.GetGame().GetGroupManager().Symbols.Count);
            /* TODO CHECK */
            foreach (GroupSymbols current2 in Oblivion.GetGame().GetGroupManager().Symbols)
            {
                Response.AppendInteger(current2.Id);
                Response.AppendString(current2.Value1);
                Response.AppendString(current2.Value2);
            }

            Response.AppendInteger(Oblivion.GetGame().GetGroupManager().BaseColours.Count);
            /* TODO CHECK */
            foreach (GroupBaseColours current3 in Oblivion.GetGame().GetGroupManager().BaseColours)
            {
                Response.AppendInteger(current3.Id);
                Response.AppendString(current3.Colour);
            }

            Response.AppendInteger(Oblivion.GetGame().GetGroupManager().SymbolColours.Count);
            /* TODO CHECK */
            foreach (GroupSymbolColours current4 in Oblivion.GetGame().GetGroupManager().SymbolColours.Values)
            {
                Response.AppendInteger(current4.Id);
                Response.AppendString(current4.Colour);
            }

            Response.AppendInteger(Oblivion.GetGame().GetGroupManager().BackGroundColours.Count);
            /* TODO CHECK */
            foreach (GroupBackGroundColours current5 in Oblivion.GetGame().GetGroupManager().BackGroundColours.Values)
            {
                Response.AppendInteger(current5.Id);
                Response.AppendString(current5.Colour);
            }

            SendResponse();
        }

        /// <summary>
        /// Purchases the group.
        /// </summary>
        internal void PurchaseGroup()
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
            var roomData = Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomid);

            if (roomData.Owner != Session.GetHabbo().UserName)
                return;

            for (var i = 0; i < (num6 * 3); i++)
                gStates.Add(Request.GetInteger());

            var image = Oblivion.GetGame().GetGroupManager().GenerateGuildImage(guildBase, guildBaseColor, gStates);

            Oblivion.GetGame().GetGroupManager().CreateGroup(name, description, roomid, image, Session,
                (!Oblivion.GetGame().GetGroupManager().SymbolColours.Contains(color)) ? 1 : color,
                (!Oblivion.GetGame().GetGroupManager().BackGroundColours.Contains(num3)) ? 1 : num3, out var theGroup);

            Session.SendMessage(CatalogPageComposer.PurchaseOk(0u, "CREATE_GUILD", 10));
            Response.Init(LibraryParser.OutgoingRequest("GroupRoomMessageComposer"));
            Response.AppendInteger(roomid);
            Response.AppendInteger(theGroup.Id);
            SendResponse();
            roomData.Group = theGroup;
            roomData.GroupId = theGroup.Id;
            roomData.SerializeRoomData(Response, Session, true);

            if (!Session.GetHabbo().InRoom || Session.GetHabbo().CurrentRoom.RoomId != roomData.Id)
            {
                Session.GetMessageHandler().PrepareRoomForUser(roomData.Id, roomData.PassWord);
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

                serverMessage.AppendInteger(CurrentLoadingRoom.LoadedGroups.Count);

                /* TODO CHECK */
                foreach (var current in CurrentLoadingRoom.LoadedGroups)
                {
                    serverMessage.AppendInteger(current.Key);
                    serverMessage.AppendString(current.Value);
                }

                CurrentLoadingRoom.SendMessage(serverMessage);
            }

            if (CurrentLoadingRoom == null || Session.GetHabbo().FavouriteGroup != theGroup.Id)
                return;

            var serverMessage2 =
                new ServerMessage(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));

            serverMessage2.AppendInteger(CurrentLoadingRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
            serverMessage2.AppendInteger(theGroup.Id);
            serverMessage2.AppendInteger(3);
            serverMessage2.AppendString(theGroup.Name);

            CurrentLoadingRoom.SendMessage(serverMessage2);
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        internal void SerializeGroupInfo()
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
        internal void SerializeGroupMembers()
        {
            uint groupId = Request.GetUInteger();
            int page = Request.GetInteger();
            string searchVal = Request.GetString();
            uint reqType = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (group == null) return;

            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));

            Oblivion.GetGame().GetGroupManager()
                .SerializeGroupMembers(Response, group, reqType, Session, searchVal, page);

            SendResponse();
        }

        /// <summary>
        /// Makes the group admin.
        /// </summary>
        internal void MakeGroupAdmin()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(num);

            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) ||
                group.Admins.ContainsKey(num2))
                return;

            group.Members[num2].Rank = 1;

            group.Admins.Add(num2, group.Members[num2]);

            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 1u, Session);

            SendResponse();

            Room room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);

            RoomUser roomUserByHabbo = room?.GetRoomUserManager()
                .GetRoomUserByHabbo(Oblivion.GetHabboById(num2).UserName);

            if (roomUserByHabbo != null)
            {
//                if (!roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
                roomUserByHabbo.AddStatus("flatctrl 1", "");

                Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                Response.AppendInteger(1);
                roomUserByHabbo.GetClient().SendMessage(GetResponse());
                roomUserByHabbo.UpdateNeeded = true;
            }

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_members SET rank='1' WHERE group_id=", num,
                    " AND user_id=", num2, " LIMIT 1;"));
        }

        /// <summary>
        /// Removes the group admin.
        /// </summary>
        internal void RemoveGroupAdmin()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();

            Guild group = Oblivion.GetGame().GetGroupManager().GetGroup(num);

            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) ||
                !group.Admins.ContainsKey(num2))
                return;

            group.Members[num2].Rank = 0;
            group.Admins.Remove(num2);

            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            SendResponse();

            Room room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);
            RoomUser roomUserByHabbo = room?.GetRoomUserManager()
                .GetRoomUserByHabbo(Oblivion.GetHabboById(num2).UserName);

            if (roomUserByHabbo != null)
            {
//                if (roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
                roomUserByHabbo.RemoveStatus("flatctrl 1");

                Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                Response.AppendInteger(0);
                roomUserByHabbo.GetClient().SendMessage(GetResponse());
                roomUserByHabbo.UpdateNeeded = true;
            }

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_members SET rank='0' WHERE group_id=", num,
                    " AND user_id=", num2, " LIMIT 1;"));
        }

        /// <summary>
        /// Accepts the membership.
        /// </summary>
        internal void AcceptMembership()
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
                    queryReactor.RunFastQuery(
                        $"DELETE FROM groups_requests WHERE group_id = '{groupId}' AND user_id = '{userId}' LIMIT 1");
                return;
            }


            member.DateJoin = Oblivion.GetUnixTimeStamp();
            group.Members.Add(userId, member);
            group.Requests.Remove(userId);
            group.Admins.Add(userId, member);

            Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            SendResponse();

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(
                    $"DELETE FROM groups_requests WHERE group_id = '{groupId}' AND user_id = '{userId}' LIMIT 1");

            using (IQueryAdapter queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryreactor2.RunFastQuery(
                    $"INSERT INTO groups_members (group_id, user_id, rank, date_join) VALUES ('{groupId}','{userId}','0','{Oblivion.GetUnixTimeStamp()}')");
        }

        /// <summary>
        /// Declines the membership.
        /// </summary>
        internal void DeclineMembership()
        {
            var groupId = Request.GetUInteger();
            var userId = Request.GetUInteger();

            var group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(Session.GetHabbo().Id) &&
                !group.Requests.ContainsKey(userId))
                return;

            group.Requests.Remove(userId);

            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 2u, Session);
            SendResponse();

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
                queryReactor.RunFastQuery("DELETE FROM groups_requests WHERE group_id=" + groupId + " AND user_id=" +
                                          userId);
        }

        /// <summary>
        /// Joins the group.
        /// </summary>
        internal void JoinGroup()
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
                        queryReactor.RunFastQuery(
                            string.Concat("INSERT INTO groups_members (user_id, group_id, date_join) VALUES (", user.Id,
                                ",", groupId, ",", Oblivion.GetUnixTimeStamp(), ")"));
                        queryReactor.RunFastQuery(string.Concat("UPDATE users_stats SET favourite_group=", groupId,
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
                            queryreactor2.RunFastQuery(
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
        internal void RemoveMember()
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
                    queryReactor.RunFastQuery(string.Concat("DELETE FROM groups_members WHERE user_id=", num2,
                        " AND group_id=", num, " LIMIT 1"));

                Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);

                if (Session.GetHabbo().FavouriteGroup == num)
                {
                    Session.GetHabbo().FavouriteGroup = 0u;

                    using (IQueryAdapter queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryreactor2.RunFastQuery($"UPDATE users_stats SET favourite_group=0 WHERE id={num2} LIMIT 1");

                    Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
                    Response.AppendInteger(Session.GetHabbo().Id);
                    Session.GetHabbo().CurrentRoom.SendMessage(Response);
                    Response.Init(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
                    Response.AppendInteger(0);
                    Response.AppendInteger(-1);
                    Response.AppendInteger(-1);
                    Response.AppendString("");
                    Session.GetHabbo().CurrentRoom.SendMessage(Response);

                    if (group.AdminOnlyDeco == 0u)
                    {
                        RoomUser roomUserByHabbo = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId)
                            .GetRoomUserManager().GetRoomUserByHabbo(Oblivion.GetHabboById(num2).UserName);

                        if (roomUserByHabbo == null)
                            return;

                        roomUserByHabbo.RemoveStatus("flatctrl 1");
                        Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));

                        Response.AppendInteger(0);

                        roomUserByHabbo.GetClient().SendMessage(GetResponse());
                    }
                }

                return;
            }

            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2))
                return;

            group.Members.Remove(num2);

            if (group.Admins.ContainsKey(num2))
                group.Admins.Remove(num2);

            Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Oblivion.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            SendResponse();

            using (IQueryAdapter queryreactor3 = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryreactor3.RunFastQuery(string.Concat("DELETE FROM groups_members WHERE group_id=", num,
                    " AND user_id=", num2, " LIMIT 1;"));
        }

        /// <summary>
        /// Makes the fav.
        /// </summary>
        internal void MakeFav()
        {
            uint groupId = Request.GetUInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup == null)
                return;

            if (!theGroup.Members.ContainsKey(Session.GetHabbo().Id))
                return;

            Session.GetHabbo().FavouriteGroup = theGroup.Id;
            Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(theGroup, Response, Session);

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Concat("UPDATE users_stats SET favourite_group =", theGroup.Id,
                    " WHERE id=", Session.GetHabbo().Id, " LIMIT 1;"));

            Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
            Response.AppendInteger(Session.GetHabbo().Id);
            Session.SendMessage(Response);

            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (!Session.GetHabbo().CurrentRoom.LoadedGroups.ContainsKey(theGroup.Id))
                {
                    Session.GetHabbo().CurrentRoom.LoadedGroups.Add(theGroup.Id, theGroup.Badge);
                    Response.Init(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                    Response.AppendInteger(Session.GetHabbo().CurrentRoom.LoadedGroups.Count);

                    /* TODO CHECK */
                    foreach (KeyValuePair<uint, string> current in Session.GetHabbo().CurrentRoom.LoadedGroups)
                    {
                        Response.AppendInteger(current.Key);
                        Response.AppendString(current.Value);
                    }

                    Session.GetHabbo().CurrentRoom.SendMessage(Response);
                }
            }

            Response.Init(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
            Response.AppendInteger(0);
            Response.AppendInteger(theGroup.Id);
            Response.AppendInteger(3);
            Response.AppendString(theGroup.Name);

            Session.SendMessage(Response);
        }

        /// <summary>
        /// Removes the fav.
        /// </summary>
        internal void RemoveFav()
        {
            Request.GetUInteger();
            Session.GetHabbo().FavouriteGroup = 0u;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(
                    $"UPDATE users_stats SET favourite_group=0 WHERE id={Session.GetHabbo().Id} LIMIT 1;");

            Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
            Response.AppendInteger(Session.GetHabbo().Id);
            Session.SendMessage(Response);
            Response.Init(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
            Response.AppendInteger(0);
            Response.AppendInteger(-1);
            Response.AppendInteger(-1);
            Response.AppendString(string.Empty);

            Session.SendMessage(Response);
        }

        /// <summary>
        /// Publishes the forum thread.
        /// </summary>
        internal void PublishForumThread()
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
                        Session.SendNotif(Oblivion.GetLanguage().GetVar("forums_cancel"));
                        return;
                    }
                }

                Session.GetHabbo().LastSqlQuery = Oblivion.GetUnixTimeStamp();
                dbClient.SetQuery(
                    "INSERT INTO groups_forums_posts (group_id, parent_id, timestamp, poster_id, poster_name, poster_look, subject, post_content) VALUES (@gid, @pard, @ts, @pid, @pnm, @plk, @subjc, @content)");
                dbClient.AddParameter("gid", groupId);
                dbClient.AddParameter("pard", threadId);
                dbClient.AddParameter("ts", timestamp);
                dbClient.AddParameter("pid", Session.GetHabbo().Id);
                dbClient.AddParameter("pnm", Session.GetHabbo().UserName);
                dbClient.AddParameter("plk", Session.GetHabbo().Look);
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
                message.AppendInteger(groupId);
                message.AppendInteger(threadId);
                message.AppendInteger(Session.GetHabbo().Id);
                message.AppendString(subject);
                message.AppendString(content);
                message.AppendBool(false);
                message.AppendBool(false);
                message.AppendInteger((Oblivion.GetUnixTimeStamp() - timestamp));
                message.AppendInteger(1);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendInteger(1);
                message.AppendString("");
                message.AppendInteger((Oblivion.GetUnixTimeStamp() - timestamp));
                message.AppendByte(1);
                message.AppendInteger(1);
                message.AppendString("");
                message.AppendInteger(42);
                Session.SendMessage(message);
            }
            else
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumNewResponseMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(threadId);
                message.AppendInteger(group.ForumMessagesCount);
                message.AppendInteger(0);
                message.AppendInteger(Session.GetHabbo().Id);
                message.AppendString(Session.GetHabbo().UserName);
                message.AppendString(Session.GetHabbo().Look);
                message.AppendInteger((Oblivion.GetUnixTimeStamp() - timestamp));
                message.AppendString(content);
                message.AppendByte(0);
                message.AppendInteger(0);
                message.AppendString("");
                message.AppendInteger(0);
                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Updates the state of the thread.
        /// </summary>
        internal void UpdateThreadState()
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
                        dbClient.RunQuery();
                    }
                }

                var thread = new GroupForumPost(row);

                if (thread.Pinned != pin)
                {
                    var notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

                    notif.AppendString((pin) ? "forums.thread.pinned" : "forums.thread.unpinned");
                    notif.AppendInteger(0);
                    Session.SendMessage(notif);
                }

                if (thread.Locked != Lock)
                {
                    var notif2 = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

                    notif2.AppendString((Lock) ? "forums.thread.locked" : "forums.thread.unlocked");
                    notif2.AppendInteger(0);
                    Session.SendMessage(notif2);
                }

                if (thread.ParentId != 0)
                    return;

                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(thread.Id);
                message.AppendInteger(thread.PosterId);
                message.AppendString(thread.PosterName);
                message.AppendString(thread.Subject);
                message.AppendBool(pin);
                message.AppendBool(Lock);
                message.AppendInteger((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendInteger(thread.MessageCount + 1);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendInteger(1);
                message.AppendString("");
                message.AppendInteger((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendByte((thread.Hidden) ? 10 : 1);
                message.AppendInteger(1);
                message.AppendString(thread.Hider);
                message.AppendInteger(0);

                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Alters the state of the forum thread.
        /// </summary>
        internal void AlterForumThreadState()
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
                        dbClient.RunQuery();
                    }
                }

                var thread = new GroupForumPost(row);
                var notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

                notif.AppendString((stateToSet == 20) ? "forums.thread.hidden" : "forums.thread.restored");
                notif.AppendInteger(0);
                Session.SendMessage(notif);

                if (thread.ParentId != 0)
                    return;

                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(thread.Id);
                message.AppendInteger(thread.PosterId);
                message.AppendString(thread.PosterName);
                message.AppendString(thread.Subject);
                message.AppendBool(thread.Pinned);
                message.AppendBool(thread.Locked);
                message.AppendInteger((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendInteger(thread.MessageCount + 1);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendString(string.Empty);
                message.AppendInteger((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendByte(stateToSet);
                message.AppendInteger(0);
                message.AppendString(thread.Hider);
                message.AppendInteger(0);

                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Reads the forum thread.
        /// </summary>
        internal void ReadForumThread()
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

                messageBuffer.AppendInteger(groupId);
                messageBuffer.AppendInteger(threadId);
                messageBuffer.AppendInteger(startIndex);
                messageBuffer.AppendInteger(b);

                int indx = 0;

                /* TODO CHECK */
                foreach (GroupForumPost post in posts)
                {
                    messageBuffer.AppendInteger(indx++ - 1);
                    messageBuffer.AppendInteger(indx - 1);
                    messageBuffer.AppendInteger(post.PosterId);
                    messageBuffer.AppendString(post.PosterName);
                    messageBuffer.AppendString(post.PosterLook);
                    messageBuffer.AppendInteger(Oblivion.GetUnixTimeStamp() - post.Timestamp);
                    messageBuffer.AppendString(post.PostContent);
                    messageBuffer.AppendByte(0);
                    messageBuffer.AppendInteger(0);
                    messageBuffer.AppendString(post.Hider);
                    messageBuffer.AppendInteger(0);
                    messageBuffer.AppendInteger(0);
                }

                Session.SendMessage(messageBuffer);
            }
        }


        /// <summary>
        /// Gets the group forum thread root.
        /// </summary>
        internal void GetGroupForumThreadRoot()
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
                message.AppendInteger(groupId);
                message.AppendInteger(startIndex);
                message.AppendInteger(threadCount);

                /* TODO CHECK */
                foreach (GroupForumPost thread in threads)
                {
                    message.AppendInteger(thread.Id);
                    message.AppendInteger(thread.PosterId);
                    message.AppendString(thread.PosterName);
                    message.AppendString(thread.Subject);
                    message.AppendBool(thread.Pinned);
                    message.AppendBool(thread.Locked);
                    message.AppendInteger((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                    message.AppendInteger(thread.MessageCount + 1);
                    message.AppendInteger(0);
                    message.AppendInteger(0);
                    message.AppendInteger(0);
                    message.AppendString(string.Empty);
                    message.AppendInteger((Oblivion.GetUnixTimeStamp() - thread.Timestamp));
                    message.AppendByte((thread.Hidden) ? 10 : 1);
                    message.AppendInteger(0);
                    message.AppendString(thread.Hider);
                    message.AppendInteger(0);
                }

                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Gets the group forum data.
        /// </summary>
        internal void GetGroupForumData()
        {
            uint groupId = Request.GetUInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup != null && theGroup.HasForum)
                Session.SendMessage(theGroup.ForumDataMessage(Session.GetHabbo().Id));
        }

        /// <summary>
        /// Gets the group forums.
        /// </summary>
        internal void GetGroupForums()
        {
            int selectType = Request.GetInteger();
            int startIndex = Request.GetInteger();

            var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumListingsMessageComposer"));
            message.AppendInteger(selectType);
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

                        message.AppendInteger(qtdForums == 0 ? 1 : qtdForums);
                        message.AppendInteger(startIndex);

                        groupList.AddRange(from DataRow rowGroupData in table.Rows
                            select uint.Parse(rowGroupData["id"].ToString())
                            into groupId
                            select Oblivion.GetGame().GetGroupManager().GetGroup(groupId));

                        message.AppendInteger(table.Rows.Count);

                        /* TODO CHECK */
                        foreach (Guild theGroup in groupList)
                            theGroup.SerializeForumRoot(message);

                        Session.SendMessage(message);
                    }

                    break;

                case 2:
                    groupList.AddRange(Session.GetHabbo().UserGroups
                        .Select(groupUser => Oblivion.GetGame().GetGroupManager().GetGroup(groupUser.GroupId))
                        .Where(aGroup => aGroup != null && aGroup.HasForum));

                    message.AppendInteger(groupList.Count == 0 ? 1 : groupList.Count);

                    groupList = groupList.OrderByDescending(x => x.ForumMessagesCount).Skip(startIndex).Take(20)
                        .ToList();

                    message.AppendInteger(startIndex);
                    message.AppendInteger(groupList.Count);

                    /* TODO CHECK */
                    foreach (Guild theGroup in groupList)
                        theGroup.SerializeForumRoot(message);

                    Session.SendMessage(message);
                    break;

                default:
                    message.AppendInteger(1);
                    message.AppendInteger(startIndex);
                    message.AppendInteger(0);
                    Session.SendMessage(message);
                    break;
            }
        }

        /// <summary>
        /// Manages the group.
        /// </summary>
        internal void ManageGroup()
        {
            var groupId = Request.GetUInteger();
            var theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup == null)
                return;

            if (!theGroup.Admins.ContainsKey(Session.GetHabbo().Id) && theGroup.CreatorId != Session.GetHabbo().Id &&
                Session.GetHabbo().Rank < 7)
                return;

            Response.Init(LibraryParser.OutgoingRequest("GroupDataEditMessageComposer"));
            Response.AppendInteger(0);
            Response.AppendBool(true);
            Response.AppendInteger(theGroup.Id);
            Response.AppendString(theGroup.Name);
            Response.AppendString(theGroup.Description);
            Response.AppendInteger(theGroup.RoomId);
            Response.AppendInteger(theGroup.Colour1);
            Response.AppendInteger(theGroup.Colour2);
            Response.AppendInteger(theGroup.State);
            Response.AppendInteger(theGroup.AdminOnlyDeco);
            Response.AppendBool(false);
            Response.AppendString(string.Empty);

            var array = theGroup.Badge.Replace("b", string.Empty).Split('s');

            Response.AppendInteger(5);

            var num = (5 - array.Length);

            var num2 = 0;
            var array2 = array;

            /* TODO CHECK */

            foreach (var text in array2)
            {
                try
                {
                    Response.AppendInteger(text.Length >= 6
                        ? uint.Parse(text.Substring(0, 3))
                        : uint.Parse(text.Substring(0, 2)));

                    Response.AppendInteger((text.Length >= 6)
                        ? uint.Parse(text.Substring(3, 2))
                        : uint.Parse(text.Substring(2, 2)));

                    if (text.Length < 5)
                        Response.AppendInteger(0);
                    else if (text.Length >= 6)
                        Response.AppendInteger(uint.Parse(text.Substring(5, 1)));
                    else
                        Response.AppendInteger(uint.Parse(text.Substring(4, 1)));
                }
                catch
                {
                    //ignored
                }
            }


            while (num2 != num)
            {
                Response.AppendInteger(0);
                Response.AppendInteger(0);
                Response.AppendInteger(0);
                num2++;
            }

            Response.AppendString(theGroup.Badge);
            Response.AppendInteger(theGroup.Members.Count);

            SendResponse();
        }

        /// <summary>
        /// Updates the name of the group.
        /// </summary>
        internal void UpdateGroupName()
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

                queryReactor.RunQuery();
            }

            theGroup.Name = text;
            theGroup.Description = text2;

            Oblivion.GetGame().GetGroupManager()
                .SerializeGroupInfo(theGroup, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Updates the group badge.
        /// </summary>
        internal void UpdateGroupBadge()
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

                    Response.Init(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                    Response.AppendInteger(room.LoadedGroups.Count);

                    foreach (KeyValuePair<uint, string> current2 in room.LoadedGroups)
                    {
                        Response.AppendInteger(current2.Key);
                        Response.AppendString(current2.Value);
                    }

                    room.SendMessage(Response);

                    Oblivion.GetGame().GetGroupManager().SerializeGroupInfo(guild, Response, Session, room);

                    using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery($"UPDATE groups_data SET badge = @badgi WHERE id = {guildId}");
                        queryReactor.AddParameter("badgi", badge);
                        queryReactor.RunQuery();
                    }

                    if (Session.GetHabbo().CurrentRoom != null)
                    {
                        Session.GetHabbo().CurrentRoom.LoadedGroups[guildId] = guild.Badge;

                        Response.Init(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                        Response.AppendInteger(Session.GetHabbo().CurrentRoom.LoadedGroups.Count);

                        /* TODO CHECK */
                        foreach (KeyValuePair<uint, string> current in Session.GetHabbo().CurrentRoom.LoadedGroups)
                        {
                            Response.AppendInteger(current.Key);
                            Response.AppendString(current.Value);
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
        internal void UpdateGroupColours()
        {
            uint groupId = Request.GetUInteger();
            int num = Request.GetInteger();
            int num2 = Request.GetInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup?.CreatorId != Session.GetHabbo().Id)
                return;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_data SET colour1= ", num, ", colour2=", num2,
                    " WHERE id=", theGroup.Id, " LIMIT 1"));

            theGroup.Colour1 = num;
            theGroup.Colour2 = num2;

            Oblivion.GetGame().GetGroupManager()
                .SerializeGroupInfo(theGroup, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Updates the group settings.
        /// </summary>
        internal void UpdateGroupSettings()
        {
            uint groupId = Request.GetUInteger();
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();

            Guild theGroup = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (theGroup?.CreatorId != Session.GetHabbo().Id)
                return;

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_data SET state='", num, "', admindeco='", num2,
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
                            Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                            Response.AppendInteger(0);
                            current.GetClient().SendMessage(GetResponse());
                        }
                        else
                        {
                            current.AddStatus("flatctrl 1", "");
                            Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                            Response.AppendInteger(1);
                            current.GetClient().SendMessage(GetResponse());
                        }

                        current.UpdateNeeded = true;
                    }
                }
            }

            Oblivion.GetGame().GetGroupManager()
                .SerializeGroupInfo(theGroup, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Requests the leave group.
        /// </summary>
        internal void RequestLeaveGroup()
        {
            uint groupId = Request.GetUInteger();
            uint userId = Request.GetUInteger();

            Guild guild = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

            if (guild == null || guild.CreatorId == userId)
                return;

            if (userId == Session.GetHabbo().Id || guild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                Response.Init(LibraryParser.OutgoingRequest("GroupAreYouSureMessageComposer"));
                Response.AppendInteger(userId);
                Response.AppendInteger(0);
                SendResponse();
            }
        }

        /// <summary>
        /// Confirms the leave group.
        /// </summary>
        internal void ConfirmLeaveGroup()
        {
            uint guild = Request.GetUInteger();
            uint userId = Request.GetUInteger();

            Guild byeGuild = Oblivion.GetGame().GetGroupManager().GetGroup(guild);

            if (byeGuild == null)
                return;

            if (byeGuild.CreatorId == userId)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("user_room_video_true"));
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
                    queryReactor.RunFastQuery(string.Concat("DELETE FROM groups_members WHERE user_id=", userId,
                        " AND group_id=", guild, " LIMIT 1"));

                Habbo byeUser = Oblivion.GetHabboById(userId);

                if (byeUser != null)
                {
                    Response.Init(LibraryParser.OutgoingRequest("GroupConfirmLeaveMessageComposer"));
                    Response.AppendInteger(guild);
                    Response.AppendInteger(type);
                    Response.AppendInteger(byeUser.Id);
                    Response.AppendString(byeUser.UserName);
                    Response.AppendString(byeUser.Look);
                    Response.AppendString(string.Empty);

                    SendResponse();
                }

                if (byeUser != null && byeUser.FavouriteGroup == guild)
                {
                    byeUser.FavouriteGroup = 0;

                    using (IQueryAdapter queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryreactor2.RunFastQuery(
                            $"UPDATE users_stats SET favourite_group=0 WHERE id={userId} LIMIT 1");

                    Room room = Session.GetHabbo().CurrentRoom;

                    Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
                    Response.AppendInteger(byeUser.Id);

                    if (room != null)
                        room.SendMessage(Response);
                    else
                        SendResponse();
                }

                Response.Init(LibraryParser.OutgoingRequest("GroupRequestReloadMessageComposer"));
                Response.AppendInteger(guild);

                SendResponse();
            }
        }


        internal void UpdateForumSettings()
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
                queryReactor.RunQuery();
            }

            Session.SendMessage(group.ForumDataMessage(Session.GetHabbo().Id));

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

        internal void DeleteGroup()
        {
            uint groupId = Request.GetUInteger();
            var group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);
            if (group == null) return;
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(group.RoomId);

            if (Session == null) return;

            if (room?.RoomData?.Group == null)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("command_group_has_no_room"));
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

                deleteGroup.AppendInteger(groupId);
                room.SendMessage(deleteGroup);

                room.GetRoomItemHandler().RemoveAllFurniture(Session);

                var roomData = room.RoomData;
                var roomId = room.RoomData.Id;

                Oblivion.GetGame().GetRoomManager().UnloadRoom(room, "Delete room");
                Oblivion.GetGame().GetRoomManager().QueueVoteRemove(roomData);

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery($"DELETE FROM users_favorites WHERE room_id = {roomId}");
                    queryReactor.RunFastQuery($"DELETE FROM items_rooms WHERE room_id = {roomId}");
                    queryReactor.RunFastQuery($"DELETE FROM rooms_rights WHERE room_id = {roomId}");
                    queryReactor.RunFastQuery($"UPDATE users SET home_room = '0' WHERE home_room = {roomId}");
                }

//                var roomData2 =
//                    (from p in Session.GetHabbo().Data.Rooms where p.Id == roomId select p).SingleOrDefault();

//                if (roomData2 != null)
                Session.GetHabbo().Data.Rooms.Remove(roomId);
            }
        }
    }
}