using System;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class MuteUser : IWiredItem
    {
        public MuteUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
            Delay = 0;
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ActionMuteUser;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public async Task<bool> Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];

            if (roomUser == null || roomUser.IsBot || roomUser.GetClient() == null ||
                roomUser.GetClient().GetHabbo() == null)
                return false;

            if (roomUser.GetClient().GetHabbo().Rank > 3)
                return false;

            if (Delay == 0)
                return false;

            var minutes = Delay / 500;

            var userId = roomUser.GetClient().GetHabbo().Id;

            if (Room.MutedUsers.ContainsKey(userId))
                Room.MutedUsers.Remove(userId);

            Room.MutedUsers.Add(userId, Convert.ToUInt32(Oblivion.GetUnixTimeStamp() + minutes * 60));

            if (!string.IsNullOrEmpty(OtherString))
                roomUser.GetClient().SendWhisper(OtherString);

            return true;
        }
    }
}