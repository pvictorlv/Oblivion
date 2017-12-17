using System;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class BotMove : IWiredItem
    {
        public BotMove(RoomItem item, Room room)
        {
            Item = item;
            Items = new ConcurrentList<RoomItem>();
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionBotMove;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var bot = Room.GetRoomUserManager().GetBotByName(OtherString);

            if (bot == null)
                return false;

            var rnd = new Random();
            var goal = Items[rnd.Next(Items.Count)];
            bot.MoveTo(goal.X, goal.Y);

            return true;
        }
    }
}