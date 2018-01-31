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
    public class KickUser : IWiredItem, IWiredCycler
    {
        private List<Interaction> _mBanned;


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
            _queue = new Queue<RoomUser>();
        }


        private long _mNext;
        private Queue<RoomUser> _queue;


        public void Dispose()
        {
            _mBanned.Clear();
            _mBanned = null;
            _queue.Clear();
            _queue = null;
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

        public double TickCount { get; set; }


        private int _delay;

        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 1000;
            }
        }

        public bool Requested;

        public async Task<bool> OnCycle()
        {
            if (!Requested) return false;
            if (_queue == null || _queue.Count <= 0) return false;

            var num = Oblivion.Now();


            if (_mNext >= num)
                return false;

            while (_queue.Count > 0)
            {
                var roomUser = _queue.Dequeue();
                await Task.Delay(Delay);


                if (!string.IsNullOrEmpty(OtherString))
                    roomUser.GetClient().SendNotif(OtherString);


                Room.GetRoomUserManager().RemoveUserFromRoom(roomUser.GetClient(), true, false);

            }

            _mNext = Oblivion.Now() + Delay;


            return true;
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

        public async Task<bool> Execute(params object[] Params)
        {
            if (Item == null || Items.Count == 0)
                return false;


            var roomUser = (RoomUser)Params[0];
            if (roomUser?.GetClient()?.GetHabbo() == null) return false;
            var item = (Interaction)Params[1];

            if (_mBanned.Contains(item))
                return false;

            if (_queue == null || _queue.Contains(roomUser)) return false;
            _queue.Enqueue(roomUser);

            Requested = true;


            return true;
        }


    }
}