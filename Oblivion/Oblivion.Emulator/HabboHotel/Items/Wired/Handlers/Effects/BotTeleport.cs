using System;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class BotTeleport : IWiredItem, IWiredCycler
    {
        public BotTeleport(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionBotTeleport;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }
        

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool OnCycle()
        {
            if (!_requested) return false;

            var num = Oblivion.Now();

            if (_mNext > num)
                return false;

            var bot = Room.GetRoomUserManager().GetBotByName(OtherString);
            if (bot == null) return false;

            Teleport(bot);

            _mNext = Oblivion.Now() + Delay;

            _requested = false;
            return true;
        }

        private void Teleport(RoomUser user)
        {
            if (Items == null || Items.Count < 0)
                return;

            if (user?.GetClient()?.GetHabbo() == null)
                return;

            var rnd = new Random();


            var roomItem = Items.OrderBy(x => rnd.Next()).FirstOrDefault(current => current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current));

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

        private bool _requested;

        public double TickCount { get; set; }

        private int _delay;
        private long _mNext;


        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 2;
            }
        }

        public bool Execute(params object[] stuff)
        {
            if (_mNext == 0L || _mNext <= Oblivion.Now())
                _mNext = Oblivion.Now() + Delay;


            var item = (Interaction)stuff[1];

            if (item == Interaction.TriggerRepeater || item == Interaction.TriggerLongRepeater)
                return false;

            _requested = true;

            return true;
        }
    }
}