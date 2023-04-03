using System.Threading.Tasks;
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

        public void Dispose()
        {
            _bot = null;
        }

        private RoomUser _bot;

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ActionBotTeleport;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }


        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        private void Teleport(RoomUser user)
        {
            if (Items == null || Items.Count < 0)
                return;

            RoomItem roomItem = null;
            while (true)
            {
                if (Items.Count <= 0)
                    break;

                roomItem = Items[Oblivion.GetRandomNumber(0, Items.Count - 1)];
                if (roomItem != null)
                    break;
            }
            if (roomItem == null) return;

            int oldX = user.X, oldY = user.Y;
            Room.GetGameMap().TeleportToItem(user, roomItem);
            Room.GetRoomUserManager().OnUserUpdateStatus(oldX, oldY);
            Room.GetRoomUserManager().OnUserUpdateStatus(roomItem.X, roomItem.Y);


            Room.GetWiredHandler().ExecuteWired(Interaction.TriggerBotReachedStuff, roomItem);
        }


        private long _mNext;
        public double TickCount { get; set; }
        private int _delay;
        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 1000;
            }
        }

        public bool Requested;

        public async Task<bool> OnCycle()
        {
            if (!Requested) return false;

            await Task.Yield();

            var num = Oblivion.Now();

            if (_mNext > num)
                return false;
          
            if (_bot == null)
                return false;

            Teleport(_bot);

            _mNext = Oblivion.Now() + Delay;
            Requested = false;
            return true;
        }
        public Task<bool> Execute(params object[] stuff)
        {
            var item = (Interaction) stuff[1];

            if (item == Interaction.TriggerRepeater || item == Interaction.TriggerLongRepeater)
                return false;

            if (string.IsNullOrEmpty(OtherString)) return false;


            if (_bot?.BotData == null || _bot.BotData.Name != OtherString)
            {
                _bot = Room.GetRoomUserManager().GetBotByName(OtherString);
            }

            Requested = true;


            return true;
        }
    }
}