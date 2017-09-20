﻿using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class TriggererNotOnFurni : IWiredItem
    {
        public TriggererNotOnFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionTriggererNotOnFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return ""; }
            set { }
        }

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
            if (!Items.Any())
                return true;

            var roomUser = stuff?[0] as RoomUser;

            if (roomUser == null)
                return false;

            foreach (var current in Items.Where(current => current != null &&
                                                           Room.GetRoomItemHandler().FloorItems.Contains(current)))
            {
                if (current.AffectedTiles.Values.Any(current2 => roomUser.X == current2.X && roomUser.Y == current2.Y))
                    return false;

                if (roomUser.X == current.X && roomUser.Y == current.Y)
                    return false;
            }

            return true;
        }
    }
}