using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class FurniHasUsers : IWiredItem
    {
        public FurniHasUsers(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurnisHaveUsers;

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
            if (Items == null || Items.Count <= 0)
                return false;
            bool all = true;
            foreach (var current in Items)
            {
                if (current == null || !Room.GetRoomItemHandler()
                        .FloorItems.ContainsKey(current.Id)) continue;
                if (!current.AffectedTiles.Values.Any(current2 => Room.GetGameMap()
                    .SquareHasUsers(current2.X, current2.Y)))
                {
                    if (!Room.GetGameMap()
                        .SquareHasUsers(current.X, current.Y))
                    {
                        all = false;
                        break;
                    }
                }
            }
            return all;
        }
    }
}