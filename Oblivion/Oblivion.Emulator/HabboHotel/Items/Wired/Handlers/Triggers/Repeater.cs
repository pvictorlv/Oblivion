using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class Repeater : IWiredItem, IWiredCycler
    {
        private int _delay;
        private long _mNext;

        public Repeater(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Delay = 500;
            _mNext = 0;
            
        }


        public void Dispose()
        {

        }

        public bool Disposed { get; set; }


        public double TickCount { get; set; }
        public bool Requested { get; set; }

        public bool OnCycle()
        {
            var num = Oblivion.Now();

            if (_mNext > num)
                return false;

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);
          

            if (conditions.Count > 0)
                foreach (var current in conditions)
                {
                    if (!current.Execute(null))
                        return false;

                    WiredHandler.OnEvent(current);
                }

            if (effects.Count > 0)
            {
                if (effects.ContainsValue(Interaction.SpecialRandom))
                {
                    var randomBox = effects.FirstOrDefault(x => x.Value == Interaction.SpecialRandom).Key;

                    if (!randomBox.Execute())
                        return false;

                    var selectedBox = Room.GetWiredHandler().GetRandomEffect(effects);
                    if (!selectedBox.Execute())
                        return false;

                    WiredHandler.OnEvent(randomBox);
                    WiredHandler.OnEvent(selectedBox);
                }
                else
                {
                    foreach (var current3 in effects.Keys)
                    {
                        if (current3.Execute(null, Type))
                            WiredHandler.OnEvent(current3);
                    }
                }
            }
            _mNext = Oblivion.Now() + Delay;
            return false;
        }

        public Interaction Type => Interaction.TriggerRepeater;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
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

        public bool Execute(params object[] stuff)
        {
            if (_mNext == 0L || _mNext <= Oblivion.Now())
                _mNext = Oblivion.Now() + Delay;
            
            return false;
        }
    }
}