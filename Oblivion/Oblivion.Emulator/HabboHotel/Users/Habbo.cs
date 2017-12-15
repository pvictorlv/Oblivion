using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Configuration;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Navigators.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users.Badges;
using Oblivion.HabboHotel.Users.Inventory;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Subscriptions;
using Oblivion.HabboHotel.Users.UserDataManagement;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Users
{
    /// <summary>
    ///     Class Habbo.
    /// </summary>
    public class Habbo
    {
        /// <summary>
        ///     The _my groups
        /// </summary>
        private List<uint> _myGroups;

        /// <summary>
        ///     The _avatar effects inventory component
        /// </summary>
        private AvatarEffectsInventoryComponent _avatarEffectsInventoryComponent;

        /// <summary>
        ///     The _badge component
        /// </summary>
        private BadgeComponent _badgeComponent;


        /// <summary>
        /// Get when the last totem effect was earned
        /// </summary>
        internal int LastTotem;

        /// <summary>
        ///     The _habboinfo saved
        /// </summary>
        private bool _habboinfoSaved;

        /// <summary>
        ///     The _inventory component
        /// </summary>
        private InventoryComponent _inventoryComponent;

        /// <summary>
        ///     The _loaded my groups
        /// </summary>
        private bool _loadedMyGroups;

        /// <summary>
        ///     The _m client
        /// </summary>
        private GameClient _mClient;

        /// <summary>
        ///     The _messenger
        /// </summary>
        private HabboMessenger _messenger;

        /// <summary>
        ///     The _subscription manager
        /// </summary>
        private SubscriptionManager _subscriptionManager;

        /// <summary>
        ///     The credits
        /// </summary>
        internal int ActivityPoints;

        // NEW
        internal bool AnsweredPool = false;

        /// <summary>
        ///     The appear offline
        /// </summary>
        internal bool AppearOffline;

        /// <summary>
        ///     Bobba Filter
        /// </summary>
        internal int BobbaFiltered = 0;

        /// <summary>
        ///     The builders expire
        /// </summary>
        internal int BuildersExpire;

        /// <summary>
        ///     The builders items maximum
        /// </summary>
        internal int BuildersItemsMax;

        /// <summary>
        ///     The builders items used
        /// </summary>
        internal int BuildersItemsUsed;

        /// <summary>
        ///     The _clothing manager
        /// </summary>
//        internal UserClothing ClothingManager;
        /// <summary>
        ///     The create date
        /// </summary>
        internal double CreateDate;

        /// <summary>
        ///     The credits
        /// </summary>
        internal int Credits, AchievementPoints;

        /// <summary>
        ///     The current quest identifier
        /// </summary>
        internal uint CurrentQuestId;

        /// <summary>
        ///     The current room identifier
        /// </summary>
        internal uint CurrentRoomId;

        /// <summary>
        ///     The current talent level
        /// </summary>
        internal int CurrentTalentLevel;

        internal bool DisableEventAlert;

        /// <summary>
        ///     The disconnected
        /// </summary>
        internal bool Disconnected;

        internal uint DutyLevel;

        /// <summary>
        ///     The favorite rooms
        /// </summary>
        internal List<uint> FavoriteRooms;

        /// <summary>
        ///     The favourite group
        /// </summary>
        internal uint FavouriteGroup;

        /// <summary>
        ///     The flood time
        /// </summary>
        internal int FloodTime;

        /// <summary>
        ///     The friend count
        /// </summary>
        internal uint FriendCount;

        /// <summary>
        ///     The guide other user
        /// </summary>
        public GameClient GuideOtherUser;

        /// <summary>
        ///     The has friend requests disabled
        /// </summary>
        internal bool HasFriendRequestsDisabled;

        /// <summary>
        ///     The hide in room
        /// </summary>
        internal bool HideInRoom;

        /// <summary>
        ///     The home room
        /// </summary>
        internal uint HomeRoom;

        /// <summary>
        ///     The hopper identifier
        /// </summary>
        internal uint HopperId;

        /// <summary>
        ///     The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        ///     The is hopping
        /// </summary>
        internal bool IsHopping;

        /// <summary>
        ///     The is teleporting
        /// </summary>
        internal bool IsTeleporting;

        /// <summary>
        ///     The last change
        /// </summary>
        internal int LastChange;

        /// <summary>
        ///     The last gift open time
        /// </summary>
        internal DateTime LastGiftOpenTime;

        /// <summary>
        ///     The last gift purchase time
        /// </summary>
        internal DateTime LastGiftPurchaseTime;

        /// <summary>
        ///     The last online
        /// </summary>
        internal int LastOnline;

        /// <summary>
        ///     The last quest completed
        /// </summary>
        internal uint LastQuestCompleted;

        public uint LastSelectedUser = 0;

        /// <summary>
        ///     The last SQL query
        /// </summary>
        internal int LastSqlQuery;

        public DateTime LastUsed = DateTime.Now;

        /// <summary>
        ///     The loading checks passed
        /// </summary>
        internal bool LoadingChecksPassed;

        /// <summary>
        ///     The loading room
        /// </summary>
        internal uint LoadingRoom;

        /// <summary>
        ///     The minimail unread messages
        /// </summary>
        internal uint MinimailUnreadMessages;

        /// <summary>
        ///     The muted
        /// </summary>
        internal bool Muted;

        /// <summary>
        ///     The muted users
        /// </summary>
        internal List<uint> MutedUsers;

        /// <summary>
        ///     The navigator logs
        /// </summary>
        internal Dictionary<int, NaviLogs> NavigatorLogs;

        /// <summary>
        ///     The on duty
        /// </summary>
        internal bool OnDuty;

        /// <summary>
        ///     The own rooms serialized
        /// </summary>
        internal bool OwnRoomsSerialized = false;

        internal UserPreferences Preferences;

        /// <summary>
        ///     The previous online
        /// </summary>
        internal int PreviousOnline;

        /// <summary>
        ///     The rank
        /// </summary>
        internal uint Rank;

        /// <summary>
        ///     The rated rooms
        /// </summary>
        internal HashSet<uint> RatedRooms;

        /// <summary>
        ///     The recently visited rooms
        /// </summary>
        internal LinkedList<uint> RecentlyVisitedRooms;

        /// <summary>
        ///     The release name
        /// </summary>
        internal string ReleaseName;

        /// <summary>
        ///     The respect
        /// </summary>
        internal int Respect, DailyRespectPoints, DailyPetRespectPoints, DailyCompetitionVotes;

        /// <summary>
        ///     The spam flood time
        /// </summary>
        internal DateTime SpamFloodTime;

        /// <summary>
        ///     The spam protection bol
        /// </summary>
        internal bool SpamProtectionBol;

        /// <summary>
        ///     The spam protection count
        /// </summary>
        internal int SpamProtectionCount = 1, SpamProtectionTime, SpamProtectionAbuse;

        /// <summary>
        ///     The spectator mode
        /// </summary>
        internal bool SpectatorMode;

        /// <summary>
        ///     The talents
        /// </summary>
        internal Dictionary<int, UserTalent> Talents;

        /// <summary>
        ///     The talent status
        /// </summary>
        internal string TalentStatus;

        /// <summary>
        ///     The teleporter identifier
        /// </summary>
        internal uint TeleporterId;

        /// <summary>
        ///     The teleporting room identifier
        /// </summary>
        internal uint TeleportingRoomId;

        /// <summary>
        ///     TimeLoggedOn
        /// </summary>
        internal DateTime TimeLoggedOn;

        /// <summary>
        ///     The timer_ elapsed
        /// </summary>
        public bool TimerElapsed;

        /// <summary>
        ///     The trade locked
        /// </summary>
        internal bool TradeLocked;

        /// <summary>
        ///     The trade lock expire
        /// </summary>
        internal int TradeLockExpire;

        /// <summary>
        ///     The user groups
        /// </summary>
        internal HashSet<GroupMember> UserGroups;

        /// <summary>
        ///     The user name
        /// </summary>
        internal string UserName, Motto, Look, Gender;


        ///<summary>
        /// User blockeds commands
        /// </summary>
        internal UserData Data;

        /// <summary>
        ///     The vip
        /// </summary>
        internal bool Vip;


        /// <summary>
        ///     Initializes a new instance of the <see cref="Habbo" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="rank">The rank.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="look">The look.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="credits">The credits.</param>
        /// <param name="activityPoints">The activity points.</param>
        /// <param name="muted">if set to <c>true</c> [muted].</param>
        /// <param name="homeRoom">The home room.</param>
        /// <param name="respect">The respect.</param>
        /// <param name="dailyRespectPoints">The daily respect points.</param>
        /// <param name="dailyPetRespectPoints">The daily pet respect points.</param>
        /// <param name="hasFriendRequestsDisabled">if set to <c>true</c> [has friend requests disabled].</param>
        /// <param name="currentQuestId">The current quest identifier.</param>
        /// <param name="achievementPoints">The achievement points.</param>
        /// <param name="lastOnline">The last online.</param>
        /// <param name="appearOffline">if set to <c>true</c> [appear offline].</param>
        /// <param name="hideInRoom">if set to <c>true</c> [hide in room].</param>
        /// <param name="vip">if set to <c>true</c> [vip].</param>
        /// <param name="createDate">The create date.</param>
        /// <param name="citizenShip">The citizen ship.</param>
        /// <param name="diamonds">The diamonds.</param>
        /// <param name="groups">The groups.</param>
        /// <param name="favId">The fav identifier.</param>
        /// <param name="lastChange">The last change.</param>
        /// <param name="tradeLocked">if set to <c>true</c> [trade locked].</param>
        /// <param name="tradeLockExpire">The trade lock expire.</param>
        /// <param name="buildersExpire">The builders expire.</param>
        /// <param name="buildersItemsMax">The builders items maximum.</param>
        /// <param name="buildersItemsUsed">The builders items used.</param>
        /// <param name="onDuty">if set to <c>true</c> [on duty].</param>
        /// <param name="naviLogs">The navi logs.</param>
        /// <param name="dailyCompetitionVotes"></param>
        /// <param name="dutyLevel"></param>
        /// <param name="disableAlert"></param>
        internal Habbo(uint id, string userName, uint rank, string motto, string look, string gender,
            int credits, int activityPoints, bool muted, uint homeRoom, int respect,
            int dailyRespectPoints, int dailyPetRespectPoints, bool hasFriendRequestsDisabled, uint currentQuestId,
            int achievementPoints, int lastOnline, bool appearOffline,
            bool hideInRoom, bool vip, double createDate, string citizenShip, int diamonds,
            HashSet<GroupMember> groups, uint favId, int lastChange, bool tradeLocked, int tradeLockExpire,
            int buildersExpire, int buildersItemsMax, int buildersItemsUsed, bool onDuty,
            Dictionary<int, NaviLogs> naviLogs, int dailyCompetitionVotes, uint dutyLevel, bool disableAlert, int lastTotem)
        {
            Id = id;
            UserName = userName;

            _myGroups = new List<uint>();

            BuildersExpire = buildersExpire;
            BuildersItemsMax = buildersItemsMax;
            BuildersItemsUsed = buildersItemsUsed;
            LastTotem = lastTotem;
            if (rank < 1u)
                rank = 1u;

            ReleaseName = string.Empty;

            OnDuty = onDuty;
            DutyLevel = dutyLevel;
            Rank = rank;
            Motto = motto;
            Look = look.ToLower();
            Vip = rank > 5 || vip;
            LastChange = lastChange;
            TradeLocked = tradeLocked;
            NavigatorLogs = naviLogs;
            TradeLockExpire = tradeLockExpire;
            Gender = gender.ToLower() == "f" ? "f" : "m";
            Credits = credits;
            ActivityPoints = activityPoints;
            Diamonds = diamonds;
            AchievementPoints = achievementPoints;
            Muted = muted;
            LoadingRoom = 0u;
            CreateDate = createDate;
            LoadingChecksPassed = false;
            FloodTime = 0;
            CurrentRoomId = 0u;
            TimeLoggedOn = DateTime.Now;
            HomeRoom = homeRoom;
            HideInRoom = hideInRoom;
            AppearOffline = appearOffline;
            FavoriteRooms = new List<uint>();
            MutedUsers = new List<uint>();
            Talents = new Dictionary<int, UserTalent>();
            RatedRooms = new HashSet<uint>();
            Respect = respect;
            DailyRespectPoints = dailyRespectPoints;
            DailyPetRespectPoints = dailyPetRespectPoints;
            IsTeleporting = false;
            TeleporterId = 0u;
            HasFriendRequestsDisabled = hasFriendRequestsDisabled;
            LastOnline = Oblivion.GetUnixTimeStamp();
            PreviousOnline = lastOnline;
            RecentlyVisitedRooms = new LinkedList<uint>();
            CurrentQuestId = currentQuestId;
            IsHopping = false;

            FavouriteGroup = Oblivion.GetGame().GetGroupManager().GetGroup(favId) != null ? favId : 0u;

            UserGroups = groups;

            if (DailyPetRespectPoints > 99)
                DailyPetRespectPoints = 99;

            if (DailyRespectPoints > 99)
                DailyRespectPoints = 99;

            LastGiftPurchaseTime = DateTime.Now;
            LastGiftOpenTime = DateTime.Now;
            TalentStatus = citizenShip;
            CurrentTalentLevel = GetCurrentTalentLevel();
            DailyCompetitionVotes = dailyCompetitionVotes;
            DisableEventAlert = disableAlert;
        }

        internal int Diamonds { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance can change name.
        /// </summary>
        /// <value><c>true</c> if this instance can change name; otherwise, <c>false</c>.</value>
        public bool CanChangeName => (ExtraSettings.ChangeNameStaff && HasFuse("fuse_can_change_name")) ||
                                     (ExtraSettings.ChangeNameVip && Vip) ||
                                     (ExtraSettings.ChangeNameEveryone &&
                                      Oblivion.GetUnixTimeStamp() > (LastChange + 604800));

        /// <summary>
        ///     Gets the head part.
        /// </summary>
        /// <value>The head part.</value>
        internal string HeadPart
        {
            get
            {
                var strtmp = Look.Split('.');
                var tmp2 = strtmp.FirstOrDefault(x => x.Contains("hd-"));
                var lookToReturn = tmp2 ?? string.Empty;

                if (Look.Contains("ha-"))
                    lookToReturn += $".{strtmp.FirstOrDefault(x => x.Contains("ha-"))}";
                if (Look.Contains("ea-"))
                    lookToReturn += $".{strtmp.FirstOrDefault(x => x.Contains("ea-"))}";
                if (Look.Contains("hr-"))
                    lookToReturn += $".{strtmp.FirstOrDefault(x => x.Contains("hr-"))}";
                if (Look.Contains("he-"))
                    lookToReturn += $".{strtmp.FirstOrDefault(x => x.Contains("he-"))}";
                if (Look.Contains("fa-"))
                    lookToReturn += $".{strtmp.FirstOrDefault(x => x.Contains("fa-"))}";

                return lookToReturn;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [in room].
        /// </summary>
        /// <value><c>true</c> if [in room]; otherwise, <c>false</c>.</value>
        internal bool InRoom => CurrentRoomId >= 1 && CurrentRoom != null;

        /// <summary>
        ///     Gets the current room.
        /// </summary>
        /// <value>The current room.</value>
        internal Room CurrentRoom;

        /// <summary>
        ///     Gets a value indicating whether this instance is helper.
        /// </summary>
        /// <value><c>true</c> if this instance is helper; otherwise, <c>false</c>.</value>
        internal bool IsHelper => TalentStatus == "helper" || Rank >= 4;

        /// <summary>
        ///     Gets a value indicating whether this instance is citizen.
        /// </summary>
        /// <value><c>true</c> if this instance is citizen; otherwise, <c>false</c>.</value>
        internal bool IsCitizen => CurrentTalentLevel > 4;

        /// <summary>
        ///     Gets the get query string.
        /// </summary>
        /// <value>The get query string.</value>
        internal string GetQueryString
        {
            get
            {
                _habboinfoSaved = true;
                return string.Concat("UPDATE users SET online='0', last_online = '", Oblivion.GetUnixTimeStamp(),
                    "', activity_points = '", ActivityPoints, "', diamonds = '", Diamonds, "', credits = '", Credits,
                    "' WHERE id = '", Id, "'; UPDATE users_stats SET achievement_score = ", AchievementPoints,
                    " WHERE id=", Id, " LIMIT 1; ");
            }
        }

        /// <summary>
        ///     Gets my groups.
        /// </summary>
        /// <value>My groups.</value>
        internal List<uint> MyGroups
        {
            get
            {
                if (!_loadedMyGroups)
                    _LoadMyGroups();

                return _myGroups;
            }
        }


        /// <summary>
        ///     Initializes the information.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InitInformation(UserData data)
        {
//            _subscriptionManager = new SubscriptionManager(Id, data);
            _badgeComponent = new BadgeComponent(Id, data);
            _messenger = new HabboMessenger(Id);
            _messenger.Init(data.Friends, data.Requests);
            SpectatorMode = false;
            Disconnected = false;
            Data = data;
        }

        /// <summary>
        ///     Initializes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="data">The data.</param>
        internal void Init(GameClient client, UserData data)
        {
            _mClient = client;
            _subscriptionManager = new SubscriptionManager(Id, data);
            _badgeComponent = new BadgeComponent(Id, data);
            _inventoryComponent = new InventoryComponent(Id, client);
            _avatarEffectsInventoryComponent = new AvatarEffectsInventoryComponent(Id, client, data);
            _messenger = new HabboMessenger(Id);
            _messenger.Init(data.Friends, data.Requests);
            FriendCount = Convert.ToUInt32(data.Friends.Count);
            SpectatorMode = false;
            Disconnected = false;
            MinimailUnreadMessages = data.MiniMailCount;
//            ClothingManager = new UserClothing(Id);
            Preferences = new UserPreferences(Id);
            Data = data;
        }

        /// <summary>
        ///     Updates the rooms.
        /// </summary>
        internal void UpdateRooms()
        {
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                Data.Rooms.Clear();
                dbClient.SetQuery("SELECT * FROM rooms_data WHERE owner = @name ORDER BY id ASC LIMIT 50");
                dbClient.AddParameter("name", UserName);

                var table = dbClient.GetTable();

                /* TODO CHECK */
                foreach (DataRow dataRow in table.Rows)
                    Data.Rooms.Add(Oblivion.GetGame()
                        .GetRoomManager()
                        .FetchRoomData(Convert.ToUInt32(dataRow["id"]), dataRow, Id));
            }
        }

        /// <summary>
        ///     Loads the data.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void LoadData(UserData data)
        {
            LoadTalents(data.Talents);
            LoadFavorites(data.FavouritedRooms);
            LoadMutedUsers(data.Ignores);
            Data = data;
        }

        /// <summary>
        ///     Serializes the quests.
        /// </summary>
        /// <param name="response">The response.</param>
        internal void SerializeQuests(ref QueuedServerMessage response)
        {
            Oblivion.GetGame().GetQuestManager().GetList(_mClient, null);
        }

        /// <summary>
        ///     Gots the command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GotCommand(string cmd) => Oblivion.GetGame().GetRoleManager().RankGotCommand(Rank, cmd);

        /// <summary>
        ///     Determines whether the specified fuse has fuse.
        /// </summary>
        /// <param name="fuse">The fuse.</param>
        /// <returns><c>true</c> if the specified fuse has fuse; otherwise, <c>false</c>.</returns>
        internal bool HasFuse(string fuse) => Oblivion.GetGame().GetRoleManager().RankHasRight(Rank, fuse) ||
                                              (GetSubscriptionManager().HasSubscription &&
                                               Oblivion.GetGame()
                                                   .GetRoleManager()
                                                   .HasVip(GetSubscriptionManager().GetSubscription().SubscriptionId,
                                                       fuse));

        /// <summary>
        ///     Loads the favorites.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        internal void LoadFavorites(List<uint> roomId)
        {
            FavoriteRooms = roomId;
        }

        /// <summary>
        ///     Loads the muted users.
        /// </summary>
        /// <param name="usersMuted">The users muted.</param>
        internal void LoadMutedUsers(List<uint> usersMuted)
        {
            MutedUsers = usersMuted;
        }

        /// <summary>
        ///     Serializes the club.
        /// </summary>
        internal void SerializeClub()
        {
            var client = GetClient();
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("SubscriptionStatusMessageComposer"));
            serverMessage.AppendString("club_habbo");
            if (client.GetHabbo().GetSubscriptionManager().HasSubscription)
            {
                double num = client.GetHabbo().GetSubscriptionManager().GetSubscription().ExpireTime;
                var num2 = num - Oblivion.GetUnixTimeStamp();
                var num3 = (int) Math.Ceiling(num2 / 86400.0);
                var i =
                    (int)
                    Math.Ceiling((Oblivion.GetUnixTimeStamp() -
                                  (double) client.GetHabbo().GetSubscriptionManager().GetSubscription().ActivateTime) /
                                 86400.0);
                var num4 = num3 / 31;

                if (num4 >= 1)
                    num4--;

                serverMessage.AppendInteger(num3 - num4 * 31);
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(num4);
                serverMessage.AppendInteger(1);
                serverMessage.AppendBool(true);
                serverMessage.AppendBool(true);
                serverMessage.AppendInteger(i);
                serverMessage.AppendInteger(i);
                serverMessage.AppendInteger(10);
            }
            else
            {
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                serverMessage.AppendBool(false);
                serverMessage.AppendBool(false);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
            }

            client.SendMessage(serverMessage);

            if (GetSubscriptionManager() == null) return;

            var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("UserClubRightsMessageComposer"));

            serverMessage2.AppendInteger(GetSubscriptionManager().HasSubscription ? 2 : 0);
            serverMessage2.AppendInteger(Rank);
            serverMessage2.AppendBool(Rank >= Convert.ToUInt32(Oblivion.GetDbConfig().DbData["ambassador.minrank"]));

            client.SendMessage(serverMessage2);
        }

        public void SaveLastTotem()
        {
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var now = Oblivion.GetUnixTimeStamp();
                LastTotem = now;
                dbClient.RunFastQuery($"UPDATE users_stats SET last_totem = '{now}' WHERE id = {Id}");
            }
        }

        /// <summary>
        ///     Loads the talents.
        /// </summary>
        /// <param name="talents">The talents.</param>
        internal void LoadTalents(Dictionary<int, UserTalent> talents)
        {
            Talents = talents;
        }

        /// <summary>
        ///     Called when [disconnect].
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal void OnDisconnect(string reason)
        {
            if (Disconnected)
                return;
            Disconnected = true;

            var navilogs = string.Empty;

            if (NavigatorLogs.Any())
            {
                navilogs = NavigatorLogs.Values.Aggregate(navilogs,
                    (current, navi) => current + $"{navi.Id},{navi.Value1},{navi.Value2};");
                navilogs = navilogs.Remove(navilogs.Length - 1);
            }

            Oblivion.GetGame().GetClientManager().UnregisterClient(Id, UserName);

//            Out.WriteLine(UserName + " disconnected from game. Reason: " + reason, "Oblivion.Users",
//                ConsoleColor.DarkYellow);

            var getOnlineSeconds = DateTime.Now - TimeLoggedOn;
            var secondsToGive = getOnlineSeconds.Seconds;

            if (!_habboinfoSaved)
            {
                _habboinfoSaved = true;
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("UPDATE users SET activity_points = '" + ActivityPoints +
                                          "', disabled_alert = '" + Oblivion.BoolToEnum(DisableEventAlert) +
                                          "', credits = '" +
                                          Credits + "', diamonds = '" + Diamonds + "', online='0', last_online = '" +
                                          Oblivion.GetUnixTimeStamp() + "', builders_items_used = '" +
                                          BuildersItemsUsed +
                                          "', navilogs = @navilogs  WHERE id = '" + Id +
                                          "' LIMIT 1;UPDATE users_stats SET achievement_score=" + AchievementPoints +
                                          " WHERE id=" + Id + " LIMIT 1;");
                    queryReactor.AddParameter("navilogs", navilogs);
                    queryReactor.RunQuery();
                    queryReactor.RunFastQuery("UPDATE users_stats SET online_seconds = online_seconds + " +
                                              secondsToGive + " WHERE id = " + Id);

                    if (Rank >= 4)
                        queryReactor.RunFastQuery(
                            $"UPDATE moderation_tickets SET status='open', moderator_id=0 WHERE status='picked' AND moderator_id={Id}");

                    queryReactor.RunFastQuery("UPDATE users SET block_newfriends = '" +
                                              Convert.ToInt32(HasFriendRequestsDisabled) + "', hide_online = '" +
                                              Convert.ToInt32(AppearOffline) + "', hide_inroom = '" +
                                              Convert.ToInt32(HideInRoom) + "' WHERE id = " + Id);
                }
            }

            if (InRoom)
                CurrentRoom?.GetRoomUserManager().RemoveUserFromRoom(_mClient, false, false);

            if (_messenger != null)
            {
                _messenger.AppearOffline = true;
                _messenger.Destroy();
                _messenger = null;
            }
            CurrentRoom = null;
            GuideOtherUser = null;
            _subscriptionManager = null;
            _avatarEffectsInventoryComponent?.Dispose();
            _avatarEffectsInventoryComponent = null;
            _badgeComponent = null;
            _myGroups?.Clear();
            _myGroups = null;
            Data?.Dispose();
            Data = null;
            _mClient = null;

            try
            {
                if (_inventoryComponent != null)
                {
                    _inventoryComponent.RunDbUpdate();
                    _inventoryComponent.Dispose();
                    _inventoryComponent = null;
                }
            }
            catch (Exception e)
            {
                _inventoryComponent?.Dispose();
                _inventoryComponent = null;

                Logging.HandleException(e, "User Inventory Dispose (habbo.cs)");
            }
        }

        internal uint LastBellId;

        /// <summary>
        ///     Initializes the messenger.
        /// </summary>
        internal void InitMessenger()
        {
            var client = GetClient();

            if (client == null)
                return;

            client.SendMessage(_messenger.SerializeCategories());
            client.SendMessage(_messenger.SerializeFriends());
            client.SendMessage(_messenger.SerializeRequests());

            if (Oblivion.OfflineMessages.ContainsKey(Id))
            {
                var list = Oblivion.OfflineMessages[Id];
                foreach (var current in list)
                    client.SendMessage(_messenger.SerializeOfflineMessages(current));
                Oblivion.OfflineMessages.Remove(Id);
                OfflineMessage.RemoveAllMessages(Oblivion.GetDatabaseManager().GetQueryReactor(), Id);
            }

            if (_messenger.Requests.Count > Oblivion.FriendRequestLimit)
                client.SendNotif(Oblivion.GetLanguage().GetVar("user_friend_request_max"));
//            _messenger.OnStatusChanged(false);
        }

        /// <summary>
        ///     Updates the credits balance.
        /// </summary>
        internal void UpdateCreditsBalance()
        {
            if (_mClient?.GetMessageHandler() == null || _mClient.GetMessageHandler().GetResponse() == null)
                return;

            _mClient.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("CreditsBalanceMessageComposer"));
            _mClient.GetMessageHandler().GetResponse().AppendString($"{Credits}.0");
            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Updates the activity points balance.
        /// </summary>
        internal void UpdateActivityPointsBalance()
        {
            if (_mClient?.GetMessageHandler() == null || _mClient.GetMessageHandler().GetResponse() == null)
                return;

            _mClient.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
            _mClient.GetMessageHandler().GetResponse().AppendInteger(3);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(0);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(ActivityPoints);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(5);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(Diamonds);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(105);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(Diamonds);
            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Updates the seasonal currency balance.
        /// </summary>
        internal void UpdateSeasonalCurrencyBalance()
        {
            if (_mClient?.GetMessageHandler() == null || _mClient.GetMessageHandler().GetResponse() == null)
                return;

            _mClient.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
            _mClient.GetMessageHandler().GetResponse().AppendInteger(3);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(0);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(ActivityPoints);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(5);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(Diamonds);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(105);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(Diamonds);
            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Notifies the new pixels.
        /// </summary>
        /// <param name="change">The change.</param>
        internal void NotifyNewPixels(int change)
        {
            if (_mClient?.GetMessageHandler() == null || _mClient.GetMessageHandler().GetResponse() == null)
                return;

            _mClient.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("ActivityPointsNotificationMessageComposer"));
            _mClient.GetMessageHandler().GetResponse().AppendInteger(ActivityPoints);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(change);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(0);
            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Notifies the new diamonds.
        /// </summary>
        /// <param name="change">The change.</param>
        internal void NotifyNewDiamonds(int change)
        {
            if (_mClient?.GetMessageHandler() == null || _mClient.GetMessageHandler().GetResponse() == null)
                return;

            _mClient.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("ActivityPointsNotificationMessageComposer"));
            _mClient.GetMessageHandler().GetResponse().AppendInteger(Diamonds);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(change);
            _mClient.GetMessageHandler().GetResponse().AppendInteger(5);
            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Notifies the voucher.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="productDescription">The product description.</param>
        internal void NotifyVoucher(bool isValid, string productName, string productDescription)
        {
            if (isValid)
            {
                _mClient.GetMessageHandler()
                    .GetResponse()
                    .Init(LibraryParser.OutgoingRequest("VoucherValidMessageComposer"));
                _mClient.GetMessageHandler().GetResponse().AppendString(productName);
                _mClient.GetMessageHandler().GetResponse().AppendString(productDescription);
                _mClient.GetMessageHandler().SendResponse();
                return;
            }

            _mClient.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("VoucherErrorMessageComposer"));
            _mClient.GetMessageHandler().GetResponse().AppendString("1");
            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Mutes this instance.
        /// </summary>
        internal void Mute()
        {
            if (!Muted)
                Muted = true;
        }

        /// <summary>
        ///     Uns the mute.
        /// </summary>
        internal void UnMute()
        {
            if (Muted)
                GetClient().SendNotif("You were unmuted.");

            Muted = false;

            if (CurrentRoom != null && CurrentRoom.MutedUsers.ContainsKey(Id))
                CurrentRoom.MutedUsers.Remove(Id);
        }

        /// <summary>
        ///     Gets the subscription manager.
        /// </summary>
        /// <returns>SubscriptionManager.</returns>
        internal SubscriptionManager GetSubscriptionManager() => _subscriptionManager;

        /// <summary>
        ///     Gets the messenger.
        /// </summary>
        /// <returns>HabboMessenger.</returns>
        internal HabboMessenger GetMessenger() => _messenger;

        /// <summary>
        ///     Gets the badge component.
        /// </summary>
        /// <returns>BadgeComponent.</returns>
        internal BadgeComponent GetBadgeComponent() => _badgeComponent;

        /// <summary>
        ///     Gets the inventory component.
        /// </summary>
        /// <returns>InventoryComponent.</returns>
        internal InventoryComponent GetInventoryComponent() => _inventoryComponent;

        /// <summary>
        ///     Gets the avatar effects inventory component.
        /// </summary>
        /// <returns>AvatarEffectsInventoryComponent.</returns>
        internal AvatarEffectsInventoryComponent GetAvatarEffectsInventoryComponent() =>
            _avatarEffectsInventoryComponent;

        /// <summary>
        ///     Runs the database update.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void RunDbUpdate(IQueryAdapter dbClient)
        {
            dbClient.RunFastQuery(string.Concat("UPDATE users SET last_online = '", Oblivion.GetUnixTimeStamp(),
                "', activity_points = '", ActivityPoints, "', credits = '", Credits, "', diamonds = '", Diamonds,
                "' WHERE id = '", Id, "' LIMIT 1; "));
        }

        /// <summary>
        ///     Gets the quest progress.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>System.Int32.</returns>
        internal int GetQuestProgress(uint p)
        {
            Data.Quests.TryGetValue(p, out var result);
            return result;
        }

        /// <summary>
        ///     Gets the achievement data.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>UserAchievement.</returns>
        internal UserAchievement GetAchievementData(string p) =>
            Data.Achievements.TryGetValue(p, out var result) ? result : null;

        /// <summary>
        ///     Gets the talent data.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>UserTalent.</returns>
        internal UserTalent GetTalentData(int t)
        {
            Talents.TryGetValue(t, out var result);
            return result;
        }

        /// <summary>
        ///     Gets the current talent level.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetCurrentTalentLevel()
        {
            return
                Talents.Values
                    .Select(current => Oblivion.GetGame().GetTalentManager().GetTalent(current.TalentId).Level)
                    .Concat(new[] {1})
                    .Max();
        }

        /// <summary>
        ///     _s the load my groups.
        /// </summary>
        internal void _LoadMyGroups()
        {
            DataTable dTable;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery($"SELECT id FROM groups_data WHERE owner_id = {Id}");
                dTable = dbClient.GetTable();
            }

            foreach (DataRow dRow in dTable.Rows)
                _myGroups.Add(Convert.ToUInt32(dRow["id"]));

            _loadedMyGroups = true;
        }

        /// <summary>
        ///     Gots the poll data.
        /// </summary>
        /// <param name="pollId">The poll identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GotPollData(uint pollId) => (Data.SuggestedPolls.Contains(pollId));

        /// <summary>
        ///     Checks the trading.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool CheckTrading()
        {
            if (!TradeLocked)
                return true;

            if (TradeLockExpire - Oblivion.GetUnixTimeStamp() > 0)
                return false;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery($"UPDATE users SET trade_lock = '0' WHERE id = {Id}");

            TradeLocked = false;
            return true;
        }

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        internal GameClient GetClient() => _mClient;
    }
}