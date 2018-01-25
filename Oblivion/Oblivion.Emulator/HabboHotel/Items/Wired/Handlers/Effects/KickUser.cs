using System.Collections;
using System.Collections.Generic;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class KickUser : IWiredItem, IWiredCycler
    {
        private readonly List<Interaction> _mBanned;
        private readonly Queue _toKick;
        public bool Requested { get; set; }

        public KickUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            TickCount = Delay / 2;
            OtherString = string.Empty;
            _toKick = new Queue();
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerRoomEnter
            };
        }

        public void Dispose()
        {

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

        public int Delay
        {
            get => (int) (TickCount * 2);
            set => TickCount = value / 2;
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

            if (roomUser == null)
                return false;
            if (_mBanned.Contains(item))
                return false;
            if (TickCount <= 0)
                TickCount = 3;

            lock (_toKick.SyncRoot)
            {
                if (!_toKick.Contains(roomUser))
                {

                    if (roomUser.GetClient().GetHabbo().HasFuse("mod_tool") ||roomUser.IsOwner())
                    {
                        roomUser.GetClient().SendWhisper("Você não pode ser kikado!");
                        return false;
                    }

                    _toKick.Enqueue(roomUser);
                    roomUser.GetClient().SendWhisper(OtherString);
                }
            }
            return true;

        }
        
        public double TickCount { get; set; }

        public bool OnCycle()
        {
            if (Room == null)
                return false;

            if (_toKick.Count == 0)
            {
                TickCount = 3;
                return true;
            }

            lock (_toKick.SyncRoot)
            {
                while (_toKick.Count > 0)
                {
                    var Player = (RoomUser)_toKick.Dequeue();
                    Room.GetRoomUserManager().RemoveUserFromRoom(Player.GetClient(), true, false);
                }
            }
            TickCount = 3;
            return true;
        }
    }
}