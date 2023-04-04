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
        internal Task ClearItems()
        {
            return Task.Factory.StartNew(async () =>
            {
                await UpdateItems(true);

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    await queryReactor.RunNoLockFastQueryAsync(
                        $"DELETE FROM items_rooms WHERE (room_id IS NULL or room_id=0) AND user_id = {UserId};");

                _mAddedItems.Clear();
                _mRemovedItems.Clear();
                _items.Clear();
                _inventoryPets.Clear();

                await _mClient.GetMessageHandler()
                    .GetResponse()
                    .InitAsync(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

                await GetClient().GetMessageHandler().SendResponse();
            });
        }

        /// <summary>
        ///     Redeemcreditses the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        internal async Task Redeemcredits(GameClient session)
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

                    await queryreactor2.RunNoLockFastQueryAsync(
                        $"DELETE FROM items_rooms WHERE id='{item.Id}' LIMIT 1;");


                    currentRoom.GetRoomItemHandler().RemoveItem(item.Id);


                    await RemoveItem(item.Id, false, 0);

                    if (num <= 0)
                        continue;
                    if (item.BaseItem.Name.StartsWith("DFD_"))
                    {
                        session.GetHabbo().Diamonds += num;
                        await session.GetHabbo().UpdateSeasonalCurrencyBalance();
                    }
                    else
                    {
                        session.GetHabbo().Credits += num;
                        await session.GetHabbo().UpdateCreditsBalance();
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
        internal async Task<bool> RemovePet(uint petId)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RemovePetFromInventoryComposer"));
            await serverMessage.AppendIntegerAsync(petId);
            await GetClient().SendMessageAsync(serverMessage);
            _inventoryPets.TryRemove(petId, out _);
            return true;
        }

        /// <summary>
        ///     Moves the pet to room.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        internal async Task MovePetToRoom(uint petId)
        {
            await RemovePet(petId);
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
        internal async Task LoadInventory()
        {
            _items.Clear();

            DataTable table;

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
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

            using (var queryReactor2 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor2.SetQuery(
                    $"SELECT * FROM bots WHERE user_id = {UserId} AND (room_id IS NULL or room_id=0)");
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
        internal async Task UpdateItems(bool fromDatabase)
        {
            if (fromDatabase)
            {
                await Task.Run(async () =>
                {
                    await RunDbUpdate();
                    await LoadInventory();

                    await _mClient.GetMessageHandler().GetResponse()
                        .InitAsync(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

                    await _mClient.GetMessageHandler().SendResponse();
                });
                return;
            }

            await _mClient.GetMessageHandler().GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

            await _mClient.GetMessageHandler().SendResponse();
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
        internal async Task<UserItem> AddNewItem(string id, uint baseItem, string extraData, uint thGroup, bool insert,
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
                        await queryReactor.RunQueryAsync();
                        var virtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(id);

                        await SendNewItems(virtualId);
                    }
                }
            }


            var userItem = new UserItem(id, baseItem, extraData, thGroup, songCode, limno, limtot);


            if (UserHoldsItem(id))
                await RemoveItem(id, false, 0);

            if (userItem.BaseItem == null) return null;


            _items.TryAdd(userItem.Id, userItem);

            if (_mRemovedItems.Contains(userItem))
                _mRemovedItems.Remove(userItem);

            if (!_mAddedItems.Contains(id))
                _mAddedItems.Add(id);

            return userItem;
        }


        public async void AddNewItem(UserItem userItem)
        {
            var virtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(userItem.Id);
            await SendNewItems(virtualId);

            var id = userItem.Id;

            if (UserHoldsItem(id))
                await RemoveItem(id, false, 0);

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
        internal async Task RemoveItem(string id, bool placedInroom, uint roomId)
        {
            await GetClient()
                .GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("RemoveInventoryObjectMessageComposer"));

            var item = GetItem(id);
            if (item == null) return;

            await GetClient().GetMessageHandler().GetResponse().AppendIntegerAsync(item.VirtualId);

            await GetClient().GetMessageHandler().SendResponse();
            if (_mAddedItems.Contains(id))
                _mAddedItems.Remove(id);

            if (placedInroom)
            {
                item.RoomId = roomId;
            }

            _items?.TryRemove(item.Id, out _);

            _mRemovedItems?.Add(item);
        }

        /// <summary>
        ///     Serializes the floor item inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal async Task SerializeFloorItemInventory(GameClient session)
        {
            if (_items == null) return;
            var i = _items.Count;

            if (i <= 0)
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("LoadInventoryMessageComposer"));
                await message.AppendIntegerAsync(1);
                await message.AppendIntegerAsync(0);
                await message.AppendIntegerAsync(0);
                await session.SendMessage(message);
            }


            int totalSent = 0;

            if (i > 4500)
                await _mClient.SendStaticMessage(StaticMessage.AdviceMaxItems);


            using (var serverMessage = new ServerMessage())
            {
                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("LoadInventoryMessageComposer"));

                await serverMessage.AppendIntegerAsync(1);
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendIntegerAsync(i >= 4500 ? 4500 : i);

                foreach (var inventoryItem in _items.Values)
                {
                    if (inventoryItem != null)
                    {
                        if (totalSent == 4500) break;
                        totalSent++;

                        if (inventoryItem.IsWallItem)
                            await inventoryItem.SerializeWall(serverMessage, true);
                        else
                            await inventoryItem.SerializeFloor(serverMessage, true);
                    }
                }

                await _mClient.SendMessage(serverMessage);
            }
        }


        internal async Task AddItemToItemInventory(RoomItem item, bool dbUpdate)
        {
            var userItem = new UserItem(item.Id, item.BaseItem.ItemId, item.ExtraData, item.GroupId, item.SongCode,
                item.LimitedNo, item.LimitedTot);
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FurniListAddMessageComposer")))
            {
                await serverMessage.AppendIntegerAsync(item.VirtualId);
                await serverMessage.AppendStringAsync(item.GetBaseItem().Type.ToString().ToUpper());
                await serverMessage.AppendIntegerAsync(item.VirtualId);
                await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                await serverMessage.AppendIntegerAsync(1);
                await serverMessage.AppendIntegerAsync(0);
                if (item.LimitedNo > 0)
                {
                    await serverMessage.AppendIntegerAsync(1);
                    await serverMessage.AppendIntegerAsync(256);
                    await serverMessage.AppendStringAsync(item.ExtraData);
                    await serverMessage.AppendIntegerAsync(item.LimitedNo);
                    await serverMessage.AppendIntegerAsync(item.LimitedTot);
                }
                else
                {
                    await serverMessage.AppendStringAsync(item.ExtraData);
                }

                serverMessage.AppendBool(item.GetBaseItem().AllowRecycle);
                serverMessage.AppendBool(item.GetBaseItem().AllowTrade);
                serverMessage.AppendBool(item.LimitedNo == 0 && item.GetBaseItem().AllowInventoryStack);
                serverMessage.AppendBool(true); //can sell in marketplace xD
                await serverMessage.AppendIntegerAsync(-1);
                serverMessage.AppendBool(false);
                await serverMessage.AppendIntegerAsync(-1);
                if (!item.IsWallItem)
                {
                    await serverMessage.AppendStringAsync(string.Empty);
                    await serverMessage.AppendIntegerAsync(0);
                }

                var id = item.Id;
                if (UserHoldsItem(id))
                    await RemoveItem(id, false, 0);


                _items.TryAdd(userItem.Id, userItem);

                if (_mRemovedItems.Contains(userItem))
                    _mRemovedItems.Remove(userItem);

                if (!_mAddedItems.Contains(id))
                    _mAddedItems.Add(id);

                await _mClient.SendMessage(serverMessage);
            }
        }

        internal async Task AddItemToItemInventory(UserItem userItem)
        {
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FurniListAddMessageComposer")))
            {
                await serverMessage.AppendIntegerAsync(userItem.VirtualId);
                await serverMessage.AppendStringAsync(userItem.BaseItem.Type.ToString().ToUpper());
                await serverMessage.AppendIntegerAsync(userItem.VirtualId);
                await serverMessage.AppendIntegerAsync(userItem.BaseItem.SpriteId);
                await serverMessage.AppendIntegerAsync(1);
                await serverMessage.AppendIntegerAsync(0);
                if (userItem.LimitedSellId > 0)
                {
                    await serverMessage.AppendIntegerAsync(1);
                    await serverMessage.AppendIntegerAsync(256);
                    await serverMessage.AppendStringAsync(userItem.ExtraData);
                    await serverMessage.AppendIntegerAsync(userItem.LimitedSellId);
                    await serverMessage.AppendIntegerAsync(userItem.LimitedStack);
                }
                else
                {
                    await serverMessage.AppendStringAsync(userItem.ExtraData);
                }

                serverMessage.AppendBool(userItem.BaseItem.AllowRecycle);
                serverMessage.AppendBool(userItem.BaseItem.AllowTrade);
                serverMessage.AppendBool(userItem.LimitedSellId == 0 && userItem.BaseItem.AllowInventoryStack);
                serverMessage.AppendBool(true); //can sell in marketplace xD
                await serverMessage.AppendIntegerAsync(-1);
                serverMessage.AppendBool(false);
                await serverMessage.AppendIntegerAsync(-1);
                if (!userItem.IsWallItem)
                {
                    await serverMessage.AppendStringAsync(string.Empty);
                    await serverMessage.AppendIntegerAsync(0);
                }

                var id = userItem.Id;
                if (UserHoldsItem(id))
                    await RemoveItem(id, false, 0);


                _items.TryAdd(userItem.Id, userItem);

                if (_mRemovedItems.Contains(userItem))
                    _mRemovedItems.Remove(userItem);

                if (!_mAddedItems.Contains(id))
                    _mAddedItems.Add(id);

                await _mClient.SendMessage(serverMessage);
            }
        }


        /// <summary>
        ///     Serializes the pet inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializePetInventory()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PetInventoryMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            var list = _inventoryPets.Values;
            serverMessage.AppendInteger(list.Count);
            foreach (var current in list)
                serverMessage = await current.SerializeInventory(serverMessage);

            return serverMessage;
        }

        /// <summary>
        ///     Serializes the bot inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializeBotInventory()
        {
            var serverMessage = new ServerMessage();
            await serverMessage.InitAsync(LibraryParser.OutgoingRequest("BotInventoryMessageComposer"));

            var list = _inventoryBots.Values;
            await serverMessage.AppendIntegerAsync(list.Count);
            foreach (var current in list)
            {
                await serverMessage.AppendIntegerAsync(current.BotId);
                await serverMessage.AppendStringAsync(current.Name);
                await serverMessage.AppendStringAsync(current.Motto);
                await serverMessage.AppendStringAsync("m");
                await serverMessage.AppendStringAsync(current.Look);
            }

            return serverMessage;
        }


        /// <summary>
        ///     Adds the item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal async Task AddItem(RoomItem item) => await AddNewItem(item.Id, item.BaseItem.ItemId, item.ExtraData,
            item.GroupId,
            true,
            true, 0, 0, item.SongCode);

        /// <summary>
        ///     Runs the database update.
        /// </summary>
        internal async Task RunDbUpdate()
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

                    using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        await dbClient.RunFastQueryAsync(builder.ToString());
                    }

                    _mAddedItems.Clear();
                    added.Clear();
                }

                if (_mRemovedItems?.Count > 0)
                {
                    var removed = _mRemovedItems.ToList();

                    try
                    {
                        using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            foreach (var item in removed)
                            {
                                if (item.RoomId > 0)
                                {
                                    var room = Oblivion.GetGame().GetRoomManager().GetRoom(item.RoomId);
                                    if (room != null)
                                        await room.GetRoomItemHandler().SaveFurniture(queryReactor);
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
                            await queryChunk.AddParameter($"{current.PetId}name", current.Name);
                            await queryChunk.AddParameter($"{current.PetId}race", current.Race);
                            await queryChunk.AddParameter($"{current.PetId}color", current.Color);


                            var roomId = (current.RoomId <= 0) ? "NULL" : current.RoomId.ToString();
                            await queryChunk.AddQuery(string.Concat("UPDATE bots SET room_id = ", roomId, ", name = @",
                                current.PetId, "name, x = ", current.X, ", Y = ", current.Y, ", Z = ", current.Z,
                                " WHERE id = ", current.PetId));

                            await queryChunk.AddQuery(string.Concat("UPDATE pets_data SET race = @", current.PetId,
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
                        await queryChunk.Execute(queryreactor2);
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
        internal async Task SendNewItems(uint id)
        {
            using (var serverMessage = new ServerMessage())
            {
                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
                await serverMessage.AppendIntegerAsync(1);
                await serverMessage.AppendIntegerAsync(1);
                await serverMessage.AppendIntegerAsync(1);
                await serverMessage.AppendIntegerAsync(id);
                await _mClient.SendMessage(serverMessage);
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