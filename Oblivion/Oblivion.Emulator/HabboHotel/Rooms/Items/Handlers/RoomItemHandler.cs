using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Oblivion.Configuration;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Datas;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Rooms.Chat;
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
        private readonly List<uint> _rollerItemsMoved, _rollerUsersMoved;

        /// <summary>
        ///     The _roller messages
        /// </summary>
        private readonly List<ServerMessage> _rollerMessages;

        /// <summary>
        ///     The _roller speed
        /// </summary>
        private uint _rollerSpeed, _roolerCycle;

        /// <summary>
        ///     The _room
        /// </summary>
        private Room _room;

        /// <summary>
        ///     The _room item update queue
        /// </summary>
        private Queue _roomItemUpdateQueue;

        private List<uint> _updatedItems, _removedItems;

        /// <summary>
        ///     The breeding terrier
        /// </summary>
        internal Dictionary<uint, RoomItem> BreedingTerrier, BreedingBear;

        /// <summary>
        ///     The floor items
        /// </summary>
        internal ConcurrentDictionary<uint, RoomItem> FloorItems, WallItems, Rollers;

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
            _removedItems = new List<uint>();
            _updatedItems = new List<uint>();
            Rollers = new ConcurrentDictionary<uint, RoomItem>();
            WallItems = new ConcurrentDictionary<uint, RoomItem>();
            FloorItems = new ConcurrentDictionary<uint, RoomItem>();
            _roomItemUpdateQueue = new Queue();
            BreedingBear = new Dictionary<uint, RoomItem>();
            BreedingTerrier = new Dictionary<uint, RoomItem>();
            GotRollers = false;
            _roolerCycle = 0;
            _rollerSpeed = 4;
            HopperCount = 0;
            _rollerItemsMoved = new List<uint>();
            _rollerUsersMoved = new List<uint>();
            _rollerMessages = new List<ServerMessage>();
        }

        public int TotalItems => WallItems.Keys.Count + FloorItems.Keys.Count;

        /// <summary>
        ///     Gets or sets a value indicating whether [got rollers].
        /// </summary>
        /// <value><c>true</c> if [got rollers]; otherwise, <c>false</c>.</value>
        internal bool GotRollers { get; set; }

        internal RoomItem GetItem(uint itemId)
        {
            if (FloorItems.ContainsKey(itemId)) return FloorItems[itemId];
            return WallItems.ContainsKey(itemId) ? WallItems[itemId] : null;
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
            pet.WaitingForBreading = BreedingBear[randomKey].Id;
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
            pet.WaitingForBreading = BreedingTerrier[randomKey].Id;
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
                if (!_updatedItems.Any() && !_removedItems.Any() &&
                    _room.GetRoomUserManager().PetCount <= 0)
                    return;

                var queryChunk = new QueryChunk();
                var queryChunk2 = new QueryChunk();

                foreach (var itemId in _removedItems)
                {
                    queryChunk.AddQuery("UPDATE items_rooms SET room_id='0', x='0', y='0', z='0', rot='0' WHERE id = " +
                                        itemId);
                }

                foreach (var roomItem in _updatedItems.Select(GetItem).Where(roomItem => roomItem != null))
                {
                    if (roomItem.GetBaseItem() != null && roomItem.GetBaseItem().IsGroupItem)
                    {
                        try
                        {
                            var gD = roomItem.GroupData.Split(';');
                            roomItem.ExtraData = roomItem.ExtraData + ";" + gD[1] + ";" +
                                                 gD[2] + ";" + gD[3];
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
                        queryChunk.AddQuery("DELETE FROM items_rooms WHERE id = " + roomItem.Id + " LIMIT 1");
                        continue;
                    }

                    var query = "UPDATE items_rooms SET room_id = " + roomItem.RoomId;
                    if (!string.IsNullOrEmpty(roomItem.ExtraData))
                    {
                        query += ", extra_data = @extraData" + roomItem.Id;
                        queryChunk2.AddParameter("extraData" + roomItem.Id, roomItem.ExtraData);
                    }

                    if (roomItem.IsFloorItem)
                    {
                        query +=
                            $", x={roomItem.X}, y={roomItem.Y}, z='{roomItem.Z.ToString(CultureInfo.InvariantCulture).Replace(',', '.')}', rot={roomItem.Rot}";
                    }
                    else
                    {
                        query += ", wall_pos = @wallPos" + roomItem.Id;
                        queryChunk2.AddParameter("wallPos" + roomItem.Id, roomItem.WallCoord);
                    }

                    query += " WHERE id = " + roomItem.Id;
                    queryChunk2.AddQuery(query);
                }

                _room.GetRoomUserManager().AppendPetsUpdateString(dbClient);

                if (session != null) session.GetHabbo().GetInventoryComponent().RunDbUpdate();

                _updatedItems.Clear();
                _removedItems.Clear();
                queryChunk.Execute(dbClient);
                queryChunk2.Execute(dbClient);
                queryChunk.Dispose();
                queryChunk2.Dispose();
                queryChunk = null;
                queryChunk2 = null;
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException("Error during saving furniture for room " + _room.RoomId + ". Stack: " + ex);
            }
        }

        /// <summary>
        ///     Checks the position item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="rItem">The r item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newRot">The new rot.</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <param name="sendNotify">if set to <c>true</c> [send notify].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CheckPosItem(GameClient session, RoomItem rItem, int newX, int newY, int newRot, bool newItem,
            bool sendNotify = true)
        {
            try
            {
                var dictionary = Gamemap.GetAffectedTiles(rItem.GetBaseItem().Length, rItem.GetBaseItem().Width, newX,
                    newY, newRot);
                if (!_room.GetGameMap().ValidTile(newX, newY))
                    return false;
                if (
                    dictionary.Values.Any(
                        coord =>
                            (_room.GetGameMap().Model.DoorX == coord.X) && (_room.GetGameMap().Model.DoorY == coord.Y)))
                    return false;
                if ((_room.GetGameMap().Model.DoorX == newX) && (_room.GetGameMap().Model.DoorY == newY))
                    return false;
                if (dictionary.Values.Any(coord => !_room.GetGameMap().ValidTile(coord.X, coord.Y)))
                    return false;
                double num = _room.GetGameMap().Model.SqFloorHeight[newX][newY];
                if (rItem.Rot == newRot && rItem.X == newX && rItem.Y == newY && rItem.Z != num)
                    return false;
                if (_room.GetGameMap().Model.SqState[newX][newY] != SquareState.Open)
                    return false;
                if (
                    dictionary.Values.Any(
                        coord => _room.GetGameMap().Model.SqState[coord.X][coord.Y] != SquareState.Open))
                    return false;
                if (!rItem.GetBaseItem().IsSeat)
                {
                    if (_room.GetGameMap().SquareHasUsers(newX, newY))
                        return false;
                    if (dictionary.Values.Any(coord => _room.GetGameMap().SquareHasUsers(coord.X, coord.Y)))
                        return false;
                }
                var furniObjects = GetFurniObjects(newX, newY);
                var collection = new List<RoomItem>();
                var list3 = new List<RoomItem>();
                foreach (
                    var list4 in
                        dictionary.Values
                            .Select(coord => GetFurniObjects(coord.X, coord.Y))
                            .Where(list4 => list4 != null))
                    collection.AddRange(list4);
                if (furniObjects == null)
                    furniObjects = new List<RoomItem>();
                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                return list3.All(item => (item.Id == rItem.Id) || item.GetBaseItem().Stackable);
            }
            catch
            {
                return false;
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
        internal List<RoomItem> RemoveAllFurniture(GameClient session)
        {
            var items = new List<RoomItem>();
            var roomGamemap = _room.GetGameMap();
            foreach (var item in FloorItems.Values.ToArray())
            {
                item.Interactor.OnRemove(session, item);
                roomGamemap.RemoveSpecialItem(item);
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpFloorItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
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
                items.Add(item);
            }

            foreach (var item in WallItems.Values.ToArray())
            {
                item.Interactor.OnRemove(session, item);
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpWallItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
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
                items.Add(item);
            }

            _removedItems.Clear();
            _updatedItems.Clear();
            WallItems.Clear();
            FloorItems.Clear();
            Rollers.Clear();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery($"UPDATE items_rooms SET room_id='0' WHERE room_id='{_room.RoomId}'");
            }
            _room.GetGameMap().GenerateMaps();

            _room.GetRoomUserManager().OnUserUpdateStatus();
            return items;
        }

        /// <summary>
        ///     Remove Items On Room GroupBy Username
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="roomItemList"></param>
        /// <param name="session"></param>
        internal void RemoveItemsByOwner(ref List<RoomItem> roomItemList, ref GameClient session)
        {
            var toUpdate = new List<GameClient>();

            foreach (var item in roomItemList)
            {
                if (item.UserId == 0)
                    item.UserId = session.GetHabbo().Id;

                var client = Oblivion.GetGame().GetClientManager().GetClientByUserId(item.UserId);

                if ((item.GetBaseItem().InteractionType != Interaction.PostIt))
                {
                    if (!toUpdate.Contains(client))
                        toUpdate.Add(client);

                    if (client == null)
                        using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                            dbClient.RunFastQuery("UPDATE items_rooms SET room_id = '0' WHERE id = " + item.Id);
                    else
                        client.GetHabbo().GetInventoryComponent().AddItem(item);
                }
            }

            foreach (var client in toUpdate)
                client?.GetHabbo().GetInventoryComponent().UpdateItems(true);
        }

        /// <summary>
        ///     Sets the speed.
        /// </summary>
        /// <param name="p">The p.</param>
        internal void SetSpeed(uint p)
        {
            _rollerSpeed = p;
        }

        /// <summary>
        ///     Loads the furniture.
        /// </summary>
        internal void LoadFurniture()
        {
            if (FloorItems == null) FloorItems = new ConcurrentDictionary<uint, RoomItem>();
            else FloorItems.Clear();

            if (WallItems == null) WallItems = new ConcurrentDictionary<uint, RoomItem>();
            else WallItems.Clear();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery("SELECT items_rooms.* , COALESCE(items_groups.group_id, 0) AS group_id FROM items_rooms LEFT OUTER JOIN items_groups ON items_rooms.id = items_groups.id WHERE items_rooms.room_id = " + _room.RoomId + " LIMIT 5000");

                var table = queryReactor.GetTable();

                if (table.Rows.Count >= 5000)
                {
                    var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId((uint) _room.RoomData.OwnerId);

                    clientByUserId?.SendNotif("Your room has more than 5000 items in it. The current limit of items per room is 5000.\nTo view the rest, pick some of these items up!");
                }

                foreach (DataRow dataRow in table.Rows)
                {
                    try
                    {
                        var id = Convert.ToUInt32(dataRow["id"]);
                        var x = Convert.ToInt32(dataRow["x"]);
                        var y = Convert.ToInt32(dataRow["y"]);
                        var z = Convert.ToDouble(dataRow["z"]);
                        var rot = Convert.ToSByte(dataRow["rot"]);
                        var ownerId = Convert.ToUInt32(dataRow["user_id"]);
                        var baseItemId = Convert.ToUInt32(dataRow["base_item"]);
                        var item = Oblivion.GetGame().GetItemManager().GetItem(baseItemId);
                        if (item == null) continue;

                        if (ownerId == 0)
                            queryReactor.RunFastQuery("UPDATE items_rooms SET user_id = " + _room.RoomData.OwnerId +
                                                      " WHERE id = " + id);

                        var locationData = item.Type == 'i' && string.IsNullOrWhiteSpace(dataRow["wall_pos"].ToString())
                            ? ":w=0,2 l=11,53 l"
                            : dataRow["wall_pos"].ToString();

                        string extraData;
                        if (DBNull.Value.Equals(dataRow["extra_data"])) extraData = string.Empty;
                        else extraData = (string)dataRow["extra_data"];

                        string songCode;
                        if (DBNull.Value.Equals(dataRow["songcode"])) songCode = string.Empty;
                        else songCode = (string)dataRow["songcode"];

                        var groupId = Convert.ToUInt32(dataRow["group_id"]);
                        if (item.Type == 'i')
                        {
                            var wallCoord = new WallCoordinate(':' + locationData.Split(':')[1]);
                            var value = new RoomItem(id, _room.RoomId, baseItemId, extraData, wallCoord, _room, ownerId,
                                groupId, item.FlatId,
                                Oblivion.EnumToBool((string)dataRow["builders"]));

                            WallItems.TryAdd(id, value);
                        }
                        else
                        {
                            var roomItem = new RoomItem(id, _room.RoomId, baseItemId, extraData, x, y, z, rot, _room,
                                ownerId,
                                groupId, item.FlatId, songCode,
                                Oblivion.EnumToBool((string)dataRow["builders"]));

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

                                queryReactor.RunFastQuery("UPDATE items_rooms SET room_id = 0 WHERE id = " + roomItem.Id);
                            }
                            else
                            {
                                if (item.InteractionType == Interaction.Hopper) HopperCount++;

                                FloorItems.TryAdd(id, roomItem);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                    
                foreach (var current in FloorItems.Values)
                {
                    if (current.IsWired)
                        _room.GetWiredHandler().LoadWired(_room.GetWiredHandler().GenerateNewItem(current));
                    if (current.IsRoller)
                        GotRollers = true;
                    else if (current.GetBaseItem().InteractionType == Interaction.Dimmer)
                    {
                        if (_room.MoodlightData == null)
                            _room.MoodlightData = new MoodlightData(current.Id);
                    }      
                    else if (current.GetBaseItem().InteractionType == Interaction.RoomBg && _room.TonerData == null)
                        _room.TonerData = new TonerData(current.Id);
                }
            }
        }

        /// <summary>
        ///     Removes the furniture.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="wasPicked">if set to <c>true</c> [was picked].</param>
        internal void RemoveFurniture(GameClient session, uint id, bool wasPicked = true)
        {
            var item = GetItem(id);
            if (item == null)
                return;
            if (item.GetBaseItem().InteractionType == Interaction.FootballGate)
                _room.GetSoccer().UnRegisterGate(item);
            if (item.GetBaseItem().InteractionType != Interaction.Gift)
                item.Interactor.OnRemove(session, item);
            if (item.GetBaseItem().InteractionType == Interaction.Bed ||
                item.GetBaseItem().InteractionType == Interaction.PressurePadBed)
                _room.ContainsBeds--;
            RemoveRoomItem(item, wasPicked);
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
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpWallItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
                serverMessage.AppendInteger(wasPicked ? item.UserId : 0);
                _room.SendMessage(serverMessage);
            }
            else if (item.IsFloorItem)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpFloorItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
                serverMessage.AppendBool(false); //expired
                serverMessage.AppendInteger(wasPicked ? item.UserId : 0); //pickerId
                serverMessage.AppendInteger(0); // delay
                _room.SendMessage(serverMessage);
            }

            RoomItem junkItem;
            if (item.IsWallItem)
            {
                WallItems.TryRemove(item.Id, out junkItem);
            }
            else
            {
                FloorItems.TryRemove(item.Id, out junkItem);
                _room.GetGameMap().RemoveFromMap(item);
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
            serverMessage.AppendInteger(item.Id);
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
            bool onRoller, bool sendMessage)
        {
            return SetFloorItem(session, item, newX, newY, newRot, newItem, onRoller, sendMessage, true, false);
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
        /// <param name="updateRoomUserStatuses">if set to <c>true</c> [update room user statuses].</param>
        /// <param name="specialMove"></param>
        /// <param name="customHeight"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(GameClient session, RoomItem item, int newX, int newY, int newRot, bool newItem,
            bool onRoller, bool sendMessage, bool updateRoomUserStatuses, bool specialMove, double? customHeight = null)
        {
            var flag = false;
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

            var height = customHeight == null ? _room.GetGameMap().Model.SqFloorHeight[newX][newY] : customHeight.Value;
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
                            current3 => _room.GetGameMap().GetRoomUsers(new Point(current3.X, current3.Y)).Any()))
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
            foreach (
                var furniObjects2 in
                    affectedTiles.Values
                        .Select(current4 => GetFurniObjects(current4.X, current4.Y))
                        .Where(furniObjects2 => furniObjects2 != null))
                list.AddRange(furniObjects2);
            list2.AddRange(furniObjects);
            list2.AddRange(list);

            var stackMagic = list2.FirstOrDefault(
                roomItem =>
                    roomItem != null && roomItem.GetBaseItem() != null &&
                    roomItem.GetBaseItem().InteractionType == Interaction.TileStackMagic);

            if (stackMagic != null)
            {
                height = stackMagic.Z;
            }
            else if (!onRoller && item.GetBaseItem().InteractionType != Interaction.TileStackMagic)
            {
                if (list2.Any(
                    current5 =>
                        current5 != null && current5.Id != item.Id && current5.GetBaseItem() != null &&
                        !current5.GetBaseItem().Stackable))
                {
                    if (!flag) return false;
                    AddOrUpdateItem(item.Id);
                    _room.GetGameMap().AddToMap(item);
                    return false;
                }

                if (item.Rot != newRot && item.X == newX && item.Y == newY) height = item.Z;
                foreach (var current6 in list2)
                    if (current6.Id != item.Id && current6.TotalHeight > height) height = current6.TotalHeight;
            }

            if (item.GetBaseItem().Name == "boutique_mannequin1" || item.GetBaseItem().Name == "gld_wall_tall")
            {
                if (newRot < 0 || newRot > 12)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "pirate_stage2" || item.GetBaseItem().Name == "pirate_stage2_g")
            {
                if (newRot < 0 || newRot > 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "gh_div_cor" || item.GetBaseItem().Name == "hblooza14_duckcrn" ||
                     item.GetBaseItem().Name == "hween13_dwarfcrn")
            {
                if (newRot != 1 && newRot != 3 && newRot != 5 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val14_b_roof" || item.GetBaseItem().Name == "val14_g_roof" ||
                     item.GetBaseItem().Name == "val14_y_roof")
            {
                if (newRot != 2 && newRot != 3 && newRot != 4 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val13_div_1")
            {
                if (newRot < 0 || newRot > 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_shrub1")
            {
                if (newRot != 0 && newRot != 2 && newRot != 3 && newRot != 4 && newRot != 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_div")
            {
                if (newRot < 0 || newRot > 5)
                {
                    newRot = 0;
                }
            }
            else
            {
                if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8)
                {
                    newRot = 0;
                }
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
                    var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddFloorItemMessageComposer"));
                    item.Serialize(serverMessage);
                    serverMessage.AppendString(_room.RoomData.Group != null
                        ? session.GetHabbo().UserName
                        : _room.RoomData.Owner);
                    _room.SendMessage(serverMessage);
                }
            }
            else
            {
                AddOrUpdateItem(item.Id);
                if (!onRoller && sendMessage)
                {
                    if (specialMove)
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        message.AppendInteger(oldCoord.X);
                        message.AppendInteger(oldCoord.Y);
                        message.AppendInteger(newX);
                        message.AppendInteger(newY);
                        message.AppendInteger(1);
                        message.AppendInteger(item.Id);
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendInteger(-1);
                        _room.SendMessage(message);
                    }
                    else
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
                        item.Serialize(message);
                        _room.SendMessage(message);
                    }
                }
                if (item.IsWired) _room.GetWiredHandler().MoveWired(item);
            }
            _room.GetGameMap().AddToMap(item);
            if (item.GetBaseItem().IsSeat) updateRoomUserStatuses = true;
            if (updateRoomUserStatuses)
            {
                _room.GetRoomUserManager().OnUserUpdateStatus(oldCoord.X, oldCoord.Y);
                _room.GetRoomUserManager().OnUserUpdateStatus(item.X, item.Y);
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

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddFloorItemMessageComposer"));
            item.Serialize(serverMessage);
            serverMessage.AppendString(_room.RoomData.Group != null ? session.GetHabbo().UserName : _room.RoomData.Owner);
            _room.SendMessage(serverMessage);

            _room.GetGameMap().AddToMap(item);
        }

        /// <summary>
        ///     Called when [height map update].
        /// </summary>
        /// <param name="affectedTiles">The affected tiles.</param>
        internal void OnHeightMapUpdate(Dictionary<int, ThreeDCoord> affectedTiles)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer"));
            message.AppendByte((byte) affectedTiles.Count);
            foreach (var coord in affectedTiles.Values)
            {
                message.AppendByte((byte) coord.X);
                message.AppendByte((byte) coord.Y);
                message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y)*256));
            }
            _room.SendMessage(message);
        }

        /// <summary>
        ///     Called when [height map update].
        /// </summary>
        /// <param name="affectedTiles">The affected tiles.</param>
        internal void OnHeightMapUpdate(ICollection affectedTiles)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer"));
            message.AppendByte((byte) affectedTiles.Count);
            foreach (Point coord in affectedTiles)
            {
                message.AppendByte((byte) coord.X);
                message.AppendByte((byte) coord.Y);
                message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y)*256));
            }
            _room.SendMessage(message);
        }

        /// <summary>
        ///     Called when [height map update].
        /// </summary>
        /// <param name="oldCoords">The old coords.</param>
        /// <param name="newCoords">The new coords.</param>
        internal void OnHeightMapUpdate(List<Point> oldCoords, List<Point> newCoords)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer"));
            message.AppendByte((byte) (oldCoords.Count + newCoords.Count));
            foreach (var coord in oldCoords)
            {
                message.AppendByte((byte) coord.X);
                message.AppendByte((byte) coord.Y);
                message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y)*256));
            }
            foreach (var nCoord in newCoords)
            {
                message.AppendByte((byte) nCoord.X);
                message.AppendByte((byte) nCoord.Y);
                message.AppendShort((short) (_room.GetGameMap().SqAbsoluteHeight(nCoord.X, nCoord.Y)*256));
            }
            _room.SendMessage(message);
        }

        /// <summary>
        ///     Gets the furni objects.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetFurniObjects(int x, int y)
        {
            return _room.GetGameMap().GetCoordinatedItems(new Point(x, y));
        }

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
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
            item.Serialize(message);
            _room.SendMessage(message);
            return true;
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

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddWallItemMessageComposer"));
            item.Serialize(serverMessage);
            serverMessage.AppendString(_room.RoomData.Owner);
            _room.SendMessage(serverMessage);
            return true;
        }

        /// <summary>
        ///     Updates the item.
        /// </summary>
        /// <param name="itemId">The item.</param>
        internal void AddOrUpdateItem(uint itemId)
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
        internal void RemoveItem(uint itemId)
        {
            if (_updatedItems.Contains(itemId))
                _updatedItems.Remove(itemId);

            if (!_removedItems.Contains(itemId))
                _removedItems.Add(itemId);

            RoomItem junkItem;
            Rollers.TryRemove(itemId, out junkItem);
        }

        /// <summary>
        ///     Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            if (GotRollers)
            {
                try
                {
                    _room.SendMessage(CycleRollers());
                }
                catch (Exception ex)
                {
                    Logging.LogThreadException(ex.ToString(),
                        $"rollers for room with ID {_room.RoomId}");
                    GotRollers = false;
                }
            }
            if (_roomItemUpdateQueue.Count > 0)
            {
                var addItems = new List<RoomItem>();
                lock (_roomItemUpdateQueue.SyncRoot)
                {
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

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            FloorItems.Clear();
            WallItems.Clear();
            _removedItems.Clear();
            _updatedItems.Clear();
            lock (_roomItemUpdateQueue.SyncRoot)
            {
                _roomItemUpdateQueue.Clear();
                _roomItemUpdateQueue = null;
            }
            _room = null;
            FloorItems = null;
            WallItems = null;
            _removedItems = null;
            _updatedItems = null;
            BreedingBear.Clear();
            BreedingTerrier.Clear();
            WallItems = null;
            BreedingBear.Clear();
            BreedingTerrier.Clear();
        }

        /// <summary>
        ///     Cycles the rollers.
        /// </summary>
        /// <returns>List&lt;ServerMessage&gt;.</returns>
        private List<ServerMessage> CycleRollers()
        {
            if (!GotRollers)
                return new List<ServerMessage>();
            if (_roolerCycle >= _rollerSpeed || _rollerSpeed == 0)
            {
                _rollerItemsMoved.Clear();
                _rollerUsersMoved.Clear();
                _rollerMessages.Clear();
                foreach (var current in Rollers.Values)
                {
                    var squareInFront = current.SquareInFront;
                    var roomItemForSquare = _room.GetGameMap().GetRoomItemForSquare(current.X, current.Y);
                    var userForSquare = _room.GetRoomUserManager().GetUserForSquare(current.X, current.Y);
                    if (!roomItemForSquare.Any() && userForSquare == null)
                        continue;
                    var coordinatedItems = _room.GetGameMap().GetCoordinatedItems(squareInFront);
                    var nextZ = 0.0;
                    var num = 0;
                    var flag = false;
                    var num2 = 0.0;
                    var flag2 = true;
                    var frontHasItem = false;
                    foreach (var current2 in coordinatedItems.Where(current2 => current2.IsRoller))
                    {
                        flag = true;
                        if (current2.TotalHeight > num2)
                            num2 = current2.TotalHeight;
                    }
                    if (coordinatedItems.Any(item => !item.GetBaseItem().Stackable)) frontHasItem = true;
                    if (flag)
                        using (var enumerator3 = coordinatedItems.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                var current3 = enumerator3.Current;
                                if (current3.TotalHeight > num2)
                                    flag2 = false;
                            }
                            goto IL_192;
                        }
                    goto IL_17C;
                    IL_192:
                    nextZ = num2;
                    var flag3 = num > 0 ||
                                _room.GetRoomUserManager().GetUserForSquare(squareInFront.X, squareInFront.Y) != null;
                    foreach (var current4 in roomItemForSquare)
                    {
                        var num3 = current4.Z - current.TotalHeight;
                        if (_rollerItemsMoved.Contains(current4.Id) || frontHasItem ||
                            !_room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) || !flag2 ||
                            !(current.Z < current4.Z) ||
                            _room.GetRoomUserManager().GetUserForSquare(squareInFront.X, squareInFront.Y) != null)
                            continue;
                        _rollerMessages.Add(UpdateItemOnRoller(current4, squareInFront, current.Id, num2 + num3));
                        _rollerItemsMoved.Add(current4.Id);
                    }
                    if (userForSquare != null && !userForSquare.IsWalking && flag2 && !flag3 &&
                        _room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) &&
                        _room.GetGameMap().GetFloorStatus(squareInFront) != 0 &&
                        !_rollerUsersMoved.Contains(userForSquare.HabboId))
                    {
                        _room.SendMessage(UpdateUserOnRoller(userForSquare, squareInFront, current.Id, nextZ));
                        _rollerUsersMoved.Add(userForSquare.HabboId);
                        _room.GetRoomUserManager().UpdateUserStatus(userForSquare, true);
                    }
                    continue;
                    IL_17C:
                    num2 += _room.GetGameMap().GetHeightForSquareFromData(squareInFront);
                    goto IL_192;
                }
                _roolerCycle = 0;
                return _rollerMessages;
            }

            {
                _roolerCycle++;
            }
            return new List<ServerMessage>();
        }

        internal bool HasFurniByItemName(string name)
        {
            var element = FloorItems.Values.Where(i => i.GetBaseItem().Name == name);
            return element.Any();
        }
    }
}