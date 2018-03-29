using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Collections;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Events;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Rooms
{
    /// <summary>
    ///     Class RoomManager.
    /// </summary>
    internal class RoomManager
    {
        /// <summary>
        ///     The _active rooms
        /// </summary>
        private readonly Dictionary<RoomData, uint> _activeRooms;

        /// <summary>
        ///     The _active rooms add queue
        /// </summary>
        private readonly Queue _activeRoomsAddQueue;

        /// <summary>
        ///     The _active rooms update queue
        /// </summary>
        private readonly Queue _activeRoomsUpdateQueue;

        /// <summary>
        ///     The _event manager
        /// </summary>
        private readonly EventManager _eventManager;

        /// <summary>
        ///     The _room models
        /// </summary>
//        private readonly HybridDictionary _roomModels;
        /// <summary>
        ///     The _voted rooms
        /// </summary>
        private readonly Dictionary<RoomData, int> _votedRooms;

        /// <summary>
        ///     The _voted rooms add queue
        /// </summary>
        private readonly Queue _votedRoomsAddQueue;

        /// <summary>
        ///     The _voted rooms remove queue
        /// </summary>
        private readonly Queue _votedRoomsRemoveQueue;

        internal readonly ConcurrentDictionary<uint, RoomData> LoadedRoomData;

        private RoomCompetitionManager _competitionManager;

        /// <summary>
        ///     The _ordered active rooms
        /// </summary>
        private IEnumerable<KeyValuePair<RoomData, uint>> _orderedActiveRooms;

        /// <summary>
        ///     The _ordered voted rooms
        /// </summary>
        private IEnumerable<KeyValuePair<RoomData, int>> _orderedVotedRooms;

        /// <summary>
        ///     The active rooms remove queue
        /// </summary>
        public Queue ActiveRoomsRemoveQueue;

        /// <summary>
        ///     The loaded rooms
        /// </summary>
        internal ConcurrentDictionary<uint, Room> LoadedRooms;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomManager" /> class.
        /// </summary>
        internal RoomManager()
        {
            LoadedRooms = new ConcurrentDictionary<uint, Room>();
            LoadedRoomData = new ConcurrentDictionary<uint, RoomData>();
            _votedRooms = new Dictionary<RoomData, int>();
            _activeRooms = new Dictionary<RoomData, uint>();
            _votedRoomsRemoveQueue = new Queue();
            _votedRoomsAddQueue = new Queue();
            ActiveRoomsRemoveQueue = new Queue();
            _activeRoomsUpdateQueue = new Queue();
            _activeRoomsAddQueue = new Queue();
            _eventManager = new EventManager();
            LoadedBallRooms = new ConcurrentList<Room>();
        }

        /// <summary>
        ///     Gets the loaded rooms count.
        /// </summary>
        /// <value>The loaded rooms count.</value>
        internal int LoadedRoomsCount => LoadedRooms.Count;

        internal RoomCompetitionManager GetCompetitionManager() => _competitionManager;

        internal void LoadCompetitionManager()
        {
            _competitionManager = new RoomCompetitionManager();
        }

        /// <summary>
        ///     Gets the active rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.UInt32&gt;[].</returns>
        internal KeyValuePair<RoomData, uint>[] GetActiveRooms() => _orderedActiveRooms?.ToArray();

        /// <summary>
        ///     Gets the voted rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.Int32&gt;[].</returns>
        internal KeyValuePair<RoomData, int>[] GetVotedRooms() => _orderedVotedRooms?.ToArray();

        /// <summary>
        ///     Gets the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>RoomModel.</returns>
        internal RoomModel GetModel(string model, uint roomId) => LoadModel(model, roomId);

        /// <summary>
        ///     Generates the nullable room data.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>RoomData.</returns>
        internal RoomData GenerateNullableRoomData(uint roomId)
        {
            if (GenerateRoomData(roomId) != null)
                return GenerateRoomData(roomId);
            var roomData = new RoomData();
            roomData.FillNull(roomId);
            return roomData;
        }


        /// <summary>
        ///     Generates the room data.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>RoomData.</returns>
        internal RoomData GenerateRoomData(uint roomId)
        {
            if (LoadedRoomData.TryGetValue(roomId, out var room))
            {
                room.LastUsed = DateTime.Now;
                return room;
            }

            var data = GetRoom(roomId);
            if (data != null)
                return data.RoomData;

            var roomData = new RoomData();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT * FROM rooms_data WHERE id = {roomId} LIMIT 1");

                var dataRow = queryReactor.GetRow();
                if (dataRow == null)
                    return null;

                roomData.Fill(dataRow);
                LoadedRoomData.TryAdd(roomId, roomData);
            }

            return roomData;
        }

        /// <summary>
        ///     Gets the event rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.UInt32&gt;[].</returns>
        internal KeyValuePair<RoomData, uint>[] GetEventRooms() => _eventManager.GetRooms();

        /// <summary>
        ///     Loads the room.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="forceLoad"></param>
        /// <returns>Room.</returns>
        internal Room LoadRoom(uint id, bool forceLoad = false)
        {
            try
            {
                var data = GetRoom(id);
                if (data != null)
                    return data;

                var roomData = GenerateRoomData(id);
                if (roomData == null)
                    return null;

                var room = new Room();

                LoadedRooms.AddOrUpdate(id, room, (key, value) => room);

                room.Start(roomData, forceLoad);

//                Out.WriteLine($"Room #{id} was loaded", "Oblivion.Room.Manager", ConsoleColor.DarkCyan);

//                room.InitBots();
//                room.InitPets();
                return room;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "RoomLoad");
                return null;
            }
        }

        internal void RemoveRoomData(uint id) => LoadedRoomData.TryRemove(id, out _);

        /// <summary>
        ///     Fetches the room data.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="dRow">The d row.</param>
        /// <returns>RoomData.</returns>
        internal RoomData FetchRoomData(uint roomId, DataRow dRow, uint user = 0u)
        {
            if (LoadedRoomData.TryGetValue(roomId, out var roomData))
            {
                roomData.LastUsed = DateTime.Now;
                return roomData;
            }

            roomData = new RoomData();

            roomData.Fill(dRow, user);
            LoadedRoomData.TryAdd(roomId, roomData);
            return roomData;
        }

        /// <summary>
        ///     Gets the room.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>Room.</returns>
        internal Room GetRoom(uint roomId) => LoadedRooms.TryGetValue(roomId, out var result) ? result : null;

        /// <summary>
        ///     Creates the room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="model">The model.</param>
        /// <param name="category">The category.</param>
        /// <param name="maxVisitors">The maximum visitors.</param>
        /// <param name="tradeState">State of the trade.</param>
        /// <returns>RoomData.</returns>
        internal RoomData CreateRoom(GameClient session, string name, string desc, string model, int category,
            int maxVisitors, int tradeState)
        {
            uint roomId;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO rooms_data (roomtype,caption,description,owner,model_name,category,users_max,trade_state) VALUES ('private',@caption,@desc,@Username,@model,@cat,@usmax,@tstate)");
                dbClient.AddParameter("caption", Oblivion.FilterInjectionChars(name));
                dbClient.AddParameter("desc", Oblivion.FilterInjectionChars(desc));
                dbClient.AddParameter("Username", session.GetHabbo().UserName);
                dbClient.AddParameter("model", model);
                dbClient.AddParameter("cat", category);
                dbClient.AddParameter("usmax", maxVisitors);
                dbClient.AddParameter("tstate", tradeState.ToString());
                roomId = (uint) dbClient.InsertQuery();
            }

            var data = GenerateRoomData(roomId);
            if (data == null) return null;

            session.GetHabbo().Data.Rooms.Add(roomId);
            return data;
        }


        internal RoomModel LoadModel(string model, uint roomid)
        {
            try
            {
                using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    if (model == "custom")
                    {
                        dbClient.SetQuery("SELECT * FROM rooms_models_customs WHERE roomid = @room LIMIT 1");
                        dbClient.AddParameter("room", roomid);
                        var row = dbClient.GetRow();

                        if (row == null) return new RoomModel(0, 0, 0, 0, "xxxxxx", false);


                        return new RoomModel((int) row["door_x"], (int) row["door_y"], (double) row["door_z"],
                            (int) row["door_dir"],
                            (string) row["heightmap"], false);
                    }

                    dbClient.SetQuery("SELECT * FROM rooms_models WHERE id = @name LIMIT 1");
                    dbClient.AddParameter("name", model);
                    var dataRow = dbClient.GetRow();
                    if (dataRow == null) return new RoomModel(0, 0, 0, 0, "xxxxxx", false);
                    return new RoomModel((int) dataRow["door_x"], (int) dataRow["door_y"],
                        (double) dataRow["door_z"],
                        (int) dataRow["door_dir"], (string) dataRow["heightmap"],
                        Oblivion.EnumToBool(dataRow["club_only"].ToString()));
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "loadmodel");
                return new RoomModel(0, 0, 0, 0, "xxxxxx", false);
            }
        }


        public ConcurrentList<Room> LoadedBallRooms;

        private DateTime _cycleBallLastExecution;


        /// <summary>
        ///     Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            if (LoadedBallRooms.Count > 0)
            {
                var sinceBallLastTime = DateTime.Now - _cycleBallLastExecution;
                if (sinceBallLastTime.TotalMilliseconds >= 145)
                {
                    _cycleBallLastExecution = DateTime.Now;
                    /* TODO CHECK */
                    foreach (var Room in LoadedBallRooms.ToList())
                    {
                        try
                        {
                            if (Room.GotSoccer())
                                Room.GetSoccer().OnCycle();
                        }
                        catch (Exception e)
                        {
                            Logging.LogCriticalException("INVALID MARIO BUG IN BALLMOVEMENT: <" + Room.RoomData.Id +
                                                         "> :" +
                                                         e);
                        }
                    }
                }
            }

            try
            {
                var flag = WorkActiveRoomsAddQueue();
                var flag2 = WorkActiveRoomsRemoveQueue();
                var flag3 = WorkActiveRoomsUpdateQueue();
                if (flag || flag2 || flag3) SortActiveRooms();
                var flag4 = WorkVotedRoomsAddQueue();
                var flag5 = WorkVotedRoomsRemoveQueue();
                if (flag4 || flag5) SortVotedRooms();

                Oblivion.GetGame().RoomManagerCycleEnded = true;
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "RoomManager.OnCycle Exception --> Not inclusive");
            }
        }

        /// <summary>
        ///     Queues the vote add.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueVoteAdd(RoomData data)
        {
            lock (_votedRoomsAddQueue.SyncRoot)
            {
                _votedRoomsAddQueue.Enqueue(data);
            }
        }

        /// <summary>
        ///     Queues the vote remove.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueVoteRemove(RoomData data)
        {
            lock (_votedRoomsRemoveQueue.SyncRoot)
            {
                _votedRoomsRemoveQueue.Enqueue(data);
            }
        }

        /// <summary>
        ///     Queues the active room update.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueActiveRoomUpdate(RoomData data)
        {
            lock (_activeRoomsUpdateQueue.SyncRoot)
            {
                _activeRoomsUpdateQueue.Enqueue(data);
            }
        }

        /// <summary>
        ///     Queues the active room add.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueActiveRoomAdd(RoomData data)
        {
            lock (_activeRoomsAddQueue.SyncRoot)
            {
                _activeRoomsAddQueue.Enqueue(data);
            }
        }

        /// <summary>
        ///     Queues the active room remove.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueActiveRoomRemove(RoomData data)
        {
            lock (ActiveRoomsRemoveQueue.SyncRoot)
            {
                ActiveRoomsRemoveQueue.Enqueue(data);
            }
        }

        /// <summary>
        ///     Removes all rooms.
        /// </summary>
        internal void RemoveAllRooms()
        {
            foreach (var current in LoadedRooms.Values)
            {
                try
                {
                    Oblivion.GetGame().GetRoomManager().UnloadRoom(current);
                }
                catch
                {
                    Out.WriteLine("Error unloading room");
                }
            }

            Out.WriteLine("RoomManager Destroyed", "Oblivion.RoomManager", ConsoleColor.DarkYellow);
        }

        /// <summary>
        ///     Unloads the room.
        /// </summary>
        /// <param name="room">The room.</param>
        internal void UnloadRoom(Room room)
        {
            if (room?.RoomData == null)
                return;

            if (room.Disposed) return;

            room.Disposed = true;

            if (Oblivion.GetGame().GetNavigator().PrivateCategories
                .TryGetValue(room.RoomData.Category, out var flatCat))
            {
                flatCat.UsersNow -= room.UserCount;
            }

            room.RoomData.UsersNow = 0;
            var state = "open";

            if (room.RoomData.State == 1)
                state = "locked";
            else if (room.RoomData.State > 1)
                state = "password";

            var roomId = room.RoomId;

            try
            {
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(
                        "UPDATE rooms_data SET caption = @caption, description = @description, password = @password, category = " +
                        room.RoomData.Category + ", state = '" + state +
                        "', tags = @tags, users_max = " +
                        room.RoomData.UsersMax + ", allow_pets = '" + Oblivion.BoolToEnum(room.RoomData.AllowPets) +
                        "', allow_pets_eat = '" +
                        Oblivion.BoolToEnum(room.RoomData.AllowPetsEating) + "', allow_walkthrough = '" +
                        Oblivion.BoolToEnum(room.RoomData.AllowWalkThrough) +
                        "', hidewall = '" + Oblivion.BoolToEnum(room.RoomData.HideWall) + "', floorthick = " +
                        room.RoomData.FloorThickness +
                        ", wallthick = " + room.RoomData.WallThickness + ", mute_settings='" +
                        room.RoomData.WhoCanMute +
                        "', kick_settings='" + room.RoomData.WhoCanKick + "',ban_settings='" + room.RoomData.WhoCanBan +
                        "', walls_height = '" + room.RoomData.WallHeight +
                        "', chat_type = @chat_t,chat_balloon = @chat_b,chat_speed = @chat_s,chat_max_distance = @chat_m,chat_flood_protection = @chat_f, trade_state = '" +
                        room.RoomData.TradeState + "' WHERE id = " + roomId);
                    queryReactor.AddParameter("caption", room.RoomData.Name);
                    queryReactor.AddParameter("description", room.RoomData.Description);
                    queryReactor.AddParameter("password", room.RoomData.PassWord);
                    queryReactor.AddParameter("tags", string.Join(",", room.RoomData.Tags));
                    queryReactor.AddParameter("chat_t", room.RoomData.ChatType);
                    queryReactor.AddParameter("chat_b", room.RoomData.ChatBalloon);
                    queryReactor.AddParameter("chat_s", room.RoomData.ChatSpeed);
                    queryReactor.AddParameter("chat_m", room.RoomData.ChatMaxDistance);
                    queryReactor.AddParameter("chat_f", room.RoomData.ChatFloodProtection);
                    queryReactor.RunQuery();
                }
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }

            if (room.GetRoomUserManager() != null && room.GetRoomUserManager().UserList != null)
            {
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    foreach (var current in room.GetRoomUserManager().UserList.Values.Where(current => current != null))
                    {
                        if (current.IsPet)
                        {
                            if (current.PetData == null)
                                continue;
                            queryReactor.SetQuery("UPDATE bots SET x=@x, y=@y, z=@z WHERE id=@id LIMIT 1");
                            queryReactor.AddParameter("x", current.X);
                            queryReactor.AddParameter("y", current.Y);
                            queryReactor.AddParameter("z", current.Z);
                            queryReactor.AddParameter("id", current.PetData.PetId);
                            queryReactor.RunQuery();

                            if (current.BotAi == null)
                                continue;

                            current.BotAi?.Dispose();
                        }
                        else if (current.IsBot)
                        {
                            if (current.BotData == null)
                                continue;
                            queryReactor.SetQuery(
                                "UPDATE bots SET x=@x, y=@y, z=@z, name=@name, motto=@motto, look=@look, rotation=@rotation, dance=@dance WHERE id=@id LIMIT 1");
                            queryReactor.AddParameter("name", current.BotData.Name);
                            queryReactor.AddParameter("motto", current.BotData.Motto);
                            queryReactor.AddParameter("look", current.BotData.Look);
                            queryReactor.AddParameter("rotation", current.BotData.Rot);
                            queryReactor.AddParameter("dance", current.BotData.DanceId);
                            queryReactor.AddParameter("x", current.X);
                            queryReactor.AddParameter("y", current.Y);
                            queryReactor.AddParameter("z", current.Z);
                            queryReactor.AddParameter("id", current.BotData.BotId);
                            queryReactor.RunQuery();

                            current.BotAi?.Dispose();
                        }
                        else
                        {
                            if (current.GetClient() != null)
                            {
                                room.GetRoomUserManager().RemoveUserFromRoom(current.GetClient(), true, false);
                                current.GetClient().GetHabbo().CurrentRoomUserId = -1;
                            }
                        }
                    }
                }
            }

            LoadedRooms.TryRemove(room.RoomId, out var _);
            LoadedBallRooms.Remove(room);
            room.Destroy();
            room = null;
        }

        /// <summary>
        ///     Sorts the active rooms.
        /// </summary>
        private void SortActiveRooms()
        {
            _orderedActiveRooms = _activeRooms.OrderByDescending(t => t.Value).Take(40);
        }

        /// <summary>
        ///     Sorts the voted rooms.
        /// </summary>
        private void SortVotedRooms()
        {
            _orderedVotedRooms = _votedRooms.OrderByDescending(t => t.Value).Take(40);
        }

        /// <summary>
        ///     Works the active rooms update queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkActiveRoomsUpdateQueue()
        {
            if (_activeRoomsUpdateQueue.Count <= 0) return false;
            lock (_activeRoomsUpdateQueue.SyncRoot)
            {
                while (_activeRoomsUpdateQueue.Count > 0)
                {
                    var roomData = (RoomData) _activeRoomsUpdateQueue.Dequeue();
                    if (roomData == null) continue;
                    if (roomData.UsersNow > 0)
                    {
                        _activeRooms[roomData] = roomData.UsersNow;
                    }
                    else
                    {
                        _activeRooms.Remove(roomData);
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Works the active rooms add queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkActiveRoomsAddQueue()
        {
            if (_activeRoomsAddQueue.Count <= 0) return false;
            lock (_activeRoomsAddQueue.SyncRoot)
            {
                while (_activeRoomsAddQueue.Count > 0)
                {
                    var roomData = (RoomData) _activeRoomsAddQueue.Dequeue();
                    if (!_activeRooms.ContainsKey(roomData))
                        _activeRooms.Add(roomData, roomData.UsersNow);
                }
            }

            return true;
        }

        /// <summary>
        ///     Works the active rooms remove queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkActiveRoomsRemoveQueue()
        {
            if (ActiveRoomsRemoveQueue.Count <= 0) return false;
            lock (ActiveRoomsRemoveQueue.SyncRoot)
            {
                while (ActiveRoomsRemoveQueue.Count > 0)
                {
                    var key = (RoomData) ActiveRoomsRemoveQueue.Dequeue();
                    _activeRooms.Remove(key);
                }
            }

            return true;
        }

        /// <summary>
        ///     Works the voted rooms add queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkVotedRoomsAddQueue()
        {
            if (_votedRoomsAddQueue.Count <= 0) return false;
            lock (_votedRoomsAddQueue.SyncRoot)
            {
                while (_votedRoomsAddQueue.Count > 0)
                {
                    var roomData = (RoomData) _votedRoomsAddQueue.Dequeue();
                    _votedRooms[roomData] = roomData.Score;
                }
            }

            return true;
        }

        /// <summary>
        ///     Works the voted rooms remove queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkVotedRoomsRemoveQueue()
        {
            if (_votedRoomsRemoveQueue.Count <= 0) return false;
            lock (_votedRoomsRemoveQueue.SyncRoot)
            {
                while (_votedRoomsRemoveQueue.Count > 0)
                {
                    var key = (RoomData) _votedRoomsRemoveQueue.Dequeue();
                    if (key == null) continue;
                    _votedRooms.Remove(key);
                }
            }

            return true;
        }
    }
}