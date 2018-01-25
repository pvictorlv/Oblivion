﻿using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Addons
{
    public class GiveHandItem : IWiredItem
    {
        public GiveHandItem(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ActionHandItem;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public async Task<bool> Execute(params object[] stuff)
        {
            await Task.Yield();

            if (stuff[0] == null)
                return false;

            var roomUser = (RoomUser) stuff[0];

            if (ushort.TryParse(OtherString, out var drink))
            {
                if (roomUser.RidingHorse)
                {
                    roomUser.GetClient().SendWhisper(Oblivion.GetLanguage().GetVar("horse_handitem_error"));
                    return true;
                }
                if (roomUser.IsLyingDown)
                    return true;

                roomUser.CarryItem(drink);
                return true;
            }

            return false;
        }
    }
}