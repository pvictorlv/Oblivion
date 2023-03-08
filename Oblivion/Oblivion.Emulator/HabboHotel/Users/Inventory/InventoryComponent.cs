using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Pets.Enums;
using Oblivion.HabboHotel.RoomBots;
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
        private ConcurrentDictionary<string, UserItem> _items;

        /// <summary>
        ///     The _inventory bots
        /// </summary>
        private ConcurrentDictionary<uint, RoomBot> _inventoryBots;

        /// <summary>
        ///     The _inventory pets
        /// </summary>
        private ConcurrentDictionary<uint, Pet> _inventoryPets;

        /// <summary>
        ///     The _m added items
        /// </summary>
        private HashSet<string> _mAddedItems;

        /// <summary>
        ///     The _m removed items
        /// </summary>
        private HashSet<UserItem> _mRemovedItems;


        /// <summary>
        ///     The _m client
        /// </summary>
        private GameClient _mClient;


        /// <summary>
        ///     The user identifier
        /// </summary>
        internal uint UserId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryComponent" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="client">The client.</param>
        internal InventoryComponent(uint userId, GameClient client)
        {
            _mClient = client;
            UserId = userId;
            _items = new ConcurrentDictionary<string, UserItem>();


            _inventoryPets = new ConcurrentDictionary<uint, Pet>();
            _inventoryBots = new ConcurrentDictionary<uint, RoomBot>();
            _mAddedItems = new HashSet<string>();
            _mRemovedItems = new HashSet<UserItem>();


            LoadInventory();
        }

        /// <summary>
        /// Gel all items
        /// </summary>
        public IEnumerable<UserItem> GetItems => _items.Values;

        public int TotalItems => _items.Count;


        /// <summary>
        ///     Gets the song disks.
        /// </summary>
        /// <value>The song disks.</value>
        //        internal Dictionary<long, UserItem> SongDisks { get; }
        public List<UserItem> GetDisks()
        {
            return _items.Values.Where(item => item.BaseItem.InteractionType == Interaction.MusicDisc).ToList();
        }

        /// <summary>
        ///     Clears the items.
        /// </summary>
        internal void ClearItems()
        {
            Task.Factory.StartNew(() =>
            {
                UpdateItems(true);

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunNoLockFastQuery(
                        $"DELETE FROM items_rooms WHERE (room_id IS NULL or room_id=0) AND user_id = {UserId};");

                _mAddedItems.Clear();
                _mRemovedItems.Clear();
                _items.Clear();
                _inventoryPets.Clear();

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

            using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.SetNoLockQuery(
                    $"SELECT id FROM items_rooms WHERE user_id={session.GetHabbo().Id} AND (room_id IS NULL or room_id=0) IS NULL;");
                var table = queryreactor2.GetTable();

                foreach (DataRow dataRow in table.Rows)
                {
                    var item = GetItem(Convert.ToString(dataRow[0]));

                    if (item == null || (!item.BaseItem.Name.StartsWith("DFD_") &&
                                         !item.BaseItem.Name.StartsWith("CF_") &&
                                         !item.BaseItem.Name.StartsWith("CFC_")))
                        continue;

                    var array = item.BaseItem.Name.Split('_');
                    var num = int.Parse(array[1]);

                    queryreactor2.RunNoLockFastQuery($"DELETE FROM items_rooms WHERE id='{item.Id}' LIMIT 1;");


                    currentRoom.GetRoomItemHandler().RemoveItem(item.Id);


                    RemoveItem(item.Id, false, 0);

                    if (num <= 0)
                        continue;
                    if (item.BaseItem.Name.StartsWith("DFD_"))
                    {
                        session.GetHabbo().Diamonds += num;
                        session.GetHabbo().UpdateSeasonalCurrencyBalance();
                    }
                    else
                    {
                        session.GetHabbo().Credits += num;
                        session.GetHabbo().UpdateCreditsBalance();
                    }
                }
            }
        }


        /// <summary>
        ///     Gets the pet.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Pet.</returns>
        internal Pet GetPet(uint id) => _inventoryPets.TryGetValue(id, out var pet) ? pet : null;

        /// <summary>
        ///     Removes the pet.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RemovePet(uint petId)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RemovePetFromInventoryComposer"));
            serverMessage.AppendInteger(petId);
            GetClient().SendMessage(serverMessage);
            _inventoryPets.TryRemove(petId, out _);
            return true;
        }

        /// <summary>
        ///     Moves the pet to room.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        internal void MovePetToRoom(uint petId)
        {
            RemovePet(petId);
        }

        /// <summary>
        ///     Adds the pet.
        /// </summary>
        /// <param name="pet">The pet.</param>
        internal void AddPet(Pet pet)
        {
            if (pet == null || !_inventoryPets.TryAdd(pet.PetId, pet))
                return;

            pet.PlacedInRoom = false;
            pet.RoomId = 0u;
        }

        /// <summary>
        ///     Loads the inventory.
        /// </summary>
        internal void LoadInventory()
        {
            _items.Clear();

            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetNoLockQuery(
                    "SELECT id,base_item,extra_data,group_id,songcode,limited FROM items_rooms WHERE user_id=@userid AND (room_id IS NULL or room_id=0) LIMIT 4500;");
                queryReactor.AddParameter("userid", ((int)UserId));

                table = queryReactor.GetTable();
            }

            foreach (DataRow dataRow in table.Rows)
            {
                var id = Convert.ToString(dataRow[0]);
                var itemId = Convert.ToUInt32(dataRow[1]);

                if (!Oblivion.GetGame().GetItemManager().ContainsItem(itemId))
                    continue;

                string extraData;

                if (!DBNull.Value.Equals(dataRow[2]))
                    extraData = (string)dataRow[2];
                else
                    extraData = string.Empty;


                if (!uint.TryParse(dataRow["group_id"].ToString(), out var group))
                {
                    group = 0;
                }

                string songCode;

                if (!DBNull.Value.Equals(dataRow["songcode"]))
                    songCode = (string)dataRow["songcode"];
                else
                    songCode = string.Empty;

                var limitedString = dataRow["limited"].ToString().Split(';');

                //0 = stack , 1 = total
                var userItem = new UserItem(id, itemId, extraData, group, songCode, Convert.ToInt32(limitedString[0]),
                    Convert.ToInt32(limitedString[1]));

                if (userItem.BaseItem == null) continue;

                _items.TryAdd(userItem.Id, userItem);
            }

