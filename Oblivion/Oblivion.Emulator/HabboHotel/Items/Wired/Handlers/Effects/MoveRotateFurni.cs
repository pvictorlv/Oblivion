using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    internal class MoveRotateFurni : IWiredItem, IWiredCycler
    {
        private readonly ConcurrentQueue<RoomItem> _toRemove = new ConcurrentQueue<RoomItem>();
        private int _rot, _dir, _cycles;

        public MoveRotateFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            Delay = 0;
            _rot = 0;
            _dir = 0;
            _cycles = 0;
        }

        public Queue ToWork
        {
            get { return null; }
            set { }
        }

        public ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; }

        public bool OnCycle()
        {
            if (Room?.GetRoomItemHandler() == null || Room.GetRoomItemHandler().FloorItems == null)
                return false;

            _cycles++;

            if (_cycles <= (Delay / 500))
                return true;

            _cycles = 0;

            HandleItems();
            return false;
        }

        public Interaction Type => Interaction.ActionMoveRotate;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return $"{_dir};{_rot}"; }
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

        public int Delay { get; set; }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;

            _cycles = 0;

            if (Delay == 0)
            {
                HandleItems();
                return true;
            }

            Room.GetWiredHandler().EnqueueCycle(this);

            return false;
        }

        private void HandleItems()
        {
            if (Room?.GetRoomItemHandler() == null)
                return;

            foreach (var item in Items)
            {
                if (item == null || Room.GetRoomItemHandler().GetItem(item.Id) == null)
                {
                    _toRemove.Enqueue(item);
                    continue;
                }
                HandleMovement(item);
            }

            RoomItem rI;

            while (_toRemove.TryDequeue(out rI))
                if (Items.Contains(rI))
                    Items.Remove(rI);
        }

        private void HandleMovement(RoomItem item)
        {
            var newPoint = Movement.HandleMovement(item.Coordinate, (MovementState)_dir, item.Rot);
            var newRotation = Movement.HandleRotation(item.Rot, (RotationState)item.Rot);

            if (newPoint != item.Coordinate && newRotation == item.Rot)
            {
                if (!Room.GetGameMap().SquareIsOpen(newPoint.X, newPoint.Y, false))
                    return;

                Room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false, true, false, true);

                return;
            }

            if (newPoint == item.Coordinate && newRotation == item.Rot)
                return;

            if (!Room.GetGameMap().SquareIsOpen(newPoint.X, newPoint.Y, false))
                return;

            Room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false, true, false, false);
        }
    }
}