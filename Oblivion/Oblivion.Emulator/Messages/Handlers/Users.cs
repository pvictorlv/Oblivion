using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task SendBullyReport()
        {
            var reportedId = Request.GetUInteger();
            await Oblivion.GetGame()
                .GetModerationTool()
                .SendNewTicket(Session, 104, 9, reportedId, "", new List<string>());

            await Response.InitAsync(LibraryParser.OutgoingRequest("BullyReportSentMessageComposer"));
            await Response.AppendIntegerAsync(0);
            await SendResponse();
        }

        /// <summary>
        /// Opens the bully reporting.
        /// </summary>
        public async Task OpenBullyReporting()
        {
            await Response.InitAsync(LibraryParser.OutgoingRequest("OpenBullyReportMessageComposer"));
            await Response.AppendIntegerAsync(0);
            await SendResponse();
        }

        /// <summary>
        /// Opens the quests.
        /// </summary>
        public async Task OpenQuests()
        {
          await  Oblivion.GetGame().GetQuestManager().GetList(Session, Request);
        }

        /// <summary>
        /// Retrieves the citizenship.
        /// </summary>
        internal async Task RetrieveCitizenship()
        {
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("CitizenshipStatusMessageComposer"));
            await GetResponse().AppendStringAsync(Request.GetString());
            await GetResponse().AppendIntegerAsync(4);
            await GetResponse().AppendIntegerAsync(4);
            await SendResponse();
        }

        /// <summary>
        /// Loads the club gifts.
        /// </summary>
        internal async Task LoadClubGifts()
        {
            if (Session?.GetHabbo() == null)
                return;
            //var i = 0;
            //var i2 = 0;
            Session.GetHabbo().GetSubscriptionManager().GetSubscription();
            var serverMessage = new ServerMessage();
            await serverMessage.InitAsync(LibraryParser.OutgoingRequest("LoadCatalogClubGiftsMessageComposer"));
            await serverMessage.AppendIntegerAsync(0); // i
            await serverMessage.AppendIntegerAsync(0); // i2
            await serverMessage.AppendIntegerAsync(1);
            await Session.SendMessageAsync(serverMessage);
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
        internal async Task GetUserTags()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo?.GetClient()?.GetHabbo()?.Data?.Tags == null || roomUserByHabbo.IsBot)
                return;
            await Response.InitAsync(LibraryParser.OutgoingRequest("UserTagsMessageComposer"));
            await Response.AppendIntegerAsync(roomUserByHabbo.GetClient().GetHabbo().Id);
            await Response.AppendIntegerAsync(roomUserByHabbo.GetClient().GetHabbo().Data.Tags.Count);
            foreach (var current in roomUserByHabbo.GetClient().GetHabbo().Data.Tags)
                await Response.AppendStringAsync(current);
            await SendResponse();

            if (Session != roomUserByHabbo.GetClient())
                return;
            if (Session.GetHabbo().Data.Tags.Count >= 5)
                await Oblivion.GetGame()
                    .GetAchievementManager()
                    .ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_UserTags", 5);
        }

        /// <summary>
        /// Gets the user badges.
        /// </summary>
        internal async Task GetUserBadges()
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
                await msg.AppendIntegerAsync(roomUserByHabbo.GetClient().GetHabbo().Id);

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
                    await msg.AppendIntegerAsync(badge.Slot);
                    await msg.AppendStringAsync(badge.Code);

                    msg.SaveArray();
                }

                msg.EndArray();
                await Session.SendMessageAsync(msg);
