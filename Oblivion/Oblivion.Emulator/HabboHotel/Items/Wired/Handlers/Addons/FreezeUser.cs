using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Addons
{
    public class FreezeUser : IWiredItem
    {
        public FreezeUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.ActionFreezeUser;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public Task<bool> Execute(params object[] stuff)
        {
            

            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null) return false;

            if (!roomUser.Frozen)
            {
                roomUser.Frozen = true;
                roomUser.ApplyEffect(12);
                roomUser.FrozenTick = 60;
                roomUser.GetClient().SendWhisper("Você foi congelado");
            }
            else
            {
                roomUser.Frozen = false;
                roomUser.ApplyEffect(0);
                roomUser.FrozenTick = 0;
                roomUser.GetClient().SendWhisper("Você foi descongelado");
            }
            return true;
        }

        public void Dispose()
        {
            //todo
        }

        public bool Disposed { get; set; }
    }
}