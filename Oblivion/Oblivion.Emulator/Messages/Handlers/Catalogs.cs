using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.Catalogs.Composers;
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
        public async Task CatalogueMode()
        {
            if (Session?.GetHabbo() == null)
                return;
            var rank = Session.GetHabbo().Rank;

            if (rank < 1)
                rank = 1;
            await Session.SendMessageAsync(CatalogPageComposer.ComposeIndex(rank, Request.GetString().ToUpper(), Session));
            await Session.SendStaticMessage(StaticMessage.CatalogOffersConfiguration);
            
        }

        

        /// <summary>
        ///     Catalogues the index.
        /// </summary>
        public async Task CatalogueIndex()
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

            await Session.SendMessageAsync(CatalogPageComposer.ComposeIndex(rank, pageType, allowedPages, Session));
            await Session.SendStaticMessage(StaticMessage.CatalogOffersConfiguration);
        }

        /// <summary>
        ///     Catalogues the page.
        /// </summary>
        public async Task CataloguePage()
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
            if (message == null) return;
            await Session.SendMessageAsync(message);
        }


        /// <summary>
        ///     Catalogues the club page.
        /// </summary>
        public async Task CatalogueClubPage()
        {
            if (Session?.GetHabbo() == null)
                return;
            var requestType = Request.GetInteger();

            await Session.SendMessageAsync(CatalogPageComposer.ComposeClubPurchasePage(Session, requestType));
        }

        /// <summary>
        ///     Reloads the ecotron.
        /// </summary>
        public async Task ReloadEcotron()
        {
            if (Session?.GetHabbo() == null)
                return;
            await Response.InitAsync(LibraryParser.OutgoingRequest("ReloadEcotronMessageComposer"));
            await Response.AppendIntegerAsync(1);
            await Response.AppendIntegerAsync(0);
            await SendResponse();
        }

        /// <summary>
        ///     Gifts the wrapping configuration.
        /// </summary>
        public async Task GiftWrappingConfig()
        {
            if (Session?.GetHabbo() == null)
                return;
            await Response.InitAsync(LibraryParser.OutgoingRequest("GiftWrappingConfigurationMessageComposer"));
            Response.AppendBool(true);
            await Response.AppendIntegerAsync(1);
            await Response.AppendIntegerAsync(GiftWrapper.GiftWrappersList.Count);

            foreach (var i in GiftWrapper.GiftWrappersList)
                await Response.AppendIntegerAsync(i);

            await Response.AppendIntegerAsync(8);

            for (var i = 0u; i != 8; i++)
                await Response.AppendIntegerAsync(i);

            await Response.AppendIntegerAsync(11);

            for (var i = 0u; i != 11; i++)
                await Response.AppendIntegerAsync(i);

            await Response.AppendIntegerAsync(GiftWrapper.OldGiftWrappers.Count);

            foreach (var i in GiftWrapper.OldGiftWrappers)
                await Response.AppendIntegerAsync(i);

            await SendResponse();
        }

        /// <summary>
        ///     Gets the recycler rewards.
        /// </summary>
        public async Task GetRecyclerRewards()
        {
            if (Session?.GetHabbo() == null)
                return;
            await Response.InitAsync(LibraryParser.OutgoingRequest("RecyclerRewardsMessageComposer"));

            var ecotronRewardsLevels = Oblivion.GetGame().GetCatalog().GetEcotronRewardsLevels();

            await Response.AppendIntegerAsync(ecotronRewardsLevels.Count);

            /* TODO CHECK */
            foreach (var current in ecotronRewardsLevels)
            {
                await Response.AppendIntegerAsync(current);
                await Response.AppendIntegerAsync(current);

                var ecotronRewardsForLevel = Oblivion.GetGame().GetCatalog()
                    .GetEcotronRewardsForLevel(uint.Parse(current.ToString()));

                await Response.AppendIntegerAsync(ecotronRewardsForLevel.Count);

                /* TODO CHECK */
                foreach (var current2 in ecotronRewardsForLevel)
                {
                    await Response.AppendStringAsync(current2.GetBaseItem().PublicName);
                    await Response.AppendIntegerAsync(1);
                    await Response.AppendStringAsync(current2.GetBaseItem().Type.ToString());
                    await Response.AppendIntegerAsync(current2.GetBaseItem().SpriteId);
                }
            }

            await SendResponse();
        }

        /// <summary>
        ///     Purchases the item.
        /// </summary>
        public async Task PurchaseItem()
        {
            if (Session?.GetHabbo() == null)
                return;

            if (Session.GetHabbo().GetInventoryComponent().TotalItems >= 3500)
            {
                await Session.SendMessageAsync(CatalogPageComposer.PurchaseOk(0, string.Empty, 0));
                await Session.SendStaticMessage(StaticMessage.AdvicePurchaseMaxItems);
                return;
            }

            var pageId = Request.GetInteger();
            var itemId = Request.GetUInteger();
            var extraData = Request.GetString();
            var priceAmount = Request.GetInteger();
            await Oblivion.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, priceAmount, false,
                string.Empty, string.Empty, 0, 0, 0, false, 0u);
           

        }

        /// <summary>
        ///     Purchases the gift.
        /// </summary>
        public async Task PurchaseGift()
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
            await Oblivion.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, 1, true, giftUser,
                giftMessage, giftSpriteId, giftLazo, giftColor, undef, 0u);
        }

        /// <summary>
        ///     Checks the name of the pet.
        /// </summary>
        public async Task CheckPetName()
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

            await Response.InitAsync(LibraryParser.OutgoingRequest("CheckPetNameMessageComposer"));
            await Response.AppendIntegerAsync(i);
            await Response.AppendStringAsync(petName);
            await SendResponse();
        }

        /// <summary>
        ///     Catalogues the offer.
        /// </summary>
        public async Task CatalogueOffer()
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
            await Session.SendMessageAsync(message);
        }

        /// <summary>
        ///     Serializes the group furni page.
        /// </summary>
        internal async Task SerializeGroupFurniPage()
        {
            var userGroups = Oblivion.GetGame().GetGroupManager().GetUserGroups(Session.GetHabbo().Id);

            await Response.InitAsync(LibraryParser.OutgoingRequest("GroupFurniturePageMessageComposer"));

            var responseList = new List<ServerMessage>();

            /* TODO CHECK */
            foreach (var habboGroup in userGroups.Where(current => current != null)
.Select(current => Oblivion.GetGame().GetGroupManager().GetGroup(current.GroupId)))
            {
                if (habboGroup == null)
                    continue;

                var subResponse = new ServerMessage();
                await subResponse.AppendIntegerAsync(habboGroup.Id);
                await subResponse.AppendStringAsync(habboGroup.Name);
                await subResponse.AppendStringAsync(habboGroup.Badge);
                await subResponse.AppendStringAsync(Oblivion.GetGame().GetGroupManager().SymbolColours.Contains(habboGroup.Colour1)
                    ? ((GroupSymbolColours)
                        Oblivion.GetGame().GetGroupManager().SymbolColours[habboGroup.Colour1]).Colour
                    : "4f8a00");
                await subResponse.AppendStringAsync(
                    Oblivion.GetGame().GetGroupManager().BackGroundColours.Contains(habboGroup.Colour2)
                        ? ((GroupBackGroundColours)
                            Oblivion.GetGame().GetGroupManager().BackGroundColours[habboGroup.Colour2]).Colour
                        : "4f8a00");
                subResponse.AppendBool(habboGroup.CreatorId == Session.GetHabbo().Id);
                await subResponse.AppendIntegerAsync(habboGroup.CreatorId);
                subResponse.AppendBool(habboGroup.HasForum);

                responseList.Add(subResponse);
            }

            await Response.AppendIntegerAsync(responseList.Count);
            Response.AppendServerMessages(responseList);

            responseList.Clear();

            await SendResponse();
        }
    }
}