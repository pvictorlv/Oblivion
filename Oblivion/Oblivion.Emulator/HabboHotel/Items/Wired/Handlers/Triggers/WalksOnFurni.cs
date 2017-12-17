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
    internal class WalksOnFurni : IWiredItem
    {
        private long _mNext;

        public WalksOnFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.TriggerWalkOnFurni;

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

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null)
                return false;
            var roomItem = (RoomItem) stuff[1];
            if (roomItem == null)
                return false;

            var userPosition = roomUser.X;
            var lastUserPosition = roomUser.CopyX;

            if (!Items.Contains(roomItem) || roomUser.LastItem != 0 && roomUser.LastItem == roomItem.Id &&
                userPosition == lastUserPosition)
                return false;

            if (roomItem.GetRoom() == null || roomItem.GetRoom().GetRoomItemHandler() == null || roomItem.GetRoom()
                    .GetRoomItemHandler().FloorItems.Values
                    .Any(i => i.X == roomItem.X && i.Y == roomItem.Y && i.Z > roomItem.Z))
                return false;

            if (Oblivion.Now() <= _mNext)
                return false;


            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Count > 0)
                /* TODO CHECK */ foreach (var current in conditions)
                {
                    if (!current.Execute(roomUser))
                        return false;

                    WiredHandler.OnEvent(current);
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
                        if (current3 == null) continue;
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