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
            if (Session?.GetHabbo() == null)
                return;
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
            if (Session?.GetHabbo() == null)
                return;
            var rank = Session.GetHabbo().Rank;

            /*Request.GetString()
"NORMAL"
Request.GetString()
"default_3x3,default_3x3_extrainfo,vip_buy,pets2,pixeleffects,info_duckets,info_loyalty,info_rentables,spaces_new,club_gifts,empty_search,marketplace,sold_ltd_items,roomads,default_3x3_color_grouping,marketplace_own_items,petcustomization,guild_frontpage,guild_custom_furni,mobile_spinner_large,mobile_subscriptions,mobile_credits,mobile_bundles,frontpage4,builders_club_addons,pets,builders_club_addons,pets"
*/

            string pageType = "NORMAL";
            string[] allowedPages = null;

            if (Request.BytesAvailable)
            {
                pageType = Request.GetString();

                string allowedPagesString = Request.GetString();

                if (allowedPagesString.Contains(","))
                    allowedPages = allowedPagesString.Split(',');
            }

            if (rank < 1)
                rank = 1;

            Session.SendMessage(CatalogPageComposer.ComposeIndex(rank, pageType, allowedPages, Session));
            Session.SendMessage(StaticMessage.CatalogOffersConfiguration);
        }

        /// <summary>
        ///     Catalogues the page.
        /// </summary>
        public void CataloguePage()
        {
            if (Session?.GetHabbo() == null)
                return;
            var pageId = Request.GetInteger();

            Request.GetInteger();

            var CataMode = Request.GetString();

            var cPage = Oblivion.GetGame().GetCatalog().GetPage(pageId);

            if (cPage == null || !cPage.Visible || cPage.MinRank > Session.GetHabbo().Rank)
                return;

            var message = CatalogPageComposer.ComposePage(Session, cPage, CataMode);
            Session.SendMessage(message);
        }


        /// <summary>
        ///     Catalogues the club page.
        /// </summary>
        public void CatalogueClubPage()
        {
            if (Session?.GetHabbo() == null)
                return;
            var requestType = Request.GetInteger();

            Session.SendMessage(CatalogPageComposer.ComposeClubPurchasePage(Session, requestType));
        }

        /// <summary>
        ///     Reloads the ecotron.
        /// </summary>
        public void ReloadEcotron()
        {
            if (Session?.GetHabbo() == null)
                return;
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
            if (Session?.GetHabbo() == null)
                return;
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
            if (Session?.GetHabbo() == null)
                return;
            Response.Init(LibraryParser.OutgoingRequest("RecyclerRewardsMessageComposer"));

            var ecotronRewardsLevels = Oblivion.GetGame().GetCatalog().GetEcotronRewardsLevels();

            Response.AppendInteger(ecotronRewardsLevels.Count);

            /* TODO CHECK */
            foreach (var current in ecotronRewardsLevels)
            {
                Response.AppendInteger(current);
                Response.AppendInteger(current);

                var ecotronRewardsForLevel = Oblivion.GetGame().GetCatalog()
                    .GetEcotronRewardsForLevel(uint.Parse(current.ToString()));

                Response.AppendInteger(ecotronRewardsForLevel.Count);

                /* TODO CHECK */
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
            if (Session?.GetHabbo() == null)
                return;
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
            if (Session?.GetHabbo() == null)
                return;
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
            if (Session?.GetHabbo() == null)
                return;

            var num = Request.GetInteger();
            var catalogItem = Oblivion.GetGame().GetCatalog().GetItemFromOffer(num);

            if (catalogItem == null || CatalogManager.LastSentOffer == num)
                return;

            CatalogManager.LastSentOffer = num;

            var message = new ServerMessage(LibraryParser.OutgoingRequest("CatalogOfferMessageComposer"));

            CatalogPageComposer.ComposeItem(Session, catalogItem, message);
            Session.SendMessage(message);
        }

        /// <summary>
        ///     Serializes the group furni page.
        /// </summary>
        internal void SerializeGroupFurniPage()
        {
            var userGroups = Oblivion.GetGame().GetGroupManager().GetUserGroups(Session.GetHabbo().Id);

            Response.Init(LibraryParser.OutgoingRequest("GroupFurniturePageMessageComposer"));

            var responseList = new List<ServerMessage>();

            /* TODO CHECK */
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