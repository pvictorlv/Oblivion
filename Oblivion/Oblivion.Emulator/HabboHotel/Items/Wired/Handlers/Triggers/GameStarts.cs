using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    public class GameStarts : IWiredItem
    {
        public GameStarts(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
        }

        public Interaction Type => Interaction.TriggerGameStart;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get { return new List<RoomItem>(); }
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

        public bool Execute(params object[] stuff)
        {
            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Any())
            {
                foreach (var current in conditions)
                {
                    if (!current.Execute(null, Type))
                        return false;

                    WiredHandler.OnEvent(current);
                }
            }
            if (effects.Any(x => x.Type == Interaction.SpecialRandom))
            {
                var randomBox = effects.FirstOrDefault(x => x.Type == Interaction.SpecialRandom);
                if (randomBox != null && !randomBox.Execute())
                    return false;

                var selectedBox = Room.GetWiredHandler().GetRandomEffect(effects);
                if (!selectedBox.Execute())
                    return false;

                WiredHandler.OnEvent(randomBox);
                WiredHandler.OnEvent(selectedBox);
            }
            else if (effects.Any())
            {
                foreach (var current2 in effects.Where(current2 => current2.Execute(null, Type)))
                {
                    WiredHandler.OnEvent(current2);
                }
            }

            WiredHandler.OnEvent(this);
            return true;
        }
    }
}