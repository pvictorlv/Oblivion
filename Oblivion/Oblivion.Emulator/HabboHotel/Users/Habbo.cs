using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oblivion.Configuration;
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


        public bool BadStaff = false;

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
        /// Specify if the user allow commands like :sex
        /// </summary>
        internal bool AllowCustomCommands;

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


        internal string[] Prefixes;

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
        internal string HopperId;

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
        ///     The navigator logs
        /// </summary>
        internal Dictionary<int, NaviLogs> NavigatorLogs;

        /// <summary>
        ///     The on duty
        /// </summary>
        internal bool OnDuty;


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
        ///     The talent status
        /// </summary>
        internal string TalentStatus;

        /// <summary>
        ///     The teleporter identifier
        /// </summary>
        internal string TeleporterId;

        /// <summary>
        ///     The teleporting room identifier
        /// </summary>
        internal uint TeleportingRoomId;

        /// <summary>
        ///     TimeLoggedOn
        /// </summary>
        internal DateTime TimeLoggedOn;


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
        internal List<GroupMember> UserGroups;

        /// <summary>
        ///     The user name
        /// </summary>
        internal string UserName, Motto, Look, Gender;

        /// <summary>
        /// Specify when user executed an custom command
        /// </summary>
        internal int LastCustomCommand;

        ///<summary>
        /// User blockeds commands
        /// </summary>
        internal UserData Data;

        /// <summary>
        ///     The vip
        /// </summary>
        internal bool Vip;

        public bool LoadedGroups;

        internal ConcurrentDictionary<string, KeyValuePair<int, int>> AchievementsToUpdate;

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
        /// <param name="lastTotem"></param>
        /// <param name="vipPoints"></param>
        /// <param name="radioRank"></param>
        /// <param name="prefixes"></param>
        internal Habbo(uint id, string userName, uint rank, string motto, string look, string gender,
            int credits, int activityPoints, bool muted, uint homeRoom, int respect,
            int dailyRespectPoints, int dailyPetRespectPoints, bool hasFriendRequestsDisabled, uint currentQuestId,
            int achievementPoints, int lastOnline, bool appearOffline,
            bool hideInRoom, bool vip, double createDate, string citizenShip, int diamonds,
            List<GroupMember> groups, uint favId, int lastChange, bool tradeLocked, int tradeLockExpire,
            int buildersExpire, int buildersItemsMax, int buildersItemsUsed, bool onDuty,
            Dictionary<int, NaviLogs> naviLogs, int dailyCompetitionVotes, uint dutyLevel, bool disableAlert,
            int lastTotem, int vipPoints, string prefixes, bool badStaff)
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

            BadStaff = badStaff;
            Prefixes = prefixes.Split(',');

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
            Graffiti = vipPoints;

            AchievementPoints = achievementPoints;
            Muted = muted;
            LoadingRoom = 0u;
            CreateDate = createDate;
            LoadingChecksPassed = false;
            AllowCustomCommands = true;
            FloodTime = 0;
            CurrentRoomId = 0u;
            TimeLoggedOn = DateTime.Now;
            HomeRoom = homeRoom;
            HideInRoom = hideInRoom;
            AppearOffline = appearOffline;
            RatedRooms = new HashSet<uint>();
            Respect = respect;
            DailyRespectPoints = dailyRespectPoints;
            DailyPetRespectPoints = dailyPetRespectPoints;
            IsTeleporting = false;
            TeleporterId = "0";
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
            AchievementsToUpdate = new ConcurrentDictionary<string, KeyValuePair<int, int>>();
            WebSocketConnId = Guid.Empty;
        }

        public Guid WebSocketConnId;

        internal int Diamonds { get; set; }

        internal int Graffiti { get; set; }


        /// <summary>
        ///     The is flooded
        /// </summary>
        internal bool IsFlooded;

        /// <summary>
        ///     The flood expiry time
        /// </summary>
        internal int FloodExpiryTime;

        public void LoadGroups()
        {
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    $"SELECT group_id, rank, date_join, has_chat FROM groups_members WHERE user_id = {Id}");
                var groupsTable = dbClient.GetTable();
                UserGroups = (from DataRow row in groupsTable.Rows
                    select new GroupMember(Id, UserName, Look, (uint)row["group_id"], Convert.ToInt16(row["rank"]),
                        (int)row["date_join"], Oblivion.EnumToBool(row["has_chat"].ToString()))).ToList();
            }

            LoadedGroups = true;
        }

        /// <summary>
        ///     Gets a value indicating whether this instance can change name.
        /// </summary>
        /// <value><c>true</c> if this instance can change name; otherwise, <c>false</c>.</value>
        public bool CanChangeName() => (ExtraSettings.ChangeNameStaff && HasFuse("fuse_can_change_name")) ||
                                       (ExtraSettings.ChangeNameVip && Vip) ||
                                       (ExtraSettings.ChangeNameEveryone &&
                                        Oblivion.GetUnixTimeStamp() > (LastChange + 604800));


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
        ///     Gets the get query string.
        /// </summary>
        /// <value>The get query string.</value>
        internal string GetQueryString()
        {
            _habboinfoSaved = true;
            return string.Concat("UPDATE users SET online='0', last_online = '", Oblivion.GetUnixTimeStamp(),
                "', activity_points = '", ActivityPoints, "', ", Oblivion.GetDbConfig().DbData["emerald.column"],
                " = '", Graffiti, "', diamonds = '", Diamonds,
                "', credits = '", Credits,
                "' WHERE id = '", Id, "'; UPDATE users_stats SET achievement_score = ", AchievementPoints,
                " WHERE id=", Id, " LIMIT 1; ");
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
            _messenger = new HabboMessenger(Id);
            _messenger.Init();

            _badgeComponent = new BadgeComponent(Id);
            SpectatorMode = false;
            Disconnected = false;
            Data = data;
        }

        private int _floodCount;

        internal async Task<bool> CanTalk(bool inRoom = false)
        {
            if (IsFlooded)
            {
                if (FloodExpiryTime <= Oblivion.GetUnixTimeStamp())
                    IsFlooded = false;
                else
                    return false;
            }


            if (Rank < 4)
            {
                var span = DateTime.Now - SpamFloodTime;
                if (span.TotalSeconds > SpamProtectionTime && SpamProtectionBol)
                {
                    _floodCount = 0;
                    SpamProtectionBol = false;
                    SpamProtectionAbuse = 0;
                }
                else if (span.TotalSeconds > 4.0)
                    _floodCount = 0;


                if (span.TotalSeconds < SpamProtectionTime && SpamProtectionBol)
                {
                    using (var msg = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer")))
                    {
                        var i = SpamProtectionTime - span.Seconds;
                        await msg.AppendIntegerAsync(i);
                        IsFlooded = true;
                        FloodExpiryTime = Oblivion.GetUnixTimeStamp() + i;
                        await GetClient().SendMessageAsync(msg);

                        return false;
                    }
                }

                var floodAddon = 1u;
                if (inRoom && CurrentRoom?.RoomData != null)
                {
                    floodAddon = CurrentRoom.RoomData.ChatFloodProtection + 1;
                }

                if (span.TotalSeconds < 4.0 && _floodCount >= 5 * floodAddon)
                {
                    using (var msg = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer")))
                    {
                        SpamProtectionCount++;
                        if (SpamProtectionCount % 2 == 0)
                            SpamProtectionTime = 10 * SpamProtectionCount;
                        else
                            SpamProtectionTime = 10 * (SpamProtectionCount - 1);
                        SpamProtectionBol = true;
                        var j = SpamProtectionTime - span.Seconds;
                        await msg.AppendIntegerAsync(j);
                        IsFlooded = true;
                        FloodExpiryTime = Oblivion.GetUnixTimeStamp() + j;
                        await GetClient().SendMessageAsync(msg);

                        return false;
                    }
                }

                SpamFloodTime = DateTime.Now;
                _floodCount++;
            }

            return true;
        }
        
        internal UserClothing ClothingManager;


        /// <summary>
        ///     Initializes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="data">The data.</param>
        internal async Task Init(GameClient client, UserData data)
        {
            _mClient = client;
            _subscriptionManager = new SubscriptionManager(Id, data);
            _badgeComponent = new BadgeComponent(Id);
            _inventoryComponent = new InventoryComponent(Id, client);
            await _inventoryComponent.LoadInventory();
            _avatarEffectsInventoryComponent = new AvatarEffectsInventoryComponent(Id, client);
            _messenger = new HabboMessenger(Id);
            _messenger.Init();
            FriendCount = Convert.ToUInt32(_messenger.Friends.Count);
            SpectatorMode = false;
            Disconnected = false;
            MinimailUnreadMessages = data.MiniMailCount;
            ClothingManager = new UserClothing(Id);
            Preferences = new UserPreferences(Id);
            Data = data;
        }


        /// <summary>
        ///     Loads the data.
        /// </summary>
        /// <param name="data">The data.</param>
        internal Task LoadData(UserData data)
        {
            Data = data;
            return Task.CompletedTask;
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
        internal bool HasFuse(string fuse)
        {
            try
            {
                return Oblivion.GetGame().GetRoleManager().RankHasRight(Rank, fuse) ||
                       (GetSubscriptionManager().HasSubscription &&
                        Oblivion.GetGame()
                            .GetRoleManager()
                            .HasVip(GetSubscriptionManager().GetSubscription().SubscriptionId,
                                fuse));
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "HasFuse(fuse)");
                return false;
            }
        }


        /// <summary>
        ///     Serializes the club.
        /// </summary>
        internal async Task SerializeClub()
        {
            var client = GetClient();
            if (client?.GetHabbo()?.GetSubscriptionManager() == null) return;
            using (var serverMessage = new ServerMessage())
            {
                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("SubscriptionStatusMessageComposer"));
                await serverMessage.AppendStringAsync("club_habbo");
                if (client.GetHabbo().GetSubscriptionManager().HasSubscription)
                {
                    double num = client.GetHabbo().GetSubscriptionManager().GetSubscription().ExpireTime;
                    var num2 = num - Oblivion.GetUnixTimeStamp();
                    var num3 = (int)Math.Ceiling(num2 / 86400.0);
                    var i =
                        (int)
                        Math.Ceiling((Oblivion.GetUnixTimeStamp() -
                                      (double)client.GetHabbo().GetSubscriptionManager().GetSubscription()
                                          .ActivateTime) /
                                     86400.0);
                    var num4 = num3 / 31;

                    if (num4 >= 1)
                        num4--;

                    await serverMessage.AppendIntegerAsync(num3 - num4 * 31);
                    await serverMessage.AppendIntegerAsync(1);
                    await serverMessage.AppendIntegerAsync(num4);
                    await serverMessage.AppendIntegerAsync(1);
                    serverMessage.AppendBool(true);
                    serverMessage.AppendBool(true);
                    await serverMessage.AppendIntegerAsync(i);
                    await serverMessage.AppendIntegerAsync(i);
                    await serverMessage.AppendIntegerAsync(10);
                }
                else
                {
                    await serverMessage.AppendIntegerAsync(0);
                    await serverMessage.AppendIntegerAsync(0);
                    await serverMessage.AppendIntegerAsync(0);
                    await serverMessage.AppendIntegerAsync(0);
                    serverMessage.AppendBool(false);
                    serverMessage.AppendBool(false);
                    await serverMessage.AppendIntegerAsync(0);
                    await serverMessage.AppendIntegerAsync(0);
                    await serverMessage.AppendIntegerAsync(0);
                }

                await client.SendMessage(serverMessage);

                if (GetSubscriptionManager() == null) return;
                using (var serverMessage2 =
                       new ServerMessage(LibraryParser.OutgoingRequest("UserClubRightsMessageComposer")))
                {
                    await serverMessage2.AppendIntegerAsync(GetSubscriptionManager().HasSubscription ? 2 : 0);
                    await serverMessage2.AppendIntegerAsync(Rank);
                    serverMessage2.AppendBool(
                        Rank >= Convert.ToUInt32(Oblivion.GetDbConfig().DbData["ambassador.minrank"]));

                    await client.SendMessage(serverMessage2);
                }
            }
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


        internal async Task Dispose()
        {
            _myGroups?.Clear();
            _myGroups = null;

            _avatarEffectsInventoryComponent?.Dispose();
            _avatarEffectsInventoryComponent = null;

            if (_badgeComponent?.BadgeList != null)
            {
                _badgeComponent.BadgeList.Clear();
                _badgeComponent.BadgeList = null;
            }

            Look = null;

            try
            {
                if (_inventoryComponent != null)
                {
                    await _inventoryComponent.RunDbUpdate();
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

            if (_messenger != null)
            {
                _messenger.AppearOffline = true;
                await _messenger.Destroy();
                _messenger = null;
            }

            _subscriptionManager?.Dispose();
            _subscriptionManager = null;

            if (Prefixes != null)
            {
                Array.Clear(Prefixes, 0, Prefixes.Length);
                Prefixes = null;
            }

            NavigatorLogs?.Clear();
            NavigatorLogs = null;

            RatedRooms?.Clear();
            RatedRooms = null;

            RecentlyVisitedRooms?.Clear();
            RecentlyVisitedRooms = null;
            UserGroups?.Clear();
            UserGroups = null;

            Data?.Dispose();
            Data = null;

            AchievementsToUpdate?.Clear();
            AchievementsToUpdate = null;

            Preferences = null;


            GuideOtherUser = null;
            _mClient = null;


            CurrentRoom = null;
        }

        /// <summary>
        ///     Called when [disconnect].
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal async Task OnDisconnect(string reason)
        {
            if (Disconnected)
                return;
            Disconnected = true;

            WebSocketConnId = Guid.Empty;

            if (AchievementsToUpdate?.Count > 0)
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.Append("REPLACE INTO `users_achievements` VALUES ");
                var i = 0;
                var count = AchievementsToUpdate.Count;
                foreach (var achievements in AchievementsToUpdate)
                {
                    i++;

                    var group = achievements.Key;
                    var level = achievements.Value.Key;
                    var progress = achievements.Value.Value;
                    queryBuilder.Append(i >= count
                        ? $"('{Id}', '{group}', '{level}', '{progress}');"
                        : $"('{Id}', '{group}', '{level}', '{progress}'),");
                }

                using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    await dbClient.RunFastQueryAsync(queryBuilder.ToString());
                }
            }


            var navilogs = string.Empty;

            if (NavigatorLogs != null && NavigatorLogs.Count > 0)
            {
                foreach (var value in NavigatorLogs.Values)
                    navilogs = navilogs + $"{value.Id},{value.Value1},{value.Value2};";

                navilogs = navilogs.Remove(navilogs.Length - 1);
            }


            var getOnlineSeconds = DateTime.Now - TimeLoggedOn;
            var secondsToGive = getOnlineSeconds.Seconds;

            if (!_habboinfoSaved)
            {
                _habboinfoSaved = true;
                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    queryReactor.SetQuery("UPDATE users SET activity_points = '" + ActivityPoints +
                                          "', disabled_alert = '" + Oblivion.BoolToEnum(DisableEventAlert) +
                                          "', credits = '" +
                                          Credits + "', diamonds = '" + Diamonds + "', online='0', " +
                                          Oblivion.GetDbConfig().DbData["emerald.column"] + " = '" +
                                          Graffiti + "', last_online = '" +
                                          Oblivion.GetUnixTimeStamp() + "', builders_items_used = '" +
                                          BuildersItemsUsed +
                                          "', navilogs = @navilogs  WHERE id = '" + Id +
                                          "' LIMIT 1;UPDATE users_stats SET achievement_score=" + AchievementPoints +
                                          ", online_seconds = online_seconds + " +
                                          secondsToGive + "  WHERE id=" + Id + " LIMIT 1;");
                    queryReactor.AddParameter("navilogs", navilogs);
                    await queryReactor.RunQueryAsync();


                    if (Rank >= 4)
                        await queryReactor.RunFastQueryAsync(
                            $"UPDATE moderation_tickets SET status='open', moderator_id=0 WHERE status='picked' AND moderator_id={Id}");

                    await queryReactor.RunFastQueryAsync("UPDATE users SET block_newfriends = '" +
                                                         Convert.ToInt32(HasFriendRequestsDisabled) +
                                                         "', hide_online = '" +
                                                         Convert.ToInt32(AppearOffline) + "', hide_inroom = '" +
                                                         Convert.ToInt32(HideInRoom) + "' WHERE id = " + Id);
                }
            }

            if (CurrentRoom?.GetRoomUserManager() != null && _mClient != null)
                CurrentRoom?.GetRoomUserManager()?.RemoveUserFromRoom(_mClient, false, false);

            await Oblivion.GetGame().GetClientManager().UnregisterClient(Id, UserName);

            await Dispose();
        }

        internal uint LastBellId;

        /// <summary>
        ///     Initializes the messenger.
        /// </summary>
        internal async Task InitMessenger()
        {
            var client = GetClient();

            if (client == null)
                return;

            using (var message = await _messenger.SerializeCategories())
            {
                await client.SendMessage(message);
            }

            using (var message = await _messenger.SerializeFriends())
            {
                await client.SendMessage(message);
            }

            using (var message = await _messenger.SerializeRequests())
            {
                await client.SendMessage(message);
            }

            if (Oblivion.OfflineMessages.TryGetValue(Id, out var list))
            {
                foreach (var current in list)
                {
                    using (var msg = _messenger.SerializeOfflineMessages(current))
                    {
                        await client.SendMessage(msg);
                    }
                }

                Oblivion.OfflineMessages.Remove(Id);
                using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    OfflineMessage.RemoveAllMessages(dbClient, Id);
                }
            }

            if (_messenger.Requests.Count > Oblivion.FriendRequestLimit)
                await client.SendNotif(Oblivion.GetLanguage().GetVar("user_friend_request_max"));
//            _messenger.OnStatusChanged(false);
        }

        /// <summary>
        ///     Updates the credits balance.
        /// </summary>
        internal async Task UpdateCreditsBalance(bool inDb = false)
        {
            if (_mClient?.GetMessageHandler()?.GetResponse() == null)
                return;

            await _mClient.GetMessageHandler().GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("CreditsBalanceMessageComposer"));
            await _mClient.GetMessageHandler().GetResponse().AppendStringAsync($"{Credits}.0");
            await _mClient.GetMessageHandler().SendResponse();


            if (inDb)
            {
                await RunDbUpdate();
            }
        }

        /// <summary>
        ///     Updates the activity points balance.
        /// </summary>
        internal async Task UpdateActivityPointsBalance(bool inDb = false)
        {
            if (_mClient?.GetMessageHandler()?.GetResponse() == null)
                return;

            await _mClient.GetMessageHandler().GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(3);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(0);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(ActivityPoints);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(5);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(Diamonds);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(102);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(Graffiti);
            await _mClient.GetMessageHandler().SendResponse();


            if (inDb)
            {
                await RunDbUpdate();
            }
        }

        /// <summary>
        ///     Updates the seasonal currency balance.
        /// </summary>
        internal async Task UpdateSeasonalCurrencyBalance(bool inDb = false)
        {
            if (_mClient?.GetMessageHandler()?.GetResponse() == null)
                return;

            await _mClient.GetMessageHandler().GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(3);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(0);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(ActivityPoints);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(5);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(Diamonds);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(102);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(Graffiti);
            await _mClient.GetMessageHandler().SendResponse();


            if (inDb)
            {
                await RunDbUpdate();
            }
        }

        /// <summary>
        ///     Notifies the new pixels.
        /// </summary>
        /// <param name="change">The change.</param>
        internal async Task NotifyNewPixels(int change)
        {
            if (_mClient?.GetMessageHandler() == null || _mClient.GetMessageHandler().GetResponse() == null)
                return;

            await _mClient.GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("ActivityPointsNotificationMessageComposer"));
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(ActivityPoints);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(change);
            await _mClient.GetMessageHandler().GetResponse().AppendIntegerAsync(0);
            await _mClient.GetMessageHandler().SendResponse();
        }


        /// <summary>
        ///     Notifies the voucher.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="productDescription">The product description.</param>
        internal async Task NotifyVoucher(bool isValid, string productName, string productDescription)
        {
            if (isValid)
            {
                await _mClient.GetMessageHandler()
                    .GetResponse()
                    .InitAsync(LibraryParser.OutgoingRequest("VoucherValidMessageComposer"));
                await _mClient.GetMessageHandler().GetResponse().AppendStringAsync(productName);
                await _mClient.GetMessageHandler().GetResponse().AppendStringAsync(productDescription);
                await _mClient.GetMessageHandler().SendResponse();
                return;
            }

            await _mClient.GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("VoucherErrorMessageComposer"));
            await _mClient.GetMessageHandler().GetResponse().AppendStringAsync("1");
            await _mClient.GetMessageHandler().SendResponse();
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
        internal async Task UnMute()
        {
            if (Muted)
                await GetClient().SendNotifyAsync("You were unmuted.");

            Muted = false;

            CurrentRoom?.MutedUsers?.Remove(Id);
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
        internal async Task RunDbUpdate()
        {
            using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await dbClient.RunFastQueryAsync(
                    $"UPDATE users SET last_online = '{Oblivion.GetUnixTimeStamp()}', activity_points = '{ActivityPoints}', credits = '{Credits}', diamonds = '{Diamonds}', {Oblivion.GetDbConfig().DbData["emerald.column"]} = '{Graffiti}' WHERE id = '{Id}' LIMIT 1; ");
            }
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
        internal UserAchievement GetAchievementData(string p)
        {
            if (Data?.Achievements == null) return null;
            return Data.Achievements.TryGetValue(p, out var result) ? result : null;
        }


        /// <summary>
        ///     Gets the current talent level.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetCurrentTalentLevel()
        {
            if (Data?.Talents == null) return 0;
            return
                Data.Talents.Values
                    .Select(current => Oblivion.GetGame().GetTalentManager().GetTalent(current.TalentId).Level)
                    .Concat(new[] { 1 })
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