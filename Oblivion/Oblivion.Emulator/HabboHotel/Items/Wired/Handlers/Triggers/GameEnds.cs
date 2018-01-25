using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class GameEnds : IWiredItem
    {
        public GameEnds(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.TriggerGameEnd;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
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

        public async Task<bool> Execute(params object[] stuff)
        {
            await Task.Yield();

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Count > 0)
                /* TODO CHECK */ foreach (var current in conditions)
                {
                    if (!current.Execute(null, Type).Result)
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
                        if (current3.Execute(null, Type).Result)
                            WiredHandler.OnEvent(current3);
                    }
                }
            }
            WiredHandler.OnEvent(this);
            return true;
        }
    }
}