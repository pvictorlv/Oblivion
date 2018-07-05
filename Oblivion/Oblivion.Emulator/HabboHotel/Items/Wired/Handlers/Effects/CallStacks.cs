﻿using System.Net;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class CallStacks : IWiredItem
    {
        public CallStacks(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.ActionCallStacks;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];

            if (Room?.GetRoomItemHandler()?.FloorItems == null) return false;
            foreach (var item in Items)
            {
                if (item == null) continue;

                if (item.IsWired && Room.GetRoomItemHandler().FloorItems.Values.Contains(item))
                {
                    var wired = Room.GetWiredHandler().GetWired(item);
                    if (wired != null && wired.Type != Interaction.ActionCallStacks &&
                        wired.Type != Interaction.TriggerRepeater && wired.Type != Interaction.TriggerLongRepeater &&
                        wired.Type != Interaction.ActionToggleState)
                    {
                        WiredHandler.OnEvent(wired);
                        wired.Execute(roomUser, Type);
                    }
                }

            }

            return true;
        }
    }
}