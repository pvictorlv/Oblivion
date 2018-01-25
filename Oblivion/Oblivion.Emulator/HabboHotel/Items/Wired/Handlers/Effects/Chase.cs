using System.Collections.Generic;
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
    internal class Chase : IWiredItem
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

        public int Delay { get; set; }

        public async Task<bool> Execute(params object[] Params)
        {
            if (Item == null || Items.Count == 0)
                return false;


            var time = Oblivion.Now();

            if (_next > time)
                await Task.Delay((int) (_next - time));

            foreach (var item in Items)
            {
                if (item == null) continue;

                if (!Room.GetRoomItemHandler().FloorItems.ContainsKey(item.Id)) continue;

                if (Room.GetWiredHandler().OtherBoxHasItem(this, item))
                {
                    _toRemove.Enqueue(item);
                    continue;
                }

                var Point = Room.GetGameMap().GetChaseMovement(item);


                if (!Room.GetGameMap().ItemCanMove(item, Point))
                    continue;

                if (Room.GetGameMap().CanRollItemHere(Point.X, Point.Y) &&
                    !Room.GetGameMap().SquareHasUsers(Point.X, Point.Y))
                {
                    var NewZ = item.Z;
                    var CanBePlaced = true;

                    var Items = Room.GetGameMap().GetCoordinatedItems(Point);
                    foreach (var IItem in Items.Where(IItem => IItem != null && IItem.Id != item.Id))
                    {
                        if (!IItem.GetBaseItem().Walkable)
                        {
                            CanBePlaced = false;
                            break;
                        }

                        if (IItem.TotalHeight > NewZ)
                            NewZ = IItem.TotalHeight;

                        if (CanBePlaced && !IItem.GetBaseItem().Stackable)
                            CanBePlaced = false;
                    }

                    if (CanBePlaced && Point != item.Coordinate)
                    {
                        var serverMessage =
                            new ServerMessage(
                                LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        serverMessage.AppendInteger(item.X);
                        serverMessage.AppendInteger(item.Y);
                        serverMessage.AppendInteger(Point.X);
                        serverMessage.AppendInteger(Point.Y);
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(item.Z.ToString(Oblivion.CultureInfo));
                        serverMessage.AppendString(NewZ.ToString(Oblivion.CultureInfo));
                        serverMessage.AppendInteger(0);
                        Room.SendMessage(serverMessage);
                        Room.GetRoomItemHandler().SetFloorItem(item, Point.X, Point.Y, NewZ);
                    }
                }
                Room.GetWiredHandler().OnUserFurniCollision(Room, item);
            }
            while (_toRemove.Count > 0)
            {
                var rI = _toRemove.Dequeue();
                if (Items.Contains(rI))
                    Items.Remove(rI);
            }

            _next = Oblivion.Now() + Delay;
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