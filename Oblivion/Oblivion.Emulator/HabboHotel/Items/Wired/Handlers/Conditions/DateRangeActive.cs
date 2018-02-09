﻿using System.Collections.Generic;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class DateRangeActive : IWiredItem
    {
        public DateRangeActive(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            OtherString = string.Empty;
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ConditionDateRangeActive;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return ""; }
            set { }
        }

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
            

            int date1;
            var date2 = 0;

            var strArray = OtherString.Split(',');

            if (string.IsNullOrWhiteSpace(strArray[0]))
                return false;

            int.TryParse(strArray[0], out date1);

            if (strArray.Length > 1)
                int.TryParse(strArray[1], out date2);

            if (date1 == 0)
                return false;

            var currentTimestamp = Oblivion.GetUnixTimeStamp();

            return date2 < 1 ? currentTimestamp >= date1 : currentTimestamp >= date1 && currentTimestamp <= date2;
        }
    }
}