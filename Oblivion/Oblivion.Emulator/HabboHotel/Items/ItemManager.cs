using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Catalogs.Wrappers;
using Oblivion.HabboHotel.Items.Interactions;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items
{
    /// <summary>
    ///     Class ItemManager.
    /// </summary>
    internal class ItemManager
    {
        /// <summary>
        ///     The items
        /// </summary>
        private readonly Dictionary<uint, Item> _items;

        private int _itemIdCounter = 1;

        private ConcurrentDictionary<uint, string> _itemsByVirtualId;
        private ConcurrentDictionary<string, uint> _itemsByRealId;
//        private ConcurrentList<uint> _virtualAddedItems;

        /// <summary>
        ///     The photo identifier
        /// </summary>
        internal uint PhotoId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemManager" /> class.
        /// </summary>
        internal ItemManager() => _items = new Dictionary<uint, Item>();


        
        /// <summary>
        ///     Loads the items.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="itemLoaded">The item loaded.</param>
        internal async Task LoadItems(IQueryAdapter dbClient, out uint itemLoaded)
        {
            LoadItems(dbClient);
            itemLoaded = (uint) _items.Count;
            _itemsByVirtualId = new ConcurrentDictionary<uint, string>();
            _itemsByRealId = new ConcurrentDictionary<string, uint>();
//            _virtualAddedItems = new ConcurrentList<uint>();
        }

     
        public uint GetVirtualId(string realId)
        {
            if (_itemsByRealId.TryGetValue(realId, out var virtualId))
            {
                return virtualId;
            }

            Interlocked.Increment(ref _itemIdCounter);

            var newId = Convert.ToUInt32(_itemIdCounter);
            _itemsByRealId.TryAdd(realId, newId);
            _itemsByVirtualId.TryAdd(newId, realId);

            return newId;
        }

        public void ResetVirtualIds()
        {
            var obj = new object();
            lock (obj)
            {
                _itemIdCounter = _itemsByVirtualId.Count;
            }
        }
        public void RemoveVirtualItem(string itemId)
        {
            _itemsByRealId.TryRemove(itemId, out var virtualId);
            _itemsByVirtualId.TryRemove(virtualId, out _);
        }

        public string GetRealId(uint virtualId)
        {
            if (_itemsByVirtualId.TryGetValue(virtualId, out var realId))
            {
                return realId;
            }
            

            return "";
        }

        public int CountItems() => _items.Count;

        /// <summary>
        ///     Loads the items.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal async Task LoadItems(IQueryAdapter dbClient)
        {
            _items.Clear();

            GiftWrapper.Clear();

            dbClient.SetQuery("SELECT * FROM furniture");

            var table = dbClient.GetTable();
            if (table == null) return;

            List<double> heights = null;

            /* TODO CHECK */
            foreach (DataRow dataRow in table.Rows)
            {
                try
                {
                    var id = Convert.ToUInt32(dataRow["id"]);
                    var type = Convert.ToChar(dataRow["type"]);
                    var name = Convert.ToString(dataRow["item_name"]);
                    var flatId = Convert.ToInt32(dataRow["flat_id"]);
                    var stackHeightStr = dataRow["stack_height"].ToString();
                    var heightAdjustable = dataRow["height_adjustable"].ToString();
                    double stackHeight;
                    uint.TryParse(dataRow["interaction_modes_count"].ToString(), out var modes);
                    var vendingIds = (string) dataRow["vending_ids"];
                    var sub = Oblivion.EnumToBool(dataRow["subscriber"].ToString());
                    var effectF = (int) dataRow["effectF"];
                    var effectM = (int) dataRow["effectM"];
                    var stackable = Convert.ToInt32(dataRow["can_stack"]) == 1;
                    var allowRecycle = Convert.ToInt32(dataRow["allow_recycle"]) == 1;
                    var allowTrade = Convert.ToInt32(dataRow["allow_trade"]) == 1;
                    var allowMarketplaceSell = Convert.ToInt32(dataRow["allow_marketplace_sell"]) == 1;
                    var allowGift = Convert.ToInt32(dataRow["allow_gift"]) == 1;
                    var allowInventoryStack = dataRow["allow_inventory_stack"].ToString() == "1";
                    var typeFromString = InteractionTypes.GetTypeFromString((string) dataRow["interaction_type"]);

                    var sprite = Convert.ToInt32(dataRow["sprite_id"]);
                    var isRare = Oblivion.EnumToBool(dataRow["is_rare"].ToString());

                    ushort x = Convert.ToUInt16(dataRow["width"]), y = Convert.ToUInt16(dataRow["length"]);
                    var publicName = Convert.ToString(dataRow["public_name"]);
                    bool canWalk = Oblivion.EnumToBool(dataRow["is_walkable"].ToString()),
                        canSit = Oblivion.EnumToBool(dataRow["can_sit"].ToString()),
                        stackMultiple = false;

                    if (name.StartsWith("external_image_wallitem_poster")) PhotoId = id;

                    if (name.StartsWith("present_gen"))
                        GiftWrapper.AddOld(sprite);
                    else if (name.StartsWith("present_wrap*"))
                        GiftWrapper.Add(sprite);

                    if (heightAdjustable.Contains(','))
                    {
                        var heightsStr = heightAdjustable.Split(',');

                        heights = heightsStr.Select(heightStr => double.Parse(heightStr, CultureInfo.InvariantCulture))
                            .ToList();

                        stackHeight = heights[0];
                        stackMultiple = true;
                    }
                    else
                        stackHeight = double.Parse(stackHeightStr, CultureInfo.InvariantCulture);

                    // If Can Walk
                    if (InteractionTypes.AreFamiliar(GlobalInteractions.Gate, typeFromString) ||
                        (typeFromString == Interaction.BanzaiPyramid) || (name.StartsWith("hole")))
                        canWalk = false;

                    // Add Item
                    var value = new Item(id, sprite, publicName, name, type, x, y, stackHeight, stackable, canWalk,
                        canSit, allowRecycle, allowTrade, allowMarketplaceSell, allowGift, allowInventoryStack,
                        typeFromString, modes, vendingIds, sub, stackMultiple,
                        heights?.ToArray(), flatId, isRare, effectF, effectM);

                    _items.Add(id, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Out.WriteLine(
                        $"Could not load item #{Convert.ToUInt32(dataRow[0])}, please verify the data is okay.",
                        "Oblivion.Items", ConsoleColor.DarkRed);
                }
            }
        }

        internal Item GetItem(uint id) => _items.TryGetValue(id, out var it) ? it : null;

        internal bool GetItem(string itemName, out Item item)
        {
            /* TODO CHECK */
            foreach (var entry in _items)
            {
                item = entry.Value;

                if (item.Name == itemName)
                    return true;
            }

            item = null;

            return false;
        }

        internal bool GetItem(uint id, out Item item) => _items.TryGetValue(id, out item);

        internal Item GetItemByName(string name) => _items.Values.FirstOrDefault(item => item.Name == name);

        internal Item GetItemBySpriteId(int spriteId) =>
            _items.Values.FirstOrDefault(item => item.SpriteId == spriteId);

        /// <summary>
        ///     Determines whether the specified identifier contains item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if the specified identifier contains item; otherwise, <c>false</c>.</returns>
        internal bool ContainsItem(uint id) => _items.ContainsKey(id);
    }
}