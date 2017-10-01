using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Catalogs.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Catalogs.Composers
{
    /// <summary>
    ///     Class CatalogPacket.
    /// </summary>
    internal static class CatalogPageComposer
    {
        /// <summary>
        ///     Composes the index.
        /// </summary>
        /// <param name="rank">The rank.</param>
        /// <param name="type">The type.</param>
        /// <param name="session">The user.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage ComposeIndex(uint rank, string type, GameClient session)
        {
            var pages =
                Oblivion.GetGame().GetCatalog().Categories.Values.OfType<CatalogPage>().ToList();

            var sortedPages = pages.Where(x => x.ParentId == -2 && x.MinRank <= rank).OrderBy(x => x.OrderNum);

            if (type == "NORMAL")
                sortedPages = pages.Where(x => x.ParentId == -1 && x.MinRank <= rank).OrderBy(x => x.OrderNum);

            var message = new ServerMessage(LibraryParser.OutgoingRequest("CatalogueIndexMessageComposer"));

            message.AppendBool(true);
            message.AppendInteger(0);
            message.AppendInteger(-1);
            message.AppendString("root");
            message.AppendString(string.Empty);
            message.AppendInteger(0);
            message.AppendInteger(CalcTreeSize(session, pages, -1));

            foreach (var cat in sortedPages)
            {
                message.AppendBool(cat.Visible);
                message.AppendInteger(cat.IconImage);
                message.AppendInteger(cat.PageId);
                message.AppendString(cat.CodeName);
                message.AppendString(cat.Caption);
                message.AppendInteger(cat.FlatOffers.Count);

                foreach (var i in cat.FlatOffers.Keys)
                    message.AppendInteger(i);
                message.AppendInteger(CalcTreeSize(session, pages, (int) cat.PageId));


                var sortedSubPages =
                    pages.Where(x => x.ParentId == cat.PageId && x.MinRank <= rank).OrderBy(x => x.OrderNum);

                foreach (var child in sortedSubPages)
                {
                    message.AppendBool(child.Visible);
                    message.AppendInteger(child.IconImage);
                    message.AppendInteger(child.PageId);
                    message.AppendString(child.CodeName);
                    message.AppendString(child.Caption);
                    message.AppendInteger(child.FlatOffers.Count);

                    foreach (var i2 in child.FlatOffers.Keys)
                        message.AppendInteger(i2);

                    message.AppendInteger(CalcTreeSize(session, pages, (int) child.PageId));
                    foreach (var subCat in pages.Where(baby => baby.ParentId == child.PageId && baby.MinRank <= rank))
                    {
                        message.AppendBool(subCat.Visible);
                        message.AppendInteger(subCat.IconImage);
                        message.AppendInteger(subCat.PageId);
                        message.AppendString(subCat.CodeName);
                        message.AppendString(subCat.Caption);
                        message.AppendInteger(subCat.FlatOffers.Count);

                        foreach (var i2 in subCat.FlatOffers.Keys)
                            message.AppendInteger(i2);

                        message.AppendInteger(0);
                    }
                }

            }

            message.AppendBool(false);
            message.AppendString(type);

            return message;
        }

        private static int CalcTreeSize(GameClient Session, IEnumerable<CatalogPage> Pages, int ParentId)
            =>
                Pages.Where(Page => Page.MinRank <= Session.GetHabbo().Rank && Page.ParentId == ParentId)
                    .Count(Page => Page.ParentId == ParentId);

        /// <summary>
        ///     Composes the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage ComposePage(CatalogPage page, string mode)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("CataloguePageMessageComposer"));
            message.AppendInteger(page.PageId);
            message.AppendString(mode);
            message.AppendString(page.Layout);
            message.AppendInteger(page.PageString1.Count);
            foreach (var str in page.PageString1)
            {
                message.AppendString(str);
            }
            message.AppendInteger(page.PageString2.Count);
            foreach (var str in page.PageString2)
            {
                message.AppendString(str);
            }
            if (!page.Layout.Equals("frontpage") && !page.Layout.Equals("frontpage") && !page.Layout.Equals("recycler"))
            {
                message.AppendInteger(page.Items.Count);
                foreach (CatalogItem item in page.Items.Values)
                    ComposeItem(item, message);
            }
            else
            {
                message.AppendInteger(0);
            }

            message.AppendInteger(-1);
            message.AppendBool(false);

            if (page.Layout.Equals("frontpage4"))
            {
                List<DataRow> list = Oblivion.GetGame().GetCatalog().IndexText;
                message.AppendInteger(list.Count); // count
                foreach (var Catalog in list)
                {
                    message.AppendInteger(1); // id
                    message.AppendString(Convert.ToString(Catalog["title"])); // name
                    message.AppendString(Convert.ToString(Catalog["image"])); // image
                    message.AppendInteger(0);
                    message.AppendString(Convert.ToString(Catalog["page_link"])); // page link?
                    message.AppendInteger(Convert.ToInt32(Catalog["page_id"])); // page id?
                }
            }
            return message;
        }

        /// <summary>
        ///     Composes the club purchase page.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="windowId">The window identifier.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage ComposeClubPurchasePage(GameClient session, int windowId)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("CatalogueClubPageMessageComposer"));
            var habboClubItems = Oblivion.GetGame().GetCatalog().HabboClubItems;

            message.AppendInteger(habboClubItems.Count);

            foreach (var item in habboClubItems)
            {
                message.AppendInteger(item.Id);
                message.AppendString(item.Name);
                message.AppendBool(false);
                message.AppendInteger(item.CreditsCost);

                if (item.DiamondsCost > 0)
                {
                    message.AppendInteger(item.DiamondsCost);
                    message.AppendInteger(105);
                }
                else
                {
                    message.AppendInteger(item.DucketsCost);
                    message.AppendInteger(0);
                }

                message.AppendBool(true);
                var fuckingArray = item.Name.Split('_'); //HABBO_CLUB_VIP_1_MONTH
                double dayTime = 31;
                if (item.Name.Contains("DAY"))
                    dayTime = int.Parse(fuckingArray[3]);
                else if (item.Name.Contains("MONTH"))
                {
                    var monthTime = int.Parse(fuckingArray[3]);
                    dayTime = monthTime * 31;
                }
                else if (item.Name.Contains("YEAR"))
                {
                    var yearTimeOmg = int.Parse(fuckingArray[3]);
                    dayTime = yearTimeOmg * 31 * 12;
                }

                var newExpiryDate = DateTime.Now.AddDays(dayTime);

                if (session.GetHabbo().GetSubscriptionManager().HasSubscription)
                    newExpiryDate =
                        Oblivion.UnixToDateTime(session.GetHabbo().GetSubscriptionManager().GetSubscription().ExpireTime)
                            .AddDays(dayTime);
                message.AppendInteger((int)dayTime / 31);
                message.AppendInteger((int)dayTime);
                message.AppendBool(false);
                message.AppendInteger((int)dayTime);
                message.AppendInteger(newExpiryDate.Year);
                message.AppendInteger(newExpiryDate.Month);
                message.AppendInteger(newExpiryDate.Day);
            }

            message.AppendInteger(windowId);
            return message;
        }

        internal static ServerMessage PurchaseOk(CatalogItem itemCatalog, Dictionary<Item, uint> items,
            int clubLevel = 1) => PurchaseOk(itemCatalog.Id, itemCatalog.Name, itemCatalog.CreditsCost, items, clubLevel,
            itemCatalog.DiamondsCost,
            itemCatalog.DucketsCost, itemCatalog.IsLimited, itemCatalog.LimitedStack, itemCatalog.LimitedSelled);

        /// <summary>
        ///     Purchases the ok.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage PurchaseOk(uint itemId, string itemName, uint creditsCost,
            Dictionary<Item, uint> items = null, int clubLevel = 1,
            uint diamondsCost = 0,
            uint activityPointsCost = 0, bool isLimited = false,
            int limitedStack = 0,
            int limitedSelled = 0)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("PurchaseOKMessageComposer"));
            message.AppendInteger(itemId);
            message.AppendString(itemName);
            message.AppendBool(false);
            message.AppendInteger(creditsCost);
            message.AppendInteger(diamondsCost);
            message.AppendInteger(activityPointsCost);
            message.AppendBool(true);
            message.AppendInteger(items?.Count ?? 0);

            if (items != null)
            {
                foreach (var itemDic in items)
                {
                    var item = itemDic.Key;
                    message.AppendString(item.Type.ToString());

                    if (item.Type == 'b')
                    {
                        message.AppendString(item.PublicName);
                        continue;
                    }

                    message.AppendInteger(item.SpriteId);
                    message.AppendString(item.PublicName);
                    message.AppendInteger(itemDic.Value); //productCount
                    message.AppendBool(isLimited);

                    if (!isLimited) continue;
                    message.AppendInteger(limitedStack);
                    message.AppendInteger(limitedSelled);
                }
            }

            message.AppendInteger(clubLevel); //clubLevel
            message.AppendBool(false); //window.visible?
            return message;
        }

        /// <summary>
        ///     Composes the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="message">The message.</param>
        internal static void ComposeItem(CatalogItem item, ServerMessage message)
        {
            message.AppendInteger(item.Id);
            message.AppendString(item.Name, true);
            message.AppendBool(false);
            message.AppendInteger(item.CreditsCost);

            if (item.DiamondsCost > 0)
            {
                message.AppendInteger(item.DiamondsCost);
                message.AppendInteger(105);
            }
            else
            {
                message.AppendInteger(item.DucketsCost);
                message.AppendInteger(0);
            }

            message.AppendBool(item.GetFirstBaseItem().AllowGift);

            switch (item.Name)
            {
                case "g0 group_product":
                    message.AppendInteger(0);
                    break;

                case "room_ad_plus_badge":
                    message.AppendInteger(1);
                    message.AppendString("b");
                    message.AppendString("RADZZ");
                    break;

                default:
                    if (item.Name.StartsWith("builders_club_addon_") || item.Name.StartsWith("builders_club_time_"))
                        message.AppendInteger(0);
                    else if (item.Badge == "")
                        message.AppendInteger(item.Items.Count);
                    else
                    {
                        message.AppendInteger(item.Items.Count + 1);
                        message.AppendString("b");
                        message.AppendString(item.Badge);
                    }
                    break;
            }
            foreach (var baseItem in item.Items.Keys)
            {
                if (item.Name == "g0 group_product" || item.Name.StartsWith("builders_club_addon_") ||
                    item.Name.StartsWith("builders_club_time_"))
                    break;
                if (item.Name == "room_ad_plus_badge")
                {
                    message.AppendString("");
                    message.AppendInteger(0);
                }
                else
                {
                    message.AppendString(baseItem.Type.ToString());
                    message.AppendInteger(baseItem.SpriteId);

                    if (item.Name.Contains("wallpaper_single") || item.Name.Contains("floor_single") ||
                        item.Name.Contains("landscape_single"))
                    {
                        var array = item.Name.Split('_');
                        message.AppendString(array[2]);
                    }
                    else if (item.Name.StartsWith("bot_") || baseItem.InteractionType == Interaction.MusicDisc ||
                             item.GetFirstBaseItem().Name == "poster")
                        message.AppendString(item.ExtraData);
                    else if (item.Name.StartsWith("poster_"))
                    {
                        var array2 = item.Name.Split('_');
                        message.AppendString(array2[1]);
                    }
                    else if (item.Name.StartsWith("poster "))
                    {
                        var array3 = item.Name.Split(' ');
                        message.AppendString(array3[1]);
                    }
                    else if (item.SongId > 0u && baseItem.InteractionType == Interaction.MusicDisc)
                        message.AppendString(item.ExtraData);
                    else
                        message.AppendString(item.ExtraData ?? string.Empty);

                    message.AppendInteger(item.Items[baseItem]);
                    message.AppendBool(item.IsLimited);
                    if (item.IsLimited)
                    {
                        message.AppendInteger(item.LimitedStack);
                        message.AppendInteger(item.LimitedStack - item.LimitedSelled);
                    }
                }
            }
            message.AppendInteger(item.ClubOnly ? 1 : 0);

            if (item.IsLimited || item.FirstAmount != 1)
            {
                message.AppendBool(false);
            }
            else
            {
                message.AppendBool(true);
            }

            message.AppendBool(item.HaveOffer && !item.IsLimited);
            message.AppendString("");
        }
    }
}