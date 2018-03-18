#region

using System;
using System.Collections.Generic;

#endregion

namespace Oblivion.HabboHotel.Catalogs.Marketplace
{
    public class MarketplaceManager
    {
        public Dictionary<int, int> MarketAverages = new Dictionary<int, int>();
        public Dictionary<int, int> MarketCounts = new Dictionary<int, int>();
        public HashSet<int> MarketItemKeys = new HashSet<int>();
        public List<MarketOffer> MarketItems = new List<MarketOffer>();

        public int AvgPriceForSprite(int spriteId)
        {
            int num;
            int num2;
            if (MarketAverages.TryGetValue(spriteId, out var marketAvg))
            {
                if (!MarketCounts.TryGetValue(spriteId, out var makertCounts)) return 0;
                if (makertCounts > 0)
                    return marketAvg / makertCounts;
                return 0;
            }

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = '" + spriteId +
                                  "' LIMIT 1");
                num = dbClient.GetInteger();

                dbClient.SetQuery("SELECT `sold` FROM `catalog_marketplace_data` WHERE `sprite` = '" + spriteId +
                                  "' LIMIT 1");
                num2 = dbClient.GetInteger();
            }


            MarketAverages.Add(spriteId, num);
            MarketCounts.Add(spriteId, num2);

            return num2 > 0 ? Convert.ToInt32(Math.Ceiling((double) (num / num2))) : 0;
        }

        public string FormatTimestampString() => FormatTimestamp().ToString().Split(',')[0];

        public double FormatTimestamp() => Oblivion.GetUnixTimeStamp() - 172800.0;

        public int OfferCountForSprite(int spriteId)
        {
            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();

            foreach (var item in MarketItems)
                if (dictionary.TryGetValue(item.SpriteId, out var makertItem))
                {
                    if (makertItem.TotalPrice > item.TotalPrice)
                    {
                        dictionary[item.SpriteId] = item;
                    }

                     dictionary2[item.SpriteId]++;
                }
                else
                {
                    dictionary.Add(item.SpriteId, item);
                    dictionary2.Add(item.SpriteId, 1);
                }
            return dictionary2.TryGetValue(spriteId, out var dicItem) ? dicItem : 0;
        }

        public int CalculateComissionPrice(float sellingPrice) => Convert.ToInt32(Math.Ceiling(sellingPrice / 100 * 1));
    }
}