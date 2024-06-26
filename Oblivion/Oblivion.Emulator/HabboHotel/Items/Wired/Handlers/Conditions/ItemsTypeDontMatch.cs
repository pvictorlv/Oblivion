﻿using System.Threading.Tasks;
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

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
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

        public Task<bool> Execute(params object[] stuff)
        {
            

            if (Items == null || Items.Count <= 0)
                return Task.FromResult(true);

            RoomItem lastitem = null;

            if (stuff.Length > 1)
                lastitem = (RoomItem) stuff[1];
           

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
                        return Task.FromResult(false);
                }
                else
                {
                    if (current.GetBaseItem().InteractionType.ToString().StartsWith("banzai") && lastitem.GetBaseItem()
                            .InteractionType.ToString().StartsWith("banzai"))
                        return Task.FromResult(false);
                    if (current.GetBaseItem().InteractionType.ToString().StartsWith("football") && lastitem
                            .GetBaseItem().InteractionType.ToString().StartsWith("football"))
                        return Task.FromResult(false);
                    if (current.GetBaseItem().InteractionType.ToString().StartsWith("freeze") && lastitem.GetBaseItem()
                            .InteractionType.ToString().StartsWith("freeze"))
                        return Task.FromResult(false);
                    if (current.GetBaseItem().InteractionType == lastitem.GetBaseItem().InteractionType)
                        return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }
    }
}