using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class FurniHasNotFurni : IWiredItem
    {
        public FurniHasNotFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurniHasNotFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return string.Empty; }
            set { }
        }

        public bool OtherBool { get; set; }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (Items == null || Items.Count <= 0)
                return true;

            return OtherBool ? AllItemsHaveNotFurni() : AnyItemHaveNotFurni();
        }

        public bool AnyItemHaveNotFurni()
        {

            foreach (RoomItem current in Items)
            {
                bool any = false;

                if (current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current))
                {
                    if (current.AffectedTiles.Values.Select(affectedTile => Room.GetGameMap()
                        .GetAllRoomItemForSquare(affectedTile.X, affectedTile.Y)
                        .Any(squareItem => squareItem.Id != current.Id && squareItem.Z + squareItem.Height >= current.Z + current.Height)).Any(any1 => !any1))
                    {
                        any = true;
                    }
                    return any;
                }

            }
            return true;
        }

        public bool AllItemsHaveNotFurni()
        {
            bool all = true;
            foreach (RoomItem current in Items)
            {
                if (current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current))
                    foreach (var affectedTile in current.AffectedTiles.Values)
                    {
                        all = Room.GetGameMap().GetRoomItemForSquare(affectedTile.X, affectedTile.Y).Any(squareItem => squareItem.Id != current.Id && squareItem.Z + squareItem.Height >= current.Z + current.Height);
                    }
            }
            return !all;
        }
        
    }
}