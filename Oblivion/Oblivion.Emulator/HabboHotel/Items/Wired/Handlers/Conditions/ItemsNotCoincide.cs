using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class ItemsNotCoincide : IWiredItem
    {
        public ItemsNotCoincide(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ConditionItemsDontMatch;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public bool Execute(params object[] stuff)
        {
            

            if (Items == null || Items.Count <= 0)
                return true;

            bool useExtradata, useRot, usePos;

            Dictionary<string, string[]> itemsOriginalData;

            try
            {
                if (string.IsNullOrWhiteSpace(OtherString) || !OtherString.Contains(",") ||
                    !OtherExtraString.Contains("|"))
                    return false;

                var booleans = OtherString.ToLower().Split(',');

                useExtradata = booleans[0] == "true";
                useRot = booleans[1] == "true";
                usePos = booleans[2] == "true";

                itemsOriginalData = OtherExtraString.Split('/').Select(data => data.Split('|'))
                    .ToDictionary(array => array[0], array => array.Skip(1).ToArray());
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());

                return false;
            }

            /* TODO CHECK */ foreach (var current in Items)
            {
                if (current == null || !itemsOriginalData.TryGetValue(current.Id, out var originalData))
                    return false;


                if (useRot)
                    if (current.Rot != int.Parse(originalData[1]))
                        return true;

                if (useExtradata)
                {
                    if (current.ExtraData == string.Empty)
                        current.ExtraData = "0";

                    if (current.ExtraData != (originalData[0] == string.Empty ? "0" : originalData[0]))
                        return true;
                }

                if (!usePos)
                    continue;

                var originalPos = originalData[2].Split(',');

                if (current.X != int.Parse(originalPos[0]) && current.Y != int.Parse(originalPos[1]))
                    return true;
            }

            return false;
        }
    }
}