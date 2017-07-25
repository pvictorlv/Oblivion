using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Catalogs.Wrappers;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.Messages.Enums;
using Oblivion.Messages.Parsers;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Catalogues the index.
        /// </summary>
        public void CatalogueIndex()
        {
            var rank = Session.GetHabbo().Rank;

            if (rank < 1)
                rank = 1;

            Session.SendMessage(StaticMessage.CatalogOffersConfiguration);
            Session.SendMessage(CatalogPageComposer.ComposeIndex(rank, Request.GetString().ToUpper()));
        }

        /// <summary>
        /// Catalogues the page.
        /// </summary>
        public void CataloguePage()
        {
            var pageId = Request.GetInteger();

            Request.GetInteger();

            var cPage = Oblivion.GetGame().GetCatalog().GetPage(pageId);

            if (cPage == null || !cPage.Enabled || !cPage.Visible || cPage.MinRank > Session.GetHabbo().Rank)
                return;

            Session.SendMessage(cPage.CachedContentsMessage);
        }

        /// <summary>
        /// Catalogues the club page.
        /// </summary>
        public void CatalogueClubPage()
        {
            var requestType = Request.GetInteger();

            Session.SendMessage(CatalogPageComposer.ComposeClubPurchasePage(Session, requestType));
        }

        /// <summary>
        /// Reloads the ecotron.
        /// </summary>
        public void ReloadEcotron()
        {
            Response.Init(LibraryParser.OutgoingRequest("ReloadEcotronMessageComposer"));
            Response.AppendInteger(1);
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Gifts the wrapping configuration.
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
        /// Gets the recycler rewards.
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

                var ecotronRewardsForLevel = Oblivion.GetGame().GetCatalog().GetEcotronRewardsForLevel(uint.Parse(current.ToString()));

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
        /// Purchases the item.
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

            int pageId = Request.GetInteger();
            uint itemId = Request.GetUInteger();
            string extraData = Request.GetString();
            int priceAmount = Request.GetInteger();
            Oblivion.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, priceAmount, false, string.Empty, string.Empty, 0, 0, 0, false, 0u);
        }

        /// <summary>
        /// Purchases the gift.
        /// </summary>
        public void PurchaseGift()
        {
            int pageId = Request.GetInteger();
            uint itemId = Request.GetUInteger();
            string extraData = Request.GetString();
            string giftUser = Request.GetString();
            string giftMessage = Request.GetString();
            int giftSpriteId = Request.GetInteger();
            int giftLazo = Request.GetInteger();
            int giftColor = Request.GetInteger();
            var undef = Request.GetBool();

            Oblivion.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, 1, true, giftUser, giftMessage, giftSpriteId, giftLazo, giftColor, undef, 0u);
        }

        /// <summary>
        /// Checks the name of the pet.
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
        /// Catalogues the offer.
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
        /// Catalogues the offer configuration.
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
        /// Serializes the group furni page.
        /// </summary>
        internal void SerializeGroupFurniPage()
        {
            var userGroups = Oblivion.GetGame().GetGroupManager().GetUserGroups(Session.GetHabbo().Id);

            Response.Init(LibraryParser.OutgoingRequest("GroupFurniturePageMessageComposer"));

            var responseList = new List<ServerMessage>();

            foreach (var habboGroup in userGroups.Where(current => current != null).Select(current => Oblivion.GetGame().GetGroupManager().GetGroup(current.GroupId)))
            {
                if (habboGroup == null)
                    continue;

                ServerMessage subResponse = new ServerMessage();
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

            Response.AppendInteger(responseList.Count());
            Response.AppendServerMessages(responseList);

            responseList.Clear();

            SendResponse();
        }
    }
}