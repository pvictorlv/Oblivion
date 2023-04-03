using System.Threading.Tasks;

using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    public class BotReachedStuff : IWiredItem
    {
        public BotReachedStuff(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.TriggerBotReachedStuff;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

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

        public Task<bool> Execute(params object[] stuff)
        {
            

            if (Items == null || Items.Count <= 0) return false;

            var rItem = (RoomItem) stuff[0];
            if (rItem == null) return false;
            if (Items.Contains(rItem)) return false;

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
                var specials = Room.GetWiredHandler().GetSpecials(this);
                if (specials.Count > 0)
                {
                    var specialBox = specials[0];
                    if (specialBox != null)
                    {
                        var selectedBox = specialBox.Type == Interaction.SpecialRandom
                            ? Room.GetWiredHandler().GetRandomEffect(effects)
                            : Room.GetWiredHandler().GetRandomUnseenEffect(effects);

                        if (selectedBox == null || !selectedBox.Execute())
                            return false;

                        WiredHandler.OnEvent(specialBox);
                        WiredHandler.OnEvent(selectedBox);
                    }
                }
                else
                {
                    foreach (var current3 in effects)
                    {
                        if (current3.Type == Interaction.ActionBotTeleport || current3.Type == Interaction.ActionBotMove) continue;

                        current3.Execute(null, Type);
                        WiredHandler.OnEvent(current3);
                    }
                }
            }
            WiredHandler.OnEvent(this);
            return true;
        }
    }
}