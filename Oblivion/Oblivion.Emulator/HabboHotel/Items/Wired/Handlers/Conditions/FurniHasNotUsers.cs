using System.Threading.Tasks;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class FurniHasNotUsers : IWiredItem
    {
        public FurniHasNotUsers(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurnisHaveNotUsers;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return ""; }
            set { }
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
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
            if (Items == null || Items.Count <= 0 || Room == null)
                return Task.FromResult(true);

            /* TODO CHECK */
            return Task.FromResult(Items.All(current => current?.AffectedTiles != null && !current.AffectedTiles.Values.Any(current2 => Room.GetGameMap().SquareHasUsers(current2.X, current2.Y))));
        }
    }
}