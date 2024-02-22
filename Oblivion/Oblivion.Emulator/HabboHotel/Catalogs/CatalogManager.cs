using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Catalogs.Interfaces;
using Oblivion.HabboHotel.Catalogs.Marketplace;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Pets.Enums;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.SoundMachine;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Security;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Catalogs
{
    /// <summary>
    ///     Class Catalog.
    /// </summary>
    internal class CatalogManager
    {
        /// <summary>
        ///     The last sent offer
        /// </summary>
        internal static int LastSentOffer;

        /// <summary>
        ///     The categories
        /// </summary>
        internal Dictionary<int, CatalogPage> Categories;

        /// <summary>
        ///     The ecotron levels
        /// </summary>
        internal List<int> EcotronLevels;

        /// <summary>
        ///     The ecotron rewards
        /// </summary>
        internal List<EcotronReward> EcotronRewards;

        /// <summary>
        ///     The flat offers
        /// </summary>
        internal Dictionary<int, uint> FlatOffers;

        /// <summary>
        ///     The habbo club items
        /// </summary>
        internal Dictionary<uint, CatalogItem> HabboClubItems;

        /// <summary>
        ///     The offers
        /// </summary>
        internal Dictionary<uint, CatalogItem> Offers;

        /// <summary>
        /// The manager for marketplace
        /// </summary>
        private MarketplaceManager _marketplace;

        /// <summary>
        ///     Checks the name of the pet.
        /// </summary>
        /// <param name="petName">Name of the pet.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool CheckPetName(string petName) => petName.Length >= 3 && petName.Length <= 15 &&
                                                             Oblivion.IsValidAlphaNumeric(petName);

        /// <summary>
        ///     Creates the bot.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="look">The look.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="bartender">if set to <c>true</c> [bartender].</param>
        /// <returns>RoomBot.</returns>
        internal static RoomBot CreateBot(uint userId, string name, string look, string motto, string gender,
            bool bartender)
        {
            uint botId;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "INSERT INTO bots (user_id,name,motto,look,gender,walk_mode,is_bartender) VALUES (@h_user,@b_name,@b_motto,@b_look,@b_gender,@b_walk,@b_bartender)");
                queryReactor.AddParameter("h_user", userId);
                queryReactor.AddParameter("b_name", name);
                queryReactor.AddParameter("b_motto", motto);
                queryReactor.AddParameter("b_look", look);
                queryReactor.AddParameter("b_gender", gender);
                queryReactor.AddParameter("b_walk", "freeroam");
                queryReactor.AddParameter("b_bartender", bartender ? "1" : "0");
                botId = Convert.ToUInt32(queryReactor.InsertQuery());
            }

            return new RoomBot(botId, userId, 0u, AiType.Generic, "freeroam", name, motto, look, 0, 0, 0.0, 0, 0, 0, 0,
                0, null, null, gender, 0, bartender);
        }

        /// <summary>
        ///     Creates the pet.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="race">The race.</param>
        /// <param name="color">The color.</param>
        /// <param name="rarity">The rarity.</param>
        /// <returns>Pet.</returns>
        internal static async Task<Pet> CreatePet(uint userId, string name, int type, string race, string color,
            int rarity = 0)
        {
            var pet = new Pet(404u, userId, 0u, name, (uint)type, race, color, 0, 100, 150, 0,
                Oblivion.GetUnixTimeStamp(),
                0, 0, 0.0, false, 0, 0, -1, rarity, DateTime.Now.AddHours(36.0), DateTime.Now.AddHours(48.0), null)
            {
                DbState = DatabaseUpdateState.NeedsUpdate
            };

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery(string.Concat("INSERT INTO bots (user_id,name, ai_type) VALUES (", pet.OwnerId,
                    ",@", pet.PetId, "name, 'pet')"));

                queryReactor.AddParameter($"{pet.PetId}name", pet.Name);

                pet.PetId = (uint)await queryReactor.InsertQueryAsync();

                queryReactor.SetQuery($"SELECT count(id) FROM pets_data WHERE id = {pet.PetId}");
                var count = await queryReactor.GetIntegerAsync();
                if (count <= 0)
                {
                    queryReactor.SetQuery(
                        string.Concat(
                            "INSERT INTO pets_data (id,type,race,color,experience,energy,createstamp,rarity,lasthealth_stamp,untilgrown_stamp) VALUES (",
                            pet.PetId, ", ", pet.Type, ",@", pet.PetId, "race,@", pet.PetId,
                            "color,0,100,UNIX_TIMESTAMP(), ", rarity,
                            ", UNIX_TIMESTAMP(now() + INTERVAL 36 HOUR), UNIX_TIMESTAMP(now() + INTERVAL 48 HOUR))"));

                    queryReactor.AddParameter($"{pet.PetId}race", pet.Race);
                    queryReactor.AddParameter($"{pet.PetId}color", pet.Color);
                    await queryReactor.RunQueryAsync();
                }
            }

            if (pet.Type == 16u)
            {
                pet.MoplaBreed = MoplaBreed.CreateMonsterplantBreed(pet);
                pet.Name = pet.MoplaBreed.Name;
                pet.DbState = DatabaseUpdateState.NeedsUpdate;
            }

            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            if (clientByUserId != null)
                await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(clientByUserId, "ACH_PetLover", 1);

            return pet;
        }

        /// <summary>
        ///     Generates the pet from row.
        /// </summary>
        /// <param name="Row">The row.</param>
        /// <param name="mRow">The m row.</param>
        /// <returns>Pet.</returns>
        internal static Pet GeneratePetFromRow(DataRow Row, DataRow mRow)
        {
            if (Row == null || mRow == null)
                return null;

            MoplaBreed moplaBreed = null;

            if (Convert.ToUInt32(mRow["type"]) == 16u)
            {
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery($"SELECT * FROM pets_plants WHERE pet_id = {Convert.ToUInt32(Row["id"])}");
                    var row = queryReactor.GetRow();
                    if (row == null)
                    {
                        return null;
                    }

                    moplaBreed = new MoplaBreed(row);
                }
            }

            if (Row["id"] == null || Row["room_id"] == null)
            {
                return null;
            }

            if (!uint.TryParse(Row["room_id"].ToString(), out var roomId))
            {
                roomId = 0;
            }

            return new Pet(Convert.ToUInt32(Row["id"]), Convert.ToUInt32(Row["user_id"]),
                roomId, (string)Row["name"], Convert.ToUInt32(mRow["type"]),
                (string)mRow["race"], (string)mRow["color"], (int)mRow["experience"], (int)mRow["energy"],
                (int)mRow["nutrition"], (int)mRow["respect"], Convert.ToDouble(mRow["createstamp"]), (int)Row["x"],
                (int)Row["y"], Convert.ToDouble(Row["z"]), (int)mRow["have_saddle"] == 1, (int)mRow["anyone_ride"],
                (int)mRow["hairdye"], (int)mRow["pethair"], (int)mRow["rarity"],
                Oblivion.UnixToDateTime((int)mRow["lasthealth_stamp"]),
                Oblivion.UnixToDateTime((int)mRow["untilgrown_stamp"]), moplaBreed);
        }

        public Dictionary<uint, ServerMessage> IndexMessages;

        /// <summary>
        ///     Gets the item from offer.
        /// </summary>
        /// <param name="offerId">The offer identifier.</param>
        /// <returns>CatalogItem.</returns>
        internal CatalogItem GetItemFromOffer(int offerId)
        {
            CatalogItem result = null;

            if (FlatOffers.TryGetValue(offerId, out var flatOff))
                result = Offers[flatOff];

            return result ?? (Oblivion.GetGame().GetCatalog().GetItem(Convert.ToUInt32(offerId)));
        }

        public List<DataRow> IndexText;

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal Task<int> Initialize(IQueryAdapter dbClient)
        {
            try
            {
                IndexMessages = new Dictionary<uint, ServerMessage>();
                _marketplace = new MarketplaceManager();
                Categories = new Dictionary<int, CatalogPage>();
                Offers = new Dictionary<uint, CatalogItem>();
                FlatOffers = new Dictionary<int, uint>();
                EcotronRewards = new List<EcotronReward>();
                EcotronLevels = new List<int>();
                HabboClubItems = new Dictionary<uint, CatalogItem>();
                IndexText = new List<DataRow>();

                dbClient.SetQuery("SELECT * FROM catalog_items ORDER BY id ASC");
                var table = dbClient.GetTable();
                dbClient.SetQuery("SELECT * FROM catalog_pages ORDER BY order_num ASC");
                var table2 = dbClient.GetTable();
                dbClient.SetQuery("SELECT * FROM catalog_ecotron ORDER BY reward_level ASC");
                var table3 = dbClient.GetTable();
                dbClient.SetQuery("SELECT * FROM catalog_items WHERE item_names LIKE '%HABBO_CLUB_VIP%'");
                var table4 = dbClient.GetTable();

                dbClient.SetQuery("SELECT * FROM `catalog_homepage`;");
                var table5 = dbClient.GetTable();

                try
                {
                    if (table != null)
                    {
                        /* TODO CHECK */
                        foreach (DataRow dataRow in table.Rows)
                        {
                            if (string.IsNullOrEmpty(dataRow["item_id"].ToString()) ||
                                string.IsNullOrEmpty(dataRow["amounts"].ToString()))
                                continue;

                            var source = dataRow["item_id"].ToString();
                            var firstItem = source;
                            if (source.Contains(';'))
                                firstItem = dataRow["item_id"].ToString().Split(';')[0];

                            if (!uint.TryParse(firstItem, out var itemId))
                            {
                                Console.WriteLine("Item id too high:" + firstItem);
                            }

                            if (!Oblivion.GetGame().GetItemManager().GetItem(itemId, out var item))
                            {
                                continue;
                            }

                            var num = item.FlatId;
                            if (!source.Contains(';'))
                            {
                                num = -1;
                                item.PublicName = dataRow["item_names"].ToString();
                            }
                            else
                            {
                                item.PublicName = dataRow["item_names"].ToString().Split(';')[0];
                            }

                            var catalogItem = new CatalogItem(dataRow, item.PublicName);

                            if (catalogItem.GetFirstBaseItem() == null)
                                continue;

                            Offers.Add(catalogItem.Id, catalogItem);

                            if (num != -1 && !FlatOffers.ContainsKey(num))
                                FlatOffers.Add(num, catalogItem.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e + "Loading items");
                }

                if (table2 != null)
                {
                    /* TODO CHECK */
                    foreach (DataRow dataRow2 in table2.Rows)
                    {
                        var visible = false;
                        var enabled = false;

                        if (dataRow2["visible"].ToString() == "1")
                            visible = true;
                        if (dataRow2["enabled"].ToString() == "1")
                            enabled = true;

                        Categories.Add(Convert.ToInt32(dataRow2["id"]),
                            new CatalogPage(Convert.ToUInt32(dataRow2["id"]),
                                int.Parse(dataRow2["parent_id"].ToString()),
                                (string)dataRow2["page_link"], (string)dataRow2["caption"], visible, enabled, false,
                                Convert.ToUInt32(dataRow2["min_rank"]), (int)dataRow2["icon_image"],
                                (string)dataRow2["page_layout"], (string)dataRow2["page_strings_1"],
                                (string)dataRow2["page_strings_2"], (int)dataRow2["order_num"], ref Offers,
                                dataRow2["blockBad"].ToString() == "1"));
                    }
                }

                if (table3 != null)
                {
                    /* TODO CHECK */
                    foreach (DataRow dataRow3 in table3.Rows)
                    {
                        EcotronRewards.Add(new EcotronReward(Convert.ToUInt32(dataRow3["display_id"]),
                            Convert.ToUInt32(dataRow3["item_id"]), Convert.ToUInt32(dataRow3["reward_level"])));

                        if (!EcotronLevels.Contains(Convert.ToInt16(dataRow3["reward_level"])))
                            EcotronLevels.Add(Convert.ToInt16(dataRow3["reward_level"]));
                    }
                }

                if (table4 != null)
                {
                    /* TODO CHECK */
                    foreach (DataRow row in table4.Rows)
                    {
                        var it = new CatalogItem(row, row["item_names"].ToString());
                        HabboClubItems.Add(it.Id, it);
                    }
                }

                if (table5 != null)
                {
                    /* TODO CHECK */
                    foreach (DataRow row in table5.Rows)
                        IndexText.Add(row);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return Task.FromResult(Categories.Count);
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns>CatalogItem.</returns>
        internal CatalogItem GetItem(uint itemId) => Offers.TryGetValue(itemId, out CatalogItem item) ? item : null;

        /// <summary>
        ///     Gets the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>CatalogPage.</returns>
        internal CatalogPage GetPage(int page) => Categories.TryGetValue(page, out CatalogPage pg) ? pg : null;

        /// <summary>
        ///     Handles the purchase.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="priceAmount">The price amount.</param>
        /// <param name="isGift">if set to <c>true</c> [is gift].</param>
        /// <param name="giftUser">The gift user.</param>
        /// <param name="giftMessage">The gift message.</param>
        /// <param name="giftSpriteId">The gift sprite identifier.</param>
        /// <param name="giftLazo">The gift lazo.</param>
        /// <param name="giftColor">Color of the gift.</param>
        /// <param name="undef">if set to <c>true</c> [undef].</param>
        /// <param name="theGroup">The theGroup.</param>
        internal async Task HandlePurchase(GameClient session, int pageId, uint itemId, string extraData,
            int priceAmount,
            bool isGift, string giftUser, string giftMessage, int giftSpriteId, int giftLazo, int giftColor, bool undef,
            uint theGroup)
        {
            if (session.GetHabbo().LastSqlQuery + 1 >= Oblivion.GetUnixTimeStamp())
            {
                await session.SendNotif("Espere um pouco");
                return;
            }

            if (session.GetHabbo().GetInventoryComponent().TotalItems >= 4500)
            {
                await session.SendNotif("Você chegou ao limite de mobis, retire alguns antes de continuar!");
                return;
            }


            if (!Categories.TryGetValue(pageId, out var catalogPage))
                return;

            if (catalogPage == null || !catalogPage.Enabled || !catalogPage.Visible)
                return;

            if (catalogPage.MinRank > session.GetHabbo().Rank || catalogPage.Layout == "sold_ltd_items")
                return;

            var item = catalogPage.GetItem(itemId);

            if (item == null)
            {
                if (!Oblivion.GetGame().GetCatalog().HabboClubItems.TryGetValue(itemId, out item))
                    return;
            }

            if (item.HaveOffer)
            {
                priceAmount = (priceAmount < 1 || priceAmount > 100) ? 1 : priceAmount;
            }
            else
            {
                priceAmount = 1;
            }

            var totalPrice = priceAmount;
            var limitedId = 0;
            var limtot = 0;

            if (priceAmount >= 6)
                totalPrice -= Convert.ToInt32(Math.Ceiling(Convert.ToDouble(priceAmount) / 6)) * 2 - 1;


            if (catalogPage.Layout == "vip_buy" || catalogPage.Layout == "club_buy" ||
                HabboClubItems.ContainsKey(itemId))
            {
                if (session.GetHabbo().Credits < item.CreditsCost)
                    return;
                if (session.GetHabbo().ActivityPoints < item.DucketsCost)
                    return;
                if (session.GetHabbo().Diamonds < item.DiamondsCost)
                    return;

                if (session.GetHabbo().Graffiti < item.EmeraldsCost)
                    return;

                var array = item.Name.Split('_');

                double dayLength;

                if (item.Name.Contains("DAY"))
                    dayLength = double.Parse(array[3]);
                else if (item.Name.Contains("MONTH"))
                    dayLength = Math.Ceiling((double.Parse(array[3]) * 31) - 0.205);
                else if (item.Name.Contains("YEAR"))
                    dayLength = (double.Parse(array[3]) * 31 * 12);
                else
                    dayLength = 31;

                await session.GetHabbo().GetSubscriptionManager().AddSubscription(dayLength);

                if (item.CreditsCost > 0)
                {
                    session.GetHabbo().Credits -= (int)item.CreditsCost * totalPrice;
                    await session.GetHabbo().UpdateCreditsBalance(true);
                }

                if (item.DucketsCost > 0)
                {
                    session.GetHabbo().ActivityPoints -= (int)item.DucketsCost * totalPrice;
                    await session.GetHabbo().UpdateActivityPointsBalance(true);
                }

                if (item.DiamondsCost > 0)
                {
                    session.GetHabbo().Diamonds -= (int)item.DiamondsCost * totalPrice;
                    await session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
                }

                if (item.EmeraldsCost > 0)
                {
                    session.GetHabbo().Graffiti -= (int)item.EmeraldsCost * totalPrice;
                    await session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
                }

                return;
            }

            if (item.Name == "room_ad_plus_badge")
                return;
            if (item.GetFirstBaseItem().InteractionType == Interaction.NamePrefix)
            {
                if (!BobbaFilter.CanUsePrefix(session, extraData))
                {
                    await session.SendNotif("Prefixo inválido, tente outro!");

                    return;
                }
            }

            if (item.ClubOnly && !session.GetHabbo().GetSubscriptionManager().HasSubscription)
            {
                using (var serverMessage =
                       new ServerMessage(LibraryParser.OutgoingRequest("CatalogPurchaseNotAllowedMessageComposer")))
                {
                    await serverMessage.AppendIntegerAsync(1);
                    await session.SendMessage(serverMessage);
                    return;
                }
            }

            var flag = item.Items.Keys.Any(
                current => InteractionTypes.AreFamiliar(GlobalInteractions.Pet, current.InteractionType));

            if (!flag && (item.CreditsCost * totalPrice < 0 || item.DucketsCost * totalPrice < 0 ||
                          item.DiamondsCost * totalPrice < 0 || item.EmeraldsCost * totalPrice < 0))
                return;

            if (item.IsLimited)
            {
                totalPrice = 1;
                priceAmount = 1;

                if (item.LimitedSelled >= item.LimitedStack)
                {
                    await session.SendMessage(new ServerMessage(
                        LibraryParser.OutgoingRequest("CatalogLimitedItemSoldOutMessageComposer")));
                    return;
                }

                item.LimitedSelled++;

                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    await queryReactor.RunFastQueryAsync(string.Concat("UPDATE catalog_items SET limited_sells = ",
                        item.LimitedSelled, " WHERE id = ", item.Id));
                    limitedId = item.LimitedSelled;
                    limtot = item.LimitedStack;
                }
            }
            else if (isGift & priceAmount > 1)
            {
                totalPrice = 1;
                priceAmount = 1;
            }

            var toUserId = 0u;

            if (session.GetHabbo().Credits < item.CreditsCost * totalPrice)
                return;
            if (session.GetHabbo().ActivityPoints < item.DucketsCost * totalPrice)
                return;
            if (session.GetHabbo().Diamonds < item.DiamondsCost * totalPrice)
                return;
            if (session.GetHabbo().Graffiti < item.EmeraldsCost * totalPrice)
                return;

            if (item.CreditsCost > 0 && !isGift)
            {
                session.GetHabbo().Credits -= (int)item.CreditsCost * totalPrice;
                await session.GetHabbo().UpdateCreditsBalance(true);
            }

            if (item.DucketsCost > 0 && !isGift)
            {
                session.GetHabbo().ActivityPoints -= (int)item.DucketsCost * totalPrice;
                await session.GetHabbo().UpdateActivityPointsBalance(true);
            }

            if (item.DiamondsCost > 0 && !isGift)
            {
                session.GetHabbo().Diamonds -= (int)item.DiamondsCost * totalPrice;
                await session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
            }

            if (item.EmeraldsCost > 0 && !isGift)
            {
                session.GetHabbo().Graffiti -= item.EmeraldsCost * totalPrice;
                await session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
            }

            session.GetHabbo().LastSqlQuery = Oblivion.GetUnixTimeStamp();

            /* TODO CHECK */
            foreach (var baseItem in item.Items.Keys)
            {
                if (isGift)
                {
                    if ((DateTime.Now - session.GetHabbo().LastGiftPurchaseTime).TotalSeconds <= 15.0)
                    {
                        await session.SendNotif(Oblivion.GetLanguage().GetVar("user_send_gift"));
                        return;
                    }

                    if (!baseItem.AllowGift)
                        return;

                    DataRow row;

                    using (var queryreactor3 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        queryreactor3.SetQuery("SELECT id FROM users WHERE username = @gift_user");
                        queryreactor3.AddParameter("gift_user", giftUser);
                        row = queryreactor3.GetRow();
                    }

                    if (row == null)
                    {
                        await session.GetMessageHandler()
                            .GetResponse()
                            .InitAsync(LibraryParser.OutgoingRequest("GiftErrorMessageComposer"));
                        await session.GetMessageHandler().GetResponse().AppendStringAsync(giftUser);
                        await session.GetMessageHandler().SendResponse();
                        return;
                    }

                    toUserId = Convert.ToUInt32(row["id"]);

                    if (toUserId == 0u)
                    {
                        await session.GetMessageHandler()
                            .GetResponse()
                            .InitAsync(LibraryParser.OutgoingRequest("GiftErrorMessageComposer"));
                        await session.GetMessageHandler().GetResponse().AppendStringAsync(giftUser);
                        await session.GetMessageHandler().SendResponse();
                        return;
                    }

                    if (item.CreditsCost > 0 && isGift)
                    {
                        session.GetHabbo().Credits -= (int)item.CreditsCost * totalPrice;
                        await session.GetHabbo().UpdateCreditsBalance(true);
                    }

                    if (item.DucketsCost > 0 && isGift)
                    {
                        session.GetHabbo().ActivityPoints -= (int)item.DucketsCost * totalPrice;
                        await session.GetHabbo().UpdateActivityPointsBalance(true);
                    }

                    if (item.DiamondsCost > 0 && isGift)
                    {
                        session.GetHabbo().Diamonds -= (int)item.DiamondsCost * totalPrice;
                        await session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
                    }

                    if (item.EmeraldsCost > 0 && isGift)
                    {
                        session.GetHabbo().Graffiti -= (int)item.EmeraldsCost * totalPrice;
                        await session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
                    }
                }

                if (isGift && baseItem.Type == 'e')
                {
                    await session.SendNotif(Oblivion.GetLanguage().GetVar("user_send_gift_effect"));
                    return;
                }

                if (item.Name.StartsWith("builders_club_addon_"))
                {
                    var furniAmount = Convert.ToInt32(item.Name.Replace("builders_club_addon_", "")
                        .Replace("furnis", ""));

                    session.GetHabbo().BuildersItemsMax += furniAmount;
                    using (var update =
                           new ServerMessage(LibraryParser.OutgoingRequest("BuildersClubMembershipMessageComposer")))
                    {
                        await update.AppendIntegerAsync(session.GetHabbo().BuildersExpire);
                        await update.AppendIntegerAsync(session.GetHabbo().BuildersItemsMax);
                        await update.AppendIntegerAsync(2);
                        await session.SendMessage(update);
                    }

                    using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        queryReactor.SetQuery("UPDATE users SET builders_items_max = @max WHERE id = @userId");
                        queryReactor.AddParameter("max", session.GetHabbo().BuildersItemsMax);
                        queryReactor.AddParameter("userId", session.GetHabbo().Id);
                        await queryReactor.RunQueryAsync();
                    }


                    await session.SendMessage(CatalogPageComposer.PurchaseOk(item, item.Items));
                    await session.SendNotif("${notification.builders_club.membership_extended.message}",
                        "${notification.builders_club.membership_extended.title}", "builders_club_membership_extended");
                    return;
                }

                if (item.Name.StartsWith("builders_club_time_"))
                {
                    var timeAmount =
                        Convert.ToInt32(item.Name.Replace("builders_club_time_", "").Replace("seconds", ""));

                    session.GetHabbo().BuildersExpire += timeAmount;

                    var update =
                        new ServerMessage(LibraryParser.OutgoingRequest("BuildersClubMembershipMessageComposer"));

                    await update.AppendIntegerAsync(session.GetHabbo().BuildersExpire);
                    await update.AppendIntegerAsync(session.GetHabbo().BuildersItemsMax);
                    await update.AppendIntegerAsync(2);
                    await session.SendMessage(update);

                    using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        queryReactor.SetQuery("UPDATE users SET builders_expire = @max WHERE id = @userId");
                        queryReactor.AddParameter("max", session.GetHabbo().BuildersExpire);
                        queryReactor.AddParameter("userId", session.GetHabbo().Id);
                        await queryReactor.RunQueryAsync();
                    }

                    await session.SendMessage(CatalogPageComposer.PurchaseOk(item, item.Items));
                    await session.SendNotif("${notification.builders_club.membership_extended.message}",
                        "${notification.builders_club.membership_extended.title}", "builders_club_membership_extended");
                    return;
                }

                var text = string.Empty;

                var interactionType = baseItem.InteractionType;

                switch (interactionType)
                {
                    case Interaction.None:
                    case Interaction.Gate:
                    case Interaction.Bed:
                    case Interaction.PressurePadBed:
                    case Interaction.Guillotine:
                    case Interaction.HcGate:
                    case Interaction.ScoreBoard:
                    case Interaction.VendingMachine:
                    case Interaction.Alert:
                    case Interaction.OneWayGate:
                    case Interaction.LoveShuffler:
                    case Interaction.HabboWheel:
                    case Interaction.Dice:
                    case Interaction.Bottle:
                    case Interaction.Hopper:
                    case Interaction.Teleport:
                    case Interaction.QuickTeleport:
                    case Interaction.Pet:
                    case Interaction.Pool:
                    case Interaction.Roller:
                        break;

                    case Interaction.PostIt:
                        extraData = "FFFF33";
                        break;

                    case Interaction.RoomEffect:
                        var number = string.IsNullOrEmpty(extraData)
                            ? 0.0
                            : double.Parse(extraData, Oblivion.CultureInfo);
                        extraData = number.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                        break;

                    case Interaction.Dimmer:
                        extraData = "1,1,1,#000000,255";
                        break;

                    case Interaction.Trophy:
                        extraData = string.Concat(session.GetHabbo().UserName, Convert.ToChar(9),
                            DateTime.Now.Day.ToString("00"), "-", DateTime.Now.Month.ToString("00"), "-",
                            DateTime.Now.Year, Convert.ToChar(9), extraData);
                        break;

                    case Interaction.Rentals:
                        extraData = item.ExtraData;
                        break;


                    case Interaction.ColorName:
                    {
                        session.GetHabbo().Prefixes[0] = item.Badge;
                        var prefixStr = string.Join(",", session.GetHabbo().Prefixes);
                        using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            dbClient.SetQuery(
                                $"UPDATE users SET prefixes = @prefixes WHERE id = '{session.GetHabbo().Id}'");
                            dbClient.AddParameter("prefixes", prefixStr);
                            await dbClient.RunQueryAsync();
                            await session.SendMessage(CatalogPageComposer.PurchaseOk(item, item.Items));

                            return;
                        }
                    }

                    case Interaction.NamePrefix:
                    {
                        session.GetHabbo().Prefixes[1] = $"[{extraData}]";
                        var prefixStr = string.Join(",", session.GetHabbo().Prefixes);
                        using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            dbClient.SetQuery(
                                $"UPDATE users SET prefixes = @prefixes WHERE id = '{session.GetHabbo().Id}'");
                            dbClient.AddParameter("prefixes", prefixStr);
                            await dbClient.RunQueryAsync();
                            await session.SendMessage(CatalogPageComposer.PurchaseOk(item, item.Items));

                            return;
                        }
                    }


                    case Interaction.ColorBubble:
                    {
                        session.GetHabbo().Prefixes[2] = item.Badge;
                        var prefixStr = string.Join(",", session.GetHabbo().Prefixes);
                        using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            dbClient.SetQuery(
                                $"UPDATE users SET prefixes = @prefixes WHERE id = '{session.GetHabbo().Id}'");
                            dbClient.AddParameter("prefixes", prefixStr);
                            await dbClient.RunQueryAsync();
                            await session.SendMessage(CatalogPageComposer.PurchaseOk(item, item.Items));

                            return;
                        }
                    }


                    case Interaction.PetDog:
                    case Interaction.PetCat:
                    case Interaction.PetCrocodile:
                    case Interaction.PetTerrier:
                    case Interaction.PetBear:
                    case Interaction.PetPig:
                    case Interaction.PetLion:
                    case Interaction.PetRhino:
                    case Interaction.PetSpider:
                    case Interaction.PetTurtle:
                    case Interaction.PetChick:
                    case Interaction.PetFrog:
                    case Interaction.PetDragon:
                    case Interaction.PetHorse:
                    case Interaction.PetMonkey:
                    case Interaction.PetGnomo:
                    case Interaction.PetMonsterPlant:
                    case Interaction.PetWhiteRabbit:
                    case Interaction.PetEvilRabbit:
                    case Interaction.PetLoveRabbit:
                    case Interaction.PetCafeRabbit:
                    case Interaction.PetPigeon:
                    case Interaction.PetEvilPigeon:
                    case Interaction.PetDemonMonkey:
                    case Interaction.Pet24:
                    case Interaction.Pet25:
                    case Interaction.Pet26:
                    case Interaction.Pet27:
                    case Interaction.Pet28:
                    case Interaction.Pet29:
                    case Interaction.Pet30:
                    case Interaction.Pet31:
                    case Interaction.Pet32:
                    case Interaction.Pet33:
                    case Interaction.Pet34:
                        var data = extraData.Split('\n');
                        var petName = data[0];
                        var race = data[1];
                        var color = data[2];

                        if (!CheckPetName(petName))
                            return;
                        if (race.Length != 1 && race.Length != 2)
                            return;
                        if (color.Length != 6)
                            return;
                        await Oblivion.GetGame().GetAchievementManager()
                            .ProgressUserAchievement(session, "ACH_PetLover", 1);
                        break;

                    case Interaction.Mannequin:
                        extraData = string.Concat("m", Convert.ToChar(5), "ch-215-92.lg-3202-1322-73",
                            Convert.ToChar(5),
                            "Mannequin");
                        break;

                    case Interaction.VipGate:
                    case Interaction.MysteryBox:
                    case Interaction.YoutubeTv:
                    case Interaction.TileStackMagic:
                    case Interaction.Tent:
                    case Interaction.BedTent:
                        break;

                    case Interaction.BadgeDisplay:
                        if (!session.GetHabbo().GetBadgeComponent().HasBadge(extraData))
                            extraData = "UMAD";

                        extraData =
                            $"{extraData}|{session.GetHabbo().UserName}|{DateTime.Now.Day:00}-{DateTime.Now.Month:00}-{DateTime.Now.Year}";
                        break;

                    case Interaction.FootballGate:
                        extraData = "hd-99999-99999.lg-270-62;hd-99999-99999.ch-630-62.lg-695-62";
                        break;

                    case Interaction.LoveLock:
                        extraData = "0";
                        break;

                    case Interaction.Pinata:
                    case Interaction.RunWaySage:
                    case Interaction.Shower:
                        extraData = "0";
                        break;

                    case Interaction.GroupForumTerminal:
                    case Interaction.GuildItem:
                    case Interaction.GuildGate:
                    case Interaction.GuildForum:
                    case Interaction.Poster:
                        break;

                    case Interaction.Moplaseed:
                        extraData = new Random().Next(0, 12).ToString();
                        break;

                    case Interaction.RareMoplaSeed:
                        extraData = new Random().Next(10, 12).ToString();
                        break;

                    case Interaction.MusicDisc:

                        var song = SoundMachineSongManager.GetSongById(item.SongId);

                        extraData = string.Empty;

                        if (song == null)
                            break;

                        extraData = string.Concat(session.GetHabbo().UserName, '\n', DateTime.Now.Year, '\n',
                            DateTime.Now.Month, '\n', DateTime.Now.Day, '\n', song.LengthSeconds, '\n', song.Name);

                        text = song.CodeName;

                        break;

                    default:
                        extraData = item.ExtraData;
                        break;
                }

                await session.GetMessageHandler().GetResponse()
                    .InitAsync(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));

                await session.GetMessageHandler().SendResponse();

                await session.SendMessage(CatalogPageComposer.PurchaseOk(item, item.Items));

                if (isGift)
                {
                    var itemBySprite = Oblivion.GetGame().GetItemManager().GetItemBySpriteId(giftSpriteId);

                    if (itemBySprite?.InteractionType != Interaction.Gift) return;

                    var guidId = Guid.NewGuid();
                    ShortGuid insertId = guidId;

                    using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        queryReactor.SetNoLockQuery("INSERT INTO items_rooms (id, base_item,user_id) VALUES ('" +
                                                    insertId + "'," +
                                                    itemBySprite.ItemId + ", " + toUserId + ");");

                        await queryReactor.RunQueryAsync();

                        queryReactor.SetQuery(string.Concat(
                            "INSERT INTO users_gifts (gift_id,item_id,extradata,giver_name,Message,ribbon,color,gift_sprite,show_sender,rare_id) VALUES ('",
                            insertId, "', ", baseItem.ItemId, ",@extradata, @name, @Message,", giftLazo, ",", giftColor,
                            ",", giftSpriteId, ",", undef ? 1 : 0, ",", limitedId, ");"));
                        queryReactor.AddParameter("extradata", extraData);
                        queryReactor.AddParameter("name", giftUser);
                        queryReactor.AddParameter("message", giftMessage);
                        await queryReactor.RunQueryAsync();

                        if (session.GetHabbo().Id != toUserId)
                        {
                            await Oblivion.GetGame().GetAchievementManager()
                                .ProgressUserAchievement(session, "ACH_GiftGiver", 1, true);
                            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.GiftOthers);

                            await queryReactor.RunFastQueryAsync(
                                "UPDATE users_stats SET gifts_given = gifts_given + 1 WHERE id = " +
                                session.GetHabbo().Id +
                                ";UPDATE users_stats SET gifts_received = gifts_received + 1 WHERE id = " + toUserId);
                        }
                    }

                    var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(toUserId);

                    if (clientByUserId != null)
                    {
                        await clientByUserId.GetHabbo().GetInventoryComponent().AddNewItem(insertId,
                            itemBySprite.ItemId,
                            string.Concat(session.GetHabbo().Id, (char)9, giftMessage, (char)9, giftLazo, (char)9,
                                giftColor, (char)9, ((undef) ? "1" : "0"), (char)9, session.GetHabbo().UserName,
                                (char)9, session.GetHabbo().Look, (char)9, item.Name), 0u, false, false, 0, 0);

                        if (clientByUserId.GetHabbo().Id != session.GetHabbo().Id)
                            await Oblivion.GetGame().GetAchievementManager()
                                .ProgressUserAchievement(clientByUserId, "ACH_GiftReceiver", 1, true);
                    }

                    session.GetHabbo().LastGiftPurchaseTime = DateTime.Now;

                    continue;
                }

                await session.GetMessageHandler().GetResponse()
                    .InitAsync(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));

                await session.GetMessageHandler().GetResponse().AppendIntegerAsync(1);

                var i = 1;

                if (baseItem.Type == 's')
                    i = InteractionTypes.AreFamiliar(GlobalInteractions.Pet, baseItem.InteractionType) ? 3 : 1;

                await session.GetMessageHandler().GetResponse().AppendIntegerAsync(i);

                var list = await DeliverItems(session, baseItem, priceAmount * (int)item.Items[baseItem], extraData,
                    limitedId,
                    limtot, text);

                await session.GetMessageHandler().GetResponse().AppendIntegerAsync(list.Count);

                /* TODO CHECK */
                foreach (var current3 in list)
                    await session.GetMessageHandler().GetResponse().AppendIntegerAsync(current3.VirtualId);

                await session.GetMessageHandler().SendResponse();
                await session.GetHabbo().GetInventoryComponent().UpdateItems(false);

                if (InteractionTypes.AreFamiliar(GlobalInteractions.Pet, baseItem.InteractionType))
                    await session.SendMessage(await session.GetHabbo().GetInventoryComponent().SerializePetInventory());
            }

            if (item.Badge.Length >= 1)
                await session.GetHabbo().GetBadgeComponent().GiveBadge(item.Badge, true, session);
        }

        /// <summary>
        ///     Delivers the items.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="limno">The limno.</param>
        /// <param name="limtot">The limtot.</param>
        /// <param name="songCode">The song code.</param>
        /// <returns>List&lt;UserItem&gt;.</returns>
        internal async Task<List<UserItem>> DeliverItems(GameClient session, Item item, int amount, string extraData,
            int limno,
            int limtot, string songCode)
        {
            var list = new List<UserItem>();

            var a = item.Type;
            if (a == 'i' || a == 's')
            {
                var i = 0;
                var subQuery = new StringBuilder();

                while (i < amount)
                {
                    var interactionType = item.InteractionType;

                    var itemGuid = Guid.NewGuid();
                    ShortGuid itemId = itemGuid;

                    switch (interactionType)
                    {
                        case Interaction.Dimmer:
                            var dimmer = new UserItem(itemId, item.ItemId, extraData, 0u, songCode, limno, limtot);

                            subQuery.Append(
                                $"INSERT INTO items_moodlight (item_id,enabled,current_preset,preset_one,preset_two,preset_three) VALUES ('{itemId}','0',1,'#000000,255,0','#000000,255,0','#000000,255,0');");

                            list.Add(dimmer);
                            break;

                        case Interaction.Teleport:
                        case Interaction.QuickTeleport:
                            var userItem = new UserItem(itemId, item.ItemId, extraData, 0u, songCode, limno, limtot);

                            var linkItemGuid = Guid.NewGuid();
                            ShortGuid linkedId = linkItemGuid;
                            var userItem2 = new UserItem(linkedId, item.ItemId, extraData, 0u, songCode, limno, limtot);


                            list.Add(userItem);
                            list.Add(userItem2);

                            subQuery.Append(
                                $"INSERT INTO items_teleports (tele_one_id,tele_two_id) VALUES ('{itemId}','{linkedId}'), ('{linkedId}','{itemId}');");

                            break;

                        case Interaction.PetDog:
                        case Interaction.PetCat:
                        case Interaction.PetCrocodile:
                        case Interaction.PetTerrier:
                        case Interaction.PetBear:
                        case Interaction.PetPig:
                        case Interaction.PetLion:
                        case Interaction.PetRhino:
                        case Interaction.PetSpider:
                        case Interaction.PetTurtle:
                        case Interaction.PetChick:
                        case Interaction.PetFrog:
                        case Interaction.PetDragon:
                        case Interaction.PetHorse:
                        case Interaction.PetMonkey:
                        case Interaction.PetGnomo:
                        case Interaction.PetMonsterPlant:
                        case Interaction.PetWhiteRabbit:
                        case Interaction.PetEvilRabbit:
                        case Interaction.PetLoveRabbit:
                        case Interaction.PetPigeon:
                        case Interaction.PetEvilPigeon:
                        case Interaction.PetDemonMonkey:
                        case Interaction.Pet24:
                        case Interaction.Pet25:
                        case Interaction.Pet26:
                        case Interaction.Pet27:
                        case Interaction.Pet28:
                        case Interaction.Pet29:
                        case Interaction.Pet30:
                        case Interaction.Pet31:
                        case Interaction.Pet32:
                        case Interaction.Pet33:
                        case Interaction.Pet34:
                            var petData = extraData.Split('\n');
                            var petId = int.Parse(item.Name.Replace("a0 pet", string.Empty));
                            var generatedPet = await CreatePet(session.GetHabbo().Id, petData[0], petId, petData[1],
                                petData[2]);

                            session.GetHabbo().GetInventoryComponent().AddPet(generatedPet);


//                            list.Add(session.GetHabbo().GetInventoryComponent()
//                                .AddNewItem("0", 1534, "0", 0u, true, false, 0, 0, string.Empty));
                            break;


                        case Interaction.RoomBg:

                            var roomBg = new UserItem(itemId, item.ItemId, extraData, 0u, songCode, limno, limtot);


                            list.Add(roomBg);

                            subQuery.Append($"INSERT INTO items_toners VALUES ('{itemId}','0',0,0,0);");

                            break;

                        case Interaction.GuildItem:
                        case Interaction.GuildGate:
                        case Interaction.GroupForumTerminal:
                            if (!uint.TryParse(extraData, out var extra))
                            {
                                extra = 0;
                            }

                            list.Add(new UserItem(itemId, item.ItemId, extraData, extra, songCode,
                                limno, limtot));
                            break;

                        case Interaction.GuildForum:

                            uint.TryParse(extraData, out var groupId);

                            var group = Oblivion.GetGame().GetGroupManager().GetGroup(groupId);

                            if (group != null)
                            {
                                if (group.CreatorId == session.GetHabbo().Id)
                                {
                                    await session.GetMessageHandler().GetResponse()
                                        .InitAsync(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                                    await session.GetMessageHandler().GetResponse().AppendStringAsync("forums.delivered");
                                    await session.GetMessageHandler().GetResponse().AppendIntegerAsync(2);
                                    await session.GetMessageHandler().GetResponse().AppendStringAsync("groupId");
                                    await session.GetMessageHandler().GetResponse().AppendStringAsync(extraData);
                                    await session.GetMessageHandler().GetResponse().AppendStringAsync("groupName");
                                    await session.GetMessageHandler().GetResponse().AppendStringAsync(group.Name);
                                    await session.GetMessageHandler().SendResponse();

                                    if (!group.HasForum)
                                    {
                                        group.HasForum = true;
                                        await group.UpdateForum();
                                    }
                                }
                                else
                                    await session.SendNotif(Oblivion.GetLanguage().GetVar("user_group_owner_error"));
                            }

                            list.Add(new UserItem(itemId, item.ItemId, extraData, Convert.ToUInt32(extraData), songCode,
                                limno, limtot));

                            break;

                        default:
                            list.Add(new UserItem(itemId, item.ItemId, extraData, 0u, songCode, limno, limtot));
                            break;
                    }

                    i++;
                }


                if (list.Count <= 0) return list;
                var query = new StringBuilder();
                query.Append(
                    "INSERT INTO items_rooms (id, base_item, user_id,group_id,extra_data, songcode, limited) VALUES ");
                var position = 0;
                foreach (var addedItem in list)
                {
                    var groupId = (addedItem.GroupId) <= 0 ? "NULL" : $"'{addedItem.GroupId}'";
                    position++;

                    query.Append(
                        position >= list.Count
                            ? $"('{addedItem.Id}', '{addedItem.BaseItem.ItemId}', '{session.GetHabbo().Id}', {groupId}, @edata, '{songCode}', '{limno};{limtot}');"
                            : $"('{addedItem.Id}', '{addedItem.BaseItem.ItemId}', '{session.GetHabbo().Id}', {groupId}, @edata, '{songCode}', '{limno};{limtot}'),");

                    session.GetHabbo().GetInventoryComponent().AddNewItem(addedItem);
                }

                using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    dbClient.SetNoLockQuery(query.ToString());
                    dbClient.AddParameter("edata", extraData);

                    await dbClient.RunQueryAsync();

                    if (subQuery.Length > 0)
                    {
                        await dbClient.RunFastQueryAsync(subQuery.ToString());
                    }
                }


                return list;
            }

            else if (a == 'e')
            {
                for (var j = 0; j < amount; j++)
                    await session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(item.SpriteId, 7200, 0);
            }
            else if (a == 'r')
            {
                if (item.Name == "bot_bartender")
                {
                    var bot = CreateBot(session.GetHabbo().Id, "Mahw",
                        "hr-9534-39.hd-600-1.ch-819-92.lg-3058-64.sh-3064-110.wa-2005",
                        "Sacia a sede e você pode dançar!", "f", true);
                    session.GetHabbo().GetInventoryComponent().AddBot(bot);
                    await session.SendMessageAsync(await session.GetHabbo().GetInventoryComponent()
                        .SerializeBotInventory());
                }
                else
                {
                    var bot2 = CreateBot(session.GetHabbo().Id, "Claudio",
                        "hr-3020-34.hd-3091-2.ch-225-92.lg-3058-100.sh-3089-1338.ca-3084-78-108.wa-2005",
                        "Fala, caminhadas, danças e vestidos", "m", false);
                    session.GetHabbo().GetInventoryComponent().AddBot(bot2);
                    await session.SendMessageAsync(await session.GetHabbo().GetInventoryComponent()
                        .SerializeBotInventory());
                }
            }

            return list;
        }

        /// <summary>
        ///     Gets the random ecotron reward.
        /// </summary>
        /// <returns>EcotronReward.</returns>
        internal EcotronReward GetRandomEcotronReward()
        {
            var level = 1u;

            if (Oblivion.GetRandomNumber(1, 2000) < 20)
                level = 5u;
            else if (Oblivion.GetRandomNumber(1, 1000) < 15)
                level = 4u;
            else if (Oblivion.GetRandomNumber(1, 150) < 10)
                level = 3u;
            else if (Oblivion.GetRandomNumber(1, 50) < 5)
                level = 2u;

            var ecotronRewardsForLevel = GetEcotronRewardsForLevel(level);

            return ecotronRewardsForLevel[Oblivion.GetRandomNumber(0, (ecotronRewardsForLevel.Count - 1))];
        }

        /// <summary>
        ///     Gets the ecotron rewards.
        /// </summary>
        /// <returns>List&lt;EcotronReward&gt;.</returns>
        internal List<EcotronReward> GetEcotronRewards() => EcotronRewards;

        /// <summary>
        ///     Gets the ecotron rewards levels.
        /// </summary>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        internal List<int> GetEcotronRewardsLevels() => EcotronLevels;

        /// <summary>
        ///     Gets the ecotron rewards for level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>List&lt;EcotronReward&gt;.</returns>
        internal List<EcotronReward> GetEcotronRewardsForLevel(uint level)
        {
            return EcotronRewards.Where(current => current.RewardLevel == level).ToList();
        }

        /// <summary>
        /// get the marketplace manager
        /// </summary>
        /// <returns></returns>
        public MarketplaceManager GetMarketplace() => _marketplace;
    }
}