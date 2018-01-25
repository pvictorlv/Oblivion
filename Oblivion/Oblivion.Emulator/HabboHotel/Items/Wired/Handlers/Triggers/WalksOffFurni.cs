using System.Collections.Generic;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class WalksOffFurni : IWiredItem
    {
        private long _mNext;

        public WalksOffFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }


        public Interaction Type => Interaction.TriggerWalkOffFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

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

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            var roomItem = (RoomItem) stuff[1];

            if (!Items.Contains(roomItem) || roomUser.LastItem != roomItem.Id)
                return false;

            if (roomItem.AffectedTiles.Values.Any(
                current => current.X == roomUser.X && current.Y == roomUser.Y ||
                           roomUser.X == roomItem.X && roomUser.Y == roomItem.Y))
                return false;

            var num = Oblivion.Now();
            if (num <= _mNext)
                return false;

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Count > 0)
                /* TODO CHECK */ foreach (var current in conditions)
                {
                    if (!current.Execute(roomUser, roomItem))
                        return false;

                    WiredHandler.OnEvent(current);
                }


            if (effects.Count > 0)
            {
                if (effects.ContainsValue(Interaction.SpecialRandom))
                {
                    var randomBox = effects.FirstOrDefault(x => x.Value == Interaction.SpecialRandom).Key;

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
                    foreach (var current3 in effects.Keys)
                    {
                        if (current3.Execute(roomUser, Type))
                            WiredHandler.OnEvent(current3);
                    }
                }
            }

            WiredHandler.OnEvent(this);

            _mNext = Oblivion.Now() + Delay;


            return true;
        }
    }
}