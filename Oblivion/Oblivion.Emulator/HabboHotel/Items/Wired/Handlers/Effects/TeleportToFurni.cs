using System.Collections.Generic;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    internal class TeleportToFurni : IWiredItem
    {
        private List<Interaction> _mBanned;

        private long _mNext;
        public bool Requested { get; set; }


        public void Dispose()
        {
            _mBanned.Clear();
            _mBanned = null;
        }

        public bool Disposed { get; set; }
        public TeleportToFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            Delay = 0;
            _mNext = 0L;
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater
            };
        }
  
        public Interaction Type => Interaction.ActionTeleportTo;

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

        public int Delay { get; set; }

        public async Task<bool> Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser?.GetClient()?.GetHabbo() == null) return false;
            var item = (Interaction) stuff[1];
            
            if (_mBanned.Contains(item))
                return false;

            if (Items?.Count < 0)
                return false;

            var num = Oblivion.Now();


            if (_mNext >= num)
                await Task.Delay((int) (_mNext - num));

            Teleport(roomUser);


            roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent()?.ActivateCustomEffect(4);
            

            _mNext = Oblivion.Now() + Delay;

            await Task.Delay(Delay);

            return true;
        }

        private void Teleport(RoomUser user)
        {
            if (Items == null || Items.Count < 0)
                return;

            if (user?.GetClient()?.GetHabbo() == null)
                return;


            RoomItem roomItem = null;
            while (true)
            {
                if (Items.Count <= 0)
                    break;

                roomItem = Items[Oblivion.GetRandomNumber(0, Items.Count - 1)];
                if (roomItem != null && Room.GetRoomItemHandler().GetItem(roomItem.Id) != null)
                    break;

                Items.Remove(roomItem);
            }
            /* TODO CHECK */

            if (roomItem == null)
            {
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                return;
            }
            int oldX = user.X, oldY = user.Y;
            Room.GetGameMap().TeleportToItem(user, roomItem);
            Room.GetRoomUserManager().OnUserUpdateStatus(oldX, oldY);
            Room.GetRoomUserManager().OnUserUpdateStatus(roomItem.X, roomItem.Y);
            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);

        }
    }
}