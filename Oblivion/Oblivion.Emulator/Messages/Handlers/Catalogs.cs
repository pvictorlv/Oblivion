using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Catalogs.Marketplace;
using Oblivion.HabboHotel.Catalogs.Wrappers;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.Messages.Enums;
using Oblivion.Messages.Parsers;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    ///     Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        ///     Catalogues the index.
        /// </summary>
        public void CatalogueMode()
        {
            var rank = Session.GetHabbo().Rank;

            if (rank < 1)
                rank = 1;
            Session.SendMessage(CatalogPageComposer.ComposeIndex(rank, Request.GetString().ToUpper(), Session));
            Session.SendMessage(StaticMessage.CatalogOffersConfiguration);
        }

        /// <summary>
        ///     Catalogues the index.
        /// </summary>
        public void CatalogueIndex()
        {
            var rank = Session.GetHabbo().Rank;

            if (rank < 1)
                rank = 1;
            Session.SendMessage(CatalogPageComposer.ComposeIndex(rank, "NORMAL", Session));
            Session.SendMessage(StaticMessage.CatalogOffersConfiguration);
        }

        /// <summary>
        ///     Catalogues the page.
        /// </summary>
        public void CataloguePage()
        {
            var pageId = Request.GetInteger();

            Request.GetInteger();

            var CataMode = Request.GetString();

            var cPage = Oblivion.GetGame().GetCatalog().GetPage(pageId);

            if (cPage == null || !cPage.Visible || cPage.MinRank > Session.GetHabbo().Rank)
                return;

            var message = CatalogPageComposer.ComposePage(cPage, CataMode);
            Session.SendMessage(message);
        }

        /// <summary>
        ///     Configure marketplace
        /// </summary>
        public void MarketPlaceConfiguration()
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("MarketplaceConfigurationMessageComposer"));
            message.AppendBool(true);
            message.AppendInteger(1);
            message.AppendInteger(0);
            message.AppendInteger(0);
            message.AppendInteger(1);
            message.AppendInteger(99999999);
            message.AppendInteger(48);
            message.AppendInteger(7);
            Session.SendMessage(message);
        }

        /// <summary>
        ///     Check if user can make offer
        /// </summary>
        public void CanMakeOffer()
        {
            var errorCode = Session.GetHabbo().TradeLockExpire > 0 ? 6 : 1;

            var message =
                new ServerMessage(LibraryParser.OutgoingRequest("MarketplaceCanMakeOfferResultMessageComposer"));
            message.AppendInteger(errorCode);
            message.AppendInteger(0);
            message.AppendInteger(0);
            Session.SendMessage(message);
        }

        /// <summary>
        ///     Catalogues the club page.
        /// </summary>
        public void CatalogueClubPage()
        {
            var requestType = Request.GetInteger();

            Session.SendMessage(CatalogPageComposer.ComposeClubPurchasePage(Session, requestType));
        }

        /// <summary>
        ///     Reloads the ecotron.
        /// </summary>
        public void ReloadEcotron()
        {
            Response.Init(LibraryParser.OutgoingRequest("ReloadEcotronMessageComposer"));
            Response.AppendInteger(1);
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        ///     Gifts the wrapping configuration.
        /// </summary>
        public void GiftWrappingConfig()
        {
            Response.Init(LibraryParser.OutgoingRequest("GiftWrappingConfigurationMessageComposer"));
            Response.AppendBool(true);
            Response.AppendInteger(1);
            Response.AppendInteger(GiftWrapper.GiftWrappersList.Count);

            foreach (var i in GiftWrapper.GiftWrappersList)
                Response.AppendInteger(i);

            Response.AppendInteger(8);

            for (var i = 0u; i != 8; i++)
                Response.AppendInteger(i);

            Response.AppendInteger(11);

            for (var i = 0u; i != 11; i++)
                Response.AppendInteger(i);

            Response.AppendInteger(GiftWrapper.OldGiftWrappers.Count);

            foreach (var i in GiftWrapper.OldGiftWrappers)
                Response.AppendInteger(i);

            SendResponse();
        }

        /// <summary>
        ///     Gets the recycler rewards.
        /// </summary>
        public void GetRecyclerRewards()
        {
            Response.Init(LibraryParser.OutgoingRequest("RecyclerRewardsMessageComposer"));

            var ecotronRewardsLevels = Oblivion.GetGame().GetCatalog().GetEcotronRewardsLevels();

            Response.AppendInteger(ecotronRewardsLevels.Count);

            foreach (var current in ecotronRewardsLevels)
            {
                Response.AppendInteger(current);
                Response.AppendInteger(current);

                var ecotronRewardsForLevel = Oblivion.GetGame().GetCatalog()
                    .GetEcotronRewardsForLevel(uint.Parse(current.ToString()));

                Response.AppendInteger(ecotronRewardsForLevel.Count);

                foreach (var current2 in ecotronRewardsForLevel)
                {
                    Response.AppendString(current2.GetBaseItem().PublicName);
                    Response.AppendInteger(1);
                    Response.AppendString(current2.GetBaseItem().Type.ToString());
                    Response.AppendInteger(current2.GetBaseItem().SpriteId);
                }
            }

            SendResponse();
        }

        /// <summary>
        ///     Purchases the item.
        /// </summary>
        public void PurchaseItem()
        {
            if (Session?.GetHabbo() == null)
                return;

            if (Session.GetHabbo().GetInventoryComponent().TotalItems >= 2799)
            {
                Session.SendMessage(CatalogPageComposer.PurchaseOk(0, string.Empty, 0));
                Session.SendMessage(StaticMessage.AdvicePurchaseMaxItems);
                return;
            }

            var pageId = Request.GetInteger();
            var itemId = Request.GetUInteger();
            var extraData = Request.GetString();
            var priceAmount = Request.GetInteger();
            Oblivion.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, priceAmount, false,
                string.Empty, string.Empty, 0, 0, 0, false, 0u);
        }

        /// <summary>
        ///     Purchases the gift.
        /// </summary>
        public void PurchaseGift()
        {
            var pageId = Request.GetInteger();
            var itemId = Request.GetUInteger();
            var extraData = Request.GetString();
            var giftUser = Request.GetString();
            var giftMessage = Request.GetString();
            var giftSpriteId = Request.GetInteger();
            var giftLazo = Request.GetInteger();
            var giftColor = Request.GetInteger();
            var undef = Request.GetBool();
            Oblivion.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, 1, true, giftUser,
                giftMessage, giftSpriteId, giftLazo, giftColor, undef, 0u);
        }

        /// <summary>
        ///     Checks the name of the pet.
        /// </summary>
        public void CheckPetName()
        {
            var petName = Request.GetString();
            var i = 0;

            if (petName.Length > 15)
                i = 1;
            else if (petName.Length < 3)
                i = 2;
            else if (!Oblivion.IsValidAlphaNumeric(petName))
                i = 3;

            Response.Init(LibraryParser.OutgoingRequest("CheckPetNameMessageComposer"));
            Response.AppendInteger(i);
            Response.AppendString(petName);
            SendResponse();
        }

        /// <summary>
        ///     Catalogues the offer.
        /// </summary>
        public void CatalogueOffer()
        {
            var num = Request.GetInteger();
            var catalogItem = Oblivion.GetGame().GetCatalog().GetItemFromOffer(num);

            if (catalogItem == null || CatalogManager.LastSentOffer == num)
                return;

            CatalogManager.LastSentOffer = num;

            var message = new ServerMessage(LibraryParser.OutgoingRequest("CatalogOfferMessageComposer"));

            CatalogPageComposer.ComposeItem(catalogItem, message);
            Session.SendMessage(message);
        }

        /// <summary>
        ///     Get marketplace offers
        /// </summary>
        public void GetOffers()
        {
            var MinCost = Request.GetInteger();
            var MaxCost = Request.GetInteger();
            var SearchQuery = Request.GetString();
            var FilterMode = Request.GetInteger();


            DataTable table;
            var builder = new StringBuilder();
            string str;
            builder.Append("WHERE `state` = '1' AND `timestamp` >= " +
                           Oblivion.GetGame().GetCatalog().GetMarketplace().FormatTimestampString());
            if (MinCost >= 0)
                builder.Append(" AND `total_price` > " + MinCost);


            if (MaxCost >= 0)
                builder.Append(" AND `total_price` < " + MaxCost);

            switch (FilterMode)
            {
                case 1:
                    str = "ORDER BY `asking_price` DESC";
                    break;

                default:
                    str = "ORDER BY `asking_price` ASC";
                    break;
            }

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `offer_id`, item_type, sprite_id, total_price, `limited_number`,`limited_stack` FROM catalog_marketplace_offers " +
                    builder + " " + str + " LIMIT 500");
                dbClient.AddParameter("search_query", "%" + SearchQuery + "%");
                if (SearchQuery.Length >= 1)
                    builder.Append(" AND public_name LIKE @search_query");
                table = dbClient.GetTable();
            }

            Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            if (table != null)
                foreach (var row in table.Rows.Cast<DataRow>().Where(row => !Oblivion.GetGame().GetCatalog()
                    .GetMarketplace().MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"]))))
                {
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItemKeys
                        .Add(Convert.ToInt32(row["offer_id"]));
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems.Add(new MarketOffer(
                        Convert.ToInt32(row["offer_id"]),
                        Convert.ToInt32(row["sprite_id"]), Convert.ToInt32(row["total_price"]),
                        int.Parse(row["item_type"].ToString()), Convert.ToInt32(row["limited_number"]),
                        Convert.ToInt32(row["limited_stack"])));
                }

            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();

            foreach (var item in Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems)
                if (dictionary.ContainsKey(item.SpriteId))
                {
                    if (item.LimitedNumber > 0)
                    {
                        if (!dictionary.ContainsKey(item.OfferId))
                            dictionary.Add(item.OfferId, item);
                        if (!dictionary2.ContainsKey(item.OfferId))
                            dictionary2.Add(item.OfferId, 1);
                    }
                    else
                    {
                        if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice)
                        {
                            dictionary.Remove(item.SpriteId);
                            dictionary.Add(item.SpriteId, item);
                        }

                        var num = dictionary2[item.SpriteId];
                        dictionary2.Remove(item.SpriteId);
                        dictionary2.Add(item.SpriteId, num + 1);
                    }
                }
                else
                {
                    if (!dictionary.ContainsKey(item.SpriteId))
                        dictionary.Add(item.SpriteId, item);
                    if (!dictionary2.ContainsKey(item.SpriteId))
                        dictionary2.Add(item.SpriteId, 1);
                }
            var message = new ServerMessage(LibraryParser.OutgoingRequest("MarketPlaceOffersMessageComposer"));
            message.AppendInteger(dictionary.Count);
            foreach (var pair in dictionary.Values.Where(x => x.TotalPrice >= MinCost && x.TotalPrice <= MaxCost))
            {
                message.AppendInteger(pair.OfferId);
                message.AppendInteger(1);
                message.AppendInteger(1);
                message.AppendInteger(pair.SpriteId);
                message.AppendInteger(256);
                message.AppendString("");
                message.AppendInteger(pair.LimitedNumber);
                message.AppendInteger(pair.LimitedStack);
                message.AppendInteger(pair.TotalPrice);
                message.AppendInteger(0);
                message.AppendInteger(Oblivion.GetGame().GetCatalog().GetMarketplace()
                    .AvgPriceForSprite(pair.SpriteId));
                message.AppendInteger(dictionary2[pair.SpriteId]);
            }
            message.AppendInteger(dictionary.Count);
        }

        /// <summary>
        ///     Catalogues the offer configuration.
        /// </summary>
        public void CatalogueOfferConfig()
        {
            Response.Init(LibraryParser.OutgoingRequest("CatalogueOfferConfigMessageComposer"));
            Response.AppendInteger(100);
            Response.AppendInteger(6);
            Response.AppendInteger(1);
            Response.AppendInteger(1);
            Response.AppendInteger(2);
            Response.AppendInteger(40);
            Response.AppendInteger(99);
            SendResponse();
        }

        /// <summary>
        ///     Serializes the group furni page.
        /// </summary>
        internal void SerializeGroupFurniPage()
        {
            var userGroups = Oblivion.GetGame().GetGroupManager().GetUserGroups(Session.GetHabbo().Id);

            Response.Init(LibraryParser.OutgoingRequest("GroupFurniturePageMessageComposer"));

            var responseList = new List<ServerMessage>();

            foreach (var habboGroup in userGroups.Where(current => current != null)
                .Select(current => Oblivion.GetGame().GetGroupManager().GetGroup(current.GroupId)))
            {
                if (habboGroup == null)
                    continue;

                var subResponse = new ServerMessage();
                subResponse.AppendInteger(habboGroup.Id);
                subResponse.AppendString(habboGroup.Name);
                subResponse.AppendString(habboGroup.Badge);
                subResponse.AppendString(Oblivion.GetGame().GetGroupManager().SymbolColours.Contains(habboGroup.Colour1)
                    ? ((GroupSymbolColours)
                        Oblivion.GetGame().GetGroupManager().SymbolColours[habboGroup.Colour1]).Colour
                    : "4f8a00");
                subResponse.AppendString(
                    Oblivion.GetGame().GetGroupManager().BackGroundColours.Contains(habboGroup.Colour2)
                        ? ((GroupBackGroundColours)
                            Oblivion.GetGame().GetGroupManager().BackGroundColours[habboGroup.Colour2]).Colour
                        : "4f8a00");
                subResponse.AppendBool(habboGroup.CreatorId == Session.GetHabbo().Id);
                subResponse.AppendInteger(habboGroup.CreatorId);
                subResponse.AppendBool(habboGroup.HasForum);

                responseList.Add(subResponse);
            }

            Response.AppendInteger(responseList.Count);
            Response.AppendServerMessages(responseList);

            responseList.Clear();

            SendResponse();
        }
    }
}