using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Catalogs.Marketplace;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages.Parsers;

namespace Oblivion.Messages.Handlers
{
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        ///     Get marketplace offers
        /// </summary>
        public void GetOffers()
        {
            if (Session?.GetHabbo() == null)
                return;
            var minCost = Request.GetInteger();
            var maxCost = Request.GetInteger();
            var searchQuery = Request.GetString();
            var filterMode = Request.GetInteger();


            DataTable table;
            var builder = new StringBuilder();
            string str;
            builder.Append("WHERE `state` = '1' AND `timestamp` >= " +
                           Oblivion.GetGame().GetCatalog().GetMarketplace().FormatTimestampString());
            if (minCost >= 0)
                builder.Append(" AND `total_price` > " + minCost);


            if (maxCost >= 0)
                builder.Append(" AND `total_price` < " + maxCost);

            switch (filterMode)
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
                dbClient.AddParameter("search_query", "%" + searchQuery + "%");
                if (searchQuery.Length >= 1)
                    builder.Append(" AND public_name LIKE @search_query");
                table = dbClient.GetTable();
            }

            Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            if (table != null)
                /* TODO CHECK */
                foreach (DataRow row in table.Rows)
                {
                    if (Oblivion.GetGame()
                        .GetCatalog()
                        .GetMarketplace()
                        .MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"]))) continue;
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Add(Convert.ToInt32(row["offer_id"]));
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems.Add(new MarketOffer(Convert.ToInt32(row["offer_id"]), Convert.ToInt32(row["sprite_id"]), Convert.ToInt32(row["total_price"]), int.Parse(row["item_type"].ToString()), Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"])));
                }

            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();

            /* TODO CHECK */
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
            /* TODO CHECK */
            if (dictionary.Count > 0)
                foreach (var pair in dictionary.Values/*.Where(x => x.TotalPrice >= minCost && x.TotalPrice <= maxCost)*/)
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
            await Session.SendMessageAsync(message);
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
            await SendResponse();
        }


        public void PurchaseOffer()
        {
            var OfferId = Request.GetInteger();

            DataRow Row;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `state`,`timestamp`,`total_price`,`extra_data`,`item_id`,`furni_id`,`user_id`,`limited_number`,`limited_stack` FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId LIMIT 1");
                dbClient.AddParameter("OfferId", OfferId);
                Row = dbClient.GetRow();
            }

            if (Row == null)
            {
                ReloadOffers(Session);
                return;
            }

            if (Convert.ToString(Row["state"]) == "2")
            {
                await Session.SendNotifyAsync("Opa, esta oferta não é mais válida.");
                ReloadOffers(Session);
                return;
            }

            if (Oblivion.GetGame().GetCatalog().GetMarketplace().FormatTimestamp() >
                Convert.ToDouble(Row["timestamp"]))
            {
                await Session.SendNotifyAsync("Que pena, essa oferta expirou.");
                ReloadOffers(Session);
                return;
            }

            var Item = Oblivion.GetGame().GetItemManager().GetItem(Convert.ToUInt32(Row["item_id"]));
            if (Item == null)
            {
                await Session.SendNotifyAsync("Este item não existe mais aqui.");
                ReloadOffers(Session);
                return;
            }
            if (Convert.ToInt32(Row["user_id"]) == Session.GetHabbo().Id)
            {
                await Session.SendNotifyAsync(
                    "Para evitar o aumento médio você não pode comprar suas próprias ofertas do mercado.");
                return;
            }

            if (Convert.ToInt32(Row["total_price"]) > Session.GetHabbo().Diamonds)
            {
                await Session.SendNotifyAsync("Oops, you do not have enough diamonds for this.");
                return;
            }
            var price = Convert.ToInt32(Row["total_price"]);
            Session.GetHabbo().Diamonds -= price;
            Session.GetHabbo().UpdateSeasonalCurrencyBalance();


            Session.GetHabbo().GetInventoryComponent().AddNewItem("0", Item.ItemId,
                Convert.ToString(Row["extra_data"]), 0, true, false, Convert.ToInt32(Row["limited_number"]),
                Convert.ToInt32(Row["limited_stack"]));
            Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            await Session.SendMessageAsync(CatalogPageComposer.PurchaseOk(Convert.ToUInt32(Row["furni_id"]), Item.PublicName,
                (uint) price));


            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `catalog_marketplace_offers` SET `state` = '2' WHERE `offer_id` = '" +
                                  OfferId +
                                  "' LIMIT 1");

                dbClient.SetQuery("SELECT `id` FROM catalog_marketplace_data WHERE sprite = " + Item.SpriteId +
                                  " LIMIT 1;");
                var Id = dbClient.GetInteger();

                if (Id > 0)
                    dbClient.RunQuery(
                        "UPDATE `catalog_marketplace_data` SET `sold` = `sold` + 1, `avgprice` = (avgprice + " +
                        Convert.ToInt32(Row["total_price"]) + ") WHERE `id` = " + Id + " LIMIT 1;");
                else
                    dbClient.RunQuery("INSERT INTO `catalog_marketplace_data` (sprite, sold, avgprice) VALUES ('" +
                                      Item.SpriteId + "', '1', '" + Convert.ToInt32(Row["total_price"]) + "')");


                if (Oblivion.GetGame().GetCatalog().GetMarketplace().MarketAverages.TryGetValue(Item.SpriteId, out var num3) &&
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketCounts.TryGetValue(Item.SpriteId, out var num5))
                {
                    var num4 = num5 + Convert.ToInt32(Row["total_price"]);

                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketAverages.Remove(Item.SpriteId);
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketAverages.Add(Item.SpriteId, num4);
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketCounts.Remove(Item.SpriteId);
                    Oblivion.GetGame().GetCatalog().GetMarketplace().MarketCounts.Add(Item.SpriteId, num3 + 1);
                }
                else
                {
                    if (
                        !Oblivion.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketAverages.ContainsKey(Item.SpriteId))
                        Oblivion.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketAverages.Add(Item.SpriteId, Convert.ToInt32(Row["total_price"]));

                    if (!Oblivion.GetGame().GetCatalog().GetMarketplace().MarketCounts.ContainsKey(Item.SpriteId))
                        Oblivion.GetGame().GetCatalog().GetMarketplace().MarketCounts.Add(Item.SpriteId, 1);
                }
            }

