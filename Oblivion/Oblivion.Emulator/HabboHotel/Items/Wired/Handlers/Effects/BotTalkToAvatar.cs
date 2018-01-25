using System.Collections.Generic;
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

        }

        public bool Disposed { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null)
                return false;
            var bot = Room.GetRoomUserManager().GetBotByName(OtherString);

            if (bot == null)
                return false;

            if (OtherBool)
            {
                var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                whisp.AppendInteger(bot.VirtualId);
                whisp.AppendString(OtherExtraString);
                whisp.AppendInteger(0);
                whisp.AppendInteger(2);
                whisp.AppendInteger(0);
                whisp.AppendInteger(-1);
                roomUser.GetClient().SendMessage(whisp);
            }
            else
            {
                bot.Chat(null, roomUser.GetUserName() + " : " + OtherExtraString, false, 0);
            }

            return true;
        }
    }
}