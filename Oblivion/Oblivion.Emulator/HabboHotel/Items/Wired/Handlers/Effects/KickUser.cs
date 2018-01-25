using System.Collections.Generic;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class KickUser : IWiredItem
    {
        private List<Interaction> _mBanned;
        public bool Requested { get; set; }

        public KickUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater,
                Interaction.TriggerRoomEnter
            };
        }

        public void Dispose()
        {
            _mBanned.Clear();
            _mBanned = null;
        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ActionKickUser;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        public int Delay { get; set; }

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

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public async Task<bool> Execute(params object[] stuff)
        {
            if (stuff.Length < 2)
                return false;

            var roomUser = (RoomUser) stuff[0];
            var item = (Interaction) stuff[1];

            if (roomUser == null)
                return false;
            if (_mBanned.Contains(item))
                return false;

            await Task.Delay(Delay);

            Room.GetRoomUserManager().RemoveUserFromRoom(roomUser.GetClient(), true, false);
            
            return true;
        }

    }
}