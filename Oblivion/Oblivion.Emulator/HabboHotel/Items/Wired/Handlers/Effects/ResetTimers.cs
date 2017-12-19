using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class ResetTimers : IWiredItem, IWiredCycler
    {
        public ResetTimers(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionResetTimer;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }


        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        private bool _requested;

        public double TickCount { get; set; }

        private int _delay;


        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 2;
            }
        }

        public bool OnCycle()
        {
            if (!_requested) return false;

            var num = Oblivion.Now();

            if (_mNext > num)
                return false;


            _mNext = Oblivion.Now() + Delay;

            _requested = false;
            return true;
        }
        private long _mNext;

        public bool Execute(params object[] stuff)
        {
            if (_mNext == 0L || _mNext <= Oblivion.Now())
                _mNext = Oblivion.Now() + Delay;

            _requested = true;

            return true;
        }
    }
}