using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    internal class MoveRotateFurni : IWiredItem, IWiredCycler
    {
        private readonly ConcurrentQueue<RoomItem> _toRemove = new ConcurrentQueue<RoomItem>();
        private double _next;
        private int _rot, _dir;

        public MoveRotateFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            Delay = 0;
            _rot = 0;
            _dir = 0;
        }


        public void Dispose()
        {

        }

        public bool Disposed { get; set; }        
    

        public Interaction Type => Interaction.ActionMoveRotate;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public string OtherString
        {
            get => $"{_dir};{_rot}";
            set
            {
                var array = value.Split(';');

                if (array.Length != 2)
                {
                    _rot = 0;
                    _dir = 0;
                    return;
                }

                int.TryParse(array[0], out _dir);
                int.TryParse(array[1], out _rot);
            }
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
        public bool Requested;

        public bool Execute(params object[] Params)
        {
            if (Item == null || Items.Count == 0)
                return false;



            Requested = true;


            return true;
        }

        public async Task<bool> OnCycle()
        {
            if (!Requested) return false;

            if (Items == null || Items.Count <= 0)
                return true;

            await Task.Yield();

            var now = Oblivion.Now();
            if (_next > now)
                return false;



            foreach (var Item in Items)
            {
                if (Item == null) continue;

                if (Room.GetWiredHandler().OtherBoxHasItem(this, Item) ||
                    Room.GetRoomItemHandler().GetItem(Item.Id) == null)
                {
                    _toRemove.Enqueue(Item);
                    continue;
                }


                var Point = HandleMovement(Convert.ToInt32(OtherString.Split(';')[0]),
                    new Point(Item.X, Item.Y));
                var newRot = HandleRotation(Convert.ToInt32(OtherString.Split(';')[1]), Item.Rot);
                var newZ = Room.GetGameMap().GetHeightForSquareFromData(Point);

                bool sameTile = (Point.X == Item.X && Point.Y == Item.Y);
                if (sameTile)
                {
                    if (newRot != Item.Rot)
                    {
                        Item.Rot = newRot;
                        Item.UpdateState(false, true);
                        Item.SetState(Item.X, Item.Y, newZ);
                        Room.GetRoomItemHandler()
                            .SetFloorItem(Item, Item.X, Item.Y, newZ, Item.Rot, true);
                    }
                    continue;
                }

                if (!Room.GetGameMap().ItemCanMove(Item, Point))
                    continue;
                if (Room.GetGameMap().SquareIsOpen(Point.X, Point.Y, false) &&
                    !Room.GetGameMap().SquareHasUsers(Point.X, Point.Y))
                {
                    var CanBePlaced = true;
                    if (newRot != Item.Rot)
                    {
                        Item.Rot = newRot;
                        Item.UpdateState(false, true);
                        Item.SetState(Item.X, Item.Y, newZ);
                        Room.GetRoomItemHandler()
                            .SetFloorItem(Item, Item.X, Item.Y, newZ, Item.Rot, true);
                    }

                    var Items = Room.GetGameMap().GetCoordinatedItems(Point);
                    foreach (var IItem in Items)
                    {
                        if (IItem != null && IItem.Id != Item.Id)
                        {
                            if (!IItem.GetBaseItem().Walkable)
                            {
                                CanBePlaced = false;
                                break;
                            }

                            if (IItem.TotalHeight > newZ)
                                newZ = IItem.TotalHeight;

                            if (CanBePlaced && !IItem.GetBaseItem().Stackable)
                                CanBePlaced = false;
                        }
                    }

                    if (CanBePlaced && Point != Item.Coordinate)
                    {
                        var serverMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        serverMessage.AppendInteger(Item.X);
                        serverMessage.AppendInteger(Item.Y);
                        serverMessage.AppendInteger(Point.X);
                        serverMessage.AppendInteger(Point.Y);
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(Item.VirtualId);
                        serverMessage.AppendString(Item.Z.ToString(Oblivion.CultureInfo));
                        serverMessage.AppendString(newZ.ToString(Oblivion.CultureInfo));
                        serverMessage.AppendInteger(0);
                        Room.SendMessage(serverMessage);

                        Room.GetRoomItemHandler().SetFloorItem(Item, Point.X, Point.Y, newZ);
                    }
                    Room.GetWiredHandler().OnUserFurniCollision(Room, Item);
                }
            }
            while (_toRemove.TryDequeue(out var rI))
                if (Items.Contains(rI))
                    Items.Remove(rI);

            _next = Oblivion.Now() + (Delay);
            Requested = false;
            return true;
        }

        private int HandleRotation(int mode, int rotation)
        {
            switch (mode)
            {
                case 1:
                {
                    rotation += 2;
                    if (rotation > 6)
                        rotation = 0;
                    break;
                }

                case 2:
                {
                    rotation -= 2;
                    if (rotation < 0)
                        rotation = 6;
                    break;
                }

                case 3:
                {
                    if (RandomNumber.Get(0, 2) == 0)
                    {
                        rotation += 2;
                        if (rotation > 6)
                            rotation = 0;
                    }
                    else
                    {
                        rotation -= 2;
                        if (rotation < 0)
                            rotation = 6;
                    }
                    break;
                }
            }
            return rotation;
        }

        private static Point HandleMovement(int Mode, Point Position)
        {
            var NewPos = new Point();
            switch (Mode)
            {
                case 0:
                {
                    NewPos = Position;
                    break;
                }
                case 1:
                {
                    switch (RandomNumber.Get(1, 5))
                    {
                        case 1:
                            NewPos = new Point(Position.X + 1, Position.Y);
                            break;
                        case 2:
                            NewPos = new Point(Position.X - 1, Position.Y);
                            break;
                        case 3:
                            NewPos = new Point(Position.X, Position.Y + 1);
                            break;
                        case 5:
                        case 4:
                            NewPos = new Point(Position.X, Position.Y - 1);
                            break;
                    }
                    break;
                }
                case 2:
                {
                    NewPos = RandomNumber.Get(0, 2) == 1
                        ? new Point(Position.X - 1, Position.Y)
                        : new Point(Position.X + 1, Position.Y);
                    break;
                }
                case 3:
                {
                    NewPos = RandomNumber.Get(0, 2) == 1
                        ? new Point(Position.X, Position.Y - 1)
                        : new Point(Position.X, Position.Y + 1);
                    break;
                }
                case 4:
                {
                    NewPos = new Point(Position.X, Position.Y - 1);
                    break;
                }
                case 5:
                {
                    NewPos = new Point(Position.X + 1, Position.Y);
                    break;
                }
                case 6:
                {
                    NewPos = new Point(Position.X, Position.Y + 1);
                    break;
                }
                case 7:
                {
                    NewPos = new Point(Position.X - 1, Position.Y);
                    break;
                }
            }

            return NewPos;
        }
    }
}