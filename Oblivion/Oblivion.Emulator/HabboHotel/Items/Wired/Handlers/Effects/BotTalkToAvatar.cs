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
    public class BotTalkToAvatar : IWiredItem
    {
        public BotTalkToAvatar(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionBotTalkToAvatar;

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

        public void Dispose()
        {
            _bot = null;
        }

        private RoomUser _bot;
        public bool Disposed { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public async Task<bool> Execute(params object[] stuff)
        {
            

            var roomUser = (RoomUser) stuff[0];
            if (roomUser?.GetClient() == null)
                return false;

            if (string.IsNullOrEmpty(OtherString)) return false;

            if (_bot?.BotData == null || _bot.BotData.Name != OtherString)
            {
                _bot = Room.GetRoomUserManager().GetBotByName(OtherString);
            }

            if (_bot == null)
                return false;

            if (OtherBool)
            {
                var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                await whisp.AppendIntegerAsync(_bot.VirtualId);
                await whisp.AppendStringAsync(OtherExtraString);
                await whisp.AppendIntegerAsync(0);
                await whisp.AppendIntegerAsync(2);
                await whisp.AppendIntegerAsync(0);
                await whisp.AppendIntegerAsync(-1);
                await roomUser.GetClient().SendMessageAsync(whisp);
            }
            else
            {
                await _bot.Chat(null, roomUser.GetUserName() + " : " + OtherExtraString, false, 0);
            }

            return true;
        }
    }
}