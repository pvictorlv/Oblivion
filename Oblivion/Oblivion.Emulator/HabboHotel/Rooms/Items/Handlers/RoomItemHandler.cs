using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Oblivion.Collections;
using Oblivion.Configuration;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Datas;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Rooms.Chat.Enums;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Rooms.Items.Handlers
{
    /// <summary>
    ///     Class RoomItemHandler.
    /// </summary>
    internal class RoomItemHandler
    {
        /// <summary>
        ///     The _roller items moved
        /// </summary>
        private List<string> _rollerItemsMoved;

        private List<uint> _rollerUsersMoved;

        /// <summary>
        ///     The _roller messages
        /// </summary>
        private List<ServerMessage> _rollerMessages;

        /// <summary>
        ///     The _roller speed
        /// </summary>
        private double _rollerSpeed, _roolerCycle;

        /// <summary>
        ///     The _room
        /// </summary>
        private Room _room;

        /// <summary>
        ///     The _room item update queue
        /// </summary>
        private Queue _roomItemUpdateQueue;

        private ConcurrentList<string> _updatedItems, _removedItems;

        /// <summary>
        ///     The breeding terrier
        /// </summary>
        internal Dictionary<uint, RoomItem> BreedingTerrier, BreedingBear;

        /// <summary>
        ///     The items
        /// </summary>
        internal ConcurrentDictionary<string, RoomItem> FloorItems, WallItems;

        /// <summary>
        /// The rollers in room
        /// </summary>
        internal ConcurrentList<RoomItem> Rollers;

        /// <summary>
        ///     The hopper count
        /// </summary>
        public int HopperCount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomItemHandler" /> class.
        /// </summary>
        /// <param name="room">The room.</param>
        public RoomItemHandler(Room room)
        {
            _room = room;
            _removedItems = new ConcurrentList<string>();
            _updatedItems = new ConcurrentList<string>();
            Rollers = new ConcurrentList<RoomItem>();
            WallItems = new ConcurrentDictionary<string, RoomItem>();
            FloorItems = new ConcurrentDictionary<string, RoomItem>();
            _roomItemUpdateQueue = new Queue();
            BreedingBear = new Dictionary<uint, RoomItem>();
            BreedingTerrier = new Dictionary<uint, RoomItem>();
            GotRollers = false;
            _roolerCycle = 0;
            _rollerSpeed = 4;
            HopperCount = 0;
            _rollerItemsMoved = new List<string>();
            _rollerUsersMoved = new List<uint>();
            _rollerMessages = new List<ServerMessage>();
        }

        public int TotalItems
        {
            get
            {
                if (WallItems == null || FloorItems == null)
                    return 0;

                return WallItems.Count + FloorItems.Count;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [got rollers].
        /// </summary>
        /// <value><c>true</c> if [got rollers]; otherwise, <c>false</c>.</value>
        internal bool GotRollers { get; set; }

        internal RoomItem GetItem(string itemId)
        {
            if (FloorItems.TryGetValue(itemId, out var item)) return item;
            return WallItems.TryGetValue(itemId, out item) ? item : null;
        }

        /// <summary>
        ///     Gets the random breeding bear.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <returns>Point.</returns>
        internal Point GetRandomBreedingBear(Pet pet)
        {
            if (!BreedingBear.Any())
                return new Point();
            var keys = new List<uint>(BreedingBear.Keys);
            var size = BreedingBear.Count;
            var rand = new Random();
            var randomKey = keys[rand.Next(size)];


            BreedingBear[randomKey].PetsList.Add(pet);
            pet.WaitingForBreading = BreedingBear[randomKey].VirtualId;
            pet.BreadingTile = BreedingBear[randomKey].Coordinate;

            return BreedingBear[randomKey].Coordinate;
        }

        /// <summary>
        ///     Gets the random breeding terrier.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <returns>Point.</returns>
        internal Point GetRandomBreedingTerrier(Pet pet)
        {
            if (!BreedingTerrier.Any())
                return new Point();
            var keys = new List<uint>(BreedingTerrier.Keys);
            var size = BreedingTerrier.Count;
            var rand = new Random();
            var randomKey = keys[rand.Next(size)];

            BreedingTerrier[randomKey].PetsList.Add(pet);
            pet.WaitingForBreading = BreedingTerrier[randomKey].VirtualId;
            pet.BreadingTile = BreedingTerrier[randomKey].Coordinate;

            return BreedingTerrier[randomKey].Coordinate;
        }

        /// <summary>
        ///     Saves the furniture.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="session">The session.</param>
        public void SaveFurniture(IQueryAdapter dbClient, GameClient session = null)
        {
            try
            {
                if (dbClient == null) return;


                if (_removedItems.Count > 0)
                {
                    var builder = new StringBuilder();
                    builder.Append("UPDATE items_rooms SET room_id='0', x='0', y='0', z='0', rot='0' WHERE id IN (");
                    var i = 0;
                    var count = _removedItems.Count;
                    foreach (var itemId in _removedItems)
                    {
                        i++;
                        builder.Append(i >= count ? $"'{itemId}'" : $"'{itemId}',");
                    }

                    builder.Append(");");

                    dbClient.RunFastQuery(builder.ToString());
                    _removedItems.Clear();
                }

                if (_updatedItems.Count > 0)
                {
                    var i = 0;
                    foreach (var it in _updatedItems)
                    {
                        i++;
                        var roomItem = GetItem(it);
                        if (roomItem == null) continue;
                        if (roomItem.GetBaseItem() != null && roomItem.GetBaseItem().IsGroupItem)
                        {
                            try
                            {
                                var gD = roomItem.GroupData.Split(';');
                                roomItem.ExtraData = roomItem.ExtraData + ";" + gD[1] + ";" + gD[2] + ";" + gD[3];
                            }
                            catch
                            {
                                roomItem.ExtraData = string.Empty;
                            }
                        }

                        if (roomItem.RoomId == 0) continue;

                        if (roomItem.GetBaseItem().Name.Contains("wallpaper_single") ||
                            roomItem.GetBaseItem().Name.Contains("floor_single") ||
                            roomItem.GetBaseItem().Name.Contains("landscape_single"))
                        {
                            dbClient.RunNoLockFastQuery("DELETE FROM items_rooms WHERE id = '" + roomItem.Id + "' LIMIT 1;");
                            continue;
                        }

                        var query = "UPDATE items_rooms SET room_id = " + roomItem.RoomId;
                        if (!string.IsNullOrEmpty(roomItem.ExtraData))
                        {
                            query += $", extra_data = @extraData{i}";
                            dbClient.AddParameter($"extraData{i}", roomItem.ExtraData);
                        }

                        if (roomItem.IsFloorItem)
                        {
                            query +=
                                $", x={roomItem.X}, y={roomItem.Y}, z='{roomItem.Z.ToString(CultureInfo.InvariantCulture).Replace(',', '.')}', rot={roomItem.Rot}";
                        }
                        else
                        {
                            query += ", wall_pos = @wallPos";
                            dbClient.AddParameter("wallPos", roomItem.WallCoord);
                        }

                        query += " WHERE id = '" + roomItem.Id + "';";
                        dbClient.RunNoLockQuery(query);
                    }

                    _updatedItems.Clear();
                }

                if (_room.GetRoomUserManager().PetCount > 0)
                {
                    _room.GetRoomUserManager().AppendPetsUpdateString(dbClient);
                }

                session?.GetHabbo()?.GetInventoryComponent().RunDbUpdate();
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(
                    "Error during saving furniture for room " + _room.RoomId + ". Stack: " + ex);
            }
        }

        /// <summary>
        ///     Queues the room item update.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void QueueRoomItemUpdate(RoomItem item)
        {
            lock (_roomItemUpdateQueue.SyncRoot)
            {
                _roomItemUpdateQueue.Enqueue(item);
            }
        }

        /// <summary>
        ///     Removes all furniture.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal void RemoveAllFurniture(GameClient session)
        {
            //todo recode with queue
            var items = new List<GameClient>();
            var roomGamemap = _room.GetGameMap();
            var isOwner = _room.CheckRights(session, true);
            foreach (var item in FloorItems.Values)
            {
                if (item.UserId == 0)
                    item.UserId = session.GetHabbo().Id;


                if (!isOwner && item.UserId != session.GetHabbo().Id) continue;

                if (item.IsWired)
                {
                    _room.GetWiredHandler().RemoveWired(item);
                }
                else if (item.IsRoller)
                {
                    Rollers.Remove(item);
                }

                FloorItems.TryRemove(item.Id, out _);


                item.Interactor.OnRemove(session, item);
                roomGamemap.RemoveSpecialItem(item);
                using (var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("PickUpFloorItemMessageComposer")))
                {
                    serverMessage.AppendString(item.VirtualId.ToString());
                    serverMessage.AppendBool(false); //expired
                    serverMessage.AppendInteger(item.UserId); //pickerId
                    serverMessage.AppendInteger(0); // delay
                    _room.SendMessage(serverMessage);
                    if (item.IsBuilder)
                    {
                        using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        {
                            queryReactor.RunFastQuery($"DELETE FROM items_rooms WHERE id='{item.Id}'");
                        }

                        continue;
                    }

                    RemoveItem(item.Id);
                    var client = Oblivion.GetGame().GetClientManager().GetClientByUserId(item.UserId);
                    if (client != null)
                        items.Add(client);
                    item.Dispose(client == null);
                }
            }

            foreach (var item in WallItems.Values)
            {
                if (item.UserId == 0)
                    item.UserId = session.GetHabbo().Id;

                if (!isOwner && item.UserId != session.GetHabbo().Id) continue;
                if (item.GetBaseItem().InteractionType == Interaction.PostIt) continue;
                WallItems.TryRemove(item.Id, out _);
                item.Interactor.OnRemove(session, item);
                using (var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("PickUpWallItemMessageComposer")))
                {
                    serverMessage.AppendString(item.VirtualId.ToString());
                    serverMessage.AppendInteger(item.UserId);
                    _room.SendMessage(serverMessage);
                    if (item.IsBuilder)
                    {
                        using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        {
                            queryReactor.RunFastQuery($"DELETE FROM items_rooms WHERE id='{item.Id}'");
                        }

                        continue;
                    }

                    RemoveItem(item.Id);

                    var client = Oblivion.GetGame().GetClientManager().GetClientByUserId(item.UserId);
                    if (client != null)
                        items.Add(client);

                    item.Dispose(client == null);
                }
            }

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                SaveFurniture(queryReactor);
            }

            _room.GetGameMap().GenerateMaps();


            foreach (var user in items)
            {
                user?.GetHabbo()?.GetInventoryComponent()?.UpdateItems(true);
            }

            _room.GetRoomUserManager().OnUserUpdateStatus();
        }

        public IEnumerable<RoomItem> GetWallAndFloor => FloorItems.Values.Concat(WallItems.Values);


        /// <summary>
        ///     Sets the speed.
        /// </summary>
        /// <param name="p">The p.</param>
        internal void SetSpeed(double p) => _rollerSpeed = p;

        /// <summary>
        ///     Loads the furniture.
        /// </summary>
        internal void LoadFurniture()
        {
            if (FloorItems == null) FloorItems = new ConcurrentDictionary<string, RoomItem>();
            else FloorItems.Clear();

            if (WallItems == null) WallItems = new ConcurrentDictionary<string, RoomItem>();
            else WallItems.Clear();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(
                    "SELECT i.id, i.x, i.y, i.z, i.rot, i.user_id, i.base_item,i.wall_pos,i.extra_data,i.songcode,i.builders,i.group_id,i.limited FROM items_rooms AS i WHERE i.room_id = " +
                    _room.RoomId + " LIMIT 5000");

                var table = queryReactor.GetTable();

                if (table.Rows.Count >= 5000)
                {
                    var clientByUserId = Oblivion.GetGame().GetClientManager()
                        .GetClientByUserId((uint) _room.RoomData.OwnerId);

                    clientByUserId?.SendNotif(
                        "Your room has more than 5000 items in it. The current limit of items per room is 5000.\nTo view the rest, pick some of these items up!");
                }

                var wireds = new List<RoomItem>();
                foreach (DataRow dataRow in table.Rows)
                {
                    try
                    {
                        var id = Convert.ToString(dataRow["id"]);
                        var x = Convert.ToInt32(dataRow["x"]);
                        var y = Convert.ToInt32(dataRow["y"]);
                        var z = Convert.ToDouble(dataRow["z"]);
                        var rot = Convert.ToSByte(dataRow["rot"]);
                        var ownerId = Convert.ToUInt32(dataRow["user_id"]);
                        var baseItemId = Convert.ToUInt32(dataRow["base_item"]);
                        var limited = dataRow["limited"].ToString().Split(';');

                        var item = Oblivion.GetGame().GetItemManager().GetItem(baseItemId);
                        if (item == null) continue;

                        if (ownerId == 0)
                            queryReactor.RunNoLockFastQuery("UPDATE items_rooms SET user_id = " + _room.RoomData.OwnerId +
                                                      " WHERE id = '" + id + "';");

                        var locationData = item.Type == 'i' && string.IsNullOrWhiteSpace(dataRow["wall_pos"].ToString())
                            ? ":w=0,2 l=11,53 l"
                            : dataRow["wall_pos"].ToString();

                        string extraData;
                        if (DBNull.Value.Equals(dataRow["extra_data"])) extraData = string.Empty;
                        else extraData = (string) dataRow["extra_data"];

                        string songCode;
                        if (DBNull.Value.Equals(dataRow["songcode"])) songCode = string.Empty;
                        else songCode = (string) dataRow["songcode"];

                        var groupId = Convert.ToUInt32(dataRow["group_id"]);
                        if (item.Type == 'i')
                        {
                            var wallCoord = new WallCoordinate(':' + locationData.Split(':')[1]);
                            var value = new RoomItem(id, _room.RoomId, baseItemId, extraData, wallCoord, _room, ownerId,
                                groupId,
                                Oblivion.EnumToBool((string) dataRow["builders"]));

                            WallItems.TryAdd(value.Id, value);
                        }
                        else
                        {
                            var roomItem = new RoomItem(id, _room.RoomId, baseItemId, extraData, x, y, z, rot, _room,
                                ownerId,
                                groupId, songCode,
                                Oblivion.EnumToBool((string) dataRow["builders"]), Convert.ToInt32(limited[0]),
                                Convert.ToInt32(limited[1]));

                            if (!_room.GetGameMap().ValidTile(x, y))
                            {
                                var clientByUserId2 = Oblivion.GetGame().GetClientManager().GetClientByUserId(ownerId);

                                if (clientByUserId2 != null)
                                {
                                    clientByUserId2.GetHabbo()
                                        .GetInventoryComponent()
                                        .AddNewItem(roomItem.Id, roomItem.BaseItem, roomItem.ExtraData, groupId, true,
                                            true, 0, 0);
                                    clientByUserId2.GetHabbo().GetInventoryComponent().UpdateItems(true);
                                }

                                queryReactor.RunFastQuery(
                                    "UPDATE items_rooms SET room_id = 0 WHERE id = '" + roomItem.Id + "'");
                            }
                            else
                            {
                                if (item.InteractionType == Interaction.Hopper) HopperCount++;
                                else if (roomItem.IsWired)
                                {
                                    wireds.Add(roomItem);
                                }
                                else if (roomItem.IsRoller)
                                    GotRollers = true;
                                else if (roomItem.GetBaseItem().InteractionType == Interaction.Dimmer)
                                {
                                    if (_room.MoodlightData == null)
                                        _room.MoodlightData = new MoodlightData(roomItem.Id);
                                }
                                else if (roomItem.GetBaseItem().InteractionType == Interaction.RoomBg &&
                                         _room.TonerData == null)
                                    _room.TonerData = new TonerData(roomItem.Id);
                                else if (roomItem.GetBaseItem().InteractionType == Interaction.JukeBox)
                                {
                                    _room.GetRoomMusicController();
                                }

                                FloorItems.TryAdd(roomItem.Id, roomItem);
                            }
                        }
                    }
                    catch
                    {
                    }
                }


                foreach (var wired in wireds)
                {
                    _room.GetWiredHandler().LoadWired(_room.GetWiredHandler().GenerateNewItem(wired));
                }

                if (_room.GotMusicController())
                    _room.LoadMusic();
            }
        }

        /// <summary>
        ///     Removes the furniture.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="wasPicked">if set to <c>true</c> [was picked].</param>
        internal void RemoveFurniture(GameClient session, string id, bool wasPicked = true)
        {
            var item = GetItem(id);
            if (item == null)
                return;
            if (item.GetBaseItem().InteractionType == Interaction.FootballGate)
                _room.GetSoccer().UnRegisterGate(item);
            if (item.GetBaseItem().InteractionType != Interaction.Gift)
                item.Interactor.OnRemove(session, item);

            if (item.IsWired)
            {
                _room.GetWiredHandler().RemoveWired(item);
            }

            RemoveRoomItem(item, wasPicked);

            item.Dispose(false);
        }


        internal void DeleteRoomItem(RoomItem item)
        {
            if (item == null) return;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery($"DELETE FROM items_rooms WHERE id = '{item.Id}'");
            }

            using (var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("PickUpFloorItemMessageComposer")))
            {
                serverMessage.AppendString(item.VirtualId.ToString());
                serverMessage.AppendBool(false); //expired
                serverMessage.AppendInteger(0); //pickerId
                serverMessage.AppendInteger(0); // delay
                _room.SendMessage(serverMessage);


                _room.GetGameMap().RemoveFromMap(item);
                _room.GetRoomUserManager().OnUserUpdateStatus(item.X, item.Y);

                FloorItems.TryRemove(item.Id, out _);
                item.Dispose(true);
            }
        }

        /// <summary>
        ///     Removes the room item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="wasPicked">if set to <c>true</c> [was picked].</param>
        internal void RemoveRoomItem(RoomItem item, bool wasPicked)
        {
            if (item.IsWallItem)
            {
                using (var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("PickUpWallItemMessageComposer")))
                {
                    serverMessage.AppendString(item.VirtualId.ToString());
                    serverMessage.AppendInteger(wasPicked ? item.UserId : 0);
                    _room.SendMessage(serverMessage);
                    WallItems.TryRemove(item.Id, out _);
                }
            }
            else if (item.IsFloorItem)
            {
                using (var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("PickUpFloorItemMessageComposer")))
                {
                    serverMessage.AppendString(item.VirtualId.ToString());
                    serverMessage.AppendBool(false); //expired
                    serverMessage.AppendInteger(wasPicked ? item.UserId : 0); //pickerId
                    serverMessage.AppendInteger(0); // delay
                    _room.SendMessage(serverMessage);

                    FloorItems.TryRemove(item.Id, out _);
                    _room.GetGameMap().RemoveFromMap(item);
                }
            }

            RemoveItem(item.Id);
            _room.GetRoomUserManager().OnUserUpdateStatus(item.X, item.Y);
        }

        /// <summary>
        ///     Updates the item on roller.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="nextCoord">The next coord.</param>
        /// <param name="rolledId">The rolled identifier.</param>
        /// <param name="nextZ">The next z.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage UpdateItemOnRoller(RoomItem item, Point nextCoord, uint rolledId, double nextZ)
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
            serverMessage.AppendInteger(item.X);
            serverMessage.AppendInteger(item.Y);
            serverMessage.AppendInteger(nextCoord.X);
            serverMessage.AppendInteger(nextCoord.Y);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(item.VirtualId);
            serverMessage.AppendString(TextHandling.GetString(item.Z));
            serverMessage.AppendString(TextHandling.GetString(nextZ));
            serverMessage.AppendInteger(rolledId);
            SetFloorItem(item, nextCoord.X, nextCoord.Y, nextZ);
            return serverMessage;
        }

        /// <summary>
        ///     Updates the user on roller.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="nextCoord">The next coord.</param>
        /// <param name="rollerId">The roller identifier.</param>
        /// <param name="nextZ">The next z.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage UpdateUserOnRoller(RoomUser user, Point nextCoord, uint rollerId, double nextZ)
        {
            user.UpdateNeededCounter = 1;

            var serverMessage = new ServerMessage(0);
            serverMessage.Init(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
            serverMessage.AppendInteger(user.X);
            serverMessage.AppendInteger(user.Y);
            serverMessage.AppendInteger(nextCoord.X);
            serverMessage.AppendInteger(nextCoord.Y);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(rollerId);
            serverMessage.AppendInteger(2);
            serverMessage.AppendInteger(user.VirtualId);
            serverMessage.AppendString(TextHandling.GetString(user.Z));
            serverMessage.AppendString(TextHandling.GetString(nextZ));
            _room.GetGameMap()
                .UpdateUserMovement(new Point(user.X, user.Y), new Point(nextCoord.X, nextCoord.Y), user);
            _room.GetGameMap().GameMap[user.X, user.Y] = 1;
            user.X = nextCoord.X;
            user.Y = nextCoord.Y;
            user.Z = nextZ;
            _room.GetGameMap().GameMap[user.X, user.Y] = 0;
            return serverMessage;
        }


        /// <summary>
        ///     Sets the floor item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newRot">The new rot.</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <param name="onRoller">if set to <c>true</c> [on roller].</param>
        /// <param name="sendMessage">if set to <c>true</c> [send message].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(GameClient session, RoomItem item, int newX, int newY, int newRot, bool newItem,
            bool onRoller, bool sendMessage) => SetFloorItem(session, item, newX, newY, newRot, newItem, onRoller,
            sendMessage, true, false, _room.CustomHeight);


        /// <summary>
        ///     Sets the floor item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newRot">The new rot.</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <param name="onRoller">if set to <c>true</c> [on roller].</param>
        /// <param name="sendMessage">if set to <c>true</c> [send message].</param>
        /// <param name="updateRoomUserStatuses">if set to <c>true</c> [update room user statuses].</param>
        /// <param name="specialMove"></param>
        /// <param name="customHeight"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(GameClient session, RoomItem item, int newX, int newY, int newRot, bool newItem,
            bool onRoller, bool sendMessage, bool updateRoomUserStatuses, bool specialMove, double? customHeight = null)
        {
            if (item?.GetBaseItem() == null) return false;
            var flag = false;
            if (_room?.GetGameMap() == null) return false;

            if (!newItem) flag = _room.GetGameMap().RemoveFromMap(item);

            var affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                item.GetBaseItem().Width, newX, newY, newRot);

            var oldCoord = item.Coordinate;

            if (!_room.GetGameMap().ValidTile(newX, newY) ||
                (_room.GetGameMap().SquareHasUsers(newX, newY) && !item.GetBaseItem().IsSeat))
            {
                if (!flag) return false;

                AddOrUpdateItem(item.Id);
                _room.GetGameMap().AddToMap(item);
                return false;
            }

            if (
                affectedTiles.Values.Any(
                    current =>
                        !_room.GetGameMap().ValidTile(current.X, current.Y) ||
                        (_room.GetGameMap().SquareHasUsers(current.X, current.Y) && !item.GetBaseItem().IsSeat)))
            {
                if (!flag) return false;
                AddOrUpdateItem(item.Id);
                _room.GetGameMap().AddToMap(item);
                return false;
            }

            double height;
            if (customHeight != null && customHeight >= 0)
            {
                height = (double) customHeight;
            }
            else height = _room.GetGameMap().Model.SqFloorHeight[newX][newY];

            if (!onRoller)
            {
                if (_room.GetGameMap().Model.SqState[newX][newY] != SquareState.Open && !item.GetBaseItem().IsSeat)
                {
                    if (!flag) return false;
                    AddOrUpdateItem(item.Id);
                    return false;
                }

                if (
                    affectedTiles.Values.Any(
                        current2 =>
                            !item.GetBaseItem().IsSeat &&
                            _room.GetGameMap().Model.SqState[current2.X][current2.Y] != SquareState.Open))
                {
                    if (!flag) return false;

                    AddOrUpdateItem(item.Id);
                    _room.GetGameMap().AddToMap(item);
                    return false;
                }

                if (!item.GetBaseItem().IsSeat && !item.IsRoller)
                    if (
                        affectedTiles.Values.Any(
                            current3 => _room.GetGameMap().GetRoomUsers(new Point(current3.X, current3.Y)).Count > 0))
                    {
                        if (!flag) return false;
                        AddOrUpdateItem(item.Id);
                        _room.GetGameMap().AddToMap(item);
                        return false;
                    }
            }

            var furniObjects = GetFurniObjects(newX, newY);
            var list = new List<RoomItem>();
            var list2 = new List<RoomItem>();
            /* TODO CHECK */
            foreach (var current4 in affectedTiles.Values)
            {
                var furniObjects2 = GetFurniObjects(current4.X, current4.Y);
                if (furniObjects2 != null) list.AddRange(furniObjects2);
            }

            list2.AddRange(furniObjects);
            list2.AddRange(list);

            var stackMagic = list2.FirstOrDefault(
                roomItem =>
                    roomItem?.GetBaseItem() != null && roomItem.GetBaseItem().InteractionType ==
                    Interaction.TileStackMagic);

            if (stackMagic != null)
            {
                height = stackMagic.Z;
            }
            else if (!onRoller && item.GetBaseItem().InteractionType != Interaction.TileStackMagic)
            {
                if (item.Rot != newRot && item.X == newX && item.Y == newY) height = item.Z;

                foreach (var current5 in list2)
                {
                    if (current5?.GetBaseItem() == null || current5.Id == item.Id) continue;

                    if (current5.TotalHeight > height) height = current5.TotalHeight;

                    if (!current5.GetBaseItem().Stackable)
                    {
                        if (!flag) return false;
                        AddOrUpdateItem(item.Id);
                        _room.GetGameMap().AddToMap(item);
                        return false;
                    }
                }
            }

            switch (item.GetBaseItem().Name)
            {
                case "boutique_mannequin1":
                case "gld_wall_tall":
                    if (newRot < 0 || newRot > 12)
                    {
                        newRot = 0;
                    }

                    break;
                case "pirate_stage2":
                case "pirate_stage2_g":
                    if (newRot < 0 || newRot > 7)
                    {
                        newRot = 0;
                    }

                    break;
                case "gh_div_cor":
                case "hblooza14_duckcrn":
                case "hween13_dwarfcrn":
                    if (newRot != 1 && newRot != 3 && newRot != 5 && newRot != 7)
                    {
                        newRot = 0;
                    }

                    break;
                case "val14_b_roof":
                case "val14_g_roof":
                case "val14_y_roof":
                    if (newRot != 2 && newRot != 3 && newRot != 4 && newRot != 7)
                    {
                        newRot = 0;
                    }

                    break;
                case "val13_div_1":
                    if (newRot < 0 || newRot > 6)
                    {
                        newRot = 0;
                    }

                    break;
                case "room_info15_shrub1":
                    if (newRot != 0 && newRot != 2 && newRot != 3 && newRot != 4 && newRot != 6)
                    {
                        newRot = 0;
                    }

                    break;
                case "room_info15_div":
                    if (newRot < 0 || newRot > 5)
                    {
                        newRot = 0;
                    }

                    break;
                default:
                    if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8)
                    {
                        newRot = 0;
                    }

                    break;
            }

            item.Rot = newRot;

            item.SetState(newX, newY, height, affectedTiles);
            if (!onRoller && session != null) item.Interactor.OnPlace(session, item);
            if (newItem)
            {
                if (FloorItems.ContainsKey(item.Id)) return true;
                if (item.IsFloorItem) FloorItems.TryAdd(item.Id, item);
                else if (item.IsWallItem) WallItems.TryAdd(item.Id, item);

                AddOrUpdateItem(item.Id);
                if (sendMessage)
                {
                    using (var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("AddFloorItemMessageComposer")))
                    {
                        item.Serialize(serverMessage);
                        serverMessage.AppendString(_room.RoomData.Group != null
                            ? session?.GetHabbo().UserName
                            : _room.RoomData.Owner);
                        _room.SendMessage(serverMessage);
                    }
                }
            }
            else
            {
                AddOrUpdateItem(item.Id);
                if (!onRoller && sendMessage)
                {
                    if (specialMove)
                    {
                        using (var message =
                            new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer")))
                        {
                            message.AppendInteger(oldCoord.X);
                            message.AppendInteger(oldCoord.Y);
                            message.AppendInteger(newX);
                            message.AppendInteger(newY);
                            message.AppendInteger(1);
                            message.AppendInteger(item.VirtualId);
                            message.AppendString(TextHandling.GetString(item.Z));
                            message.AppendString(TextHandling.GetString(item.Z));
                            message.AppendInteger(-1);
                            _room.SendMessage(message);
                        }
                    }
                    else
                    {
                        using (var message =
                            new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer")))
                        {
                            item.Serialize(message);
                            _room.SendMessage(message);
                        }
                    }
                }

//                if (item.IsWired) _room.GetWiredHandler().MoveWired(item);
            }

            _room.GetGameMap().AddToMap(item);
            if (item.GetBaseItem().IsSeat) updateRoomUserStatuses = true;
            if (updateRoomUserStatuses)
            {
                foreach (var current in _room.GetRoomUserManager().UserList.Values)
                {
                    if ((current.X == oldCoord.X && current.Y == oldCoord.Y) ||
                        (current.X == item.X && current.Y == item.Y))
                        _room.GetRoomUserManager().UpdateUserStatus(current, false);
                }
            }

            if (newItem) OnHeightMapUpdate(affectedTiles);

            return true;
        }

        internal void DeveloperSetFloorItem(GameClient session, RoomItem item)
        {
            if (FloorItems.ContainsKey(item.Id)) return;
            FloorItems.TryAdd(item.Id, item);

            AddOrUpdateItem(item.Id);

            var affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, item.X,
                item.Y, item.Rot);
            item.SetState(item.X, item.Y, item.Z, affectedTiles);
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddFloorItemMessageComposer")))
            {
                item.Serialize(serverMessage);
                serverMessage.AppendString(
                    _room.RoomData.Group != null ? session.GetHabbo().UserName : _room.RoomData.Owner);
                _room.SendMessage(serverMessage);

                _room.GetGameMap().AddToMap(item);
            }
        }

        /// <summary>
        ///     Called when [height map update].
        /// </summary>
        /// <param name="affectedTiles">The affected tiles.</param>
        internal void OnHeightMapUpdate(Dictionary<int, ThreeDCoord> affectedTiles)
        {
            using (var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer")))
            {
                message.AppendByte((byte) affectedTiles.Count);
                foreach (var coord in affectedTiles.Values)
                {
                    message.AppendByte((byte) coord.X);
                    message.AppendByte((byte) coord.Y);
                    message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y) * 256));
                }

                _room.SendMessage(message);
            }
        }

        /// <summary>
        ///     Called when [height map update].
        /// </summary>
        /// <param name="affectedTiles">The affected tiles.</param>
        internal void OnHeightMapUpdate(ICollection affectedTiles)
        {
            using (var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer")))
            {
                message.AppendByte((byte) affectedTiles.Count);
                foreach (Point coord in affectedTiles)
                {
                    message.AppendByte((byte) coord.X);
                    message.AppendByte((byte) coord.Y);
                    message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y) * 256));
                }

                _room.SendMessage(message);
            }
        }

        /// <summary>
        ///     Called when [height map update].
        /// </summary>
        /// <param name="oldCoords">The old coords.</param>
        /// <param name="newCoords">The new coords.</param>
        internal void OnHeightMapUpdate(List<Point> oldCoords, List<Point> newCoords)
        {
            using (var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer")))
            {
                message.AppendByte((byte) (oldCoords.Count + newCoords.Count));
                foreach (var coord in oldCoords)
                {
                    message.AppendByte((byte) coord.X);
                    message.AppendByte((byte) coord.Y);
                    message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y) * 256));
                }

                foreach (var nCoord in newCoords)
                {
                    message.AppendByte((byte) nCoord.X);
                    message.AppendByte((byte) nCoord.Y);
                    message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(nCoord.X, nCoord.Y) * 256));
                }

                _room.SendMessage(message);
            }
        }

        /// <summary>
        ///     Gets the furni objects.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal ConcurrentList<RoomItem> GetFurniObjects(int x, int y) =>
            _room.GetGameMap().GetCoordinatedItems(new Point(x, y));

        /// <summary>
        ///     Sets the floor item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newZ">The new z.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(RoomItem item, int newX, int newY, double newZ)
        {
            _room.GetGameMap().RemoveFromMap(item);
            item.SetState(newX, newY, newZ,
                Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                    item.GetBaseItem().Width, newX, newY, item.Rot));
            if (item.GetBaseItem().InteractionType == Interaction.RoomBg && _room.TonerData == null)
                _room.TonerData = new TonerData(item.Id);
            AddOrUpdateItem(item.Id);
            _room.GetGameMap().AddItemToMap(item);
            return true;
        }

        /// <summary>
        ///     Sets the floor item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newZ">The new z.</param>
        /// <param name="rot">The rot.</param>
        /// <param name="sendUpdate">if set to <c>true</c> [sendupdate].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(RoomItem item, int newX, int newY, double newZ, int rot, bool sendUpdate)
        {
            _room.GetGameMap().RemoveFromMap(item);
            item.SetState(newX, newY, newZ,
                Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                    item.GetBaseItem().Width, newX, newY, rot));
            if (item.GetBaseItem().InteractionType == Interaction.RoomBg && _room.TonerData == null)
                _room.TonerData = new TonerData(item.Id);
            AddOrUpdateItem(item.Id);
            _room.GetGameMap().AddItemToMap(item);
            if (!sendUpdate)
                return true;
            using (var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer")))
            {
                item.Serialize(message);
                _room.SendMessage(message);
                return true;
            }
        }

        /// <summary>
        ///     Sets the wall item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetWallItem(GameClient session, RoomItem item)
        {
            if (!item.IsWallItem || WallItems.ContainsKey(item.Id))
                return false;
            if (FloorItems.ContainsKey(item.Id))
                return true;
            item.Interactor.OnPlace(session, item);
            if (item.GetBaseItem().InteractionType == Interaction.Dimmer && _room.MoodlightData == null)
            {
                _room.MoodlightData = new MoodlightData(item.Id);
                item.ExtraData = _room.MoodlightData.GenerateExtraData();
            }

            WallItems.TryAdd(item.Id, item);
            AddOrUpdateItem(item.Id);
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddWallItemMessageComposer")))
            {
                item.Serialize(serverMessage);
                serverMessage.AppendString(_room.RoomData.Owner);
                _room.SendMessage(serverMessage);
                return true;
            }
        }

        /// <summary>
        ///     Updates the item.
        /// </summary>
        /// <param name="itemId">The item.</param>
        internal void AddOrUpdateItem(string itemId)
        {
            if (_removedItems.Contains(itemId))
                _removedItems.Remove(itemId);
            if (_updatedItems.Contains(itemId))
                return;

            _updatedItems.Add(itemId);
        }

        /// <summary>
        ///     Removes the item.
        /// </summary>
        /// <param name="itemId"></param>
        internal void RemoveItem(string itemId)
        {
            if (_updatedItems.Contains(itemId))
                _updatedItems.Remove(itemId);

            if (!_removedItems.Contains(itemId))
                _removedItems.Add(itemId);
        }

        /// <summary>
        ///     Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            try
            {
                if (GotRollers)
                {
                    if (Rollers.Count <= 0)
                    {
                        GotRollers = false;
                    }

                    try
                    {
                        var roller = CycleRollers();
                        if (roller == null)
                            return;
                        _room.SendMessage(roller);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogThreadException(ex.ToString(),
                            $"rollers for room with ID {_room.RoomId}");
                        GotRollers = false;
                    }
                }

                if (_roomItemUpdateQueue != null)
                {
                    lock (_roomItemUpdateQueue.SyncRoot)
                        if (_roomItemUpdateQueue.Count > 0)
                        {
                            var addItems = new ConcurrentList<RoomItem>();

                            while (_roomItemUpdateQueue.Count > 0)
                            {
                                var roomItem = (RoomItem) _roomItemUpdateQueue.Dequeue();
                                roomItem.ProcessUpdates();

                                if (roomItem.IsTrans || roomItem.UpdateCounter > 0)
                                    addItems.Add(roomItem);
                            }

                            foreach (var item in addItems)
                                _roomItemUpdateQueue.Enqueue(item);
                        }
                }
            }
            catch (Exception e)
            {
                Logging.LogThreadException(e.ToString(),
                    $"rollers for room with ID {_room.RoomId}");
            }
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            FloorItems.Clear();
            WallItems.Clear();
            _removedItems.Clear();
            _removedItems.Dispose();
            _updatedItems.Clear();
            _updatedItems.Dispose();
            if (_roomItemUpdateQueue?.Count > 0)
                lock (_roomItemUpdateQueue.SyncRoot)
                {
                    _roomItemUpdateQueue.Clear();
                    _roomItemUpdateQueue = null;
                }

            Rollers?.Clear();
            Rollers?.Dispose();
            _rollerMessages?.Clear();
            _rollerItemsMoved?.Clear();
            _rollerUsersMoved?.Clear();
            _rollerUsersMoved = null;
            _rollerItemsMoved = null;
            _rollerMessages = null;
            Rollers = null;
            _room = null;
            FloorItems = null;
            WallItems = null;
            _removedItems = null;
            _updatedItems = null;
            BreedingBear.Clear();
            BreedingTerrier.Clear();
            WallItems = null;
            BreedingBear = null;
            BreedingTerrier = null;
        }

        /// <summary>
        ///     Cycles the rollers.
        /// </summary>
        /// <returns>List&lt;ServerMessage&gt;.</returns>
        private List<ServerMessage> CycleRollers()
        {
            if (!GotRollers)
                return new List<ServerMessage>();
            if (_roolerCycle >= _rollerSpeed || _rollerSpeed <= 0)
            {
                if (Rollers.Count <= 0)
                {
                    return new List<ServerMessage>();
                }

                _rollerItemsMoved.Clear();
                _rollerUsersMoved.Clear();
                _rollerMessages.Clear();
                foreach (var current in Rollers)
                {
                    if (current == null) continue;
                    var squareInFront = current.SquareInFront;
                    var roomItemForSquare = _room.GetGameMap().GetRoomItemForSquare(current.X, current.Y);

                    var flag = false;
                    var nextZ = 0.0;
                    var flag2 = true;
                    var frontHasItem = false;

                    var userInFront = _room.GetRoomUserManager().GetUserForSquare(squareInFront.X, squareInFront.Y) !=
                                      null;
                    if (roomItemForSquare != null)
                    {
                        foreach (var current2 in roomItemForSquare)
                        {
                            if (!current2.IsRoller) continue;

                            flag = true;
                            if (current2.TotalHeight > nextZ)
                                nextZ = current2.TotalHeight;
                            if (!current2.GetBaseItem().Stackable)
                                frontHasItem = true;
                            if (current2.TotalHeight > nextZ)
                                flag2 = false;
                        }

                        if (!flag)
                            nextZ += _room.GetGameMap().GetHeightForSquareFromData(squareInFront);


                        foreach (var current4 in roomItemForSquare)
                        {
                            var num3 = current4.Z - current.TotalHeight;
                            if (_rollerItemsMoved.Contains(current4.Id) || frontHasItem ||
                                !_room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) || !flag2 ||
                                !(current.Z < current4.Z) || userInFront)
                                continue;
                            _rollerMessages.Add(UpdateItemOnRoller(current4, squareInFront, current.VirtualId,
                                nextZ + num3));
                            _rollerItemsMoved.Add(current4.Id);
                        }
                    }

                    var userForSquare = _room.GetRoomUserManager().GetUserForSquare(current.X, current.Y);


                    if (userForSquare != null && !userForSquare.IsWalking && flag2 && !userInFront &&
                        _room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) &&
                        _room.GetGameMap().GetFloorStatus(squareInFront) != 0 &&
                        !_rollerUsersMoved.Contains(userForSquare.HabboId))
                    {
                        using (var msg = UpdateUserOnRoller(userForSquare, squareInFront, current.VirtualId, nextZ))
                            _room.SendMessage(msg);
                        _rollerUsersMoved.Add(userForSquare.HabboId);
                        _room.GetRoomUserManager().UpdateUserStatus(userForSquare, true);
                    }
                }

                _roolerCycle = 0;
                return _rollerMessages;
            }

            _roolerCycle++;
            return new List<ServerMessage>();
        }

        internal bool HasFurniByItemName(string name)
        {
            var element = FloorItems.Values.Where(i => i.GetBaseItem().Name == name);
            return element.Any();
        }
    }
}