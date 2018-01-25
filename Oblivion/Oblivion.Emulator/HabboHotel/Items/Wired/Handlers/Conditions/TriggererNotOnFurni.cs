using System.Threading.Tasks;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class TriggererNotOnFurni : IWiredItem
    {
        public TriggererNotOnFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ConditionTriggererNotOnFurni;

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

        public async Task<bool> Execute(params object[] stuff)
        {
            await Task.Yield();

            if (Items == null || Items.Count <= 0)
                return true;

            var roomUser = stuff?[0] as RoomUser;

            if (roomUser == null)
                return false;

            /* TODO CHECK */ foreach (var current in Items.Where(current => current != null &&
                                                           Room.GetRoomItemHandler().FloorItems.Values.Contains(current)))
            {
                if (current.AffectedTiles.Values.Any(current2 => roomUser.X == current2.X && roomUser.Y == current2.Y))
                    return false;

                if (roomUser.X == current.X && roomUser.Y == current.Y)
                    return false;
            }

            return true;
        }
    }
}