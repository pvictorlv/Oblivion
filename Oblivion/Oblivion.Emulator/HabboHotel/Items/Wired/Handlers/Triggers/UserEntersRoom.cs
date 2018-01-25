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
    public class UserEntersRoom : IWiredItem
    {
        public UserEntersRoom(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.TriggerRoomEnter;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
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
            if (roomUser == null) return false;

            if (!string.IsNullOrEmpty(OtherString) && roomUser.GetUserName() != OtherString &&
                !roomUser.GetClient().GetHabbo().IsTeleporting)
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
            return true;
        }
    }
}