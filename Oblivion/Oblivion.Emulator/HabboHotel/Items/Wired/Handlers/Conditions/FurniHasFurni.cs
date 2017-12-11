using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class FurniHasFurni : IWiredItem
    {
        public FurniHasFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurniHasFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

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

        public bool OtherBool { get; set; }

        public int Delay
        {
            get { return 0; }
            set { }
        }
        
        

        public bool AnyItemHaveFurni() => (from current in Items where current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current) from affectedTile in current.AffectedTiles.Values from item in Room.GetGameMap().GetAllRoomItemForSquare(affectedTile.X, affectedTile.Y) where current.Id != item.Id && item.Z >= affectedTile.Z select current).Any();
        public bool AllItemHaveFurni() => (from current in Items
                                           where current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current)
                                           from affectedTile in current.AffectedTiles.Values
                                           select Room.GetGameMap()
                                               .GetAllRoomItemForSquare(affectedTile.X, affectedTile.Y)
                                               .Any(item => current.Id != item.Id && item.Z >= affectedTile.Z)).All(all => all);

        public bool Execute(params object[] stuff)
        {
            if (Items == null || Items.Count <= 0)
                return true;

            return OtherBool ? AllItemHaveFurni() : AnyItemHaveFurni();

        }
    }
}