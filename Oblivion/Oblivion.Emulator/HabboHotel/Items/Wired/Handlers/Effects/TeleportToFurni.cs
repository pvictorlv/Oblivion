using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    internal class TeleportToFurni : IWiredItem, IWiredCycler
    {
        private readonly List<Interaction> _mBanned;
        private int _delay;

        private long _mNext;
        public bool Requested { get; set; }

        public TeleportToFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            ToWorkConcurrentQueue = new Queue<RoomUser>();
            Items = new List<RoomItem>();
            _delay = 0;
            _mNext = 0L;
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater
            };
        }


        public Queue<RoomUser> ToWorkConcurrentQueue { get; set; }

        public double TickCount { get; set; }

        public bool OnCycle()
        {
            if (ToWorkConcurrentQueue.Count <= 0)
            {
                ToWorkConcurrentQueue.Clear();
                TickCount = Delay / 1000;
                return false;
            }


            var num = Oblivion.Now();


            if (_mNext >= num)
                return false;

            while (ToWorkConcurrentQueue.Count > 0)
            {
                var roomUser = ToWorkConcurrentQueue.Dequeue();
                if (roomUser?.GetClient()?.GetHabbo() == null)
                    continue;


                Teleport(roomUser);
            }

            _mNext = Oblivion.Now() + Delay;
            return true;
        }

        public Interaction Type => Interaction.ActionTeleportTo;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

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
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 500;
            }
        }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null) return false;
            var item = (Interaction) stuff[1];

            if (_mBanned.Contains(item))
                return false;

            if (Items?.Count < 0)
                return false;

            roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent()?.ActivateCustomEffect(4);

            if (!ToWorkConcurrentQueue.Contains(roomUser))
                ToWorkConcurrentQueue.Enqueue(roomUser);
           

            return true;
        }

        private void Teleport(RoomUser user)
        {
            if (Items?.Count < 0)
                return;

            if (user?.GetClient()?.GetHabbo() == null)
                return;

            var rnd = new Random();

            Items = (from x in Items orderby rnd.Next() select x).ToList();

            RoomItem roomItem = null;

            /* TODO CHECK */
            foreach (var current in Items)
            {
                if (current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current))
                    roomItem = current;
            }

            if (roomItem == null)
            {
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                return;
            }
            int oldX = user.X, oldY = user.Y;
            Room.GetGameMap().TeleportToItem(user, roomItem);
            Room.GetRoomUserManager().OnUserUpdateStatus(oldX, oldY);
            Room.GetRoomUserManager().OnUserUpdateStatus(roomItem.X, roomItem.Y);
            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);

            return;
        }
    }
}