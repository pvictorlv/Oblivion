using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class Repeater : IWiredItem, IWiredCycler
    {
   

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


        public async Task<bool> OnCycle()
        {
            var num = Oblivion.Now();

            if (_mNext > num)
                return false;

            await Task.Yield();

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);


            if (conditions.Count > 0)
                foreach (var current in conditions)
                {
                    if (!await current.Execute(null))
                        return false;

                    await WiredHandler.OnEvent(current);
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

                        if (selectedBox == null || !await selectedBox.Execute())
                            return false;

                        await WiredHandler.OnEvent(specialBox);
                        await WiredHandler.OnEvent(selectedBox);
                    }
                }
                else
                {
                    foreach (var current3 in effects)
                    {
                        await current3.Execute(null, Type);
                        await WiredHandler.OnEvent(current3);
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

        private long _mNext;


        private int _delay;

        public double TickCount { get; set; }
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

        public Task<bool> Execute(params object[] stuff)
        {
            return Task.FromResult(true);
        }
    }
}