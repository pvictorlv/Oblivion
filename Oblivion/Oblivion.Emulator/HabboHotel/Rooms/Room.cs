using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Datas;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Rooms.Chat;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Rooms.Items.Games;
using Oblivion.HabboHotel.Rooms.Items.Games.Handlers;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Banzai;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Freeze;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Soccer;
using Oblivion.HabboHotel.Rooms.Items.Handlers;
using Oblivion.HabboHotel.Rooms.RoomInvokedItems;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;
using Oblivion.HabboHotel.Rooms.User.Trade;
using Oblivion.HabboHotel.SoundMachine;
using Oblivion.HabboHotel.SoundMachine.Songs;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Rooms
{
    /// <summary>
    ///     Class Room.
    /// </summary>
    public class Room : IDisposable
    {
        /// <summary>
        ///     The _banzai
        /// </summary>
        private BattleBanzai _banzai;

        /// <summary>
        ///     The _freeze
        /// </summary>
        private Freeze _freeze;

        /// <summary>
        ///     The _game
        /// </summary>
        private GameManager _game;

        /// <summary>
        ///     The _game item handler
        /// </summary>
        private GameItemHandler _gameItemHandler;

        /// <summary>
        ///     The _game map
        /// </summary>
        private Gamemap _gameMap;

        /// <summary>
        ///     The _idle time
        /// </summary>
        private int _idleTime;

        /// <summary>
        ///     The _is crashed
        /// </summary>
        private bool _isCrashed;

        /// <summary>
        ///     The _m cycle ended
        /// </summary>
        private bool _mCycleEnded;

        /// <summary>
        ///     The _music controller
        /// </summary>
        private SoundMachineManager _musicController;

        /// <summary>
        ///     The _process timer
        /// </summary>
        private Timer _processTimer;

        /// <summary>
        ///     The _room item handling
        /// </summary>
        private RoomItemHandler _roomItemHandler;

        /// <summary>
        ///     The _room kick
        /// </summary>
        private Queue _roomKick;

        /// <summary>
        /// The _room thread
        /// </summary>
        private Task _roomThread;

        /// <summary>
        ///     The _room user manager
        /// </summary>
        private RoomUserManager _roomUserManager;

        /// <summary>
        ///     The _soccer
        /// </summary>
        private Soccer _soccer;

        /// <summary>
        ///     The _wired handler
        /// </summary>
        private WiredHandler _wiredHandler;

        /// <summary>
        ///     The active trades
        /// </summary>
        internal ArrayList ActiveTrades;

        /// <summary>
        ///     The bans
        /// </summary>
        internal Dictionary<long, double> Bans;

        /// <summary>
        ///     The _containsBeds count of bed on room
        /// </summary>
        internal int ContainsBeds;

        /// <summary>
        ///     The _m disposed
        /// </summary>
        public bool Disposed;

        /// <summary>
        ///     The everyone got rights
        /// </summary>
        internal bool EveryoneGotRights, RoomMuted;

        /// <summary>
        ///     The just loaded
        /// </summary>
        public bool JustLoaded = true;

        internal DateTime LastTimerReset;

        /// <summary>
        ///     The loaded groups
        /// </summary>
        internal Dictionary<uint, string> LoadedGroups;

        /// <summary>
        ///     The moodlight data
        /// </summary>
        internal MoodlightData MoodlightData;

        /// <summary>
        ///     The muted bots
        /// </summary>
        internal bool MutedBots, DiscoMode, MutedPets;

        /// <summary>
        ///     The muted users
        /// </summary>
        internal Dictionary<uint, uint> MutedUsers;

        /// <summary>
        ///     The team banzai
        /// </summary>
        internal TeamManager TeamBanzai;

        /// <summary>
        ///     The team freeze
        /// </summary>
        internal TeamManager TeamFreeze;

        /// <summary>
        ///     The toner data
        /// </summary>
        internal TonerData TonerData;

        /// <summary>
        ///     The users with rights
        /// </summary>
        internal List<uint> UsersWithRights;


        /// <summary>
        ///     Gets the user count.
        /// </summary>
        /// <value>The user count.</value>
        internal int UserCount => _roomUserManager?.GetRoomUserCount() ?? 0;

        /// <summary>
        ///     Gets the tag count.
        /// </summary>
        /// <value>The tag count.</value>
        internal int TagCount => RoomData.Tags.Count;

        /// <summary>
        ///     Gets the room identifier.
        /// </summary>
        /// <value>The room identifier.</value>
        internal uint RoomId { get; private set; }

        internal double CustomHeight;


        /// <summary>
        ///     Gets a value indicating whether this instance can trade in room.
        /// </summary>
        /// <value><c>true</c> if this instance can trade in room; otherwise, <c>false</c>.</value>
        internal bool CanTradeInRoom => true;

        /// <summary>
        ///     Gets the room data.
        /// </summary>
        /// <value>The room data.</value>
        internal RoomData RoomData { get; private set; }


        internal void Start(RoomData data, bool forceLoad = false)
        {
            InitializeFromRoomData(data, forceLoad);
            GetRoomItemHandler().LoadFurniture();
            GetGameMap().GenerateMaps();
        }

        /// <summary>
        ///     Gets the wired handler.
        /// </summary>
        /// <returns>WiredHandler.</returns>
        public WiredHandler GetWiredHandler() => _wiredHandler ?? (_wiredHandler = new WiredHandler(this));


        /// <summary>
        ///     Check if handler isn't null
        /// </summary>
        /// <returns>WiredHandler.</returns>
        public bool GotWireds() => _wiredHandler != null;

        /// <summary>
        ///     Gets the game map.
        /// </summary>
        /// <returns>Gamemap.</returns>
        internal Gamemap GetGameMap() => _gameMap;

        /// <summary>
        ///     Gets the room item handler.
        /// </summary>
        /// <returns>RoomItemHandler.</returns>
        internal RoomItemHandler GetRoomItemHandler() => _roomItemHandler;

        /// <summary>
        ///     Gets the room user manager.
        /// </summary>
        /// <returns>RoomUserManager.</returns>
        internal RoomUserManager GetRoomUserManager() => _roomUserManager;

        /// <summary>
        ///     Gets the soccer.
        /// </summary>
        /// <returns>Soccer.</returns>
        internal Soccer GetSoccer() => _soccer ?? (_soccer = new Soccer(this));

        /// <summary>
        ///     Gets the team manager for banzai.
        /// </summary>
        /// <returns>TeamManager.</returns>
        internal TeamManager GetTeamManagerForBanzai() =>
            TeamBanzai ?? (TeamBanzai = TeamManager.CreateTeamforGame("banzai"));

        /// <summary>
        ///     Gets the team manager for freeze.
        /// </summary>
        /// <returns>TeamManager.</returns>
        internal TeamManager GetTeamManagerForFreeze() =>
            TeamFreeze ?? (TeamFreeze = TeamManager.CreateTeamforGame("freeze"));

        /// <summary>
        ///     Gets the banzai.
        /// </summary>
        /// <returns>BattleBanzai.</returns>
        internal BattleBanzai GetBanzai() => _banzai ?? (_banzai = new BattleBanzai(this));

        /// <summary>
        ///     Gets the freeze.
        /// </summary>
        /// <returns>Freeze.</returns>
        internal Freeze GetFreeze() => _freeze ?? (_freeze = new Freeze(this));

        /// <summary>
        ///     Gets the game manager.
        /// </summary>
        /// <returns>GameManager.</returns>
        internal GameManager GetGameManager() => _game ?? (_game = new GameManager(this));

        /// <summary>
        ///     Gets the game item handler.
        /// </summary>
        /// <returns>GameItemHandler.</returns>
        internal GameItemHandler GetGameItemHandler() =>
            _gameItemHandler ?? (_gameItemHandler = new GameItemHandler(this));

        /// <summary>
        ///     Gets the room music controller.
        /// </summary>
        /// <returns>RoomMusicController.</returns>
        internal SoundMachineManager GetRoomMusicController() =>
            _musicController ?? (_musicController = new SoundMachineManager());

        /// <summary>
        ///     Gots the music controller.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GotMusicController() => _musicController != null;

        /// <summary>
        ///     Gots the soccer.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GotSoccer() => _soccer != null;

        /// <summary>
        ///     Gots the banzai.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GotBanzai() => _banzai != null;

        /// <summary>
        ///     Gots the freeze.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GotFreeze() => _freeze != null;

        /// <summary>
        /// Starts the room processing.
        /// </summary>
        internal void StartRoomProcessing()
        {
            _processTimer = new Timer(ProcessRoom, null, 500, 500);
        }

        private bool _processingWireds;

        internal void StartWiredProcessing()
        {
            if (_processingWireds) return;

            _processingWireds = true;

            new Task(async () =>
            {
                while (GotWireds() && !Disposed)
                {
                    _wiredHandler.OnCycle();
                    await Task.Delay(250);
                }
            }, TaskCreationOptions.LongRunning).Start();
        }

        /// <summary>
        ///     Initializes the user bots.
        /// </summary>
        internal void InitUserBots()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT * FROM bots WHERE room_id = {RoomId}");
                var table = queryReactor.GetTable();
                if (table == null)
                    return;
                foreach (DataRow dataRow in table.Rows)
                {
                    if (dataRow["ai_type"].ToString() == "pet")
                    {
                        queryReactor.SetQuery($"SELECT * FROM pets_data WHERE id = '{dataRow["id"]}' LIMIT 1");
                        var row = queryReactor.GetRow();

                        if (row == null)
                            continue;

                        var pet = CatalogManager.GeneratePetFromRow(dataRow, row);

                        var bot = new RoomBot(pet.PetId, Convert.ToUInt32(RoomData.OwnerId), AiType.Pet, false);
                        bot.Update(RoomId, "freeroam", pet.Name, "", pet.Look, pet.X, pet.Y, ((int) pet.Z), 4, 0, 0, 0,
                            0,
                            null, null, "", 0, 0, false, false);
                        _roomUserManager.DeployBot(bot, pet);

                        continue;
                    }
                    var roomBot = BotManager.GenerateBotFromRow(dataRow);
                    _roomUserManager.DeployBot(roomBot, null);
                }
            }
        }

        /// <summary>
        ///     Clears the tags.
        /// </summary>
        internal void ClearTags()
        {
            RoomData.Tags.Clear();
        }

        /// <summary>
        ///     Adds the tag range.
        /// </summary>
        /// <param name="tags">The tags.</param>
        internal void AddTagRange(List<string> tags)
        {
            RoomData.Tags.AddRange(tags);
        }


        /// <summary>
        ///     Deploys the bot.
        /// </summary>
        /// <param name="bot">The bot.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser DeployBot(RoomBot bot) => _roomUserManager.DeployBot(bot, null);

        /// <summary>
        ///     Queues the room kick.
        /// </summary>
        /// <param name="kick">The kick.</param>
        internal void QueueRoomKick(RoomKick kick)
        {
            lock (_roomKick.SyncRoot)
            {
                _roomKick.Enqueue(kick);
            }
        }

        /// <summary>
        ///     Called when [room kick].
        /// </summary>
        internal void OnRoomKick()
        {
            foreach (var t in _roomUserManager.UserList.Values.ToList())
            {
                if (!t.IsBot && t.GetClient().GetHabbo().Rank < 4u)
                {
                    GetRoomUserManager().RemoveUserFromRoom(t, true, false);
                    t.GetClient().CurrentRoomUserId = -1;
                }
            }
        }

        /// <summary>
        ///     Called when [user enter].
        /// </summary>
        /// <param name="user">The user.</param>
        internal void OnUserEnter(RoomUser user)
        {
            if (GotWireds())
                GetWiredHandler().ExecuteWired(Interaction.TriggerRoomEnter, user);

            //int count = 0;


            /* TODO CHECK */
            foreach (var current in _roomUserManager.Bots.Values)
            {
                if (current.IsBot || current.IsPet)
                {
                    current.BotAi.OnUserEnterRoom(user);
                    //count++;
                }

                //if (count >= 3)
                //    break;
            }
        }

        /// <summary>
        ///     Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        /// <param name="shout">if set to <c>true</c> [shout].</param>
        internal void OnUserSay(RoomUser user, string message, bool shout)
        {
            foreach (var current in _roomUserManager.Bots.Values)
            {
                try
                {
                    if (!current.IsPet && message.ToLower().StartsWith(current.BotData.Name.ToLower()))
                    {
                        message = message.Substring(current.BotData.Name.Length);

                        if (shout)
                            current.BotAi.OnUserShout(user, message);
                        else
                            current.BotAi.OnUserSay(user, message);
                    }
                    else if (!current.IsPet && !current.BotAi.GetBotData().AutomaticChat)
                    {
                        current.BotAi.OnChatTick();
                    }
                    else if (current.IsPet && message.StartsWith(current.PetData.Name) && current.PetData.Type != 16)
                    {
                        message = message.Substring(current.PetData.Name.Length);
                        current.BotAi.OnUserSay(user, message);
                    }
                }
                catch (Exception)
                {
                    //return?
                    break;
                }
            }
        }

        /// <summary>
        ///     Loads the music.
        /// </summary>
        internal void LoadMusic()
        {
            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"SELECT items_songs.songid,items_rooms.id,items_rooms.base_item FROM items_songs LEFT JOIN items_rooms ON items_rooms.id = items_songs.itemid WHERE items_songs.roomid = {RoomId}");

                table = queryReactor.GetTable();
            }

            if (table == null)
                return;

            /* TODO CHECK */
            foreach (DataRow dataRow in table.Rows)
            {
                var songId = (uint) dataRow[0];
                if (!int.TryParse(dataRow[1].ToString(), out var num))
                {
                    num = 0;
                }
                if (!int.TryParse(dataRow[2].ToString(), out var baseItem))
                {
                    continue;
                }
//                var num = Convert.ToUInt32(dataRow[1]);
//                var baseItem = Convert.ToInt32(dataRow[2]);
                var songCode = string.Empty;
                var extraData = string.Empty;

                using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.SetQuery($"SELECT extra_data,songcode FROM items_rooms WHERE id = {num}");
                    var row = queryreactor2.GetRow();

                    if (row != null)
                    {
                        extraData = (string) row["extra_data"];
                        songCode = (string) row["songcode"];
                    }
                }

                var virtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(num);

                var diskItem = new SongItem(virtualId, songId, baseItem, extraData, songCode);

                GetRoomMusicController().AddDisk(diskItem);
            }
        }

        /// <summary>
        ///     Loads the rights.
        /// </summary>
        internal void LoadRights()
        {
            UsersWithRights = new List<uint>();
            DataTable dataTable;
            if (RoomData.Group != null)
                return;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT rooms_rights.user_id FROM rooms_rights WHERE room_id = {RoomId}");
                dataTable = queryReactor.GetTable();
            }
            if (dataTable == null)
                return;
            /* TODO CHECK */
            foreach (DataRow dataRow in dataTable.Rows)
                UsersWithRights.Add(Convert.ToUInt32(dataRow["user_id"]));
        }

        /// <summary>
        ///     Loads the bans.
        /// </summary>
        internal void LoadBans()
        {
            Bans = new Dictionary<long, double>();
            DataTable table;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT user_id, expire FROM rooms_bans WHERE room_id = {RoomId}");
                table = queryReactor.GetTable();
            }
            if (table == null)
                return;
            /* TODO CHECK */
            foreach (DataRow dataRow in table.Rows)
                Bans.Add((uint) dataRow[0], Convert.ToDouble(dataRow[1]));
        }

        /// <summary>
        ///     Checks the rights.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requireOwnerShip">if set to <c>true</c> [require ownership].</param>
        /// <param name="checkForGroups">if set to <c>true</c> [check for groups].</param>
        /// <param name="groupMembers"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool CheckRights(GameClient session, bool requireOwnerShip = false, bool checkForGroups = false,
            bool groupMembers = false, bool adminOnly = false)
        {
            try
            {
                if (session?.GetHabbo() == null) return false;
                if (session.GetHabbo().UserName == RoomData.Owner && RoomData.Type == "private") return true;
                if (session.GetHabbo().HasFuse("fuse_admin") || session.GetHabbo().HasFuse("fuse_any_room_controller"))
                    return true;

                if (RoomData.Type != "private") return false;

                if (!requireOwnerShip)
                {
                    if (session.GetHabbo().HasFuse("fuse_any_rooms_rights")) return true;
                    if (EveryoneGotRights ||
                        (UsersWithRights != null && UsersWithRights.Contains(session.GetHabbo().Id))) return true;
                }
                else return false;

                if (RoomData.Group == null) return false;

                if (groupMembers)
                {
                    if (RoomData.Group.Admins.ContainsKey(session.GetHabbo().Id)) return true;
                    if (adminOnly) return false;
                    if (RoomData.Group.Members.ContainsKey(session.GetHabbo().Id)) return true;
                }
                else if (checkForGroups)
                {
                    if (RoomData.Group.Admins.ContainsKey(session.GetHabbo().Id)) return true;
                    if (adminOnly) return false;
                    if (RoomData.Group.AdminOnlyDeco == 0u && RoomData.Group.Members.ContainsKey(session.GetHabbo().Id))
                        return true;
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.CheckRights");
            }
            return false;
        }

        /// <summary>
        ///     Checks the rights DoorBell
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requireOwnerShip">if set to <c>true</c> [require ownership].</param>
        /// <param name="checkForGroups">if set to <c>true</c> [check for groups].</param>
        /// <param name="groupMembers"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool CheckRightsDoorBell(GameClient session, bool requireOwnerShip = false,
            bool checkForGroups = false,
            bool groupMembers = false)
        {
            try
            {
                if (session?.GetHabbo() == null) return false;
                if (session.GetHabbo().UserName == RoomData.Owner && RoomData.Type == "private") return true;
                if (session.GetHabbo().HasFuse("fuse_admin") || session.GetHabbo().HasFuse("fuse_any_room_controller"))
                    return true;

                if (RoomData.Type != "private") return false;

                if (!requireOwnerShip)
                {
                    if (session.GetHabbo().HasFuse("fuse_any_rooms_rights")) return true;
                    if (EveryoneGotRights ||
                        (UsersWithRights != null && UsersWithRights.Contains(session.GetHabbo().Id))) return true;
                }
                else if (RoomData.Group != null)
                {
                    if (groupMembers)
                    {
                        if (RoomData.Group.Admins.ContainsKey(session.GetHabbo().Id)) return true;
                        if (RoomData.Group.Members.ContainsKey(session.GetHabbo().Id)) return true;
                    }
                    else if (checkForGroups)
                    {
                        if (RoomData.Group.Admins.ContainsKey(session.GetHabbo().Id)) return true;
                        if (RoomData.Group.AdminOnlyDeco == 0u &&
                            RoomData.Group.Members.ContainsKey(session.GetHabbo().Id)) return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.CheckRights");
            }
            return false;
        }

        /// <summary>
        ///     Processes the room.
        /// </summary>
        internal void ProcessRoom(object callItem)
        {
            try
            {
                if (_isCrashed || Disposed || Oblivion.ShutdownStarted)
                    return;

                try
                {
                    var idle = 0;
                    if (UserCount > 0)
                    {
                        GetRoomItemHandler().OnCycle();
                    }
                    GetRoomUserManager().OnCycle(ref idle);

                    if (idle > 0)
                        _idleTime++;
                    else
                        _idleTime = 0;

                    if (!_mCycleEnded)
                    {
                        if ((_idleTime >= 25 && !JustLoaded) || (_idleTime >= 100 && JustLoaded))
                        {
                            Oblivion.GetGame().GetRoomManager().UnloadRoom(this, "No users");
                            return;
                        }
                        var serverMessage = GetRoomUserManager().SerializeStatusUpdates(false);
                        if (serverMessage != null)
                            SendMessage(serverMessage);
                    }

                    if (UserCount <= 0) return;
                    _gameItemHandler?.OnCycle();

                    _game?.OnCycle();

                    if (GotMusicController())
                    {
                        GetRoomMusicController().Update(this);
                    }
                    if (GotWireds())
                    {
                        StartWiredProcessing();
                    }
                    WorkRoomKickQueue();
                }
                catch (Exception e)
                {
                    Writer.Writer.LogException(e.ToString());
                    OnRoomCrash(e);
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException($"Sub crash in room cycle: {e}");
            }
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(byte[] message)
        {
            try
            {
                if (_roomUserManager?.UserList != null)
                    foreach (var user in _roomUserManager.UserList.Values.Where(user =>
                        user?.GetClient()?.GetConnection() != null && !user.IsBot))
                    {
                        user.GetClient().SendMessage(message);
                    }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "SendMessage");
            }
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessageWithRange(Vector2D currentLocation, ServerMessage message)
        {
            try
            {
                var data = message.GetReversedBytes();
                if (_roomUserManager?.UserList == null) return;
                foreach (var user in from user in _roomUserManager.UserList.Values
                    where user?.GetClient()?.GetConnection() != null && !user.IsBot
                    let userCoord = new Vector2D(user.X, user.Y)
                    where userCoord.GetDistanceSquared(userCoord) <=
                          RoomData.ChatMaxDistance * RoomData.ChatMaxDistance * 2
                    select user)
                {
                    user.GetClient().SendMessage(data);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "SendMessage");
            }
        }

        /// <summary>
        ///     Broadcasts the chat message.
        /// </summary>
        /// <param name="chatMsg">The chat MSG.</param>
        /// <param name="roomUser">The room user.</param>
        /// <param name="p">The p.</param>
        internal void BroadcastChatMessageWithRange(ServerMessage chatMsg, RoomUser roomUser, uint p)
        {
            try
            {
                if (roomUser == null) return;
                var packetData = chatMsg.GetReversedBytes();
                var senderCoord = new Vector2D(roomUser.X, roomUser.Y);
                foreach (var user in _roomUserManager.UserList.Values)
                {
                    if (user == null) continue;

                    if (user.IsBot || user.IsPet)
                        continue;

                    var usersClient = user.GetClient();
                    if (usersClient?.GetHabbo()?.Data == null)
                        continue;

                    Vector2D userCoord = new Vector2D(user.X, user.Y);
                    if (senderCoord.GetDistanceSquared(userCoord) <=
                        (RoomData.ChatMaxDistance * RoomData.ChatMaxDistance * 2))
                    {
                        try
                        {
                            if (user.OnCampingTent || !roomUser.OnCampingTent)
                            {
                                if (!usersClient.GetHabbo().Data.Ignores.Contains(p))
                                    usersClient.SendMessage(packetData);
                            }
                        }
                        catch (Exception e)
                        {
                            Logging.HandleException(e, "Room.SendMessageToUsersWithRights");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SendMessageToUsersWithRights");
            }
        }


        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(ServerMessage message)
        {
            if (message != null)
                SendMessage(message.GetReversedBytes());
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="messages">The messages.</param>
        internal void SendMessage(List<ServerMessage> messages)
        {
            if (messages.Count == 0)
                return;

            try
            {
                var totalBytes = new byte[0];
                var currentWorking = 0;

                /* TODO CHECK */
                foreach (var message in messages)
                {
                    var toAppend = message.GetReversedBytes();

                    var newLength = totalBytes.Length + toAppend.Length;

                    Array.Resize(ref totalBytes, newLength);

                    /* TODO CHECK */
                    foreach (var t in toAppend)
                    {
                        totalBytes[currentWorking] = t;
                        currentWorking++;
                    }
                }

                SendMessage(totalBytes);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "SendMessage List<ServerMessage>");
            }
        }


        /// <summary>
        ///     Sends the message to users with rights.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessageToUsersWithRights(ServerMessage message)
        {
            var messagebytes = message.GetReversedBytes();

            try
            {
                /* TODO CHECK */
                foreach (var unit in _roomUserManager.UserList.Values)
                {
                    var user = unit;
                    if (user == null)
                        continue;

                    if (user.IsBot)
                        continue;

                    var usersClient = user.GetClient();
                    if (usersClient?.GetConnection() == null)
                        continue;

                    if (!CheckRights(usersClient))
                        continue;

                    usersClient.SendMessage(messagebytes);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "SendMessageToUsersWithRights");
            }
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            SendMessage(new ServerMessage(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer")));
            Dispose();
        }

        /// <summary>
        ///     Users the is banned.
        /// </summary>
        /// <param name="pId">The p identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool UserIsBanned(uint pId) => Bans.ContainsKey(pId);

        /// <summary>
        ///     Removes the ban.
        /// </summary>
        /// <param name="pId">The p identifier.</param>
        internal void RemoveBan(uint pId)
        {
            Bans.Remove(pId);
        }

        /// <summary>
        ///     Adds the ban.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="time">The time.</param>
        internal void AddBan(int userId, long time)
        {
            if (!Bans.ContainsKey(Convert.ToInt32(userId)))
                Bans.Add(userId, ((Oblivion.GetUnixTimeStamp()) + time));

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery("REPLACE INTO rooms_bans VALUES (" + userId + ", " + RoomId + ", '" +
                                          (Oblivion.GetUnixTimeStamp() + time) + "')");
        }

        /// <summary>
        ///     Banneds the users.
        /// </summary>
        /// <returns>List&lt;System.UInt32&gt;.</returns>
        internal List<uint> BannedUsers()
        {
            var list = new List<uint>();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"SELECT user_id FROM rooms_bans WHERE room_id={RoomId} AND expire > UNIX_TIMESTAMP()");
                var table = queryReactor.GetTable();
                list.AddRange(from DataRow dataRow in table.Rows select (uint) dataRow[0]);
            }
            return list;
        }

        /// <summary>
        ///     Determines whether [has ban expired] [the specified p identifier].
        /// </summary>
        /// <param name="pId">The p identifier.</param>
        /// <returns><c>true</c> if [has ban expired] [the specified p identifier]; otherwise, <c>false</c>.</returns>
        internal bool HasBanExpired(uint pId) => !UserIsBanned(pId) || Bans[pId] < Oblivion.GetUnixTimeStamp();

        /// <summary>
        ///     Unbans the specified user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void Unban(uint userId)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery("DELETE FROM rooms_bans WHERE user_id=" + userId + " AND room_id=" + RoomId +
                                          " LIMIT 1");
            Bans.Remove(userId);
        }

        /// <summary>
        ///     Determines whether [has active trade] [the specified user].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if [has active trade] [the specified user]; otherwise, <c>false</c>.</returns>
        internal bool HasActiveTrade(RoomUser user) => !user.IsBot && HasActiveTrade(user.GetClient().GetHabbo().Id);

        /// <summary>
        ///     Determines whether [has active trade] [the specified user identifier].
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns><c>true</c> if [has active trade] [the specified user identifier]; otherwise, <c>false</c>.</returns>
        internal bool HasActiveTrade(uint userId)
        {
            var array = ActiveTrades.ToArray();
            return array.Cast<Trade>().Any(trade => trade.ContainsUser(userId));
        }

        /// <summary>
        ///     Gets the user trade.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Trade.</returns>
        internal Trade GetUserTrade(uint userId)
        {
            return ActiveTrades.Cast<Trade>().FirstOrDefault(trade => trade.ContainsUser(userId));
        }

        /// <summary>
        ///     Tries the start trade.
        /// </summary>
        /// <param name="userOne">The user one.</param>
        /// <param name="userTwo">The user two.</param>
        internal void TryStartTrade(RoomUser userOne, RoomUser userTwo)
        {
            if (userOne == null || userTwo == null || userOne.IsBot || userTwo.IsBot || userOne.IsTrading ||
                userTwo.IsTrading || HasActiveTrade(userOne) || HasActiveTrade(userTwo))
                return;
            ActiveTrades.Add(new Trade(userOne.GetClient().GetHabbo().Id, userTwo.GetClient().GetHabbo().Id, RoomId));
        }

        /// <summary>
        ///     Tries the stop trade.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void TryStopTrade(uint userId)
        {
            var userTrade = GetUserTrade(userId);
            if (userTrade == null)
                return;
            userTrade.CloseTrade(userId);
            ActiveTrades.Remove(userTrade);
        }

        /// <summary>
        ///     Sets the maximum users.
        /// </summary>
        /// <param name="maxUsers">The maximum users.</param>
        internal void SetMaxUsers(uint maxUsers)
        {
            RoomData.UsersMax = maxUsers;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery("UPDATE rooms_data SET users_max = " + maxUsers + " WHERE id = " + RoomId);
        }

        /// <summary>
        ///     Check if u can go to bed
        /// </summary>
        /// <param name="user">The maximum users.</param>
        /// <param name="goalX">Click To X.</param>
        /// <param name="goalY">Click To Y.</param>
        internal bool MovedToBed(RoomUser user, ref int goalX, ref int goalY)
        {
            if (ContainsBeds > 0)
            {
                var furni = GetGameMap().GetCoordinatedItems(new Point(goalX, goalY));
                /* TODO CHECK */
                foreach (var furno in furni)
                {
                    if (furno.GetBaseItem().InteractionType == Interaction.Bed ||
                        furno.GetBaseItem().InteractionType == Interaction.PressurePadBed)
                    {
                        if (furno.Rot == 0 || furno.Rot == 4)
                        {
                            if (user.X == goalX)
                                return false;
                        }
                        else
                        {
                            if (user.Y == goalY)
                                return false;
                        }
                        /* TODO CHECK */
                        foreach (var casella in furno.GetCoords())
                        {
                            if (casella.X == goalX && casella.Y == goalY)
                            {
                                if (furno.Rot == 0 || furno.Rot == 4)
                                {
                                    if (GetGameMap().CanWalk(casella.X, furno.Y, false))
                                    {
                                        goalX = casella.X;
                                        goalY = furno.Y;
                                        return true;
                                    }
                                    return false;
                                }
                                if (GetGameMap().CanWalk(furno.X, casella.Y, false))
                                {
                                    goalX = furno.X;
                                    goalY = casella.Y;
                                    return true;
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///     Flushes the settings.
        /// </summary>
        internal void FlushSettings()
        {
            _mCycleEnded = true;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                GetRoomItemHandler().SaveFurniture(queryReactor);
            RoomData.Tags.Clear();
            UsersWithRights.Clear();
            Bans.Clear();
            ActiveTrades.Clear();
            LoadedGroups.Clear();
            if (GotFreeze())
                _freeze = new Freeze(this);
            if (GotBanzai())
                _banzai = new BattleBanzai(this);
            if (GotSoccer())
                _soccer = new Soccer(this);
            if (_gameItemHandler != null)
                _gameItemHandler = new GameItemHandler(this);
        }

        /// <summary>
        ///     Reloads the settings.
        /// </summary>
        internal void ReloadSettings()
        {
            var data = Oblivion.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            InitializeFromRoomData(data, false);
        }

        /// <summary>
        ///     Updates the furniture.
        /// </summary>
        internal void UpdateFurniture()
        {
            var list = new List<ServerMessage>();

            /* TODO CHECK */
            foreach (var roomItem in GetRoomItemHandler().FloorItems.Values.ToList())
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
                roomItem.Serialize(serverMessage);
                list.Add(serverMessage);
            }
            /* TODO CHECK */
            foreach (var roomItem2 in GetRoomItemHandler().WallItems.Values.ToList())
            {
                var serverMessage2 =
                    new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomWallItemMessageComposer"));
                roomItem2.Serialize(serverMessage2);
                list.Add(serverMessage2);
            }
            SendMessage(list);
        }

        /// <summary>
        ///     Checks the mute.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool CheckMute(GameClient session)
        {
            if (RoomMuted || session.GetHabbo().Muted)
                return true;
            if (!MutedUsers.TryGetValue(session.GetHabbo().Id, out var user))
                return false;
            if (user >= Oblivion.GetUnixTimeStamp())
                return true;
            MutedUsers.Remove(session.GetHabbo().Id);
            return false;
        }

        /// <summary>
        ///     Adds the chatlog.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="globalMessage"></param>
        internal void AddChatlog(uint id, string message, bool globalMessage)
        {
            if (RoomData?.RoomChat == null) return;

            if (RoomData.RoomChat.Count >= 200)
            {
                var toRemove = RoomData.RoomChat.FirstOrDefault();
                if (toRemove == null) return;
                RoomData.RoomChat.Remove(toRemove);
            }
            RoomData.RoomChat.Add(new Chatlog(id, message, DateTime.Now, globalMessage));
        }

        /// <summary>
        ///     Resets the game map.
        /// </summary>
        /// <param name="newModelName">New name of the model.</param>
        /// <param name="wallHeight">Height of the wall.</param>
        /// <param name="wallThick">The wall thick.</param>
        /// <param name="floorThick">The floor thick.</param>
        internal void ResetGameMap(string newModelName, int wallHeight, int wallThick, int floorThick)
        {
            RoomData.ModelName = newModelName;
            RoomData.ModelName = newModelName;
            RoomData.ResetModel();
            RoomData.WallHeight = wallHeight;
            RoomData.WallThickness = wallThick;
            RoomData.FloorThickness = floorThick;
            _gameMap = new Gamemap(this);
        }

        /// <summary>
        ///     Initializes from room data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="forceLoad"></param>
        private void InitializeFromRoomData(RoomData data, bool forceLoad)
        {
            Initialize(data.Id, data, data.AllowRightsOverride, forceLoad);
        }

        /// <summary>
        ///     Initializes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="roomData">The room data.</param>
        /// <param name="rightOverride">if set to <c>true</c> [right override].</param>
        /// <param name="forceLoad"></param>
        private void Initialize(uint id, RoomData roomData, bool rightOverride, bool forceLoad)
        {
            RoomData = roomData;
            Disposed = false;
            RoomId = id;
            CustomHeight = -1;
            Bans = new Dictionary<long, double>();
            MutedUsers = new Dictionary<uint, uint>();
            ActiveTrades = new ArrayList();
            MutedBots = false;
            MutedPets = false;
            _mCycleEnded = false;
            EveryoneGotRights = rightOverride;
            LoadedGroups = new Dictionary<uint, string>();
            _roomKick = new Queue();
            _idleTime = 0;
            RoomMuted = false;
            _gameMap = new Gamemap(this);
            _roomItemHandler = new RoomItemHandler(this);
            _roomUserManager = new RoomUserManager(this);

            LoadRights();
            LoadBans();
            InitUserBots();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                if (roomData?.WordFilter != null)
                {
                    queryReactor.SetQuery($"SELECT word FROM rooms_wordfilter WHERE room_id = {id}");
                    var tableFilter = queryReactor.GetTable();

                    foreach (DataRow dataRow in tableFilter.Rows)
                        roomData.WordFilter.Add(dataRow["word"].ToString());

                }

                if (roomData.BlockedCommands != null)
                {
                    queryReactor.SetQuery(
                        $"SELECT command_name FROM room_blockcmd WHERE room_id = '{id}'");
                    var tableCmd = queryReactor.GetTable();
                    foreach (DataRow data in tableCmd.Rows)
                        roomData.BlockedCommands.Add(data["command_name"].ToString());
                }
            }
            if (!forceLoad)
            {
                _roomThread = new Task(StartRoomProcessing, TaskCreationOptions.LongRunning);
                _roomThread.Start();
            }

            Oblivion.GetGame().GetRoomManager().QueueActiveRoomAdd(RoomData);
        }

        /// <summary>
        ///     Works the room kick queue.
        /// </summary>
        private void WorkRoomKickQueue()
        {
            if (_roomKick == null) return;

            lock (_roomKick.SyncRoot)
            {
                while (_roomKick.Count > 0)
                {
                    var roomKick = (RoomKick) _roomKick.Dequeue();
                    foreach (var current in _roomUserManager.UserList.Values)
                    {
                        if (current?.GetClient()?.GetHabbo() != null && !current.IsBot &&
                            current.GetClient().GetHabbo().Rank < roomKick.MinRank)
                        {
                            if (roomKick.Alert.Length > 0)
                                current.GetClient()
                                    .SendNotif(string.Format(Oblivion.GetLanguage().GetVar("kick_mod_room_message"),
                                        roomKick.Alert));
                            GetRoomUserManager().RemoveUserFromRoom(current, true, false);
                            current.GetClient().CurrentRoomUserId = -1;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Called when [room crash].
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnRoomCrash(Exception e)
        {
            Logging.LogThreadException(e.ToString(), $"Room cycle task for room {RoomId}");
            Oblivion.GetGame().GetRoomManager().UnloadRoom(this, "Room crashed");
            _isCrashed = true;
        }

        /// <summary>
        ///     Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            _roomUserManager.Disposed = true;
            _mCycleEnded = true;
            Oblivion.GetGame().GetRoomManager().QueueActiveRoomRemove(RoomData);
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                GetRoomItemHandler().SaveFurniture(queryReactor);
            }
            if (GotSoccer())
            {
                _soccer.Destroy();
                _soccer = null;
            }
            if (GotBanzai())
            {
                _banzai.Destroy();
                _banzai = null;
            }
            if (GotFreeze())
            {
                _freeze.Destroy();
                _freeze = null;
            }
            if (GotWireds())
            {
                _wiredHandler.Destroy();
                _wiredHandler = null;
            }
            if (GotMusicController())
            {
                _musicController.Destroy();
                _musicController = null;
            }

            if (RoomData.RoomChat != null && RoomData.RoomChat.Count > 0)
            {
                var i = 0;

                var builder = new StringBuilder();
                var limit = RoomData.RoomChat.Count;
                using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    builder.Append("INSERT INTO users_chatlogs (user_id, room_id, timestamp, message) VALUES ");
                    foreach (var chat in RoomData.RoomChat)
                    {
                        i++;

                        if (i >= limit || i >= 100)
                        {
                            builder.Append(
                                $"('{chat.UserId}', '{RoomId}', '{Oblivion.DateTimeToUnix(chat.TimeStamp)}', @message{i});");
                            dbClient.AddParameter("message" + i, chat.Message);
                            break;
                        }
                        builder.Append(
                            $"('{chat.UserId}', '{RoomId}', '{Oblivion.DateTimeToUnix(chat.TimeStamp)}', @message{i}),");
                        dbClient.AddParameter("message" + i, chat.Message);

//                        chat.Save(RoomId, dbClient);
                    }
                    dbClient.RunQuery(builder.ToString());
                }
                RoomData.RoomChat.Clear();
            }

            _game?.Destroy();
            _game = null;
            _gameItemHandler?.Destroy();
            _gameItemHandler = null;
            if (_gameMap != null)
            {
                _gameMap.Model?.Destroy();
                _gameMap.StaticModel?.Destroy();
                _gameMap.Destroy();
            }
            _gameMap = null;
            _processTimer?.Dispose();
            _processTimer = null;
            RoomData?.Tags?.Clear();
            RoomData.Tags = null;
            RoomData?.BlockedCommands?.Clear();
            RoomData.BlockedCommands = null;
            UsersWithRights.Clear();

            Bans.Clear();
            Bans = null;
            LoadedGroups.Clear();
            MoodlightData = null;
            foreach (var current in _roomItemHandler.GetWallAndFloor)
            {
                current.Destroy();
            }
            ActiveTrades.Clear();
            ActiveTrades = null;
            _roomItemHandler.Destroy();
            _roomItemHandler = null;
            _roomUserManager.Destroy();
            _roomUserManager = null;
            RoomData?.Dispose();
            RoomData = null;
            TonerData = null;
            MutedUsers.Clear();
            MutedUsers = null;
            TeamFreeze?.Dispose();
            TeamBanzai?.Dispose();
            TeamBanzai = null;
            TeamFreeze = null;
            UsersWithRights = null;
            Oblivion.GetGame().GetRoomManager().RemoveRoomData(RoomId);

            _roomThread?.Dispose();
            _roomThread = null;

            _roomKick?.Clear();
            _roomKick = null;
        }
    }
}