//                await SendResponse();
            }
        }

        /// <summary>
        /// Gives the respect.
        /// </summary>
        internal async Task GiveRespect()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || Session.GetHabbo().DailyRespectPoints <= 0)
                return;
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo?.GetClient()?.GetHabbo() == null) return;

            if (roomUserByHabbo.GetClient().GetHabbo().Id == Session.GetHabbo().Id ||
                roomUserByHabbo.IsBot)
                return;
            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialRespect);
            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_RespectGiven", 1, true);
            await Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_RespectEarned", 1, true);
            Session.GetHabbo().DailyRespectPoints--;
            roomUserByHabbo.GetClient().GetHabbo().Respect++;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync("UPDATE users_stats SET respect = respect + 1 WHERE id = " +
                                                     roomUserByHabbo.GetClient().GetHabbo().Id +
                                                     " LIMIT 1;UPDATE users_stats SET daily_respect_points = daily_respect_points - 1 WHERE id= " +
                                                     Session.GetHabbo().Id + " LIMIT 1");
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("GiveRespectsMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.GetClient().GetHabbo().Id);
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.GetClient().GetHabbo().Respect);
            await room.SendMessage(serverMessage);

            var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName);
            if (roomUser == null) return;
            var thumbsUp = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
            await thumbsUp.AppendIntegerAsync(roomUser.VirtualId);
            await thumbsUp.AppendIntegerAsync(7);
            await room.SendMessage(thumbsUp);
        }

        /// <summary>
        /// Applies the effect.
        /// </summary>
        internal async Task ApplyEffect()
        {
            var effectId = Request.GetInteger();
            if (Session?.GetHabbo()?.CurrentRoom == null) return;
            var roomUserByHabbo = Session.GetHabbo().CurrentRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(Session.GetHabbo().UserName);
            if (roomUserByHabbo == null) return;
            if (!roomUserByHabbo.RidingHorse)
                await Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);
        }

        /// <summary>
        /// Enables the effect.
        /// </summary>
        internal async Task EnableEffect()
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
                await Session.GetHabbo()
                    .GetAvatarEffectsInventoryComponent()
                    .StopEffect(Session.GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect);
                return;
            }

            await Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateEffect(num);
        }

        /// <summary>
        /// Mutes the user.
        /// </summary>
        internal async Task MuteUser()
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

            await Oblivion.GetGame()
                .GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, roomUserByHabbo.GetUserName(),
                    "Mute", "Muted user");
            await roomUserByHabbo.GetClient()
                .SendNotif(string.Format(Oblivion.GetLanguage().GetVar("room_owner_has_mute_user"), num2));
            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModMuteSeen", 1);
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        internal async Task GetUserInfo()
        {
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            await GetResponse().AppendIntegerAsync(-1);
            await GetResponse().AppendStringAsync(Session.GetHabbo().Look);
            await GetResponse().AppendStringAsync(Session.GetHabbo().Gender.ToLower());
            await GetResponse().AppendStringAsync(Session.GetHabbo().Motto);
            await GetResponse().AppendIntegerAsync(Session.GetHabbo().AchievementPoints);
            await SendResponse();
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("AchievementPointsMessageComposer"));
            await GetResponse().AppendIntegerAsync(Session.GetHabbo().AchievementPoints);
            await SendResponse();
        }

        /// <summary>
        /// Gets the balance.
        /// </summary>
        internal async Task GetBalance()
        {
            if (Session?.GetHabbo() == null) return;

            await Session.GetHabbo().UpdateCreditsBalance();
            await Session.GetHabbo().UpdateSeasonalCurrencyBalance();
        }

        /// <summary>
        /// Gets the subscription data.
        /// </summary>
        internal async Task GetSubscriptionData()
        {
            await Session.GetHabbo().SerializeClub();
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        internal async Task LoadSettings()
        {
            var preferences = Session.GetHabbo().Preferences;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadVolumeMessageComposer"));

            serverMessage.AppendIntegersArray(preferences.Volume, ',', 3, 0, 100);

            serverMessage.AppendBool(preferences.PreferOldChat);
            serverMessage.AppendBool(preferences.IgnoreRoomInvite);
            serverMessage.AppendBool(preferences.DisableCameraFollow);
            await serverMessage.AppendIntegerAsync(0); // collapse friends (3 = no)
            await serverMessage.AppendIntegerAsync(preferences.ChatColor); //bubble
            await Session.SendMessageAsync(serverMessage);
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
        internal async Task GetBadges()
        {
            await Session.SendMessage(Session?.GetHabbo()?.GetBadgeComponent()?.Serialize());
        }

        /// <summary>
        /// Updates the badges.
        /// </summary>
        internal async Task UpdateBadges()
        {
            await Session.GetHabbo().GetBadgeComponent().ResetSlots();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(
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
                    await queryreactor2.RunQueryAsync();
                }
            }

            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileBadge);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            await serverMessage.AppendIntegerAsync(Session.GetHabbo().Id);

            serverMessage.StartArray();
            /* TODO CHECK */
            foreach (
                var badge in
                Session.GetHabbo()
                    .GetBadgeComponent()
                    .BadgeList.Values
                    .Where(badge => badge.Slot > 0))
            {
                await serverMessage.AppendIntegerAsync(badge.Slot);
                await serverMessage.AppendStringAsync(badge.Code);

                serverMessage.SaveArray();
            }

            serverMessage.EndArray();
            if (Session.GetHabbo().InRoom &&
                Session.GetHabbo().CurrentRoom != null)
            {
                await Oblivion.GetGame()
                    .GetRoomManager()
                    .GetRoom(Session.GetHabbo().CurrentRoomId)
                    .SendMessage(serverMessage);
                return;
            }

            await Session.SendMessageAsync(serverMessage);
        }

        /// <summary>
        /// Gets the achievements.
        /// </summary>
        internal async Task GetAchievements()
        {
            await Oblivion.GetGame().GetAchievementManager().GetList(Session, Request);
        }

        /// <summary>
        /// Prepares the campaing.
        /// </summary>
        internal async Task PrepareCampaing()
        {
            var text = Request.GetString();
            await Response.InitAsync(LibraryParser.OutgoingRequest("SendCampaignBadgeMessageComposer"));
            await Response.AppendStringAsync(text);
            Response.AppendBool(Session.GetHabbo().GetBadgeComponent().HasBadge(text));
            await SendResponse();
        }

        /// <summary>
        /// Loads the profile.
        /// </summary>
        internal async Task LoadProfile()
        {
            var userId = Request.GetUInteger();
            Request.GetBool();

            if (Session?.GetHabbo()?.GetMessenger() == null) return;

            var habbo = Oblivion.GetHabboById(userId);
            if (habbo?.GetMessenger() == null)
            {
                await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return;
            }


            if (Session.GetHabbo().WebSocketConnId != Guid.Empty)
            {
                Oblivion.GetWebSocket().SendMessage(Session.GetHabbo().WebSocketConnId,
                    $"3|{habbo.UserName}|{habbo.Gender}|{habbo.Look}");
            }

            var createTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(habbo.CreateDate);

            var msg = new ServerMessage(LibraryParser.OutgoingRequest("UserProfileMessageComposer"));
            await msg.AppendIntegerAsync(habbo.Id);
            await msg.AppendStringAsync(habbo.UserName);
            await msg.AppendStringAsync(habbo.Look);
            await msg.AppendStringAsync(habbo.Motto);
            await msg.AppendStringAsync(createTime.ToString("dd/MM/yyyy"));
            await msg.AppendIntegerAsync(habbo.AchievementPoints);
            await msg.AppendIntegerAsync(habbo.GetMessenger().Friends.Count);
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
            await msg.AppendIntegerAsync(groups.Count);

            foreach (var group in groups.Select(groupUs =>
                Oblivion.GetGame().GetGroupManager().GetGroup(groupUs.GroupId)))
            {
                if (group != null)
                {
                    await msg.AppendIntegerAsync(group.Id);
                    await msg.AppendStringAsync(group.Name);
                    await msg.AppendStringAsync(group.Badge);
                    await msg.AppendStringAsync(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour1, true));
                    await msg.AppendStringAsync(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour2, false));
                    msg.AppendBool(group.Id == habbo.FavouriteGroup);
                    await msg.AppendIntegerAsync(-1);
                    msg.AppendBool(group.HasForum);
                }
                else
                {
                    await msg.AppendIntegerAsync(1);
                    await msg.AppendStringAsync("THIS GROUP IS INVALID");
                    await msg.AppendStringAsync("");
                    await msg.AppendStringAsync("");
                    await msg.AppendStringAsync("");
                    msg.AppendBool(false);
                    await msg.AppendIntegerAsync(-1);
                    msg.AppendBool(false);
                }
            }

            if (habbo.PreviousOnline == 0)
                await msg.AppendIntegerAsync(-1);
            else if (Oblivion.GetGame().GetClientManager().GetClientByUserId(habbo.Id) == null)
                await msg.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - habbo.PreviousOnline));
            else
                await msg.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - habbo.LastOnline));

            msg.AppendBool(true);
            await Session.SendMessageAsync(msg);

            if (habbo.GetBadgeComponent()?.BadgeList == null) return;

            var msg2 = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            await msg2.AppendIntegerAsync(habbo.Id);

            msg2.StartArray();
            foreach (var badge in habbo.GetBadgeComponent().BadgeList.Values.Where(badge => badge.Slot > 0))
            {
                await msg2.AppendIntegerAsync(badge.Slot);
                await msg2.AppendStringAsync(badge.Code);
                msg2.SaveArray();
            }

            msg2.EndArray();

            await Session.SendMessageAsync(msg2);
        }

        /// <summary>
        /// Changes the look.
        /// </summary>
        internal async Task ChangeLook()
        {
            var text = Request.GetString().ToUpper();
            var text2 = Request.GetString();
            text2 = Oblivion.FilterFigure(text2);

            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileChangeLook);
            Session.GetHabbo().Look = text2;
            Session.GetHabbo().Gender = text.ToLower() == "f" ? "f" : "m";
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"UPDATE users SET look = @look, gender = @gender WHERE id = {Session.GetHabbo().Id}");
                queryReactor.AddParameter("look", text2);
                queryReactor.AddParameter("gender", text);
                await queryReactor.RunQueryAsync();
            }

            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_AvatarLooks", 1);
            if (Session.GetHabbo().Look.Contains("ha-1006"))
                await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WearHat);
            await Session.GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("UpdateAvatarAspectMessageComposer"));
            await Session.GetMessageHandler().GetResponse().AppendStringAsync(Session.GetHabbo().Look);
            await Session.GetMessageHandler().GetResponse().AppendStringAsync(Session.GetHabbo().Gender.ToUpper());
            await Session.GetMessageHandler().SendResponse();
            await Session.GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            await Session.GetMessageHandler().GetResponse().AppendIntegerAsync(-1);
            await Session.GetMessageHandler().GetResponse().AppendStringAsync(Session.GetHabbo().Look);
            await Session.GetMessageHandler().GetResponse().AppendStringAsync(Session.GetHabbo().Gender.ToLower());
            await Session.GetMessageHandler().GetResponse().AppendStringAsync(Session.GetHabbo().Motto);
            await Session.GetMessageHandler().GetResponse().AppendIntegerAsync(Session.GetHabbo().AchievementPoints);
            await Session.GetMessageHandler().SendResponse();
            if (!Session.GetHabbo().InRoom)
                return;
            var currentRoom = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = currentRoom?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId); //BUGG
            //serverMessage.AppendInt32(-1);
            await serverMessage.AppendStringAsync(Session.GetHabbo().Look);
            await serverMessage.AppendStringAsync(Session.GetHabbo().Gender.ToLower());
            await serverMessage.AppendStringAsync(Session.GetHabbo().Motto);
            await serverMessage.AppendIntegerAsync(Session.GetHabbo().AchievementPoints);
            await currentRoom.SendMessageAsync(serverMessage);

            if (Session.GetHabbo().GetMessenger() != null) await Session.GetHabbo().GetMessenger().OnStatusChanged(true);
        }

        /// <summary>
        /// Changes the motto.
        /// </summary>
        internal async Task ChangeMotto()
        {
            var text = Request.GetString();
            if (text == Session.GetHabbo().Motto)
                return;


            if (!await BobbaFilter.CanTalk(Session, text))
            {
                return;
            }

            Session.GetHabbo().Motto = text;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"UPDATE users SET motto = @motto WHERE id = '{Session.GetHabbo().Id}'");
                queryReactor.AddParameter("motto", text);
                await queryReactor.RunQueryAsync();
            }

            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileChangeMotto);
            if (Session.GetHabbo().InRoom)
            {
                var currentRoom = Session.GetHabbo().CurrentRoom;
                var roomUserByHabbo = currentRoom?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (roomUserByHabbo == null)
                    return;
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId); //BUGG
                //serverMessage.AppendInt32(-1);
                await serverMessage.AppendStringAsync(Session.GetHabbo().Look);
                await serverMessage.AppendStringAsync(Session.GetHabbo().Gender.ToLower());
                await serverMessage.AppendStringAsync(Session.GetHabbo().Motto);
                await serverMessage.AppendIntegerAsync(Session.GetHabbo().AchievementPoints);
                await currentRoom.SendMessage(serverMessage);
            }

            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Motto", 1);
        }

        /// <summary>
        /// Gets the wardrobe.
        /// </summary>
        internal async Task GetWardrobe()
        {
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("LoadWardrobeMessageComposer"));
            await GetResponse().AppendIntegerAsync(0);
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"SELECT slot_id, look, gender FROM users_wardrobe WHERE user_id = {Session.GetHabbo().Id}");
                var table = queryReactor.GetTable();
                if (table == null)
                    await GetResponse().AppendIntegerAsync(0);
                else
                {
                    await GetResponse().AppendIntegerAsync(table.Rows.Count);
                    /* TODO CHECK */
                    foreach (DataRow dataRow in table.Rows)
                    {
                        await GetResponse().AppendIntegerAsync(Convert.ToUInt32(dataRow["slot_id"]));
                        await GetResponse().AppendStringAsync((string) dataRow["look"]);
                        await GetResponse().AppendStringAsync(dataRow["gender"].ToString().ToUpper());
                    }
                }

                await SendResponse();
            }
        }

        /// <summary>
        /// Saves the wardrobe.
        /// </summary>
        internal async Task SaveWardrobe()
        {
            var num = Request.GetUInteger();
            var text = Request.GetString();
            var text2 = Request.GetString().ToUpper() == "F" ? "F" : "M";

            text = Oblivion.FilterFigure(text);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Concat(
                    "REPLACE INTO users_wardrobe (user_id,slot_id,look,gender) VALUES (", Session.GetHabbo().Id, ",",
                    num, ",@look,@gender)   "));
                queryReactor.AddParameter("look", text);
                queryReactor.AddParameter("gender", text2);
                await queryReactor.RunQueryAsync();
            }


            await Oblivion.GetGame()
                .GetQuestManager()
                .ProgressUserQuest(Session, QuestType.ProfileChangeLook);
        }

        /// <summary>
        /// Gets the pets inventory.
        /// </summary>
        internal async Task GetPetsInventory()
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
                return;
            await Session.SendMessageAsync(await Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        /// <summary>
        /// Gets the bots inventory.
        /// </summary>
        internal async Task GetBotsInventory()
        {
            await Session.SendMessageAsync(await Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
            await SendResponse();
        }


        /// <summary>
        /// Checks the name.
        /// </summary>
        internal async Task CheckName()
        {
            var text = Request.GetString();
            if (string.Equals(text, Session.GetHabbo().UserName, StringComparison.CurrentCultureIgnoreCase))
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                await Response.AppendIntegerAsync(0);
                await Response.AppendStringAsync(text);
                await Response.AppendIntegerAsync(0);
                await SendResponse();
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
                    await Response.InitAsync(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    await Response.AppendIntegerAsync(4);
                    await Response.AppendStringAsync(text);
                    await Response.AppendIntegerAsync(0);
                    await SendResponse();
                    return;
                }

                if (lower.Contains("mod") || lower.Contains("m0d") ||
                    lower.Contains("admin"))
                {
                    await Response.InitAsync(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    await Response.AppendIntegerAsync(4);
                    await Response.AppendStringAsync(text);
                    await Response.AppendIntegerAsync(0);
                    await SendResponse();
                }
                else if (text.Length > 15)
                {
                    await Response.InitAsync(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    await Response.AppendIntegerAsync(3);
                    await Response.AppendStringAsync(text);
                    await Response.AppendIntegerAsync(0);
                    await SendResponse();
                }
                else if (text.Length < 3)
                {
                    await Response.InitAsync(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    await Response.AppendIntegerAsync(2);
                    await Response.AppendStringAsync(text);
                    await Response.AppendIntegerAsync(0);
                    await SendResponse();
                }
                else if (string.IsNullOrWhiteSpace(@string))
                {
                    await Response.InitAsync(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    await Response.AppendIntegerAsync(0);
                    await Response.AppendStringAsync(text);
                    await Response.AppendIntegerAsync(0);
                    await SendResponse();
                }
                else
                {
                    queryReactor.SetQuery("SELECT tag FROM users_tags ORDER BY RAND() LIMIT 3");
                    var table = queryReactor.GetTable();
                    await Response.InitAsync(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    await Response.AppendIntegerAsync(5);
                    await Response.AppendStringAsync(text);
                    await Response.AppendIntegerAsync(table.Rows.Count);
                    /* TODO CHECK */
                    foreach (DataRow dataRow in table.Rows)
                        await Response.AppendStringAsync($"{text}{dataRow[0]}");
                    await SendResponse();
                }
            }
        }

        /// <summary>
        /// Changes the name.
        /// </summary>
        internal async Task ChangeName()
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

                queryReactor.SetQuery(
                    "UPDATE users SET username = @newname, last_name_change = @timestamp WHERE id = @userid");
                queryReactor.AddParameter("newname", text);
                queryReactor.AddParameter("timestamp", Oblivion.GetUnixTimeStamp() + 43200);
                queryReactor.AddParameter("userid", Session.GetHabbo().Id);
                await queryReactor.RunQueryAsync();
                
                Session.GetHabbo().LastChange = Oblivion.GetUnixTimeStamp() + 43200;
                Session.GetHabbo().UserName = text;
                await Response.InitAsync(LibraryParser.OutgoingRequest("UpdateUsernameMessageComposer"));
                await Response.AppendIntegerAsync(0);
                await Response.AppendStringAsync(text);
                await Response.AppendIntegerAsync(0);
                await SendResponse();
                await Response.InitAsync(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                await Response.AppendIntegerAsync(-1);
                await Response.AppendStringAsync(Session.GetHabbo().Look);
                await Response.AppendStringAsync(Session.GetHabbo().Gender.ToLower());
                await Response.AppendStringAsync(Session.GetHabbo().Motto);
                await Response.AppendIntegerAsync(Session.GetHabbo().AchievementPoints);
                await SendResponse();
                
                Session.GetHabbo().CurrentRoom.GetRoomUserManager().UpdateUser(userName, text);
                if (Session.GetHabbo().CurrentRoom != null)
                {
                    await Response.InitAsync(LibraryParser.OutgoingRequest("UserUpdateNameInRoomMessageComposer"));
                    await Response.AppendIntegerAsync(Session.GetHabbo().Id);
                    await Response.AppendIntegerAsync(Session.GetHabbo().CurrentRoom.RoomId);
                    await Response.AppendStringAsync(text);
                }

                /* TODO CHECK */
                foreach (var data in Session.GetHabbo().Data.Rooms.ToList())
                {
                    var current = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(data);
                    current.Owner = text;
                    await current.SerializeRoomData(Response, Session, false, true);
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
                            await current3.Serialize(Response, Session);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        internal async Task GetRelationships()
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

            await Response.InitAsync(LibraryParser.OutgoingRequest("RelationshipMessageComposer"));
            await Response.AppendIntegerAsync(habboForId.Id);

            Response.StartArray();
//            Response.AppendInteger(habboForId.Data.Relations.Count);

            if (habboForId.Data?.Relations == null)
            {
                return;
            }
            foreach (var current in habboForId.Data.Relations.Values.OrderBy(x => rand.Next()))
            {
                if (!habboForId.GetMessenger().FriendshipExists((uint) current.UserId))
                    continue;

                var habboForId2 = Oblivion.GetHabboById(Convert.ToUInt32(current.UserId));
                if (habboForId2 == null)
                {
                    await Response.AppendIntegerAsync(0);
                    await Response.AppendIntegerAsync(0);
                    await Response.AppendIntegerAsync(0);
                    await Response.AppendStringAsync("Placeholder");
                    await Response.AppendStringAsync("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62");
                }
                else
                {
                    await Response.AppendIntegerAsync(current.Type);
                    await Response.AppendIntegerAsync((current.Type == 1) ? num : ((current.Type == 2) ? num2 : num3));
                    await Response.AppendIntegerAsync(current.UserId);
                    await Response.AppendStringAsync(habboForId2.UserName);
                    await Response.AppendStringAsync(habboForId2.Look);
                }

                Response.SaveArray();
            }

            Response.EndArray();
            await SendResponse();
        }

        /// <summary>
        /// Sets the relationship.
        /// </summary>
        internal async Task SetRelationship()
        {
            var num = Request.GetUInteger();
            var num2 = Request.GetInteger();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                if (num2 == 0)
                {
                    queryReactor.SetNoLockQuery(
                        "SELECT id FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1;");
                    queryReactor.AddParameter("id", Session.GetHabbo().Id);
                    queryReactor.AddParameter("target", num);
                    var integer = (uint) queryReactor.GetInteger();
                    queryReactor.SetNoLockQuery(
                        "DELETE FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1;");
                    queryReactor.AddParameter("id", Session.GetHabbo().Id);
                    queryReactor.AddParameter("target", num);
                    await queryReactor.RunQueryAsync();
                    if (Session.GetHabbo().Data.Relations.ContainsKey(integer))
                        Session.GetHabbo().Data.Relations.Remove(integer);
                }
                else
                {
                    queryReactor.SetNoLockQuery(
                        "SELECT id FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1;");
                    queryReactor.AddParameter("id", Session.GetHabbo().Id);
                    queryReactor.AddParameter("target", num);
                    var integer2 = (uint) queryReactor.GetInteger();
                    if (integer2 > 0)
                    {
                        queryReactor.SetNoLockQuery(
                            "DELETE FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1;");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        await queryReactor.RunQueryAsync();
                        if (Session.GetHabbo().Data.Relations.ContainsKey(integer2))
                            Session.GetHabbo().Data.Relations.Remove(integer2);
                    }

                    queryReactor.SetNoLockQuery(
                        "INSERT INTO users_relationships (user_id, target, type) VALUES (@id, @target, @type);");
                    queryReactor.AddParameter("id", Session.GetHabbo().Id);
                    queryReactor.AddParameter("target", num);
                    queryReactor.AddParameter("type", num2);
                    var num3 = (uint) await queryReactor.InsertQueryAsync();
                    Session.GetHabbo().Data.Relations[num3] = new Relationship(num3, num, num2);
                }

                var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(num);
                await Session.GetHabbo().GetMessenger().UpdateFriend(num, clientByUserId, true);
            }
        }

        /// <summary>
        /// Starts the quest.
        /// </summary>
        public async Task StartQuest()
        {
            await Oblivion.GetGame().GetQuestManager().ActivateQuest(Session, Request);
        }

        /// <summary>
        /// Stops the quest.
        /// </summary>
        public async Task StopQuest()
        {
            await Oblivion.GetGame().GetQuestManager().CancelQuest(Session, Request);
        }

        /// <summary>
        /// Gets the current quest.
        /// </summary>
        public async Task GetCurrentQuest()
        {
            await Oblivion.GetGame().GetQuestManager().GetCurrentQuest(Session, Request);
        }

        /// <summary>
        /// Starts the seasonal quest.
        /// </summary>
        public async Task StartSeasonalQuest()
        {
            RoomData roomData;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var quest = Oblivion.GetGame().GetQuestManager().GetQuest(Request.GetUInteger());
                if (quest == null)
                    return;
                await queryReactor.RunFastQueryAsync(string.Concat("REPLACE INTO users_quests_data(user_id,quest_id) VALUES (",
                    Session.GetHabbo().Id, ", ", quest.Id, ")"));
                await queryReactor.RunFastQueryAsync(string.Concat("UPDATE users_stats SET quest_id = ", quest.Id, " WHERE id = ",
                    Session.GetHabbo().Id));
                Session.GetHabbo().CurrentQuestId = quest.Id;
                await Session.SendMessageAsync(QuestStartedComposer.Compose(Session, quest));
                await Oblivion.GetGame().GetQuestManager().ActivateQuest(Session, Request);
//                queryReactor.SetQuery("SELECT id FROM rooms_data WHERE state='open' ORDER BY users_now DESC LIMIT 1");
//                var @string = queryReactor.GetString();

                roomData = Oblivion.GetGame().GetRoomManager().GetActiveRooms()
                    .FirstOrDefault(x => x.Key.UsersNow > 0 && x.Key.State == 0).Key;
            }

            if (roomData != null)
            {
                await roomData.SerializeRoomData(Response, Session, true);
                await Session.GetMessageHandler().PrepareRoomForUser(roomData.Id, "");
                return;
            }

            await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("start_quest_need_room"));
        }

        /// <summary>
        /// Receives the nux gifts.
        /// </summary>
        public async Task ReceiveNuxGifts()
        {
            if (!ExtraSettings.NewUsersGiftsEnabled)
            {
                await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("nieuwe_gebruiker_kado_error_1"));
                return;
            }

            if (Session.GetHabbo().Vip)
            {
                await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("nieuwe_gebruiker_kado_error_2"));
                return;
            }

            var item = await Session.GetHabbo().GetInventoryComponent()
                .AddNewItem("0", ExtraSettings.NewUserGiftYttv2Id, "", 0, true, false, 0, 0);
            await Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            Session.GetHabbo().Diamonds += 25;
            await Session.GetHabbo().UpdateSeasonalCurrencyBalance();
            if (item != null)
                await Session.GetHabbo().GetInventoryComponent().SendNewItems(item.VirtualId);

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                await dbClient.RunFastQueryAsync(
                    Session.GetHabbo().Vip
                        ? $"UPDATE users SET vip = '1', vip_expire = DATE_ADD(vip_expire, INTERVAL 1 DAY) WHERE id = {Session.GetHabbo().Id}"
                        : $"UPDATE users SET vip = '1', vip_expire = DATE_ADD(NOW(), INTERVAL 1 DAY) WHERE id = {Session.GetHabbo().Id}");

            Session.GetHabbo().Vip = true;
        }

        /// <summary>
        /// Accepts the nux gifts.
        /// </summary>
        public async Task AcceptNuxGifts()
        {
            if (ExtraSettings.NewUsersGiftsEnabled == false || Request.GetInteger() != 0)
                return;

            var nuxGifts = new ServerMessage(LibraryParser.OutgoingRequest("NuxListGiftsMessageComposer"));
            await nuxGifts.AppendIntegerAsync(3); //Cantidad

            await nuxGifts.AppendIntegerAsync(0);
            await nuxGifts.AppendIntegerAsync(0);
            await nuxGifts.AppendIntegerAsync(1); //Cantidad
            // ahora nuevo bucle
            await nuxGifts.AppendStringAsync("");
            await nuxGifts.AppendStringAsync("nux/gift_yttv2.png");
            await nuxGifts.AppendIntegerAsync(1); //cantidad
            //Ahora nuevo bucle...
            await nuxGifts.AppendStringAsync("yttv2");
            await nuxGifts.AppendStringAsync("");

            await nuxGifts.AppendIntegerAsync(2);
            await nuxGifts.AppendIntegerAsync(1);
            await nuxGifts.AppendIntegerAsync(1);
            await nuxGifts.AppendStringAsync("");
            await nuxGifts.AppendStringAsync("nux/gift_diamonds.png");
            await nuxGifts.AppendIntegerAsync(1);
            await nuxGifts.AppendStringAsync("nux_gift_diamonds");
            await nuxGifts.AppendStringAsync("");

            await nuxGifts.AppendIntegerAsync(3);
            await nuxGifts.AppendIntegerAsync(1);
            await nuxGifts.AppendIntegerAsync(1);
            await nuxGifts.AppendStringAsync("");
            await nuxGifts.AppendStringAsync("nux/gift_vip1day.png");
            await nuxGifts.AppendIntegerAsync(1);
            await nuxGifts.AppendStringAsync("nux_gift_vip_1_day");
            await nuxGifts.AppendStringAsync("");

            await Session.SendMessageAsync(nuxGifts);
        }

        /// <summary>
        /// Talentses this instance.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        internal async Task Talents()
        {
            var trackType = Request.GetString();
            var talents = Oblivion.GetGame().GetTalentManager().GetTalents(trackType, -1);
            var failLevel = -1;

            if (talents == null)
                return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("TalentsTrackMessageComposer"));
            await Response.AppendStringAsync(trackType);
            await Response.AppendIntegerAsync(talents.Count);

            /* TODO CHECK */
            foreach (var current in talents)
            {
                await Response.AppendIntegerAsync(current.Level);

                var nm = (failLevel == -1) ? 1 : 0;

                await Response.AppendIntegerAsync(nm);

                var talents2 = Oblivion.GetGame().GetTalentManager().GetTalents(trackType, current.Id);
                await Response.AppendIntegerAsync(talents2.Count);

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
                    await Response.AppendIntegerAsync(current2.GetAchievement().Id);
                    await Response.AppendIntegerAsync(0);
                    await Response.AppendStringAsync($"{current2.AchievementGroup}{current2.AchievementLevel}");
                    await Response.AppendIntegerAsync(num);

                    var achievementData = Session.GetHabbo().GetAchievementData(current2.AchievementGroup);

                    await Response.AppendIntegerAsync(achievementData?.Progress ?? 0);
                    await Response.AppendIntegerAsync(current2.GetAchievement().Levels[current2.AchievementLevel].Requirement);

                    if (num != 2 && failLevel == -1)
                        failLevel = current2.Level;
                }

                await Response.AppendIntegerAsync(0);

                if (current.Type == "citizenship" && current.Level == 4)
                {
                    await Response.AppendIntegerAsync(2);
                    await Response.AppendStringAsync("HABBO_CLUB_VIP_7_DAYS");
                    await Response.AppendIntegerAsync(7);
                    await Response.AppendStringAsync(current.Prize);
                    await Response.AppendIntegerAsync(0);
                }
                else
                {
                    await Response.AppendIntegerAsync(1);
                    await Response.AppendStringAsync(current.Prize);
                    await Response.AppendIntegerAsync(0);
                }
            }

            await SendResponse();
        }

        /// <summary>
        /// Completes the safety quiz.
        /// </summary>
        internal async Task CompleteSafetyQuiz()
        {
            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SafetyQuizGraduate", 1);
            await Session.SendMessageAsync(new ServerMessage(2873));
        }

        /// <summary>
        /// Hotels the view countdown.
        /// </summary>
        internal async Task HotelViewCountdown()
        {
            string time = Request.GetString();
            DateTime.TryParse(time, out var date);
            TimeSpan diff = date - DateTime.Now;
            await Response.InitAsync(LibraryParser.OutgoingRequest("HotelViewCountdownMessageComposer"));
            await Response.AppendStringAsync(time);
            await Response.AppendIntegerAsync(Convert.ToInt32(diff.TotalSeconds));
            await SendResponse();
        }

        /// <summary>
        /// Hotels the view dailyquest.
        /// </summary>
        internal Task HotelViewDailyquest()
        {
            return Task.CompletedTask;
        }

        internal async Task FindMoreFriends()
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
                    await Session.SendMessageAsync(success);
                    return;
                }

                success.AppendBool(true);
                await Session.SendMessageAsync(success);
                var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                await roomFwd.AppendIntegerAsync(randomRoom.Id);
                await Session.SendMessageAsync(roomFwd);
            }
        }

        internal async Task HotelViewRequestBadge()
        {
            string name = Request.GetString();
            var hotelViewBadges = Oblivion.GetGame().GetHotelView().HotelViewBadges;
            if (!hotelViewBadges.TryGetValue(name, out var badge))
                return;
            await Session.GetHabbo().GetBadgeComponent().GiveBadge(badge, true, Session, true);
        }

        internal async Task GetCameraPrice()
        {
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("SetCameraPriceMessageComposer"));
            await GetResponse().AppendIntegerAsync(Oblivion.GetGame().GetCameraManager().PurchaseCoinsPrice); //credits
            await GetResponse().AppendIntegerAsync(Oblivion.GetGame().GetCameraManager().PurchaseDucketsPrice); //duckets
            await GetResponse().AppendIntegerAsync(Oblivion.GetGame().GetCameraManager().PublishDucketsPrice); //duckets publish
            await SendResponse();
        }

        internal async Task GetHotelViewHallOfFame()
        {
            string code = Request.GetString();
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("HotelViewHallOfFameMessageComposer"));
            await GetResponse().AppendStringAsync(code);
            var rankings = Oblivion.GetGame().GetHallOfFame().Rankings;
            GetResponse().StartArray();
            int rank = 1;
            /* TODO CHECK */
            foreach (HallOfFameElement element in rankings)
            {
                await GetResponse().AppendIntegerAsync(element.UserId);
                await GetResponse().AppendStringAsync(element.Username);
                await GetResponse().AppendStringAsync(element.Look);
                await GetResponse().AppendIntegerAsync(rank);
                await GetResponse().AppendIntegerAsync(element.Score);
                rank++;
                GetResponse().SaveArray();
            }

            GetResponse().EndArray();
            await SendResponse();
        }

        internal Task FriendRequestListLoad()
        {
            return Task.CompletedTask;
        }
    }
}