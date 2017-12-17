using System.Collections.Generic;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class ItemsTypeDontMatch : IWiredItem
    {
        public ItemsTypeDontMatch(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurniTypeDontMatch;

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

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;

            RoomItem lastitem = null;

            /* TODO CHECK */ foreach (var current in Items)
            {
                if (lastitem == null)
                {
                    lastitem = current;
                    continue;
                }

                if (current.GetBaseItem().InteractionType == Interaction.None ||
                    lastitem.GetBaseItem().InteractionType == Interaction.None)
                {
                    if (current.GetBaseItem().SpriteId == lastitem.GetBaseItem().SpriteId)
                        return false;
                }
                else
                {
                    if (current.GetBaseItem().InteractionType.ToString().StartsWith("banzai") && lastitem.GetBaseItem()
                            .InteractionType.ToString().StartsWith("banzai"))
                        return false;
                    if (current.GetBaseItem().InteractionType.ToString().StartsWith("football") && lastitem
                            .GetBaseItem().InteractionType.ToString().StartsWith("football"))
                        return false;
                    if (current.GetBaseItem().InteractionType.ToString().StartsWith("freeze") && lastitem.GetBaseItem()
                            .InteractionType.ToString().StartsWith("freeze"))
                        return false;
                    if (current.GetBaseItem().InteractionType == lastitem.GetBaseItem().InteractionType)
                        return false;
                }
            }

            return true;
        }
    }
}