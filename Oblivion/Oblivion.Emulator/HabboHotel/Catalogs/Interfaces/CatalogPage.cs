using System.Collections.Generic;
using System.Linq;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Catalogs.Interfaces
{
    /// <summary>
    ///     Class CatalogPage.
    /// </summary>
    internal class CatalogPage
    {

        /// <summary>
        ///     The caption
        /// </summary>
        internal string Caption;

        /// <summary>
        ///     The code name
        /// </summary>
        internal string CodeName;

        /// <summary>
        ///     The coming soon
        /// </summary>
        internal bool ComingSoon;

        /// <summary>
        ///     The enabled
        /// </summary>
        internal bool Enabled;

        /// <summary>
        ///     The flat offers
        /// </summary>
        internal Dictionary<int, uint> FlatOffers;

        /// <summary>
        ///     The icon image
        /// </summary>
        internal int IconImage;

        /// <summary>
        ///     The items
        /// </summary>
        internal Dictionary<uint, CatalogItem> Items;

        /// <summary>
        ///     The layout
        /// </summary>
        internal string Layout;


        /// <summary>
        ///     The minimum rank
        /// </summary>
        internal uint MinRank;

        /// <summary>
        ///     The order number
        /// </summary>
        internal int OrderNum;


        /// <summary>
        ///     The parent identifier
        /// </summary>
        internal int ParentId;

        internal ServerMessage PageMessage;
        /// <summary>
        ///     The visible
        /// </summary>
        internal bool Visible;

        internal List<string> PageString1;
        internal List<string> PageString2;
        /// <summary>
        ///     Initializes a new instance of the <see cref="CatalogPage" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="codeName">Name of the code.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="comingSoon">if set to <c>true</c> [coming soon].</param>
        /// <param name="minRank">The minimum rank.</param>
        /// <param name="iconImage">The icon image.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="orderNum">The order number.</param>
        /// <param name="cataItems">The cata items.</param>
        internal CatalogPage(uint id, int parentId, string codeName, string caption, bool visible, bool enabled,
            bool comingSoon, uint minRank, int iconImage, string layout, string strings1, string strings2,
            int orderNum, ref Dictionary<uint, CatalogItem> cataItems)
        {
            PageId = id;
            ParentId = parentId;
            CodeName = codeName;
            Caption = caption;
            Visible = visible;
            Enabled = enabled;
            ComingSoon = comingSoon;
            MinRank = minRank;
            IconImage = iconImage;
            Layout = layout;
            PageString1 = strings1.Split('|').ToList();
            PageString2 = strings2.Split('|').ToList();
            OrderNum = orderNum;
            if (layout.StartsWith("frontpage"))
                OrderNum = -2;

            Items = new Dictionary<uint, CatalogItem>();
            FlatOffers = new Dictionary<int, uint>();
            /* TODO CHECK */ foreach (
                var catalogItem in
                    cataItems.Values.Where(x => x.PageId == id && x.GetFirstBaseItem() != null))
            {
                Items.Add(catalogItem.Id, catalogItem);
                var flatId = catalogItem.GetFirstBaseItem().FlatId;
                if (flatId != -1 && !FlatOffers.ContainsKey(flatId))
                    FlatOffers.Add(catalogItem.GetFirstBaseItem().FlatId, catalogItem.Id);
            }

//            CachedContentsMessage = CatalogPageComposer.ComposePage(this);
        }

        /// <summary>
        ///     Gets the page identifier.
        /// </summary>
        /// <value>The page identifier.</value>
        internal uint PageId { get; }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="pId">The p identifier.</param>
        /// <returns>CatalogItem.</returns>
        internal CatalogItem GetItem(uint pId)
        {
            var num = pId;
            var flatInt = (int)pId;
            if (FlatOffers.TryGetValue(flatInt, out var flatOff))
                return Items[flatOff];
            return Items.TryGetValue(num, out CatalogItem it) ? it : null;
        }
    }
}