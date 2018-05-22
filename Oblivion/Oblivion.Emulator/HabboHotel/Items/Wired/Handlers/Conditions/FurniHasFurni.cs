using System.Threading.Tasks;
using System.Linq;
using Oblivion.Collections;
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
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurniHasFurni;

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

        public bool OtherBool { get; set; }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }

        public bool Execute(params object[] stuff)
        {
            

            if (Items == null || Items.Count <= 0)
                return true;

            if (OtherBool)
            {
                bool all = true;
                foreach (var item in Items)
                {
                    if (!all) break;

                    foreach (var coord in item.GetCoords())
                    {
                        var itemsForSquare = item.GetRoom().GetGameMap().GetAllRoomItemForSquare(coord.X, coord.Y)
                            .Where(x => x.Id != item.Id && x.Z >= item.Z);
                        if (!itemsForSquare.Any())
                        {
                            all = false;
                            break;
                        }


                    }
                }

                return all;
            }

            foreach (var item in Items)
            {
                foreach (var coord in item.GetCoords())
                {
                    var itemsForSquare = item.GetRoom().GetGameMap().GetAllRoomItemForSquare(coord.X, coord.Y)
                        .Where(x => x.Id != item.Id && x.Z >= item.Z);
                    if (itemsForSquare.Any())
                    {
                        return true;
                    }
                }
            }

            return false;

        }
    }
}