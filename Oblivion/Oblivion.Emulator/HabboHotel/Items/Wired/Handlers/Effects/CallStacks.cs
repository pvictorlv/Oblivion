﻿using System.Collections.Generic;
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
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ActionCallStacks;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            
            foreach (var item in Items)
                if (item.IsWired && Room.GetRoomItemHandler().FloorItems.Values.Contains(item))
                {
                    var wired = Room.GetWiredHandler().GetWired(item);
                    if (wired != null && wired.Type != Interaction.ActionCallStacks &&
                        wired.Type != Interaction.TriggerRepeater && wired.Type != Interaction.TriggerLongRepeater)
                    {
                        WiredHandler.OnEvent(wired);
                        wired.Execute(roomUser, Type);
                    }
                }


            return true;
        }
    }
}