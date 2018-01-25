using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class BotClothes : IWiredItem
    {
        public BotClothes(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionBotClothes;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }


        public void Dispose()
        {

        }

        public bool Disposed { get; set; }

        private RoomUser _bot;
        public async Task<bool> Execute(params object[] stuff)
        {
            if (_bot?.BotData == null || _bot.BotData.Name != OtherString)
            {
                _bot = Room.GetRoomUserManager().GetBotByName(OtherString);

            }

            if (_bot == null || OtherExtraString == "null")
                return false;

            _bot.BotData.Look = OtherExtraString;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
            serverMessage.AppendInteger(1);
            _bot.Serialize(serverMessage);
            Room.SendMessage(serverMessage);

            return true;
        }
    }
}