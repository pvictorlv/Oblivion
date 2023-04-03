using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    internal class Chase : IWiredItem, IWiredCycler
    {
        public Chase(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            Delay = 0;
        }

        public Interaction Type => Interaction.ActionChase;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        private Queue<RoomItem> _toRemove = new Queue<RoomItem>();

        public string OtherString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return string.Empty; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

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

        public async Task<bool> OnCycle()
        {
            if (Items == null || Items.Count == 0)
                return false;

            if (!Requested) return false;


            await Task.Yield();

            var time = Oblivion.Now();

            if (_next > time)
                return false;


            foreach (var item in Items)
            {
                if (item == null)
                    continue;

                if (!Room.GetRoomItemHandler().FloorItems.ContainsKey(item.Id))
                    continue;


                MovementState movement = Room.GetGameMap().GetChasingMovement(item.X, item.Y);
                if (movement == MovementState.None)
                    continue;

                var Point = HandleMovement(item.Coordinate, movement, item.Rot);
                
                

                if (Room.GetGameMap().ItemCanMove(item, Point))
                {
                    var NewZ = Room.GetGameMap().SqAbsoluteHeight(Point.X, Point.Y);

                    if (Point != item.Coordinate)
                    {
                        var serverMessage =
                            new ServerMessage(
                                LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        await serverMessage.AppendIntegerAsync(item.X);
                        await serverMessage.AppendIntegerAsync(item.Y);
                        await serverMessage.AppendIntegerAsync(Point.X);
                        await serverMessage.AppendIntegerAsync(Point.Y);
                        await serverMessage.AppendIntegerAsync(1);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync(item.Z.ToString(Oblivion.CultureInfo));
                        await serverMessage.AppendStringAsync(NewZ.ToString(Oblivion.CultureInfo));
                        await serverMessage.AppendIntegerAsync(0);
                        Room.SendMessage(serverMessage);
                        Room.GetRoomItemHandler().SetFloorItem(item, Point.X, Point.Y, NewZ);
                    }
                }

                await Room.GetWiredHandler().OnUserFurniCollision(Room, item);
            }

            while (_toRemove.Count > 0)
            {
                var rI = _toRemove.Dequeue();
                Items.Remove(rI);
            }

            _next = Oblivion.Now() + Delay;

            Requested = false;

            return true;
        }

        public bool Requested;


        private static void HandleMovement(ref Point coordinate, MovementState state)
        {
            switch (state)
            {
                case MovementState.Down:
                {
                    coordinate.Y++;
                    break;
                }

                case MovementState.Up:
                {
                    coordinate.Y--;
                    break;
                }

                case MovementState.Left:
                {
                    coordinate.X--;
                    break;
                }

                case MovementState.Right:
                {
                    coordinate.X++;
                    break;
                }
            }
        }

        protected Point HandleMovement(Point newCoordinate, MovementState state, int newRotation)
        {
            var newPoint = new Point(newCoordinate.X, newCoordinate.Y);

            switch (state)
            {
                case MovementState.Up:
                case MovementState.Down:
                case MovementState.Left:
                case MovementState.Right:
                {
                    HandleMovement(ref newPoint, state);
                    break;
                }

                case MovementState.LeftRight:
                {
                    if (Oblivion.GetRandomNumber(0, 2) == 1)
                    {
                        HandleMovement(ref newPoint, MovementState.Left);
                    }
                    else
                    {
                        HandleMovement(ref newPoint, MovementState.Right);
                    }

                    break;
                }

                case MovementState.UpDown:
                {
                    if (Oblivion.GetRandomNumber(0, 2) == 1)
                    {
                        HandleMovement(ref newPoint, MovementState.Up);
                    }
                    else
                    {
                        HandleMovement(ref newPoint, MovementState.Down);
                    }

                    break;
                }

                case MovementState.Random:
                {
                    switch (Oblivion.GetRandomNumber(1, 5))
                    {
                        case 1:
                        {
                            HandleMovement(ref newPoint, MovementState.Up);
                            break;
                        }
                        case 2:
                        {
                            HandleMovement(ref newPoint, MovementState.Down);
                            break;
                        }

                        case 3:
                        {
                            HandleMovement(ref newPoint, MovementState.Left);
                            break;
                        }
                        case 4:
                        {
                            HandleMovement(ref newPoint, MovementState.Right);
                            break;
                        }
                    }

                    break;
                }
            }

            return newPoint;
        }

        public async Task<bool> Execute(params object[] Params)
        {
            if (Item == null || Items.Count == 0)
                return false;


            Requested = true;


            return true;
        }

        public void Dispose()
        {
            _toRemove.Clear();
            _toRemove = null;
        }

        public bool Disposed { get; set; }

        private double _next;
    }
}