//            SongDisks.Clear();
            _inventoryPets.Clear();
            _inventoryBots.Clear();

            using (var queryReactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor2.SetQuery($"SELECT * FROM bots WHERE user_id = {UserId} AND (room_id IS NULL or room_id=0)");
                var table2 = queryReactor2.GetTable();

                if (table2 == null)
                    return;

                foreach (DataRow botRow in table2.Rows)
                {
                    if ((string)botRow["ai_type"] == "pet")
                    {
                        queryReactor2.SetQuery($"SELECT * FROM pets_data WHERE id={botRow[0]} LIMIT 1");
                        var row = queryReactor2.GetRow();

                        if (row == null)
                            continue;

                        var pet = CatalogManager.GeneratePetFromRow(botRow, row);

                        _inventoryPets[pet.PetId] = pet;
                    }
                    else if ((string)botRow["ai_type"] == "generic")
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

            _mClient.GetMessageHandler().GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>UserItem.</returns>
        internal UserItem GetItem(string id) => _items.TryGetValue(id, out var item) ? item : null;

        internal bool HasBaseItem(uint id)
        {
            return
                _items.Values.Any(item => item?.BaseItem != null && item.BaseItem.ItemId == id);
        }

        /// <summary>
        ///     Adds the bot.
        /// </summary>
        /// <param name="bot">The bot.</param>
        internal void AddBot(RoomBot bot)
        {
            if (bot == null || !_inventoryBots.TryAdd(bot.BotId, bot))
                return;

            bot.RoomId = 0u;
        }

        /// <summary>
        ///     Gets the bot.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>RoomBot.</returns>
        internal RoomBot GetBot(uint id) => _inventoryBots.TryGetValue(id, out var bot) ? bot : null;

        /// <summary>
        ///     Removes the bot.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RemoveBot(uint petId) => _inventoryBots.TryRemove(petId, out _);

        /// <summary>
        ///     Moves the bot to room.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        internal void MoveBotToRoom(uint petId)
        {
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
        internal UserItem AddNewItem(string id, uint baseItem, string extraData, uint thGroup, bool insert,
            bool fromRoom,
            int limno, int limtot, string songCode = "")
        {
            if (id == "0" || id == "0u")
            {
                var guidId = Guid.NewGuid();
                id = (ShortGuid)guidId;
            }

            if (insert)
            {
                if (!fromRoom)
                {
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        var groupId = (thGroup) <= 0 ? "NULL" : $"'{thGroup}'";


                        queryReactor.SetNoLockQuery(
                            $"INSERT INTO items_rooms (id, base_item, user_id, group_id, extra_data, songcode, limited) VALUES ('{id}', '{baseItem}', '{UserId}', {groupId}, @edata, '{songCode}', '{limno};{limtot}');");

                        queryReactor.AddParameter("edata", extraData);
                        queryReactor.RunQuery();
                        var virtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(id);

                        SendNewItems(virtualId);
                    }
                }
            }


            var userItem = new UserItem(id, baseItem, extraData, thGroup, songCode, limno, limtot);


            if (UserHoldsItem(id))
                RemoveItem(id, false, 0);

            if (userItem.BaseItem == null) return null;


            _items.TryAdd(userItem.Id, userItem);

            if (_mRemovedItems.Contains(userItem))
                _mRemovedItems.Remove(userItem);

            if (!_mAddedItems.Contains(id))
                _mAddedItems.Add(id);

            return userItem;
        }


        public void AddNewItem(UserItem userItem)
        {
            var virtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(userItem.Id);
            SendNewItems(virtualId);

            var id = userItem.Id;

            if (UserHoldsItem(id))
                RemoveItem(id, false, 0);

            if (userItem.BaseItem == null) return;


            _items.TryAdd(userItem.Id, userItem);

            if (_mRemovedItems.Contains(userItem))
                _mRemovedItems.Remove(userItem);

            if (!_mAddedItems.Contains(id))
                _mAddedItems.Add(id);
        }

        /// <summary>
        ///     Removes the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="placedInroom">if set to <c>true</c> [placed inroom].</param>
        /// <param name="roomId">the room who is placed the item</param>
        internal void RemoveItem(string id, bool placedInroom, uint roomId)
        {
            GetClient()
                .GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("RemoveInventoryObjectMessageComposer"));

            var item = GetItem(id);
            if (item == null) return;

            GetClient().GetMessageHandler().GetResponse().AppendInteger(item.VirtualId);

            GetClient().GetMessageHandler().SendResponse();
            if (_mAddedItems.Contains(id))
                _mAddedItems.Remove(id);

            if (placedInroom)
            {
                item.RoomId = roomId;
            }

            _items?.TryRemove(item.Id, out _);

            if (_mRemovedItems.Contains(item))
                return;

            _mRemovedItems?.Add(item);
        }

        /// <summary>
        ///     Serializes the floor item inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal void SerializeFloorItemInventory(GameClient session)
        {
            if (_items == null) return;
            var i = _items.Count;

            if (i <= 0)
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("LoadInventoryMessageComposer"));
                message.AppendInteger(1);
                message.AppendInteger(0);
                message.AppendInteger(0);
                session.SendMessage(message);
            }

            var ITEMS_PER_PAGE = 2000d;
            int totalPages = (int)Math.Ceiling(i / ITEMS_PER_PAGE);


            int totalSent = 0;
            int currentPage = 0;

            if (i > 4500)
                _mClient.SendMessage(StaticMessage.AdviceMaxItems);


            var inventoryItems = new List<UserItem>();

            using (var serverMessage = new ServerMessage())
            {
                serverMessage.Init(LibraryParser.OutgoingRequest("LoadInventoryMessageComposer"));

                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(_items.Count);

                foreach (var inventoryItem in _items.Values)
            {
                totalSent++;

             //   inventoryItems.Add(userItem.Value);

             //   if (inventoryItems.Count >= ITEMS_PER_PAGE || totalSent >= i)
                {

                        

           //             foreach (var inventoryItem in inventoryItems)
                        {
                            if (inventoryItem.IsWallItem)
                                inventoryItem.SerializeWall(serverMessage, true);
                            else
                                inventoryItem.SerializeFloor(serverMessage, true);
                        }

                    }
                _mClient.SendMessage(serverMessage);

                    inventoryItems = new List<UserItem>();
                    currentPage++;
                }
            }
        }


        internal void AddItemToItemInventory(RoomItem item, bool dbUpdate)
        {
            var userItem = new UserItem(item.Id, item.BaseItem.ItemId, item.ExtraData, item.GroupId, item.SongCode,
                item.LimitedNo, item.LimitedTot);
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FurniListAddMessageComposer")))
            {
                serverMessage.AppendInteger(item.VirtualId);
                serverMessage.AppendString(item.GetBaseItem().Type.ToString().ToUpper());
                serverMessage.AppendInteger(item.VirtualId);
                serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(0);
                if (item.LimitedNo > 0)
                {
                    serverMessage.AppendInteger(1);
                    serverMessage.AppendInteger(256);
                    serverMessage.AppendString(item.ExtraData);
                    serverMessage.AppendInteger(item.LimitedNo);
                    serverMessage.AppendInteger(item.LimitedTot);
                }
                else
                {
                    serverMessage.AppendString(item.ExtraData);
                }

                serverMessage.AppendBool(item.GetBaseItem().AllowRecycle);
                serverMessage.AppendBool(item.GetBaseItem().AllowTrade);
                serverMessage.AppendBool(item.LimitedNo == 0 && item.GetBaseItem().AllowInventoryStack);
                serverMessage.AppendBool(true); //can sell in marketplace xD
                serverMessage.AppendInteger(-1);
                serverMessage.AppendBool(false);
                serverMessage.AppendInteger(-1);
                if (!item.IsWallItem)
                {
                    serverMessage.AppendString(string.Empty);
                    serverMessage.AppendInteger(0);
                }

                var id = item.Id;
                if (UserHoldsItem(id))
                    RemoveItem(id, false, 0);


                _items.TryAdd(userItem.Id, userItem);

                if (_mRemovedItems.Contains(userItem))
                    _mRemovedItems.Remove(userItem);

                if (!_mAddedItems.Contains(id))
                    _mAddedItems.Add(id);

                _mClient.SendMessage(serverMessage);
            }
        }

        internal void AddItemToItemInventory(UserItem userItem)
        {
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FurniListAddMessageComposer")))
            {
                serverMessage.AppendInteger(userItem.VirtualId);
                serverMessage.AppendString(userItem.BaseItem.Type.ToString().ToUpper());
                serverMessage.AppendInteger(userItem.VirtualId);
                serverMessage.AppendInteger(userItem.BaseItem.SpriteId);
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(0);
                if (userItem.LimitedSellId > 0)
                {
                    serverMessage.AppendInteger(1);
                    serverMessage.AppendInteger(256);
                    serverMessage.AppendString(userItem.ExtraData);
                    serverMessage.AppendInteger(userItem.LimitedSellId);
                    serverMessage.AppendInteger(userItem.LimitedStack);
                }
                else
                {
                    serverMessage.AppendString(userItem.ExtraData);
                }

                serverMessage.AppendBool(userItem.BaseItem.AllowRecycle);
                serverMessage.AppendBool(userItem.BaseItem.AllowTrade);
                serverMessage.AppendBool(userItem.LimitedSellId == 0 && userItem.BaseItem.AllowInventoryStack);
                serverMessage.AppendBool(true); //can sell in marketplace xD
                serverMessage.AppendInteger(-1);
                serverMessage.AppendBool(false);
                serverMessage.AppendInteger(-1);
                if (!userItem.IsWallItem)
                {
                    serverMessage.AppendString(string.Empty);
                    serverMessage.AppendInteger(0);
                }

                var id = userItem.Id;
                if (UserHoldsItem(id))
                    RemoveItem(id, false, 0);


                _items.TryAdd(userItem.Id, userItem);

                if (_mRemovedItems.Contains(userItem))
                    _mRemovedItems.Remove(userItem);

                if (!_mAddedItems.Contains(id))
                    _mAddedItems.Add(id);

                _mClient.SendMessage(serverMessage);
            }
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
            var list = _inventoryPets.Values;
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

            var list = _inventoryBots.Values;
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
        ///     Adds the item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void AddItem(RoomItem item) => AddNewItem(item.Id, item.BaseItem.ItemId, item.ExtraData, item.GroupId,
            true,
            true, 0, 0, item.SongCode);

        /// <summary>
        ///     Runs the database update.
        /// </summary>
        internal void RunDbUpdate()
        {
            try
            {
                if (_mAddedItems?.Count > 0)
                {
                    var added = _mAddedItems.ToList();

                    var builder = new StringBuilder();
                    builder.Append($"UPDATE items_rooms SET user_id='{UserId}', room_id = NULL WHERE id IN (");
                    var i = 0;
                    var count = added.Count;
                    foreach (var itemId in added)
                    {
                        i++;
                        builder.Append(i >= count ? $"'{itemId}'" : $"'{itemId}',");
                    }

                    builder.Append(");");

                    using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery(builder.ToString());
                    }

                    _mAddedItems.Clear();
                    added.Clear();
                }

                if (_mRemovedItems?.Count > 0)
                {
                    var removed = _mRemovedItems.ToList();

                    try
                    {
                        using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        {
                            foreach (var item in removed)
                            {
                                if (item.RoomId > 0)
                                {
                                    var room = Oblivion.GetGame().GetRoomManager().GetRoom(item.RoomId);
                                    room?.GetRoomItemHandler().SaveFurniture(queryReactor);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    removed.Clear();
                    _mRemovedItems?.Clear();
                }

                if (_inventoryPets?.Count > 0)
                {
                    var queryChunk = new QueryChunk();

                    foreach (Pet current in _inventoryPets.Values)
                    {
                        if (current.DbState == DatabaseUpdateState.NeedsUpdate)
                        {
                            queryChunk.AddParameter($"{current.PetId}name", current.Name);
                            queryChunk.AddParameter($"{current.PetId}race", current.Race);
                            queryChunk.AddParameter($"{current.PetId}color", current.Color);


                            var roomId = (current.RoomId <= 0) ? "NULL" : current.RoomId.ToString();
                            queryChunk.AddQuery(string.Concat("UPDATE bots SET room_id = ", roomId, ", name = @",
                                current.PetId, "name, x = ", current.X, ", Y = ", current.Y, ", Z = ", current.Z,
                                " WHERE id = ", current.PetId));

                            queryChunk.AddQuery(string.Concat("UPDATE pets_data SET race = @", current.PetId,
                                "race, color = @", current.PetId, "color, type = ", current.Type, ", experience = ",
                                current.Experience, ", energy = ", current.Energy, ", nutrition = ", current.Nutrition,
                                ", respect = ", current.Respect, ", createstamp = '", current.CreationStamp,
                                "', lasthealth_stamp = ", Oblivion.DateTimeToUnix(current.LastHealth),
                                ", untilgrown_stamp = ", Oblivion.DateTimeToUnix(current.UntilGrown), " WHERE id = ",
                                current.PetId));
                        }

                        current.DbState = DatabaseUpdateState.Updated;
                    }

                    using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryChunk.Execute(queryreactor2);
                }
            }
            catch (Exception ex)
            {
                Logging.LogCacheError($"FATAL ERROR DURING USER INVENTORY DB UPDATE: {ex}");
            }
        }


        /// <summary>
        ///     Sends the new items.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal void SendNewItems(uint id)
        {
            using (var serverMessage = new ServerMessage())
            {
                serverMessage.Init(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(id);
                _mClient.SendMessage(serverMessage);
            }
        }

        internal void Dispose()
        {
            _mClient = null;
            _inventoryBots?.Clear();
            _inventoryBots = null;

            _inventoryPets?.Clear();
            _inventoryPets = null;

            _mAddedItems?.Clear();
            _mAddedItems = null;

            _mRemovedItems?.Clear();
            _mRemovedItems = null;

            try
            {
                if (_items != null)
                    foreach (var item in _items.Values)
                    {
                        item?.Dispose();
                    }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "inventory dispose");
            }

            _items?.Clear();
            _items = null;
        }

        /// <summary>
        ///     Users the holds item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool UserHoldsItem(string itemId) => _items.ContainsKey(itemId);

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient GetClient() => _mClient ?? Oblivion.GetGame().GetClientManager().GetClientByUserId(UserId);
    }
}