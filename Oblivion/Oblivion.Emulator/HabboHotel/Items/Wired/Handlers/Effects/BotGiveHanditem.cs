using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class BotGiveHanditem : IWiredItem
    {
        public BotGiveHanditem(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionBotGiveHanditem;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public void Dispose()
        {
            _bot = null;
        }

        public bool Disposed { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        private RoomUser _bot;
        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public async Task<bool> Execute(params object[] stuff)
        {
            if (string.IsNullOrEmpty(OtherString)) return false;

            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null) return false;
            var handitem = Delay / 500;

            if (handitem < 0)
                return false;


            if (_bot?.BotData == null || _bot.BotData.Name != OtherString)
                _bot = Room?.GetRoomUserManager()?.GetBotByName(OtherString);

            if (_bot == null) return false;

            _bot.Chat(null, Oblivion.GetLanguage().GetVar("bot_give_handitem"), false, 0);
            roomUser.CarryItem(handitem);

            return true;
        }
    }
}