using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class TimerTrigger : IWiredItem, IWiredCycler
    {
        public TimerTrigger(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.TriggerTimer;

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

        private bool _requested;

        public double TickCount { get; set; }

        private int _delay;
        private long _mNext;


        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 2;
            }
        }

        public async Task<bool> OnCycle()
        {
            if (!_requested) return false;

            var num = Oblivion.Now();

            if (_mNext > num)
                return false;

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);


            if (conditions.Count > 0)
                foreach (var current in conditions)
                {
                    if (!current.Execute(null).Result)
                        return false;

                    WiredHandler.OnEvent(current);
                }

            if (effects.Count > 0)
            {
                var specials = Room.GetWiredHandler().GetSpecials(this);
                if (specials.Count > 0)
                {
                    var specialBox = specials[0];
                    if (specialBox != null)
                    {
                        var selectedBox = specialBox.Type == Interaction.SpecialRandom
                            ? Room.GetWiredHandler().GetRandomEffect(effects)
                            : Room.GetWiredHandler().GetRandomUnseenEffect(effects);

                        if (selectedBox == null || !selectedBox.Execute().Result)
                            return false;

                        WiredHandler.OnEvent(specialBox);
                        WiredHandler.OnEvent(selectedBox);
                    }
                }
                else
                {
                    foreach (var current3 in effects)
                    {
                        if (current3.Type == Interaction.ActionResetTimer) continue;

                        if (current3.Execute(null, Type).Result)
                            WiredHandler.OnEvent(current3);
                    }
                }
            }

            _mNext = Oblivion.Now() + Delay;

            _requested = false;
            return true;
        }


        public async Task<bool> Execute(params object[] stuff)
        {
            if (_mNext == 0L || _mNext <= Oblivion.Now())
                _mNext = Oblivion.Now() + Delay;

            _requested = true;

            return true;
        }

        public void Dispose()
        {
            
        }

        public bool Disposed { get; set; }
    }
}