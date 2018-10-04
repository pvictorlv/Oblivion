using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Quests.Composer;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Relationships;
using Oblivion.Messages.Parsers;
using Oblivion.Security;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    partial class GameClientMessageHandler
    {
        /// <summary>
        /// Sends the bully report.
        /// </summary>
        public void SendBullyReport()
        {
            var reportedId = Request.GetUInteger();
            Oblivion.GetGame()
                .GetModerationTool()
                .SendNewTicket(Session, 104, 9, reportedId, "", new List<string>());

            Response.Init(LibraryParser.OutgoingRequest("BullyReportSentMessageComposer"));
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Opens the bully reporting.
        /// </summary>
        public void OpenBullyReporting()
        {
            Response.Init(LibraryParser.OutgoingRequest("OpenBullyReportMessageComposer"));
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Opens the quests.
        /// </summary>
        public void OpenQuests()
        {
            Oblivion.GetGame().GetQuestManager().GetList(Session, Request);
        }

        /// <summary>
        /// Retrieves the citizenship.
        /// </summary>
        internal void RetrieveCitizenship()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("CitizenshipStatusMessageComposer"));
            GetResponse().AppendString(Request.GetString());
            GetResponse().AppendInteger(4);
            GetResponse().AppendInteger(4);
            SendResponse();
        }

        /// <summary>
        /// Loads the club gifts.
        /// </summary>
        internal void LoadClubGifts()
        {
            if (Session?.GetHabbo() == null)
                return;
            //var i = 0;
            //var i2 = 0;
            Session.GetHabbo().GetSubscriptionManager().GetSubscription();
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("LoadCatalogClubGiftsMessageComposer"));
            serverMessage.AppendInteger(0); // i
            serverMessage.AppendInteger(0); // i2
            serverMessage.AppendInteger(1);
        }

        /// <summary>
        /// Chooses the club gift.
        /// </summary>
        internal void ChooseClubGift()
        {
            if (Session?.GetHabbo() == null)
                return;
            Request.GetString();
        }

        /// <summary>
        /// Gets the user tags.
        /// </summary>
        internal void GetUserTags()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo?.GetClient()?.GetHabbo()?.Data?.Tags == null || roomUserByHabbo.IsBot)
                return;
            Response.Init(LibraryParser.OutgoingRequest("UserTagsMessageComposer"));
            Response.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Id);
            Response.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Data.Tags.Count);
            foreach (var current in roomUserByHabbo.GetClient().GetHabbo().Data.Tags)
                Response.AppendString(current);
            SendResponse();

            if (Session != roomUserByHabbo.GetClient())
                return;
            if (Session.GetHabbo().Data.Tags.Count >= 5)
                Oblivion.GetGame()
                    .GetAchievementManager()
                    .ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_UserTags", 5);
        }

        /// <summary>
        /// Gets the user badges.
        /// </summary>
        internal void GetUserBadges()
        {
            if (Session?.GetHabbo() == null) return;


            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo != null && !roomUserByHabbo.IsBot && roomUserByHabbo.GetClient() != null &&
                roomUserByHabbo.GetClient().GetHabbo() != null)
            {
                Session.GetHabbo().LastSelectedUser = roomUserByHabbo.UserId;

                if (Session.GetHabbo().WebSocketConnId != Guid.Empty)
                {
                    Oblivion.GetWebSocket().SendMessage(Session.GetHabbo().WebSocketConnId,
                        $"3|{roomUserByHabbo.GetUserName()}|{roomUserByHabbo.GetClient().GetHabbo().Gender}|{roomUserByHabbo.GetClient().GetHabbo().Look}");
                }

                var msg = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
                msg.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Id);

                if (roomUserByHabbo.GetClient()?.GetHabbo()?.GetBadgeComponent()?.BadgeList == null) return;

                msg.StartArray();
                /* TODO CHECK */
                foreach (var badge in
                    roomUserByHabbo.GetClient()
                        .GetHabbo()
                        .GetBadgeComponent()
                        .BadgeList.Values
                        .Where(badge => badge.Slot > 0).Take(5))
                {
                    msg.AppendInteger(badge.Slot);
                    msg.AppendString(badge.Code);

                    msg.SaveArray();
                }

                msg.EndArray();
                Session.SendMessage(msg);
