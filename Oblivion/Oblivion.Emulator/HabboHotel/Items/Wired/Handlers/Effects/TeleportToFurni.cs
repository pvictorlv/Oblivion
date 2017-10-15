using System;
using System.Collections;
using System.Collections.Concurrent;
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

        public TeleportToFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            ToWorkConcurrentQueue = new ConcurrentQueue<RoomUser>();
            Items = new List<RoomItem>();
            _delay = 0;
            _mNext = 0L;
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater
            };
        }

        public Queue ToWork { get; set; }

        public ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; }

        public double TickCount { get; set; }

        public bool OnCycle()
        {
            if (!ToWorkConcurrentQueue.Any())
                return true;

            if (Room?.GetRoomItemHandler() == null || Room.GetRoomItemHandler().FloorItems.Values == null)
                return false;

            var num = Oblivion.Now();
            var toAdd = new List<RoomUser>();
            RoomUser roomUser;

            while (ToWorkConcurrentQueue.TryDequeue(out roomUser))
            {
                if (roomUser?.GetClient() == null)
                    continue;

                if (_mNext <= num)
                {
                    if (Teleport(roomUser))
                        continue;

                    return false;
                }

                if (_mNext - num < 500L && roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() != null)
                    roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(4);

                toAdd.Add(roomUser);
            }

            foreach (var roomUserToAdd in toAdd.Where(roomUserToAdd => !ToWorkConcurrentQueue.Contains(roomUserToAdd)))
                ToWorkConcurrentQueue.Enqueue(roomUserToAdd);

            toAdd.Clear();

            if (_mNext >= num)
                return false;

            _mNext = 0L;
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
                TickCount = value / 1000;
            }
        }

        public bool Execute(params object[] stuff)
        {
            if (stuff[0] == null)
                return false;

            var roomUser = (RoomUser) stuff[0];
            var item = (Interaction) stuff[1];

            if (_mBanned.Contains(item))
                return false;

            if (!Items.Any())
                return false;

            if (!ToWorkConcurrentQueue.Contains(roomUser))
                ToWorkConcurrentQueue.Enqueue(roomUser);

            if (Delay < 500)
                Delay = 500;


            if (_mNext == 0L || _mNext < Oblivion.Now())
                _mNext = Oblivion.Now() + Delay;


            return true;
        }

        private bool Teleport(RoomUser user)
        {
            if (!Items.Any())
                return true;

            if (user?.GetClient() == null || user.GetClient().GetHabbo() == null)
                return true;

            var rnd = new Random();

            Items = (from x in Items orderby rnd.Next() select x).ToList();

            RoomItem roomItem = null;

            foreach (var current in Items.Where(current => current != null &&
                                                           Room.GetRoomItemHandler().FloorItems.Values.Contains(current)))
                roomItem = current;

            if (roomItem == null)
            {
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                return false;
            }

            Room.GetGameMap().TeleportToItem(user, roomItem);
            Room.GetRoomUserManager().OnUserUpdateStatus();
            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);

            return true;
        }
    }
}