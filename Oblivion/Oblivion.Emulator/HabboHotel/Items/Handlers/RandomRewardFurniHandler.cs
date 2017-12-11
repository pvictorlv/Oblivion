using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Items.Handlers
{
    public class RandomRewardFurniHandler
    {
        private Dictionary<int, Dictionary<int, string>> _furnis;

        public RandomRewardFurniHandler()
        {
            _furnis = new Dictionary<int, Dictionary<int, string>>();

            DataTable table;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM crackable_rewards");
                table = dbClient.GetTable();
            }
            if (table == null) return;
            foreach (DataRow row in table.Rows)
            {
                var campaing = Convert.ToInt32(row["campaing"]);
                var level = Convert.ToInt32(row["egg_level"]);

                if (_furnis.TryGetValue(campaing, out var items))
                {
                    items.Add(level, row["items"].ToString());
                    _furnis[campaing] = items;
                    continue;
                }

                items = new Dictionary<int, string> {{level, row["items"].ToString()}};
                _furnis.Add(campaing, items);
            }
        }


        internal uint GetRandomPrize(int campaing, int level)
        {
            if (!_furnis.TryGetValue(campaing, out var furnis))
            {
                return 0;
            }
            //
            if (!furnis.TryGetValue(level, out var itemsString))
            {
                return 0;
            }
            var items = itemsString.Split(',');

            var rnd = Oblivion.GetRandomNumber(0, items.Length - 1);
            var randomItem = items[rnd];
            return Convert.ToUInt32(randomItem);
        }


        #region Crackable Items

        internal int MaxCracks(string itemName)
        {
            //todo: piñatas
            switch (itemName)
            {
                case "easter13_egg_0":
                    return 1000;

                case "easter13_egg_1":
                    return 5000;

                case "easter13_egg_2":
                    return 10000;

                case "easter13_egg_3":
                    return 20000;

                default:
                    return 10;
            }
        }

        internal ServerMessage GetEggServerMessage(ServerMessage message, RoomItem item)
        {
            var cracks = 0;
            var cracksMax = MaxCracks(item.GetBaseItem().Name);

            if (Oblivion.IsNum(item.ExtraData))
                cracks = Convert.ToInt16(item.ExtraData);

            var state = "0";

            if (cracks >= cracksMax)
                state = "14";
            else if (cracks >= cracksMax * 6 / 7)
                state = "12";
            else if (cracks >= cracksMax * 5 / 7)
                state = "10";
            else if (cracks >= cracksMax * 4 / 7)
                state = "8";
            else if (cracks >= cracksMax * 3 / 7)
                state = "6";
            else if (cracks >= cracksMax * 2 / 7)
                state = "4";
            else if (cracks >= cracksMax * 1 / 7)
                state = "2";

            message.AppendInteger(7);
            message.AppendString(state); //state (0-7)
            message.AppendInteger(cracks); //actual
            message.AppendInteger(cracksMax); //max

            return message;
        }

        #endregion
    }
}