            ReloadOffers(Session);
        }


        private static void ReloadOffers(GameClient session)
        {
            const string SearchQuery = "";
            DataTable table;
            var builder = new StringBuilder();
            builder.Append("WHERE state = '1' AND timestamp >= " +
                           Oblivion.GetGame().GetCatalog().GetMarketplace().FormatTimestampString());
            const string str = "ORDER BY asking_price DESC";

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `offer_id`,`item_type`,`sprite_id`,`total_price`,`limited_number`,`limited_stack` FROM `catalog_marketplace_offers` " +
                    builder + " " + str + " LIMIT 500");
                dbClient.AddParameter("search_query", "%" + SearchQuery + "%");
                if (SearchQuery.Length >= 1)
                    builder.Append(" AND `public_name` LIKE @search_query");
                table = dbClient.GetTable();
            }

            Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            if (table != null)
                foreach (DataRow row in table.Rows)
                    if (
                        !Oblivion.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"])))
                    {
                        var item = new MarketOffer(Convert.ToInt32(row["offer_id"]), Convert.ToInt32(row["sprite_id"]),
                            Convert.ToInt32(row["total_price"]), int.Parse(row["item_type"].ToString()),
                            Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]));
                        Oblivion.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketItemKeys.Add(Convert.ToInt32(row["offer_id"]));
                        Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems.Add(item);
                    }

            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();

            foreach (var item in Oblivion.GetGame().GetCatalog().GetMarketplace().MarketItems)
                if (dictionary.TryGetValue(item.SpriteId, out var it))
                {
                    if (it.TotalPrice > item.TotalPrice)
                    {
                        dictionary[item.SpriteId] = item;
                    }

                    var num = dictionary2[item.SpriteId];
                    dictionary2.Remove(item.SpriteId);
                    dictionary2.Add(item.SpriteId, num + 1);
                }
                else
                {
                    dictionary.Add(item.SpriteId, item);
                    dictionary2.Add(item.SpriteId, 1);
                }

            var msg = new ServerMessage(LibraryParser.OutgoingRequest("MarketPlaceOffersMessageComposer"));
            msg.AppendInteger(dictionary.Count);
            if (dictionary.Count > 0)
                foreach (var pair in dictionary)
                {
                    msg.AppendInteger(pair.Value.OfferId);
                    msg.AppendInteger(1); //State
                    msg.AppendInteger(1);
                    msg.AppendInteger(pair.Value.SpriteId);

                    msg.AppendInteger(256);
                    msg.AppendString("");
                    msg.AppendInteger(pair.Value.LimitedNumber);
                    msg.AppendInteger(pair.Value.LimitedStack);

                    msg.AppendInteger(pair.Value.TotalPrice);
                    msg.AppendInteger(0);
                    msg.AppendInteger(
                        Oblivion.GetGame().GetCatalog().GetMarketplace().AvgPriceForSprite(pair.Value.SpriteId));
                    msg.AppendInteger(dictionary2[pair.Value.SpriteId]);
                }
            msg.AppendInteger(dictionary.Count);
            session.SendMessage(msg);
        }

        public void CancelOffer()
        {
            if (Session?.GetHabbo() == null)
                return;

            DataRow row;
            var offerId = Request.GetInteger();

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT furni_id, item_id, user_id, extra_data, offer_id, state, timestamp, limited_number, limited_stack FROM catalog_marketplace_offers WHERE offer_id = @OfferId LIMIT 1");
                dbClient.AddParameter("OfferId", offerId);
                row = dbClient.GetRow();
            }

            if (row == null)
            {
                var msg = new ServerMessage(
                    LibraryParser.OutgoingRequest("MarketplaceCancelOfferResultMessageComposer"));
                msg.AppendInteger(offerId);
                msg.AppendBool(false);
                await Session.SendMessageAsync(msg);
                return;
            }

            if (Convert.ToInt32(row["user_id"]) != Session.GetHabbo().Id)
            {
                var msg = new ServerMessage(
                    LibraryParser.OutgoingRequest("MarketplaceCancelOfferResultMessageComposer"));
                msg.AppendInteger(offerId);
                msg.AppendBool(false);
                await Session.SendMessageAsync(msg);

                return;
            }

            var item = Oblivion.GetGame().GetItemManager().GetItem(Convert.ToUInt32(row["item_id"]));
            if (item == null)
            {
                var msg = new ServerMessage(
                    LibraryParser.OutgoingRequest("MarketplaceCancelOfferResultMessageComposer"));
                msg.AppendInteger(offerId);
                msg.AppendBool(false);
                await Session.SendMessageAsync(msg);
                return;
            }

            var userItem = Session.GetHabbo().GetInventoryComponent().AddNewItem("0", item.ItemId,
                Convert.ToString(row["extra_data"]), 0, true, false, Convert.ToInt32(row["limited_number"]),
                Convert.ToInt32(row["limited_stack"]));
            Session.GetHabbo().GetInventoryComponent().AddItemToItemInventory(userItem);
