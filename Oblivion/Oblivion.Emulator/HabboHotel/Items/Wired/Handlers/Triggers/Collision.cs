﻿using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    public class Collision : IWiredItem
    {
        public Collision(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherBool = false;
        }

        public Interaction Type => Interaction.TriggerCollision;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get { return new List<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString { get; set; }

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

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Count > 0)
                /* TODO CHECK */ foreach (var current in conditions)
                {
                    WiredHandler.OnEvent(current);

                    if (!current.Execute(roomUser))
                        return true;
                }

            if (effects.Count > 0)
            {
                var randomBox = effects.FirstOrDefault(x => x.Type == Interaction.SpecialRandom);
                if (randomBox != null)
                {
                    if (!randomBox.Execute())
                        return false;

                    var selectedBox = Room.GetWiredHandler().GetRandomEffect(effects);
                    if (!selectedBox.Execute())
                        return false;

                    WiredHandler.OnEvent(randomBox);
                    WiredHandler.OnEvent(selectedBox);
                }
                else
                {
                    foreach (var current3 in effects)
                    {
                        if (current3.Execute(roomUser, Type))
                            WiredHandler.OnEvent(current3);
                    }
                }
            }
            return true;
        }
    }
}