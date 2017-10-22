﻿using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class ItemsCoincide : IWiredItem
    {
        public ItemsCoincide(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ConditionItemsMatches;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

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

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;

            bool useExtradata, useRot, usePos;

            Dictionary<uint, string[]> itemsOriginalData;

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
                    .ToDictionary(array => uint.Parse(array[0]), array => array.Skip(1).ToArray());
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
                return false;
            }

            /* TODO CHECK */ foreach (var current in Items)
            {
                if (current == null || !itemsOriginalData.ContainsKey(current.Id))
                    return false;

                var originalData = itemsOriginalData[current.Id];

                if (useRot)
                    if (current.Rot != int.Parse(originalData[1]))
                        return false;

                if (useExtradata)
                {
                    if (current.ExtraData == string.Empty)
                        current.ExtraData = "0";

                    if (current.ExtraData != (originalData[0] == string.Empty ? "0" : originalData[0]))
                        return false;
                }

                if (!usePos)
                    continue;

                var originalPos = originalData[2].Split(',');

                if (current.X != int.Parse(originalPos[0]) || current.Y != int.Parse(originalPos[1]))
                    return false;
            }

            return true;
        }
    }
}