//                SendResponse();
            }
        }

        /// <summary>
        /// Gives the respect.
        /// </summary>
        internal void GiveRespect()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || Session.GetHabbo().DailyRespectPoints <= 0)
                return;
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo?.GetClient()?.GetHabbo() == null) return;

            if (roomUserByHabbo.GetClient().GetHabbo().Id == Session.GetHabbo().Id ||
                roomUserByHabbo.IsBot)
                return;
            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialRespect);
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_RespectGiven", 1, true);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_RespectEarned", 1, true);
            Session.GetHabbo().DailyRespectPoints--;
            roomUserByHabbo.GetClient().GetHabbo().Respect++;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery("UPDATE users_stats SET respect = respect + 1 WHERE id = " +
                                          roomUserByHabbo.GetClient().GetHabbo().Id +
                                          " LIMIT 1;UPDATE users_stats SET daily_respect_points = daily_respect_points - 1 WHERE id= " +
                                          Session.GetHabbo().Id + " LIMIT 1");
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("GiveRespectsMessageComposer"));
            serverMessage.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Id);
            serverMessage.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Respect);
            room.SendMessage(serverMessage);

            var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName);
            if (roomUser == null) return;
            var thumbsUp = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
            thumbsUp.AppendInteger(roomUser.VirtualId);
            thumbsUp.AppendInteger(7);
            room.SendMessage(thumbsUp);
        }

        /// <summary>
        /// Applies the effect.
        /// </summary>
        internal void ApplyEffect()
        {
            var effectId = Request.GetInteger();
            if (Session?.GetHabbo()?.CurrentRoom == null) return;
            var roomUserByHabbo = Session.GetHabbo().CurrentRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(Session.GetHabbo().UserName);
            if (roomUserByHabbo == null) return;
            if (!roomUserByHabbo.RidingHorse)
                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);
        }

        /// <summary>
        /// Enables the effect.
        /// </summary>
        internal void EnableEffect()
        {
            if (Session?.GetHabbo()?.CurrentRoom == null) return;

            var currentRoom = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = currentRoom?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var num = Request.GetInteger();
            if (roomUserByHabbo.RidingHorse)
                return;
            if (num == 0)
            {
                Session.GetHabbo()
                    .GetAvatarEffectsInventoryComponent()
                    .StopEffect(Session.GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect);
                return;
            }

            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateEffect(num);
        }

        /// <summary>
        /// Mutes the user.
        /// </summary>
        internal void MuteUser()
        {
            var num = Request.GetUInteger();
            Request.GetUInteger();
            var num2 = Request.GetUInteger();
            var currentRoom = Session.GetHabbo().CurrentRoom;
            if (currentRoom == null ||
                (currentRoom.RoomData.WhoCanBan == 0 && !currentRoom.CheckRights(Session, true) ||
                 currentRoom.RoomData.WhoCanBan == 1 && !currentRoom.CheckRights(Session)) && Session.GetHabbo().Rank <
                Convert.ToUInt32(Oblivion.GetDbConfig().DbData["ambassador.minrank"]))
                return;
            var roomUserByHabbo = currentRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(Oblivion.GetHabboById(num).UserName);
            if (roomUserByHabbo == null)
                return;
            if (roomUserByHabbo.GetClient().GetHabbo().Rank >= Session.GetHabbo().Rank)
                return;
            if (currentRoom.MutedUsers.TryGetValue(num, out var muted))
            {
                if (muted >= (ulong) Oblivion.GetUnixTimeStamp())
                    return;
                currentRoom.MutedUsers.Remove(num);
            }

            currentRoom.MutedUsers.Add(num,
                uint.Parse(
                    ((Oblivion.GetUnixTimeStamp()) + checked(num2 * 60u)).ToString()));

            Oblivion.GetGame()
                .GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, roomUserByHabbo.GetUserName(),
                    "Mute", "Muted user");
            roomUserByHabbo.GetClient()
                .SendNotif(string.Format(Oblivion.GetLanguage().GetVar("room_owner_has_mute_user"), num2));
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModMuteSeen", 1);
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        internal void GetUserInfo()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            GetResponse().AppendInteger(-1);
            GetResponse().AppendString(Session.GetHabbo().Look);
            GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
            GetResponse().AppendString(Session.GetHabbo().Motto);
            GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            SendResponse();
            GetResponse().Init(LibraryParser.OutgoingRequest("AchievementPointsMessageComposer"));
            GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            SendResponse();
        }

        /// <summary>
        /// Gets the balance.
        /// </summary>
        internal void GetBalance()
        {
            if (Session?.GetHabbo() == null) return;

            Session.GetHabbo().UpdateCreditsBalance();
            Session.GetHabbo().UpdateSeasonalCurrencyBalance();
        }

        /// <summary>
        /// Gets the subscription data.
        /// </summary>
        internal void GetSubscriptionData()
        {
            Session.GetHabbo().SerializeClub();
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        internal void LoadSettings()
        {
            var preferences = Session.GetHabbo().Preferences;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadVolumeMessageComposer"));

            serverMessage.AppendIntegersArray(preferences.Volume, ',', 3, 0, 100);

            serverMessage.AppendBool(preferences.PreferOldChat);
            serverMessage.AppendBool(preferences.IgnoreRoomInvite);
            serverMessage.AppendBool(preferences.DisableCameraFollow);
            serverMessage.AppendInteger(3); // collapse friends (3 = no)
            serverMessage.AppendInteger(preferences.ChatColor); //bubble
            Session.SendMessage(serverMessage);
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        internal void SaveSettings()
        {
            var num = Request.GetInteger();
            var num2 = Request.GetInteger();
            var num3 = Request.GetInteger();
            Session.GetHabbo().Preferences.Volume = num + "," + num2 + "," + num3;
        }

        /// <summary>
        /// Sets the chat preferrence.
        /// </summary>
        internal void SetChatPreferrence()
        {
            bool enable = Request.GetBool();
            Session.GetHabbo().Preferences.PreferOldChat = enable;
        }

        internal void SetInvitationsPreference()
        {
            bool enable = Request.GetBool();
            Session.GetHabbo().Preferences.IgnoreRoomInvite = enable;
        }

        internal void SetRoomCameraPreferences()
        {
            bool enable = Request.GetBool();
            Session.GetHabbo().Preferences.DisableCameraFollow = enable;
        }

        /// <summary>
        /// Gets the badges.
        /// </summary>
        internal void GetBadges()
        {
            Session?.SendMessage(Session?.GetHabbo()?.GetBadgeComponent()?.Serialize());
        }

        /// <summary>
        /// Updates the badges.
        /// </summary>
        internal void UpdateBadges()
        {
            Session.GetHabbo().GetBadgeComponent().ResetSlots();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(
                    $"UPDATE users_badges SET badge_slot = 0 WHERE user_id = {Session.GetHabbo().Id}");
            for (var i = 0; i < 5; i++)
            {
                var slot = Request.GetInteger();
                var code = Request.GetString();
                if (code.Length == 0) continue;
                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(code) || slot < 1 || slot > 5) return;
                Session.GetHabbo().GetBadgeComponent().GetBadge(code).Slot = slot;
                using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.SetQuery("UPDATE users_badges SET badge_slot = " + slot +
                                           " WHERE badge_id = @badge AND user_id = " + Session.GetHabbo().Id);
                    queryreactor2.AddParameter("badge", code);
                    queryreactor2.RunQuery();
                }
            }

            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileBadge);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            serverMessage.AppendInteger(Session.GetHabbo().Id);

            serverMessage.StartArray();
            /* TODO CHECK */
            foreach (
                var badge in
                Session.GetHabbo()
                    .GetBadgeComponent()
                    .BadgeList.Values
                    .Where(badge => badge.Slot > 0))
            {
                serverMessage.AppendInteger(badge.Slot);
                serverMessage.AppendString(badge.Code);

                serverMessage.SaveArray();
            }

            serverMessage.EndArray();
            if (Session.GetHabbo().InRoom &&
                Session.GetHabbo().CurrentRoom != null)
            {
                Oblivion.GetGame()
                    .GetRoomManager()
                    .GetRoom(Session.GetHabbo().CurrentRoomId)
                    .SendMessage(serverMessage);
                return;
            }

            Session.SendMessage(serverMessage);
        }

        /// <summary>
        /// Gets the achievements.
        /// </summary>
        internal void GetAchievements()
        {
            Oblivion.GetGame().GetAchievementManager().GetList(Session, Request);
        }

        /// <summary>
        /// Prepares the campaing.
        /// </summary>
        internal void PrepareCampaing()
        {
            var text = Request.GetString();
            Response.Init(LibraryParser.OutgoingRequest("SendCampaignBadgeMessageComposer"));
            Response.AppendString(text);
            Response.AppendBool(Session.GetHabbo().GetBadgeComponent().HasBadge(text));
            SendResponse();
        }

        /// <summary>
        /// Loads the profile.
        /// </summary>
        internal void LoadProfile()
        {
            var userId = Request.GetUInteger();
            Request.GetBool();

            if (Session?.GetHabbo()?.GetMessenger() == null) return;

            var habbo = Oblivion.GetHabboById(userId);
            if (habbo?.GetMessenger() == null)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("user_not_found"));
                return;
            }


            if (Session.GetHabbo().WebSocketConnId != Guid.Empty)
            {
                Oblivion.GetWebSocket().SendMessage(Session.GetHabbo().WebSocketConnId,
                    $"3|{habbo.UserName}|{habbo.Gender}|{habbo.Look}");
            }

            var createTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(habbo.CreateDate);

            var msg = new ServerMessage(LibraryParser.OutgoingRequest("UserProfileMessageComposer"));
            msg.AppendInteger(habbo.Id);
            msg.AppendString(habbo.UserName);
            msg.AppendString(habbo.Look);
            msg.AppendString(habbo.Motto);
            msg.AppendString(createTime.ToString("dd/MM/yyyy"));
            msg.AppendInteger(habbo.AchievementPoints);
            msg.AppendInteger(habbo.GetMessenger().Friends.Count);
            msg.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                           Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id));
            msg.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                           !Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id) &&
                           Session.GetHabbo().GetMessenger().RequestExists(habbo.Id));
            msg.AppendBool(Oblivion.GetGame().GetClientManager().GetClientByUserId(habbo.Id) != null);

            if (!habbo.LoadedGroups)
            {
                habbo.LoadGroups();
            }

            var groups = habbo.UserGroups;
            msg.AppendInteger(groups.Count);

            foreach (var group in groups.Select(groupUs =>
                Oblivion.GetGame().GetGroupManager().GetGroup(groupUs.GroupId)))
            {
                if (group != null)
                {
                    msg.AppendInteger(group.Id);
                    msg.AppendString(group.Name);
                    msg.AppendString(group.Badge);
                    msg.AppendString(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour1, true));
                    msg.AppendString(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour2, false));
                    msg.AppendBool(group.Id == habbo.FavouriteGroup);
                    msg.AppendInteger(-1);
                    msg.AppendBool(group.HasForum);
                }
                else
                {
                    msg.AppendInteger(1);
                    msg.AppendString("THIS GROUP IS INVALID");
                    msg.AppendString("");
                    msg.AppendString("");
                    msg.AppendString("");
                    msg.AppendBool(false);
                    msg.AppendInteger(-1);
                    msg.AppendBool(false);
                }
            }

            if (habbo.PreviousOnline == 0)
                msg.AppendInteger(-1);
            else if (Oblivion.GetGame().GetClientManager().GetClientByUserId(habbo.Id) == null)
                msg.AppendInteger((Oblivion.GetUnixTimeStamp() - habbo.PreviousOnline));
            else
                msg.AppendInteger((Oblivion.GetUnixTimeStamp() - habbo.LastOnline));

            msg.AppendBool(true);
            Session.SendMessage(msg);

            if (habbo.GetBadgeComponent()?.BadgeList == null) return;

            var msg2 = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            msg2.AppendInteger(habbo.Id);

            msg2.StartArray();
            foreach (var badge in habbo.GetBadgeComponent().BadgeList.Values.Where(badge => badge.Slot > 0))
            {
                msg2.AppendInteger(badge.Slot);
                msg2.AppendString(badge.Code);
                msg2.SaveArray();
            }

            msg2.EndArray();

            Session.SendMessage(msg2);
        }

        /// <summary>
        /// Changes the look.
        /// </summary>
        internal void ChangeLook()
        {
            var text = Request.GetString().ToUpper();
            var text2 = Request.GetString();
            text2 = Oblivion.FilterFigure(text2);

            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileChangeLook);
            Session.GetHabbo().Look = text2;
            Session.GetHabbo().Gender = text.ToLower() == "f" ? "f" : "m";
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"UPDATE users SET look = @look, gender = @gender WHERE id = {Session.GetHabbo().Id}");
                queryReactor.AddParameter("look", text2);
                queryReactor.AddParameter("gender", text);
                queryReactor.RunQuery();
            }

            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_AvatarLooks", 1);
            if (Session.GetHabbo().Look.Contains("ha-1006"))
                Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WearHat);
            Session.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateAvatarAspectMessageComposer"));
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToUpper());
            Session.GetMessageHandler().SendResponse();
            Session.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            Session.GetMessageHandler().GetResponse().AppendInteger(-1);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
            Session.GetMessageHandler().GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            Session.GetMessageHandler().SendResponse();
            if (!Session.GetHabbo().InRoom)
                return;
            var currentRoom = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = currentRoom?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            serverMessage.AppendInteger(roomUserByHabbo.VirtualId); //BUGG
            //serverMessage.AppendInt32(-1);
            serverMessage.AppendString(Session.GetHabbo().Look);
            serverMessage.AppendString(Session.GetHabbo().Gender.ToLower());
            serverMessage.AppendString(Session.GetHabbo().Motto);
            serverMessage.AppendInteger(Session.GetHabbo().AchievementPoints);
            currentRoom.SendMessage(serverMessage);

            if (Session.GetHabbo().GetMessenger() != null) Session.GetHabbo().GetMessenger().OnStatusChanged(true);
        }

        /// <summary>
        /// Changes the motto.
        /// </summary>
        internal void ChangeMotto()
        {
            var text = Request.GetString();
            if (text == Session.GetHabbo().Motto)
                return;


            if (!BobbaFilter.CanTalk(Session, text))
            {
                return;
            }

            Session.GetHabbo().Motto = text;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"UPDATE users SET motto = @motto WHERE id = '{Session.GetHabbo().Id}'");
                queryReactor.AddParameter("motto", text);
                queryReactor.RunQuery();
            }

            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileChangeMotto);
            if (Session.GetHabbo().InRoom)
            {
                var currentRoom = Session.GetHabbo().CurrentRoom;
                var roomUserByHabbo = currentRoom?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (roomUserByHabbo == null)
                    return;
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                serverMessage.AppendInteger(roomUserByHabbo.VirtualId); //BUGG
                //serverMessage.AppendInt32(-1);
                serverMessage.AppendString(Session.GetHabbo().Look);
                serverMessage.AppendString(Session.GetHabbo().Gender.ToLower());
                serverMessage.AppendString(Session.GetHabbo().Motto);
                serverMessage.AppendInteger(Session.GetHabbo().AchievementPoints);
                currentRoom.SendMessage(serverMessage);
            }

            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Motto", 1);
        }

        /// <summary>
        /// Gets the wardrobe.
        /// </summary>
        internal void GetWardrobe()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("LoadWardrobeMessageComposer"));
            GetResponse().AppendInteger(0);
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"SELECT slot_id, look, gender FROM users_wardrobe WHERE user_id = {Session.GetHabbo().Id}");
                var table = queryReactor.GetTable();
                if (table == null)
                    GetResponse().AppendInteger(0);
                else
                {
                    GetResponse().AppendInteger(table.Rows.Count);
                    /* TODO CHECK */
                    foreach (DataRow dataRow in table.Rows)
                    {
                        GetResponse().AppendInteger(Convert.ToUInt32(dataRow["slot_id"]));
                        GetResponse().AppendString((string) dataRow["look"]);
                        GetResponse().AppendString(dataRow["gender"].ToString().ToUpper());
                    }
                }

                SendResponse();
            }
        }

        /// <summary>
        /// Saves the wardrobe.
        /// </summary>
        internal void SaveWardrobe()
        {
            var num = Request.GetUInteger();
            var text = Request.GetString();
            var text2 = Request.GetString().ToUpper() == "F" ? "F" : "M";

            text = Oblivion.FilterFigure(text);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Concat(
                    "REPLACE INTO users_wardrobe (user_id,slot_id,look,gender) VALUES (", Session.GetHabbo().Id, ",",
                    num, ",@look,@gender) ON DUPLICATE KEY UPDATE SET look = @look, gender = @gender"));
                queryReactor.AddParameter("look", text);
                queryReactor.AddParameter("gender", text2);
                queryReactor.RunQuery();
            }


            Oblivion.GetGame()
                .GetQuestManager()
                .ProgressUserQuest(Session, QuestType.ProfileChangeLook);
        }

        /// <summary>
        /// Gets the pets inventory.
        /// </summary>
        internal void GetPetsInventory()
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
                return;
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        /// <summary>
        /// Gets the bots inventory.
        /// </summary>
        internal void GetBotsInventory()
        {
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
            SendResponse();
        }


        /// <summary>
        /// Checks the name.
        /// </summary>
        internal void CheckName()
        {
            var text = Request.GetString();
            if (string.Equals(text, Session.GetHabbo().UserName, StringComparison.CurrentCultureIgnoreCase))
            {
                Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                Response.AppendInteger(0);
                Response.AppendString(text);
                Response.AppendInteger(0);
                SendResponse();
                return;
            }

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT username FROM users WHERE username=@name LIMIT 1");
                queryReactor.AddParameter("name", text);
                var @string = queryReactor.GetString();
                var lower = text.ToLower();
                var array = lower.ToCharArray();
                const string source = "abcdefghijklmnopqrstuvwxyz1234567890.,_-;:?!@";
                if (array.Any(c => !source.Contains(char.ToLower(c))))
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(4);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                    return;
                }

                if (lower.Contains("mod") || lower.Contains("m0d") ||
                    lower.Contains("admin"))
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(4);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else if (text.Length > 15)
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(3);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else if (text.Length < 3)
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(2);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else if (string.IsNullOrWhiteSpace(@string))
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(0);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else
                {
                    queryReactor.SetQuery("SELECT tag FROM users_tags ORDER BY RAND() LIMIT 3");
                    var table = queryReactor.GetTable();
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(5);
                    Response.AppendString(text);
                    Response.AppendInteger(table.Rows.Count);
                    /* TODO CHECK */
                    foreach (DataRow dataRow in table.Rows)
                        Response.AppendString($"{text}{dataRow[0]}");
                    SendResponse();
                }
            }
        }

        /// <summary>
        /// Changes the name.
        /// </summary>
        internal void ChangeName()
        {
            var text = Request.GetString();
            if (text == null) return;
            var userName = Session.GetHabbo().UserName;
            if (Session?.GetHabbo()?.CurrentRoom == null) return;

            if (string.IsNullOrWhiteSpace(text) || text.Length < 3 || text.Length > 15)
                return;

            var lower = text.ToLower();
            var array = lower.ToCharArray();
            const string source = "abcdefghijklmnopqrstuvwxyz1234567890.,_-;:?!@";
            if (array.Any(c => !source.Contains(char.ToLower(c))))
            {
                return;
            }

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT username FROM users WHERE username=@name LIMIT 1");
                queryReactor.AddParameter("name", text);
                var String = queryReactor.GetString();

                if (!string.IsNullOrWhiteSpace(String) &&
                    !string.Equals(userName, text, StringComparison.CurrentCultureIgnoreCase))
                    return;
                queryReactor.SetQuery("UPDATE rooms_data SET owner = @newowner WHERE owner = @oldowner");
                queryReactor.AddParameter("newowner", text);
                queryReactor.AddParameter("oldowner", Session.GetHabbo().UserName);
                queryReactor.RunQuery();

                queryReactor.SetQuery(
                    "UPDATE users SET username = @newname, last_name_change = @timestamp WHERE id = @userid");
                queryReactor.AddParameter("newname", text);
                queryReactor.AddParameter("timestamp", Oblivion.GetUnixTimeStamp() + 43200);
                queryReactor.AddParameter("userid", Session.GetHabbo().Id);
                queryReactor.RunQuery();

                Session.GetHabbo().LastChange = Oblivion.GetUnixTimeStamp() + 43200;
                Session.GetHabbo().UserName = text;
                Response.Init(LibraryParser.OutgoingRequest("UpdateUsernameMessageComposer"));
                Response.AppendInteger(0);
                Response.AppendString(text);
                Response.AppendInteger(0);
                SendResponse();
                Response.Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                Response.AppendInteger(-1);
                Response.AppendString(Session.GetHabbo().Look);
                Response.AppendString(Session.GetHabbo().Gender.ToLower());
                Response.AppendString(Session.GetHabbo().Motto);
                Response.AppendInteger(Session.GetHabbo().AchievementPoints);
                SendResponse();
                Session.GetHabbo().CurrentRoom.GetRoomUserManager().UpdateUser(userName, text);
                if (Session.GetHabbo().CurrentRoom != null)
                {
                    Response.Init(LibraryParser.OutgoingRequest("UserUpdateNameInRoomMessageComposer"));
                    Response.AppendInteger(Session.GetHabbo().Id);
                    Response.AppendInteger(Session.GetHabbo().CurrentRoom.RoomId);
                    Response.AppendString(text);
                }

                /* TODO CHECK */
                foreach (var data in Session.GetHabbo().Data.Rooms.ToList())
                {
                    var current = Oblivion.GetGame().GetRoomManager().GenerateRoomData(data);
                    current.Owner = text;
                    current.SerializeRoomData(Response, Session, false, true);
                    var room = Oblivion.GetGame().GetRoomManager().GetRoom(current.Id);
                    if (room != null)
                        room.RoomData.Owner = text;
                }

                /* TODO CHECK */
                foreach (MessengerBuddy current2 in Session.GetHabbo().GetMessenger().Friends.Values.ToList())
                {
                    if (current2.Client?.GetHabbo() != null)
                    {
                        var list = current2.Client.GetHabbo()
                            .GetMessenger()?
                            .Friends?.Values.Where(x => x.UserName == userName).ToList();
                        if (list == null) continue;
                        foreach (var current3 in list)
                        {
                            current3.UserName = text;
                            current3.Serialize(Response, Session);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        internal void GetRelationships()
        {
            var userId = Request.GetUInteger();
            var habboForId = Oblivion.GetHabboById(userId);

            if (habboForId?.Data == null)
                return;

            if (!habboForId.Data.LoadedRelations)
            {
                habboForId.Data.LoadRelations();
            }

            if (habboForId.Data.Relations == null) return;
            var rand = new Random();
            var num = 0;
            var num2 = 0;
            var num3 = 0;
            foreach (var x in habboForId.Data.Relations)
            {
                if (!habboForId.GetMessenger().FriendshipExists((uint) x.Value.UserId))
                    continue;

                if (x.Value.Type == 1) num++;
                else if (x.Value.Type == 2) num2++;
                else if (x.Value.Type == 3) num3++;
            }

            Response.Init(LibraryParser.OutgoingRequest("RelationshipMessageComposer"));
            Response.AppendInteger(habboForId.Id);

            Response.StartArray();
//            Response.AppendInteger(habboForId.Data.Relations.Count);

            foreach (var current in habboForId.Data.Relations.Values.OrderBy(x => rand.Next()))
            {
                if (!habboForId.GetMessenger().FriendshipExists((uint) current.UserId))
                    continue;

                var habboForId2 = Oblivion.GetHabboById(Convert.ToUInt32(current.UserId));
                if (habboForId2 == null)
                {
                    Response.AppendInteger(0);
                    Response.AppendInteger(0);
                    Response.AppendInteger(0);
                    Response.AppendString("Placeholder");
                    Response.AppendString("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62");
                }
                else
                {
                    Response.AppendInteger(current.Type);
                    Response.AppendInteger((current.Type == 1) ? num : ((current.Type == 2) ? num2 : num3));
                    Response.AppendInteger(current.UserId);
                    Response.AppendString(habboForId2.UserName);
                    Response.AppendString(habboForId2.Look);
                }

                Response.SaveArray();
            }

            Response.EndArray();
            SendResponse();
        }

        /// <summary>
        /// Sets the relationship.
        /// </summary>
        internal void SetRelationship()
        {
            var num = Request.GetUInteger();
            var num2 = Request.GetInteger();

            {
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    if (num2 == 0)
                    {
                        queryReactor.SetQuery(
                            "SELECT id FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        var integer = queryReactor.GetInteger();
                        queryReactor.SetQuery(
                            "DELETE FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        queryReactor.RunQuery();
                        if (Session.GetHabbo().Data.Relations.ContainsKey(integer))
                            Session.GetHabbo().Data.Relations.Remove(integer);
                    }
                    else
                    {
                        queryReactor.SetQuery(
                            "SELECT id FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        var integer2 = queryReactor.GetInteger();
                        if (integer2 > 0)
                        {
                            queryReactor.SetQuery(
                                "DELETE FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                            queryReactor.AddParameter("id", Session.GetHabbo().Id);
                            queryReactor.AddParameter("target", num);
                            queryReactor.RunQuery();
                            if (Session.GetHabbo().Data.Relations.ContainsKey(integer2))
                                Session.GetHabbo().Data.Relations.Remove(integer2);
                        }

                        queryReactor.SetQuery(
                            "INSERT INTO users_relationships (user_id, target, type) VALUES (@id, @target, @type)");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        queryReactor.AddParameter("type", num2);
                        var num3 = (int) queryReactor.InsertQuery();
                        Session.GetHabbo().Data.Relations.Add(num3, new Relationship(num3, (int) num, num2));
                    }

                    var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(num);
                    Session.GetHabbo().GetMessenger().UpdateFriend(num, clientByUserId, true);
                }
            }
        }

        /// <summary>
        /// Starts the quest.
        /// </summary>
        public void StartQuest()
        {
            Oblivion.GetGame().GetQuestManager().ActivateQuest(Session, Request);
        }

        /// <summary>
        /// Stops the quest.
        /// </summary>
        public void StopQuest()
        {
            Oblivion.GetGame().GetQuestManager().CancelQuest(Session, Request);
        }

        /// <summary>
        /// Gets the current quest.
        /// </summary>
        public void GetCurrentQuest()
        {
            Oblivion.GetGame().GetQuestManager().GetCurrentQuest(Session, Request);
        }

        /// <summary>
        /// Starts the seasonal quest.
        /// </summary>
        public void StartSeasonalQuest()
        {
            RoomData roomData;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var quest = Oblivion.GetGame().GetQuestManager().GetQuest(Request.GetUInteger());
                if (quest == null)
                    return;
                queryReactor.RunFastQuery(string.Concat("REPLACE INTO users_quests_data(user_id,quest_id) VALUES (",
                    Session.GetHabbo().Id, ", ", quest.Id, ")"));
                queryReactor.RunFastQuery(string.Concat("UPDATE users_stats SET quest_id = ", quest.Id, " WHERE id = ",
                    Session.GetHabbo().Id));
                Session.GetHabbo().CurrentQuestId = quest.Id;
                Session.SendMessage(QuestStartedComposer.Compose(Session, quest));
                Oblivion.GetGame().GetQuestManager().ActivateQuest(Session, Request);
//                queryReactor.SetQuery("SELECT id FROM rooms_data WHERE state='open' ORDER BY users_now DESC LIMIT 1");
//                var @string = queryReactor.GetString();

                roomData = Oblivion.GetGame().GetRoomManager().GetActiveRooms()
                    .FirstOrDefault(x => x.Key.UsersNow > 0 && x.Key.State == 0).Key;
            }

            if (roomData != null)
            {
                roomData.SerializeRoomData(Response, Session, true);
                Session.GetMessageHandler().PrepareRoomForUser(roomData.Id, "");
                return;
            }

            Session.SendNotif(Oblivion.GetLanguage().GetVar("start_quest_need_room"));
        }

        /// <summary>
        /// Receives the nux gifts.
        /// </summary>
        public void ReceiveNuxGifts()
        {
            if (!ExtraSettings.NewUsersGiftsEnabled)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("nieuwe_gebruiker_kado_error_1"));
                return;
            }

            if (Session.GetHabbo().Vip)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("nieuwe_gebruiker_kado_error_2"));
                return;
            }

            var item = Session.GetHabbo().GetInventoryComponent()
                .AddNewItem("0", ExtraSettings.NewUserGiftYttv2Id, "", 0, true, false, 0, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            Session.GetHabbo().Diamonds += 25;
            Session.GetHabbo().UpdateSeasonalCurrencyBalance();
            if (item != null)
                Session.GetHabbo().GetInventoryComponent().SendNewItems(item.VirtualId);

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                dbClient.RunFastQuery(
                    Session.GetHabbo().Vip
                        ? $"UPDATE users SET vip = '1', vip_expire = DATE_ADD(vip_expire, INTERVAL 1 DAY) WHERE id = {Session.GetHabbo().Id}"
                        : $"UPDATE users SET vip = '1', vip_expire = DATE_ADD(NOW(), INTERVAL 1 DAY) WHERE id = {Session.GetHabbo().Id}");

            Session.GetHabbo().Vip = true;
        }

        /// <summary>
        /// Accepts the nux gifts.
        /// </summary>
        public void AcceptNuxGifts()
        {
            if (ExtraSettings.NewUsersGiftsEnabled == false || Request.GetInteger() != 0)
                return;

            var nuxGifts = new ServerMessage(LibraryParser.OutgoingRequest("NuxListGiftsMessageComposer"));
            nuxGifts.AppendInteger(3); //Cantidad

            nuxGifts.AppendInteger(0);
            nuxGifts.AppendInteger(0);
            nuxGifts.AppendInteger(1); //Cantidad
            // ahora nuevo bucle
            nuxGifts.AppendString("");
            nuxGifts.AppendString("nux/gift_yttv2.png");
            nuxGifts.AppendInteger(1); //cantidad
            //Ahora nuevo bucle...
            nuxGifts.AppendString("yttv2");
            nuxGifts.AppendString("");

            nuxGifts.AppendInteger(2);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("");
            nuxGifts.AppendString("nux/gift_diamonds.png");
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("nux_gift_diamonds");
            nuxGifts.AppendString("");

            nuxGifts.AppendInteger(3);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("");
            nuxGifts.AppendString("nux/gift_vip1day.png");
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("nux_gift_vip_1_day");
            nuxGifts.AppendString("");

            Session.SendMessage(nuxGifts);
        }

        /// <summary>
        /// Talentses this instance.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        internal void Talents()
        {
            var trackType = Request.GetString();
            var talents = Oblivion.GetGame().GetTalentManager().GetTalents(trackType, -1);
            var failLevel = -1;

            if (talents == null)
                return;

            Response.Init(LibraryParser.OutgoingRequest("TalentsTrackMessageComposer"));
            Response.AppendString(trackType);
            Response.AppendInteger(talents.Count);

            /* TODO CHECK */
            foreach (var current in talents)
            {
                Response.AppendInteger(current.Level);

                var nm = (failLevel == -1) ? 1 : 0;

                Response.AppendInteger(nm);

                var talents2 = Oblivion.GetGame().GetTalentManager().GetTalents(trackType, current.Id);
                Response.AppendInteger(talents2.Count);

                /* TODO CHECK */
                foreach (var current2 in talents2)
                {
                    var userAchievement = Session.GetHabbo().GetAchievementData(current2.AchievementGroup);
                    var num = (failLevel != -1 && failLevel < current2.Level)
                        ? 0
                        : (Session.GetHabbo().GetAchievementData(current2.AchievementGroup) == null)
                            ? 1
                            : userAchievement != null && userAchievement.Level >= current2.AchievementLevel
                                ? 2
                                : 1;
                    Response.AppendInteger(current2.GetAchievement().Id);
                    Response.AppendInteger(0);
                    Response.AppendString($"{current2.AchievementGroup}{current2.AchievementLevel}");
                    Response.AppendInteger(num);

                    var achievementData = Session.GetHabbo().GetAchievementData(current2.AchievementGroup);

                    Response.AppendInteger(achievementData?.Progress ?? 0);
                    Response.AppendInteger(current2.GetAchievement().Levels[current2.AchievementLevel].Requirement);

                    if (num != 2 && failLevel == -1)
                        failLevel = current2.Level;
                }

                Response.AppendInteger(0);

                if (current.Type == "citizenship" && current.Level == 4)
                {
                    Response.AppendInteger(2);
                    Response.AppendString("HABBO_CLUB_VIP_7_DAYS");
                    Response.AppendInteger(7);
                    Response.AppendString(current.Prize);
                    Response.AppendInteger(0);
                }
                else
                {
                    Response.AppendInteger(1);
                    Response.AppendString(current.Prize);
                    Response.AppendInteger(0);
                }
            }

            SendResponse();
        }

        /// <summary>
        /// Completes the safety quiz.
        /// </summary>
        internal void CompleteSafetyQuiz()
        {
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SafetyQuizGraduate", 1);
            Session.SendMessage(new ServerMessage(2873));
        }

        /// <summary>
        /// Hotels the view countdown.
        /// </summary>
        internal void HotelViewCountdown()
        {
            string time = Request.GetString();
            DateTime.TryParse(time, out var date);
            TimeSpan diff = date - DateTime.Now;
            Response.Init(LibraryParser.OutgoingRequest("HotelViewCountdownMessageComposer"));
            Response.AppendString(time);
            Response.AppendInteger(Convert.ToInt32(diff.TotalSeconds));
            SendResponse();
        }

        /// <summary>
        /// Hotels the view dailyquest.
        /// </summary>
        internal void HotelViewDailyquest()
        {
        }

        internal void FindMoreFriends()
        {
            var allRooms = Oblivion.GetGame().GetRoomManager().GetActiveRooms();
            if (allRooms != null)
            {
                Random rnd = new Random();
                var randomRoom = allRooms[rnd.Next(allRooms.Length)].Key;
                var success = new ServerMessage(LibraryParser.OutgoingRequest("FindMoreFriendsSuccessMessageComposer"));
                if (randomRoom == null)
                {
                    success.AppendBool(false);
                    Session.SendMessage(success);
                    return;
                }

                success.AppendBool(true);
                Session.SendMessage(success);
                var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                roomFwd.AppendInteger(randomRoom.Id);
                Session.SendMessage(roomFwd);
            }
        }

        internal void HotelViewRequestBadge()
        {
            string name = Request.GetString();
            var hotelViewBadges = Oblivion.GetGame().GetHotelView().HotelViewBadges;
            if (!hotelViewBadges.TryGetValue(name, out var badge))
                return;
            Session.GetHabbo().GetBadgeComponent().GiveBadge(badge, true, Session, true);
        }

        internal void GetCameraPrice()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("SetCameraPriceMessageComposer"));
            GetResponse().AppendInteger(Oblivion.GetGame().GetCameraManager().PurchaseCoinsPrice); //credits
            GetResponse().AppendInteger(Oblivion.GetGame().GetCameraManager().PurchaseDucketsPrice); //duckets
            GetResponse().AppendInteger(Oblivion.GetGame().GetCameraManager().PublishDucketsPrice); //duckets publish
            SendResponse();
        }

        internal void GetHotelViewHallOfFame()
        {
            string code = Request.GetString();
            GetResponse().Init(LibraryParser.OutgoingRequest("HotelViewHallOfFameMessageComposer"));
            GetResponse().AppendString(code);
            var rankings = Oblivion.GetGame().GetHallOfFame().Rankings;
            GetResponse().StartArray();
            int rank = 1;
            /* TODO CHECK */
            foreach (HallOfFameElement element in rankings)
            {
                GetResponse().AppendInteger(element.UserId);
                GetResponse().AppendString(element.Username);
                GetResponse().AppendString(element.Look);
                GetResponse().AppendInteger(rank);
                GetResponse().AppendInteger(element.Score);
                rank++;
                GetResponse().SaveArray();
            }

            GetResponse().EndArray();
            SendResponse();
        }

        internal void FriendRequestListLoad()
        {
        }
    }
}