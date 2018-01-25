using System.Collections.Generic;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class IsGroupMember : IWiredItem
    {
        public IsGroupMember(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ConditionGroupMember;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

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

        public async Task<bool> Execute(params object[] stuff)
        {
            var roomUser = stuff?[0] as RoomUser;

            if (roomUser == null)
                return false;

            return Room.RoomData.Group != null &&
                   Room.RoomData.Group.Members.ContainsKey(roomUser.GetClient().GetHabbo().Id);
        }
    }
}