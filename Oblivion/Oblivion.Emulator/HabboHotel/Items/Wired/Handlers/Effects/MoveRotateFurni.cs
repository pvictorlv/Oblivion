using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    internal class MoveRotateFurni : IWiredItem, IWiredCycler
    {
        private readonly ConcurrentQueue<RoomItem> _toRemove = new ConcurrentQueue<RoomItem>();
        private int _delay;
        private double _next;
        private bool _requested;
        private int _rot, _dir;

        public MoveRotateFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            _delay = 0;
            TickCount = 0;
            _rot = 0;
            _dir = 0;
            _requested = false;
        }


        public double TickCount { get; set; }

        public bool OnCycle()
        {
            if (Room?.GetRoomItemHandler() == null || !_requested || _next < 1)
                return false;

            var now = Oblivion.GetUnixTimeStamp();
            if (_next <= now)
            {
                HandleItems();
                _next = 0;
            }
            return true;
        }

        public Interaction Type => Interaction.ActionMoveRotate;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

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

        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 1000 + 1;
            }
        }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;

            if (_next < 1 || _next < Oblivion.GetUnixTimeStamp())
                _next = Oblivion.GetUnixTimeStamp() + Delay;


            if (!_requested)
            {
                TickCount = Delay / 1000;
                _requested = true;
            }

            if (TickCount <= 0)
            {
                HandleItems();
                return true;
            }

            return false;
        }

        private void HandleItems()
        {
            if (Room?.GetRoomItemHandler() == null)
                return;

            foreach (var item in Items)
            {

                if (item == null || Room.GetRoomItemHandler().GetItem(item.Id) == null || Room.GetWiredHandler().OtherBoxHasItem(this, Item))
                {
                    _toRemove.Enqueue(item);
                    continue;
                }
                HandleMovement(item);
            }

            while (_toRemove.TryDequeue(out var rI))
                if (Items.Contains(rI))
                    Items.Remove(rI);
        }

        private void HandleMovement(RoomItem item)
        {
            var newPoint = Movement.HandleMovement(item.Coordinate, (MovementState) _dir, item.Rot);
            var newRotation = Movement.HandleRotation(item.Rot, (RotationState) _rot);

            if (newPoint != item.Coordinate && newRotation == item.Rot)
            {
                if (!Room.GetGameMap().SquareIsOpen(newPoint.X, newPoint.Y, false))
                    return;

                Room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false,
                    true, false, true);

                return;
            }

            if (newPoint == item.Coordinate && newRotation == item.Rot)
                return;

            if (!Room.GetGameMap().SquareIsOpen(newPoint.X, newPoint.Y, false))
                return;


            item.Rot = newRotation;
            item.UpdateState(false, true);

            Room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false, true,
                false, false);
        }
    }
}