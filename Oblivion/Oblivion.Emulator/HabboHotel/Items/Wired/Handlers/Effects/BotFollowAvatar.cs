using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class BotFollowAvatar : IWiredItem
    {
        public BotFollowAvatar(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.ActionBotFollowAvatar;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }


        public int Delay { get; set; }


        private double _next;

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }
        public bool Disposed { get; set; }

        public async Task<bool> Execute(params object[] Params)
        {
            if (Disposed) return false;

            var user = (RoomUser) Params[0];
            if (user == null || user.IsBot) return false;


            var time = Oblivion.Now();

            if (_next > time)
                await Task.Delay((int) (_next - time));

            if (!string.IsNullOrEmpty(OtherString))
            {
                if (_bot?.BotData == null || _bot.BotData.Name != OtherString)
                {
                    _bot = Room.GetRoomUserManager().GetBotByName(OtherString);
                }

                if (OtherBool)
                {
                    _bot.FollowingOwner = user;

                    if (Room.GetGameMap().SquareIsOpen(user.SquareInFront.X, user.SquareInFront.Y, false))
                    {
                        _bot.MoveTo(user.SquareInFront);
                    }
                    else
                    {
                        _bot.MoveTo(user.SquareBehind);
                    }
                }
                else
                {
                    _bot.FollowingOwner = null;
                }
            }


            _next = Oblivion.Now() + Delay;

            return true;
        }

        private RoomUser _bot;

        public void Dispose()
        {
            Disposed = true;
            _bot = null;
        }
    }
}