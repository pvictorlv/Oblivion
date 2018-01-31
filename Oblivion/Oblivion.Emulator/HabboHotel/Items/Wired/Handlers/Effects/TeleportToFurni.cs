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
    internal class TeleportToFurni : IWiredItem, IWiredCycler
    {
        private List<Interaction> _mBanned;

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

        public TeleportToFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            Delay = 0;
            _mNext = 0L;
            _queue = new Queue<RoomUser>();
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater
            };
        }

        public Interaction Type => Interaction.ActionTeleportTo;

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

        public async Task<bool> Execute(params object[] Params)
        {
            if (Item == null || Items.Count == 0)
                return false;


            var roomUser = (RoomUser) Params[0];
            if (roomUser?.GetClient()?.GetHabbo() == null) return false;
            var item = (Interaction) Params[1];

            if (_mBanned.Contains(item))
                return false;

            if (_queue == null || _queue.Contains(roomUser)) return false;
            _queue.Enqueue(roomUser);

            Requested = true;


            return true;
        }

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
                await Task.Delay(1000);

                Teleport(roomUser);
            }

            _mNext = Oblivion.Now() + Delay;


            return true;
        }



        private void Teleport(RoomUser user)
        {
            if (Items == null || Items.Count < 0)
                return;

            if (user?.GetClient()?.GetHabbo() == null)
                return;


            RoomItem roomItem = null;
            while (true)
            {
                if (Items.Count <= 0)
                    break;

                roomItem = Items[Oblivion.GetRandomNumber(0, Items.Count - 1)];
                if (roomItem != null && Room.GetRoomItemHandler().GetItem(roomItem.Id) != null)
                    break;

                Items.Remove(roomItem);
            }

            if (roomItem == null)
            {
                return;
            }
            int oldX = user.X, oldY = user.Y;
            Room.GetGameMap().TeleportToItem(user, roomItem);
            Room.GetRoomUserManager().OnUserUpdateStatus(oldX, oldY);
            Room.GetRoomUserManager().OnUserUpdateStatus(roomItem.X, roomItem.Y);
        }
    }
}