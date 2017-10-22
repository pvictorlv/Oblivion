using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
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
        internal void LoadItems(IQueryAdapter dbClient, out uint itemLoaded)
        {
            LoadItems(dbClient);
            itemLoaded = (uint) _items.Count;
        }

        public int CountItems() => _items.Count;

        /// <summary>
        ///     Loads the items.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadItems(IQueryAdapter dbClient)
        {
            _items.Clear();

            GiftWrapper.Clear();

            dbClient.SetQuery("SELECT * FROM furniture");

            var table = dbClient.GetTable();
            if (table == null) return;

            List<double> heights = null;

            /* TODO CHECK */ foreach (DataRow dataRow in table.Rows)
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
                    var effect = (int) dataRow["effectid"];
                    var stackable = Convert.ToInt32(dataRow["can_stack"]) == 1;
                    var allowRecycle = Convert.ToInt32(dataRow["allow_recycle"]) == 1;
                    var allowTrade = Convert.ToInt32(dataRow["allow_trade"]) == 1;
                    var allowMarketplaceSell = Convert.ToInt32(dataRow["allow_marketplace_sell"]) == 1;
                    var allowGift = Convert.ToInt32(dataRow["allow_gift"]) == 1;
                    var allowInventoryStack = Convert.ToInt32(dataRow["allow_inventory_stack"]) == 1;
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
                        typeFromString, modes, vendingIds, sub, effect, stackMultiple,
                        heights?.ToArray(), flatId, isRare);

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

        internal Item GetItem(uint id)
        {
            return _items.TryGetValue(id, out Item it) ? it : null;
        }

        internal bool GetItem(string itemName, out Item item)
        {
            /* TODO CHECK */ foreach (var entry in _items)
            {
                item = entry.Value;

                if (item.Name == itemName)
                    return true;
            }

            item = null;

            return false;
        }

        internal bool GetItem(uint id, out Item item)
        {
            if (_items.ContainsKey(id))
                return _items.TryGetValue(id, out item);
            item = null;
            return false;
        }

        internal Item GetItemByName(string name) => _items.Values.FirstOrDefault(item => item.Name == name);

        internal Item GetItemBySpriteId(int spriteId) => _items.Values.FirstOrDefault(item => item.SpriteId == spriteId);

        /// <summary>
        ///     Determines whether the specified identifier contains item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if the specified identifier contains item; otherwise, <c>false</c>.</returns>
        internal bool ContainsItem(uint id) => _items.ContainsKey(id);
    }
}