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
        public List<int> MarketItemKeys = new List<int>();
        public List<MarketOffer> MarketItems = new List<MarketOffer>();

        public int AvgPriceForSprite(int spriteId)
        {
            int num;
            int num2;
            if (MarketAverages.ContainsKey(spriteId) && MarketCounts.ContainsKey(spriteId))
            {
                if (MarketCounts[spriteId] > 0)
                    return MarketAverages[spriteId] / MarketCounts[spriteId];
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

            return num2 > 0 ? Convert.ToInt32(Math.Ceiling((double)(num / num2))) : 0;
        }

        public string FormatTimestampString() => FormatTimestamp().ToString().Split(',')[0];

        public double FormatTimestamp() => Oblivion.GetUnixTimeStamp() - 172800.0;

        public int OfferCountForSprite(int spriteId)
        {
            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();
            /* TODO CHECK */ foreach (var item in MarketItems)
                if (dictionary.ContainsKey(item.SpriteId))
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
                else
                {
                    dictionary.Add(item.SpriteId, item);
                    dictionary2.Add(item.SpriteId, 1);
                }
            return dictionary2.ContainsKey(spriteId) ? dictionary2[spriteId] : 0;
        }

        public int CalculateComissionPrice(float sellingPrice) => Convert.ToInt32(Math.Ceiling(sellingPrice / 100 * 1));
    }
}