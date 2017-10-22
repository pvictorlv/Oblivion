using System;
using System.Collections.Generic;
using System.Timers;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class KickUser : IWiredItem
    {
        private readonly List<Interaction> _mBanned;
        private readonly List<RoomUser> _mUsers;
        private Timer _mTimer;

        public KickUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            _mUsers = new List<RoomUser>();
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerRoomEnter
            };
        }

        public Interaction Type => Interaction.ActionKickUser;

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

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (stuff.Length < 2)
                return false;

            var roomUser = (RoomUser) stuff[0];
            var item = (Interaction) stuff[1];

            if (_mBanned.Contains(item))
                return false;

            if (roomUser?.GetClient() != null && roomUser.GetClient().GetHabbo() != null &&
                !string.IsNullOrWhiteSpace(OtherString))
            {
                if (roomUser.GetClient().GetHabbo().HasFuse("fuse_mod") ||
                    Room.RoomData.Owner == roomUser.GetUserName())
                    return false;

                roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(4, false);
                roomUser.GetClient().SendWhisper(OtherString);
                _mUsers.Add(roomUser);
            }

            if (_mTimer == null)
                _mTimer = new Timer(2000);

            _mTimer.Elapsed += ExecuteKick;
            _mTimer.Enabled = true;

            return true;
        }

        private void ExecuteKick(object source, ElapsedEventArgs eea)
        {
            try
            {
                _mTimer?.Stop();

                lock (_mUsers)
                {
                    /* TODO CHECK */ foreach (var user in _mUsers)
                        Room.GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, false);
                }

                _mUsers.Clear();
                _mTimer = null;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}