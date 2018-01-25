using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

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

        }

        public bool Disposed { get; set; }
        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null) return false;
            var handitem = Delay / 500;

            if (handitem < 0)
                return false;

            roomUser.CarryItem(handitem);
            var bot = Room?.GetRoomUserManager()?.GetBotByName(OtherString);

            bot?.Chat(null, Oblivion.GetLanguage().GetVar("bot_give_handitem"), false, 0);
            return true;
        }
    }
}