using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class ToggleFurniState : IWiredItem, IWiredCycler
    {
        private int _delay;
        private long _mNext;
        private bool _requested;

        public ToggleFurniState(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            _delay = 0;
            _mNext = 0L;
        }

     
        public double TickCount { get; set; }

        public bool OnCycle()
        {
            if (!_requested || _mNext < 1)
                return false;
            if (!Items.Any())
                return true;

            var num = Oblivion.Now();

            if (_mNext < num)
                /* TODO CHECK */ foreach (var current in Items.Where(
                    current => current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current)))
                    current.Interactor.OnWiredTrigger(current);

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
            if (!Items.Any())
                return false;

            if (!_requested)
                _requested = true;

            if (_mNext == 0L || _mNext < Oblivion.Now())
                _mNext = Oblivion.Now() + Delay;


            return true;
        }
    }
}