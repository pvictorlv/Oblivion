using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    public class FurniStateToggled : IWiredItem
    {
        private long _mNext;

        public FurniStateToggled(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            Delay = 0;
        }


        public void Dispose()
        {
        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.TriggerStateChanged;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

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
        
        public async Task<bool> Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            var roomItem = stuff[1];

            if (!(roomItem is RoomItem item))
            {
                return false;
            }
            if (roomUser == null)
                return false;

            if (!Items.Contains(item))
                return false;

;
            //todo delay

//            if (_mNext > num)
//                await Task.Delay((int) (_mNext - num));

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Count > 0)
                /* TODO CHECK */
                foreach (var current2 in conditions)
                {
                    if (!await current2.Execute(roomUser, item)) return false;
                    await WiredHandler.OnEvent(current2);
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
                        await current3.Execute(roomUser, Type);
                        await WiredHandler.OnEvent(current3);
                    }
                }
            }

            await WiredHandler.OnEvent(this);

            _mNext = Oblivion.Now() + Delay;

            return true;
        }
    }
}