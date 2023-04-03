using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class UserHasFurni : IWiredItem
    {
        public UserHasFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            OtherString = string.Empty;
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ConditionUserHasFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public string OtherString { get; set; }

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
            

            var roomUser = stuff?[0] as RoomUser;

            if ((roomUser?.IsBot ?? true) || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null ||
                roomUser.GetClient().GetHabbo().GetInventoryComponent() == null || string.IsNullOrEmpty(OtherString))
                return false;

            var itemsIdsArray = OtherString.Split(';');

            /* TODO CHECK */ foreach (var itemIdStr in itemsIdsArray)
            {
                if (!uint.TryParse(itemIdStr, out var itemId))
                    continue;

                if (roomUser.GetClient().GetHabbo().GetInventoryComponent().HasBaseItem(itemId))
                    return true;
            }

            return false;
        }
    }
}