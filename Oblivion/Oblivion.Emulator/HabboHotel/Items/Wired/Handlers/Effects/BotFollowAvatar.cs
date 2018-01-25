using System.Collections.Generic;
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
            _toWorkQueue = new Queue<RoomUser>();
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
        public bool Requested;

        public bool OnCycle()
        {
            if (Disposed) return false;

            if (!Requested) return false;

            var time = Oblivion.GetUnixTimeStamp();

            if (time < _next)
                return false;

            if (!string.IsNullOrEmpty(OtherString))
            {
                if (_bot?.BotData == null || _bot.BotData.Name != OtherString)
                {
                    _bot = Room.GetRoomUserManager().GetBotByName(OtherString);
                }

                if (OtherBool)
                {
                    while (_toWorkQueue.Count > 0)
                    {
                        var user = _toWorkQueue.Dequeue();
                        _bot.FollowingOwner = user;
                        if (user != null && !user.IsBot)
                        {
                            if (Room.GetGameMap().ValidTile(user.SquareInFront.X, user.SquareInFront.Y))
                                _bot.MoveTo(user.SquareInFront);
                            else
                                _bot.MoveTo(user.SquareBehind);
                        }
                    }
                }
                else
                {
                    _bot.FollowingOwner = null;
                }
            }


            _next = Oblivion.GetUnixTimeStamp() + Delay;
            Requested = false;

            return true;
        }

        

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }
        public bool Disposed { get; set; }

        public bool Execute(params object[] Params)
        {
            if (Disposed) return false;

            var user = (RoomUser) Params[0];
            if (user == null) return false;
            _toWorkQueue.Enqueue(user);
            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }

            return true;
        }

        private RoomUser _bot;
        private Queue<RoomUser> _toWorkQueue;
        public void Dispose()
        {
            Disposed = true;
            _bot = null;
            _toWorkQueue.Clear();
            _toWorkQueue = null;
        }
    }
}