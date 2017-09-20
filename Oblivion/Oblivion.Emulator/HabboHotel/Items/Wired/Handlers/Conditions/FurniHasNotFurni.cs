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
            if (!Items.Any())
                return true;

            return OtherBool ? AllItemsHaveNotFurni() : AnyItemHaveNotFurni();
        }

        public bool AllItemsHaveNotFurni() => Items
            .Where(item => item != null && Room.GetRoomItemHandler().FloorItems.Contains(item)).All(current => !current
                .AffectedTiles.Values.Where(square => Room.GetGameMap().SquareHasFurni(square.X, square.Y)).Any(
                    square => Room.GetGameMap().GetRoomItemForSquare(square.X, square.Y)
                        .Any(squareItem => squareItem.Id != current.Id &&
                                           squareItem.Z + squareItem.Height >= current.Z + current.Height)));

        public bool AnyItemHaveNotFurni() => Items
            .Where(item => item != null && Room.GetRoomItemHandler().FloorItems.Contains(item)).Any(current => current
                .AffectedTiles.Values.Where(square => Room.GetGameMap().SquareHasFurni(square.X, square.Y)).Any(
                    square => !Room.GetGameMap().GetRoomItemForSquare(square.X, square.Y)
                        .Any(squareItem => squareItem.Id != current.Id &&
                                           squareItem.Z + squareItem.Height >= current.Z + current.Height)));
    }
}