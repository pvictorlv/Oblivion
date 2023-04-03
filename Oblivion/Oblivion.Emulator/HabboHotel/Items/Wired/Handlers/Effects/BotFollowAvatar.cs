using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class BotFollowAvatar : IWiredItem, IWiredCycler
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


        private double _next;

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }
        public bool Disposed { get; set; }

        public async Task<bool> OnCycle()
        {
            if (_following == null) return false;

            await Task.Yield();

            var time = Oblivion.Now();

            if (_next > time)
                return false;

            if (!string.IsNullOrEmpty(OtherString))
            {
                if (_bot?.BotData == null || _bot.BotData.Name != OtherString)
                {
                    _bot = Room.GetRoomUserManager().GetBotByName(OtherString);
                }

                if (OtherBool)
                {
                    _bot.FollowingOwner = _following;

                    if (Room.GetGameMap().SquareIsOpen(_following.SquareInFront.X, _following.SquareInFront.Y, false))
                    {
                        _bot.MoveTo(_following.SquareInFront);
                    }
                    else
                    {
                        _bot.MoveTo(_following.SquareBehind);
                    }
                }
                else
                {
                    _bot.FollowingOwner = null;
                }
            }

            _following = null;

            _next = Oblivion.Now() + Delay;
            return true;
        }

        public Task<bool> Execute(params object[] Params)
        {
            if (Disposed) return false;

            var user = (RoomUser) Params[0];
            if (user == null || user.IsBot) return false;

            _following = user;


            return true;
        }

        private RoomUser _bot;
        private RoomUser _following;

        public void Dispose()
        {
            Disposed = true;
            _bot = null;
            _following = null;
        }
    }
}