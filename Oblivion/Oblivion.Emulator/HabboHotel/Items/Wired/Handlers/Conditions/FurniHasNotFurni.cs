﻿using System.Threading.Tasks;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class FurniHasNotFurni : IWiredItem
    {

        public FurniHasNotFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurniHasNotFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return string.Empty; }
            set { }
        }

        public bool OtherBool { get; set; }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public Task<bool> Execute(params object[] stuff)
        {
            

            if (Items == null || Items.Count <= 0)
                return Task.FromResult(true);

            if (!OtherBool)
            {
                bool all = true;
                foreach (var item in Items)
                {
                    if (item.GetRoom().GetGameMap().HasHeightestItem(item.Coordinate, item.Z)) continue;
                    all = false;
                    break;
                }

                return Task.FromResult(!all);
            }

            bool any = false;
            foreach (var item in Items)
            {
                if (!item.GetRoom().GetGameMap().HasHeightestItem(item.Coordinate, item.Z)) continue;
                any = true;
                break;
            }

            return Task.FromResult(!any);
        }

        public void Dispose()
        {
        
        }

        public bool Disposed { get; set; }
    }
}