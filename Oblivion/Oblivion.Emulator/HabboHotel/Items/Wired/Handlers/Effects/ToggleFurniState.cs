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
    public class ToggleFurniState : IWiredItem, IWiredCycler
    {
        private long _mNext;

        public ToggleFurniState(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            Delay = 0;
            _mNext = 0L;
        }

        public Queue ToWork
        {
            get { return null; }
            set { }
        }

        public ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; }

        public bool OnCycle()
        {
            if (!Items.Any())
                return true;

            var num = Oblivion.Now();

            if (_mNext < num)
            {
                foreach (var current in Items.Where(current => current != null && Room.GetRoomItemHandler().FloorItems.ContainsKey(current.Id)))
                    current.Interactor.OnWiredTrigger(current);
            }

            if (_mNext >= num)
                return false;

            _mNext = 0L;
            return true;
        }

        public Interaction Type => Interaction.ActionToggleState;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return string.Empty; }
            set { }
        }

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

        public int Delay { get; set; }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return false;

            if (_mNext == 0L || _mNext < Oblivion.Now())
                _mNext = (Oblivion.Now() + (Delay));

            Room.GetWiredHandler().EnqueueCycle(this);

            return true;
        }
    }
}