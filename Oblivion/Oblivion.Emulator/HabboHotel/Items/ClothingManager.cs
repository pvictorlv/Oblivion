using System.Collections.Generic;
using System.Data;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items
{
    /// <summary>
    ///     Class ClothingManager.
    /// </summary>
    internal class ClothingManager
    {
        /// <summary>
        ///     The _table
        /// </summary>
        private DataTable _table;

        /// <summary>
        ///     The clothing items
        /// </summary>
        internal Dictionary<string, ClothingItem> ClothingItems;

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal async Task Initialize(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM catalog_clothing");
            ClothingItems = new Dictionary<string, ClothingItem>();
            _table = dbClient.GetTable();

            /* TODO CHECK */ foreach (DataRow dataRow in _table.Rows)
                ClothingItems.Add((string)dataRow["item_name"], new ClothingItem(dataRow));
        }

        /// <summary>
        ///     Gets the clothes in furni.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>ClothingItem.</returns>
        internal ClothingItem GetClothesInFurni(string name)
        {
            ClothingItem clothe;
            ClothingItems.TryGetValue(name, out clothe);

            return clothe;
        }
    }
}