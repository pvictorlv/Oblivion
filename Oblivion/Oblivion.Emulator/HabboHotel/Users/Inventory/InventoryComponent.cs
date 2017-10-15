using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Pets.Enums;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Users.UserDataManagement;
using Oblivion.Messages;
using Oblivion.Messages.Enums;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Users.Inventory
{
    /// <summary>
    ///     Class InventoryComponent.
    /// </summary>
    internal class InventoryComponent
    {
        /// <summary>
        ///     The _floor items
        /// </summary>
        private readonly Dictionary<uint, UserItem> _floorItems;

        /// <summary>
        ///     The _inventory bots
        /// </summary>
        private readonly HybridDictionary _inventoryBots;

        /// <summary>
        ///     The _inventory pets
        /// </summary>
        private readonly HybridDictionary _inventoryPets;

        /// <summary>
        ///     The _m added items
        /// </summary>
        private readonly List<uint> _mAddedItems;

        /// <summary>
        ///     The _m removed items
        /// </summary>
        private readonly List<uint> _mRemovedItems;

        /// <summary>
        ///     The _wall items
        /// </summary>
        private readonly Dictionary<uint, UserItem> _wallItems;

        /// <summary>
        ///     The _is updated
        /// </summary>
        private bool _isUpdated;

        /// <summary>
        ///     The _m client
        /// </summary>
        private GameClient _mClient;

        /// <summary>
        ///     The _user attatched
        /// </summary>
        private bool _userAttatched;

        /// <summary>
        ///     The user identifier
        /// </summary>
        internal uint UserId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryComponent" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="client">The client.</param>
        /// <param name="userData">The user data.</param>
        internal InventoryComponent(uint userId, GameClient client, UserData userData)
        {
            _mClient = client;
            UserId = userId;
            _floorItems = new Dictionary<uint, UserItem>();
            _wallItems = new Dictionary<uint, UserItem>();
            SongDisks = new HybridDictionary();

            foreach (var current in userData.Inventory)
            {
                if (current.BaseItem.InteractionType == Interaction.MusicDisc)
                    SongDisks.Add(current.Id, current);
                if (current.IsWallItem)
                    _wallItems.Add(current.Id, current);
                else
                    _floorItems.Add(current.Id, current);
            }

            _inventoryPets = new HybridDictionary();
            _inventoryBots = new HybridDictionary();
            _mAddedItems = new List<uint>();
            _mRemovedItems = new List<uint>();
            _isUpdated = false;

            foreach (var bot in userData.Bots)
                AddBot(bot.Value);

            foreach (var pets in userData.Pets)
                AddPets(pets.Value);
        }

        /// <summary>
        /// Gel all items
        /// </summary>
        public IEnumerable<UserItem> GetItems => _floorItems.Values.Concat(_wallItems.Values);

        public int TotalItems => _floorItems.Count + _wallItems.Count + SongDisks.Count;

        /// <summary>
        ///     Gets a value indicating whether this instance is inactive.
        /// </summary>
        /// <value><c>true</c> if this instance is inactive; otherwise, <c>false</c>.</value>
        public bool IsInactive => !_userAttatched;

        /// <summary>
        ///     Gets a value indicating whether [needs update].
        /// </summary>
        /// <value><c>true</c> if [needs update]; otherwise, <c>false</c>.</value>
        internal bool NeedsUpdate => !_userAttatched && !_isUpdated;

        /// <summary>
        ///     Gets the song disks.
        /// </summary>
        /// <value>The song disks.</value>
        internal HybridDictionary SongDisks { get; }

        /// <summary>
        ///     Clears the items.
        /// </summary>
        internal void ClearItems()
        {
            Task.Factory.StartNew(() =>
            {
                UpdateItems(true);

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunFastQuery($"DELETE FROM items_rooms WHERE room_id='0' AND user_id = {UserId}");

                _mAddedItems.Clear();
                _mRemovedItems.Clear();
                _floorItems.Clear();
                _wallItems.Clear();
                SongDisks.Clear();
                _inventoryPets.Clear();
                _isUpdated = true;

                _mClient.GetMessageHandler()
                    .GetResponse()
                    .Init(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

                GetClient().GetMessageHandler().SendResponse();
            });
        }

        /// <summary>
        ///     Redeemcreditses the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        internal void Redeemcredits(GameClient session)
        {
            var currentRoom = session.GetHabbo().CurrentRoom;

            if (currentRoom == null)
                return;

            DataTable table;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"SELECT id FROM items_rooms WHERE user_id={session.GetHabbo().Id} AND room_id='0'");
                table = queryReactor.GetTable();
            }

            foreach (DataRow dataRow in table.Rows)
            {
                var item = GetItem(Convert.ToUInt32(dataRow[0]));

                if (item == null || (!item.BaseItem.Name.StartsWith("CF_") && !item.BaseItem.Name.StartsWith("CFC_")))
                    continue;

                var array = item.BaseItem.Name.Split('_');
                var num = int.Parse(array[1]);

                using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                    queryreactor2.RunFastQuery($"DELETE FROM items_rooms WHERE id={item.Id} LIMIT 1");

                currentRoom.GetRoomItemHandler().RemoveItem(item.Id);

                RemoveItem(item.Id, false);

                if (num <= 0)
                    continue;

                session.GetHabbo().Credits += num;
                session.GetHabbo().UpdateCreditsBalance();
            }
        }

        /// <summary>
        ///     Sets the state of the active.
        /// </summary>
        /// <param name="client">The client.</param>
        internal void SetActiveState(GameClient client)
        {
            _mClient = client;
            _userAttatched = true;
        }

        /// <summary>
        ///     Sets the state of the idle.
        /// </summary>
        internal void SetIdleState()
        {
            _userAttatched = false;
            _mClient = null;
        }

        /// <summary>
        ///     Gets the pet.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Pet.</returns>
        internal Pet GetPet(uint id) => _inventoryPets.Contains(id) ? _inventoryPets[id] as Pet : null;

        /// <summary>
        ///     Removes the pet.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RemovePet(uint petId)
        {
            _isUpdated = false;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RemovePetFromInventoryComposer"));
            serverMessage.AppendInteger(petId);
            GetClient().SendMessage(serverMessage);
            _inventoryPets.Remove(petId);
            return true;
        }

        /// <summary>
        ///     Moves the pet to room.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        internal void MovePetToRoom(uint petId)
        {
            _isUpdated = false;
            RemovePet(petId);
        }

        /// <summary>
        ///     Adds the pet.
        /// </summary>
        /// <param name="pet">The pet.</param>
        internal void AddPet(Pet pet)
        {
            _isUpdated = false;

            if (pet == null || _inventoryPets.Contains(pet.PetId))
                return;

            pet.PlacedInRoom = false;
            pet.RoomId = 0u;

            _inventoryPets.Add(pet.PetId, pet);

            SerializePetInventory();
        }

        /// <summary>
        ///     Loads the inventory.
        /// </summary>
        internal void LoadInventory()
        {
            _floorItems.Clear();
            _wallItems.Clear();

            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT id,base_item,extra_data,group_id,songcode FROM items_rooms WHERE user_id=@userid AND room_id='0' LIMIT 8000;");
                queryReactor.AddParameter("userid", ((int) UserId));

                table = queryReactor.GetTable();
            }

            foreach (DataRow dataRow in table.Rows)
            {
                var id = Convert.ToUInt32(dataRow[0]);
                var itemId = Convert.ToUInt32(dataRow[1]);

                if (!Oblivion.GetGame().GetItemManager().ContainsItem(itemId))
                    continue;

                string extraData;

                if (!DBNull.Value.Equals(dataRow[2]))
                    extraData = (string) dataRow[2];
                else
                    extraData = string.Empty;

                var group = Convert.ToUInt32(dataRow["group_id"]);

                string songCode;

                if (!DBNull.Value.Equals(dataRow["songcode"]))
                    songCode = (string) dataRow["songcode"];
                else
                    songCode = string.Empty;

                var userItem = new UserItem(id, itemId, extraData, group, songCode);

                if (userItem.BaseItem.InteractionType == Interaction.MusicDisc && !SongDisks.Contains(id))
                    SongDisks.Add(id, userItem);

                if (userItem.IsWallItem)
                {
                    if (!_wallItems.ContainsKey(userItem.Id))
                        _wallItems.Add(userItem.Id, userItem);
                }
                else if (!_floorItems.ContainsKey(userItem.Id))
                    _floorItems.Add(userItem.Id, userItem);
            }

//            SongDisks.Clear();
            _inventoryPets.Clear();
            _inventoryBots.Clear();

            using (var queryReactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor2.SetQuery($"SELECT * FROM bots WHERE user_id = {UserId} AND room_id = 0");
                var table2 = queryReactor2.GetTable();

                if (table2 == null)
                    return;

                foreach (DataRow botRow in table2.Rows)
                {
                    if ((string) botRow["ai_type"] == "pet")
                    {
                        queryReactor2.SetQuery($"SELECT * FROM pets_data WHERE id={botRow[0]} LIMIT 1");
                        var row = queryReactor2.GetRow();

                        if (row == null)
                            continue;

                        var pet = CatalogManager.GeneratePetFromRow(botRow, row);

                        if (_inventoryPets.Contains(pet.PetId))
                            _inventoryPets.Remove(pet.PetId);

                        _inventoryPets.Add(pet.PetId, pet);
                    }
                    else if ((string) botRow["ai_type"] == "generic")
                        AddBot(BotManager.GenerateBotFromRow(botRow));
                }
            }
        }

        /// <summary>
        ///     Updates the items.
        /// </summary>
        /// <param name="fromDatabase">if set to <c>true</c> [from database].</param>
        internal void UpdateItems(bool fromDatabase)
        {
            if (fromDatabase)
            {
                RunDbUpdate();
                LoadInventory();
            }

            _mClient.GetMessageHandler().GetResponse().Init(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>UserItem.</returns>
        internal UserItem GetItem(uint id)
        {
            _isUpdated = false;
            UserItem item;
            if (_floorItems.ContainsKey(id))
            {
                _floorItems.TryGetValue(id, out item);
            }
            else
            {
                _wallItems.TryGetValue(id, out item);
            }
            return item;
        }

        internal bool HasBaseItem(uint id)
        {
            return
                _floorItems.Values.Any(item => item?.BaseItem != null && item.BaseItem.ItemId == id) ||
                _wallItems.Values.Any(item => item?.BaseItem != null && item.BaseItem.ItemId == id);
        }

        /// <summary>
        ///     Adds the bot.
        /// </summary>
        /// <param name="bot">The bot.</param>
        internal void AddBot(RoomBot bot)
        {
            _isUpdated = false;

            if (bot == null || _inventoryBots.Contains(bot.BotId))
                return;

            bot.RoomId = 0u;

            _inventoryBots.Add(bot.BotId, bot);
        }

        internal void AddPets(Pet bot)
        {
            _isUpdated = false;

            if (bot == null || _inventoryPets.Contains(bot.PetId))
                return;

            bot.RoomId = 0u;

            _inventoryPets.Add(bot.PetId, bot);
        }

        /// <summary>
        ///     Gets the bot.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>RoomBot.</returns>
        internal RoomBot GetBot(uint id) => _inventoryBots.Contains(id) ? _inventoryBots[id] as RoomBot : null;

        /// <summary>
        ///     Removes the bot.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RemoveBot(uint petId)
        {
            _isUpdated = false;

            if (_inventoryBots.Contains(petId))
                _inventoryBots.Remove(petId);

            return true;
        }

        /// <summary>
        ///     Moves the bot to room.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        internal void MoveBotToRoom(uint petId)
        {
            _isUpdated = false;
            RemoveBot(petId);
        }

        /// <summary>
        ///     Adds the new item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="baseItem">The base item.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="thGroup">The thGroup.</param>
        /// <param name="insert">if set to <c>true</c> [insert].</param>
        /// <param name="fromRoom">if set to <c>true</c> [from room].</param>
        /// <param name="limno">The limno.</param>
        /// <param name="limtot">The limtot.</param>
        /// <param name="songCode">The song code.</param>
        /// <returns>UserItem.</returns>
        internal UserItem AddNewItem(uint id, uint baseItem, string extraData, uint thGroup, bool insert, bool fromRoom, int limno, int limtot, string songCode = "")
        {
            _isUpdated = false;

            if (insert)
            {
                if (fromRoom)
                {
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryReactor.RunFastQuery("UPDATE items_rooms SET user_id = '" + UserId + "', room_id= '0' WHERE (id='" + id + "')");
                }
                else
                {
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery($"INSERT INTO items_rooms (base_item, user_id, group_id) VALUES ('{baseItem}', '{UserId}', '{thGroup}');");

                        if (id == 0)
                            id = ((uint) queryReactor.InsertQuery());

                        SendNewItems(id);

                        if (!string.IsNullOrEmpty(extraData))
                        {
                            queryReactor.SetQuery("UPDATE items_rooms SET extra_data = @extraData WHERE id = " + id);
                            queryReactor.AddParameter("extraData", extraData);
                            queryReactor.RunQuery();
                        }

                        if (limno > 0)
                            queryReactor.RunFastQuery($"INSERT INTO items_limited VALUES ('{id}', '{limno}', '{limtot}');");

                        if (!string.IsNullOrEmpty(songCode))
                        {
                            queryReactor.SetQuery($"UPDATE items_rooms SET songcode='{songCode}' WHERE id='{id}' LIMIT 1");
                            queryReactor.RunQuery();
                        }
                    }
                }
            }

            if (id == 0)
                return null;

            var userItem = new UserItem(id, baseItem, extraData, thGroup, songCode);

            if (UserHoldsItem(id))
                RemoveItem(id, false);

            if (userItem.BaseItem.InteractionType == Interaction.MusicDisc)
                SongDisks.Add(userItem.Id, userItem);

            if (userItem.IsWallItem)
                _wallItems.Add(userItem.Id, userItem);
            else
                _floorItems.Add(userItem.Id, userItem);

            if (_mRemovedItems.Contains(id))
                _mRemovedItems.Remove(id);

            if (!_mAddedItems.Contains(id))
                _mAddedItems.Add(id);

            AddItemToItemInventory(userItem);
            return userItem;
        }

        /// <summary>
        ///     Removes the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="placedInroom">if set to <c>true</c> [placed inroom].</param>
        internal void RemoveItem(uint id, bool placedInroom)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null ||
                GetClient().GetHabbo().GetInventoryComponent() == null)
            {
                GetClient().Disconnect("user null RemoveItem");
                return;
            }

            _isUpdated = false;
            GetClient()
                .GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("RemoveInventoryObjectMessageComposer"));

            GetClient().GetMessageHandler().GetResponse().AppendInteger(id);
            //this.GetClient().GetMessageHandler().GetResponse().AppendInt32(Convert.ToInt32(this.GetClient().GetHabbo().Id));

            GetClient().GetMessageHandler().SendResponse();
            if (_mAddedItems.Contains(id))
                _mAddedItems.Remove(id);

            if (_mRemovedItems.Contains(id))
                return;

            var item = GetClient().GetHabbo().GetInventoryComponent().GetItem(id);

            SongDisks.Remove(id);
            _floorItems.Remove(item.Id);
            _wallItems.Remove(item.Id);
            _mRemovedItems.Add(id);
        }

        /// <summary>
        ///     Serializes the floor item inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeFloorItemInventory()
        {
            var floor = _floorItems.Values.ToList();
            var wall = _wallItems.Values.ToList();
            var i = (floor.Count + SongDisks.Count + wall.Count);

            if (i > 2800)
                _mClient.SendMessage(StaticMessage.AdviceMaxItems);

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadInventoryMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(i > 2800 ? 2800 : i);

            var inc = 0;

            foreach (var userItem in floor)
            {
                if (inc == 3500)
                    return serverMessage;

                inc++;

                userItem.SerializeFloor(serverMessage, true);
            }
            floor.Clear();
            foreach (var userItem in wall)
            {
                if (inc == 3500)
                    return serverMessage;

                inc++;

                userItem.SerializeWall(serverMessage, true);
            }
            wall.Clear();
            foreach (UserItem userItem in SongDisks.Values)
            {
                if (inc == 3500)
                    return serverMessage;

                inc++;

                userItem.SerializeFloor(serverMessage, true);
            }

            return serverMessage;
        }
        
        /// <summary>
        ///     Add item to inventory
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage AddItemToItemInventory(UserItem item)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FurniListAddMessageComposer"));
            serverMessage.AppendInteger(item.Id);
            serverMessage.AppendString(item.BaseItem.Type.ToString().ToUpper());
            if (item.LimitedSellId > 0)
            {
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(256);
                serverMessage.AppendString(item.ExtraData);
                serverMessage.AppendInteger(item.LimitedSellId);
                serverMessage.AppendInteger(item.LimitedStack);
            }
            else
            {
                serverMessage.AppendString(item.ExtraData);
            }

            serverMessage.AppendBool(item.BaseItem.AllowRecycle);
            serverMessage.AppendBool(item.BaseItem.AllowTrade);
            serverMessage.AppendBool(item.LimitedSellId == 0 && item.BaseItem.AllowInventoryStack);
            serverMessage.AppendBool(item.BaseItem.IsRare); //can sell in marketplace xD
            serverMessage.AppendInteger(-1);
            serverMessage.AppendBool(true);
            serverMessage.AppendInteger(-1);
            if (!item.IsWallItem)
            {
                serverMessage.AppendString(string.Empty);
                serverMessage.AppendInteger(0);

            }
            return serverMessage;
        }
        

        /// <summary>
        ///     Serializes the pet inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializePetInventory()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PetInventoryMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            var list = _inventoryPets.Values.Cast<Pet>().ToList();
            serverMessage.AppendInteger(list.Count);
            foreach (var current in list)
                current.SerializeInventory(serverMessage);
            return serverMessage;
        }

        /// <summary>
        ///     Serializes the bot inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeBotInventory()
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("BotInventoryMessageComposer"));

            var list = _inventoryBots.Values.OfType<RoomBot>().ToList();
            serverMessage.AppendInteger(list.Count);
            foreach (var current in list)
            {
                serverMessage.AppendInteger(current.BotId);
                serverMessage.AppendString(current.Name);
                serverMessage.AppendString(current.Motto);
                serverMessage.AppendString("m");
                serverMessage.AppendString(current.Look);
            }
            return serverMessage;
        }

        /// <summary>
        ///     Adds the item array.
        /// </summary>
        /// <param name="roomItemList">The room item list.</param>
        internal void AddItemArray(List<RoomItem> roomItemList)
        {
            foreach (var current in roomItemList)
                AddItem(current);
        }

        /// <summary>
        ///     Adds the item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void AddItem(RoomItem item)
        {
            AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, 0, 0, item.SongCode);
        }

        /// <summary>
        ///     Runs the cycle update.
        /// </summary>
        internal void RunCycleUpdate()
        {
            _isUpdated = true;
            RunDbUpdate();
        }

        /// <summary>
        ///     Runs the database update.
        /// </summary>
        internal void RunDbUpdate()
        {
            try
            {
                if (_mRemovedItems.Count <= 0 && _mAddedItems.Count <= 0 && _inventoryPets.Count <= 0)
                    return;

                var queryChunk = new QueryChunk();

                if (_mAddedItems.Count > 0)
                {
                    foreach (var itemId in _mAddedItems)
                        queryChunk.AddQuery($"UPDATE items_rooms SET user_id='{UserId}', room_id='0' WHERE id='{itemId}'");

                    _mAddedItems.Clear();
                }

                if (_mRemovedItems.Count > 0)
                {
                    try
                    {
                        foreach (var itemId in _mRemovedItems)
                        {
                            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                                GetClient().GetHabbo().CurrentRoom.GetRoomItemHandler().SaveFurniture(queryReactor);

                            if (SongDisks.Contains(itemId))
                                SongDisks.Remove(itemId);
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    _mRemovedItems.Clear();
                }

                foreach (Pet current in _inventoryPets.Values)
                {
                    if (current.DbState == DatabaseUpdateState.NeedsUpdate)
                    {
                        queryChunk.AddParameter($"{current.PetId}name", current.Name);
                        queryChunk.AddParameter($"{current.PetId}race", current.Race);
                        queryChunk.AddParameter($"{current.PetId}color", current.Color);

                        queryChunk.AddQuery(string.Concat("UPDATE bots SET room_id = ", current.RoomId, ", name = @", current.PetId, "name, x = ", current.X, ", Y = ", current.Y, ", Z = ", current.Z, " WHERE id = ", current.PetId));

                        queryChunk.AddQuery(string.Concat("UPDATE pets_data SET race = @", current.PetId, "race, color = @", current.PetId, "color, type = ", current.Type, ", experience = ", current.Experience, ", energy = ", current.Energy, ", nutrition = ", current.Nutrition, ", respect = ", current.Respect, ", createstamp = '", current.CreationStamp, "', lasthealth_stamp = ", Oblivion.DateTimeToUnix(current.LastHealth), ", untilgrown_stamp = ", Oblivion.DateTimeToUnix(current.UntilGrown), " WHERE id = ", current.PetId));
                    }

                    current.DbState = DatabaseUpdateState.Updated;
                }

                using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                    queryChunk.Execute(queryreactor2);
            }
            catch (Exception ex)
            {
                Logging.LogCacheError($"FATAL ERROR DURING USER INVENTORY DB UPDATE: {ex}");
            }
        }

        /// <summary>
        ///     Serializes the music discs.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeMusicDiscs()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SongsLibraryMessageComposer"));

            serverMessage.AppendInteger(SongDisks.Count);

            foreach (var current in from x in _floorItems.Values where x.BaseItem.InteractionType == Interaction.MusicDisc select x)
            {
                uint.TryParse(current.ExtraData, out var i);

                serverMessage.AppendInteger(current.Id);
                serverMessage.AppendInteger(i);
            }

            return serverMessage;
        }

        /// <summary>
        ///     Gets the pets.
        /// </summary>
        /// <returns>List&lt;Pet&gt;.</returns>
        internal List<Pet> GetPets() => _inventoryPets.Values.Cast<Pet>().ToList();

        /// <summary>
        ///     Sends the floor inventory update.
        /// </summary>
        internal void SendFloorInventoryUpdate()
        {
//            _mClient.SendMessage(SerializeFloorItemInventory());
        }

        /// <summary>
        ///     Sends the new items.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal void SendNewItems(uint id)
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(id);
            _mClient.SendMessage(serverMessage);
        }

        /// <summary>
        ///     Users the holds item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool UserHoldsItem(uint itemId) => SongDisks.Contains(itemId) || _floorItems.ContainsKey(itemId) || _wallItems.ContainsKey(itemId);

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient GetClient() => Oblivion.GetGame().GetClientManager().GetClientByUserId(UserId);
    }
}