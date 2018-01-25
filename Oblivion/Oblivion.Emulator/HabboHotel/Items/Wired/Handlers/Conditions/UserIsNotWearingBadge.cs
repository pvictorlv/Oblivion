using System;
using System.Threading.Tasks;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Users.Badges;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class UserIsNotWearingBadge : IWiredItem
    {
        public UserIsNotWearingBadge(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            OtherString = string.Empty;
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ConditionUserNotWearingBadge;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

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

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public async Task<bool> Execute(params object[] stuff)
        {
            var roomUser = stuff?[0] as RoomUser;

            if ((roomUser?.IsBot ?? true) || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null ||
                roomUser.GetClient().GetHabbo().GetBadgeComponent() == null || string.IsNullOrWhiteSpace(OtherString))
                return false;

            return roomUser.GetClient().GetHabbo().GetBadgeComponent().BadgeList.Values.Cast<Badge>()
                .All(badge => badge.Slot <= 0 ||
                              !string.Equals(badge.Code, OtherString, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}