//            Session.GetHabbo().GetInventoryComponent().UpdateItems(true);

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "DELETE FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId AND `user_id` = @UserId LIMIT 1");
                dbClient.AddParameter("OfferId", offerId);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            var sucessMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("MarketplaceCancelOfferResultMessageComposer"));
            sucessMessage.AppendInteger(offerId);
            sucessMessage.AppendBool(true);
            await Session.SendMessageAsync(sucessMessage);
        }


        /// <summary>
        ///     Check if user can make offer
        /// </summary>
        public void CanMakeOffer()
        {
            if (Session?.GetHabbo() == null)
                return;
            var errorCode = Session.GetHabbo().TradeLockExpire > 0 ? 6 : 1;

            var message =
                new ServerMessage(LibraryParser.OutgoingRequest("MarketplaceCanMakeOfferResultMessageComposer"));
            message.AppendInteger(errorCode);
            message.AppendInteger(0);
            message.AppendInteger(0);
            await Session.SendMessageAsync(message);
        }


        /// <summary>
        ///     Configure marketplace
        /// </summary>
        public void MarketPlaceConfiguration()
        {
            if (Session?.GetHabbo() == null)
                return;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("MarketplaceConfigurationMessageComposer"));
            message.AppendBool(true);
            message.AppendInteger(1);
            message.AppendInteger(0);
            message.AppendInteger(0);
            message.AppendInteger(1);
            message.AppendInteger(99999999);
            message.AppendInteger(48);
            message.AppendInteger(7);
            await Session.SendMessageAsync(message);
        }


        public void GetItemStats()
        {
            var ItemId = Request.GetInteger();
            var SpriteId = Request.GetInteger();

            DataRow Row;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = @SpriteId LIMIT 1");
                dbClient.AddParameter("SpriteId", SpriteId);
                Row = dbClient.GetRow();
            }

            var msg = new ServerMessage(LibraryParser.OutgoingRequest("MarketplaceItemStatsMessageComposer"));
            msg.AppendInteger(Row != null ? Convert.ToInt32(Row["avgprice"]) : 0);
            msg.AppendInteger(Oblivion.GetGame().GetCatalog().GetMarketplace().OfferCountForSprite(SpriteId));
            msg.AppendInteger(0);
            msg.AppendInteger(0);
            msg.AppendInteger(ItemId);
            msg.AppendInteger(SpriteId);
            await Session.SendMessageAsync(msg);
        }

        public void GetMyOffers()
        {
            if (Session.GetHabbo() == null) return;

            var userId = Session.GetHabbo().Id;

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT timestamp, state, offer_id, item_type, sprite_id, total_price, limited_number, limited_stack FROM catalog_marketplace_offers WHERE user_id = '" +
                    userId + "'");
                var table = dbClient.GetTable();

                dbClient.SetQuery(
                    "SELECT SUM(asking_price) FROM catalog_marketplace_offers WHERE state = '2' AND user_id = '" +
                    userId + "'");
                var i = dbClient.GetInteger();
                var msg = new ServerMessage(LibraryParser.OutgoingRequest("MarketPlaceOwnOffersMessageComposer"));
                msg.AppendInteger(i);
                if (table != null)
                {
                    msg.AppendInteger(table.Rows.Count);
                    foreach (DataRow row in table.Rows)
                    {
                        var num2 =
                            Convert.ToInt32(
                                Math.Floor(((double) row["timestamp"] + 172800.0 - Oblivion.GetUnixTimeStamp()) /
                                           60.0));
                        var num3 = int.Parse(row["state"].ToString());
                        if (num2 <= 0 && num3 != 2)
                        {
                            num3 = 3;
                            num2 = 0;
                        }
                        msg.AppendInteger(Convert.ToInt32(row["offer_id"]));
                        msg.AppendInteger(num3);
                        msg.AppendInteger(1);
                        msg.AppendInteger(Convert.ToInt32(row["sprite_id"]));

                        msg.AppendInteger(256);
                        msg.AppendString("");
                        msg.AppendInteger(Convert.ToInt32(row["limited_number"]));
                        msg.AppendInteger(Convert.ToInt32(row["limited_stack"]));

                        msg.AppendInteger(Convert.ToInt32(row["total_price"]));
                        msg.AppendInteger(num2);
                        msg.AppendInteger(Convert.ToInt32(row["sprite_id"]));
                    }
                }
                else
                {
                    msg.AppendInteger(0);
                }
                await Session.SendMessageAsync(msg);
            }
        }

        public void MakeOffer()
        {
            var SellingPrice = Request.GetInteger();
            Request.GetInteger();
            var ItemId = Oblivion.GetGame().GetItemManager().GetRealId(Request.GetUInteger());

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("MarketplaceMakeOfferResultMessageComposer"));

            if (Item == null)
            {
                msg.AppendInteger(0);
                await Session.SendMessageAsync(msg);
                return;
            }

            if (SellingPrice > 70000000 || SellingPrice <= 0)
            {
                msg.AppendInteger(0);
                await Session.SendMessageAsync(msg);
                return;
            }

            var Comission = Oblivion.GetGame().GetCatalog().GetMarketplace()
                .CalculateComissionPrice(SellingPrice);
            var TotalPrice = SellingPrice + Comission;
            var ItemType = 1;
            if (Item.BaseItem.Type == 'i')
                ItemType++;

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `catalog_marketplace_offers` (`furni_id`,`item_id`,`user_id`,`asking_price`,total_price,public_name,sprite_id,item_type,timestamp,extra_data,limited_number,limited_stack) VALUES ('" +
                    ItemId + "','" + Item.BaseItem.ItemId + "','" + Session.GetHabbo().Id + "','" + SellingPrice +
                    "','" +
                    TotalPrice + "',@public_name,'" + Item.BaseItem.SpriteId + "','" + ItemType + "','" +
                    Oblivion.GetUnixTimeStamp() + "',@extra_data, '" + Item.LimitedSellId + "', '" + Item.LimitedStack +
                    "')");
                dbClient.AddParameter("public_name", Item.BaseItem.PublicName);
                dbClient.AddParameter("extra_data", Item.ExtraData);
                dbClient.RunQuery();

                dbClient.RunQuery("DELETE FROM `items_rooms` WHERE `id` = '" + ItemId + "' AND `user_id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
            }

            Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId, false, 0);

            msg.AppendInteger(1);
            await Session.SendMessageAsync(msg);
        }

        public void ReedemCredits()
        {
            var CreditsOwed = 0;

            DataTable Table;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `asking_price` FROM `catalog_marketplace_offers` WHERE `user_id` = '" +
                                  Session.GetHabbo().Id + "' AND state = '2'");
                Table = dbClient.GetTable();
            }

            if (Table != null)
            {
                CreditsOwed += Table.Rows.Cast<DataRow>().Sum(row => Convert.ToInt32(row["asking_price"]));

                if (CreditsOwed >= 1)
                {
                    Session.GetHabbo().Diamonds += CreditsOwed;
                    Session.GetHabbo().UpdateSeasonalCurrencyBalance();
                }

                using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `catalog_marketplace_offers` WHERE `user_id` = '" +
                                      Session.GetHabbo().Id + "' AND `state` = '2'");
                }
            }
        }
    }
}