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
        private Dictionary<long, UserItem> _items;

        /// <summary>
        ///     The _inventory bots
        /// </summary>
        private HybridDictionary _inventoryBots;

        /// <summary>
        ///     The _inventory pets
        /// </summary>
        private HybridDictionary _inventoryPets;

        /// <summary>
        ///     The _m added items
        /// </summary>
        private HashSet<long> _mAddedItems;

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
            _items = new Dictionary<long, UserItem>();
            SongDisks = new Dictionary<long, UserItem>();


            _inventoryPets = new HybridDictionary();
            _inventoryBots = new HybridDictionary();
            _mAddedItems = new HashSet<long>();
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
        internal Dictionary<long, UserItem> SongDisks { get; }


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
                _items.Clear();
                SongDisks.Clear();
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

                if (item == null || (!item.BaseItem.Name.StartsWith("DFD_") && !item.BaseItem.Name.StartsWith("CF_") &&
                                     !item.BaseItem.Name.StartsWith("CFC_")))
                    continue;

                var array = item.BaseItem.Name.Split('_');
                var num = int.Parse(array[1]);

                using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                    queryreactor2.RunFastQuery($"DELETE FROM items_rooms WHERE id={item.Id} LIMIT 1");


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
            RemovePet(petId);
        }

        /// <summary>
        ///     Adds the pet.
        /// </summary>
        /// <param name="pet">The pet.</param>
        internal void AddPet(Pet pet)
        {
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
            _items.Clear();

            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT id,base_item,extra_data,group_id,songcode,limited FROM items_rooms WHERE user_id=@userid AND room_id='0' LIMIT 4500;");
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

                var limitedString = dataRow["limited"].ToString().Split(';');

                //0 = stack , 1 = total
                var userItem = new UserItem(id, itemId, extraData, group, songCode, Convert.ToInt32(limitedString[0]),
                    Convert.ToInt32(limitedString[1]));

                if (userItem.BaseItem.InteractionType == Interaction.MusicDisc && !SongDisks.ContainsKey(id))
                    SongDisks.Add(id, userItem);
                if (!_items.ContainsKey(userItem.Id))
                    _items.Add(userItem.Id, userItem);
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

            _mClient.GetMessageHandler().GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

            _mClient.GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>UserItem.</returns>
        internal UserItem GetItem(long id) => _items.TryGetValue(id, out var item) ? item : null;

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
            if (bot == null || _inventoryBots.Contains(bot.BotId))
                return;

            bot.RoomId = 0u;

            _inventoryBots.Add(bot.BotId, bot);
        }

        internal void AddPets(Pet bot)
        {
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
        internal UserItem AddNewItem(long id, uint baseItem, string extraData, uint thGroup, bool insert, bool fromRoom,
            int limno, int limtot, string songCode = "")
        {
            if (insert)
            {
                if (fromRoom)
                {
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryReactor.RunFastQuery("UPDATE items_rooms SET user_id = '" + UserId +
                                                  "', room_id= '0' WHERE (id='" + id + "')");
                }
                else
                {
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery(
                            $"INSERT INTO items_rooms (base_item, user_id, group_id, extra_data, songcode, limited) VALUES ('{baseItem}', '{UserId}', '{thGroup}', @edata, '{songCode}', '{limno};{limtot}');");
                        queryReactor.AddParameter("edata", extraData);
                        if (id == 0)
                            id = queryReactor.InsertQuery();
                        var virtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(id);

                        SendNewItems(virtualId);
                    }
                }
            }

            if (id == 0)
                return null;

            var userItem = new UserItem(id, baseItem, extraData, thGroup, songCode, limno, limtot);


            if (UserHoldsItem(id))
                RemoveItem(id, false, 0);

            if (userItem.BaseItem == null) return null;

            if (userItem.BaseItem.InteractionType == Interaction.MusicDisc)
            {
                if (!SongDisks.ContainsKey(userItem.Id))
                    SongDisks.Add(userItem.Id, userItem);
            }


            _items.Add(userItem.Id, userItem);

            if (_mRemovedItems.Contains(userItem))
                _mRemovedItems.Remove(userItem);

            if (!_mAddedItems.Contains(id))
                _mAddedItems.Add(id);

            return userItem;
        }


        /// <summary>
        ///     Removes the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="placedInroom">if set to <c>true</c> [placed inroom].</param>
        internal void RemoveItem(long id, bool placedInroom, uint roomId)
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
            if (_mRemovedItems.Contains(item))
                return;

            SongDisks?.Remove(id);
            _items?.Remove(item.Id);
            _mRemovedItems?.Add(item);
        }

        /// <summary>
        ///     Serializes the floor item inventory.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeFloorItemInventory()
        {
            if (_items == null) return null;
            var items = _items.Values.ToList();
            var i = items.Count;

            if (i > 4500)
                _mClient.SendMessage(StaticMessage.AdviceMaxItems);

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadInventoryMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(i > 4500 ? 4500 : i);

            var inc = 0;

            foreach (var userItem in items)
            {
                if (userItem == null) continue;
                if (inc == 4500)
                    return serverMessage;

                inc++;

                if (userItem.IsWallItem)
                    userItem.SerializeWall(serverMessage, true);
                else
                    userItem.SerializeFloor(serverMessage, true);
            }
            return serverMessage;
        }


        internal void AddItemToItemInventory(RoomItem item, bool dbUpdate)
        {
            var userItem = new UserItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, item.SongCode,
                item.LimitedNo, item.LimitedTot);

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FurniListAddMessageComposer"));
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

            if (userItem.BaseItem.InteractionType == Interaction.MusicDisc)
            {
                if (!SongDisks.ContainsKey(userItem.Id))
                    SongDisks.Add(userItem.Id, userItem);
            }


            _items.Add(userItem.Id, userItem);

            if (_mRemovedItems.Contains(userItem))
                _mRemovedItems.Remove(userItem);

            if (!_mAddedItems.Contains(id))
                _mAddedItems.Add(id);

            _mClient.SendMessage(serverMessage);
        }

        internal void AddItemToItemInventory(UserItem userItem)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FurniListAddMessageComposer"));
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

            if (userItem.BaseItem.InteractionType == Interaction.MusicDisc)
            {
                if (!SongDisks.ContainsKey(userItem.Id))
                    SongDisks.Add(userItem.Id, userItem);
            }


            _items.Add(userItem.Id, userItem);

            if (_mRemovedItems.Contains(userItem))
                _mRemovedItems.Remove(userItem);

            if (!_mAddedItems.Contains(id))
                _mAddedItems.Add(id);

            _mClient.SendMessage(serverMessage);
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
            /* TODO CHECK */
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
            /* TODO CHECK */
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
        internal void AddItem(RoomItem item) => AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true,
            true, 0, 0, item.SongCode);

        /// <summary>
        ///     Runs the database update.
        /// </summary>
        internal void RunDbUpdate(bool fromRoom = false)
        {
            try
            {
                if (_mAddedItems?.Count > 0)
                {
                    var added = _mAddedItems.ToList();

                    using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        foreach (var itemId in added)
                            dbClient.RunFastQuery(
                                $"UPDATE items_rooms SET user_id='{UserId}', room_id='0' WHERE id='{itemId}'");
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
                                if (item.RoomId <= 0) continue;

                                if (!fromRoom)
                                {
                                    var room = Oblivion.GetGame().GetRoomManager().GetRoom(item.RoomId);
                                    room?.GetRoomItemHandler().SaveFurniture(queryReactor, null, true);
                                }

                                if (SongDisks.ContainsKey(item.Id))
                                    SongDisks.Remove(item.Id);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    removed.Clear();
                    _mRemovedItems.Clear();
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

                            queryChunk.AddQuery(string.Concat("UPDATE bots SET room_id = ", current.RoomId,
                                ", name = @",
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
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(id);
            _mClient.SendMessage(serverMessage);
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
                    foreach (var item in _items.Values.ToList())
                    {
                        item?.Dispose();
                    }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "inventory dispose");
            }
            SongDisks?.Clear();
            _items?.Clear();
            _items = null;
        }

        /// <summary>
        ///     Users the holds item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool UserHoldsItem(long itemId) => _items.ContainsKey(itemId);

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient GetClient() => Oblivion.GetGame().GetClientManager().GetClientByUserId(UserId);
    }
}