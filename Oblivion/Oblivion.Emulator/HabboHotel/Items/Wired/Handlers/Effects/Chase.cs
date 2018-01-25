﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

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

        public bool Execute(params object[] Params)
        {
            if (Items?.Count == 0)
                return false;


            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }

            return true;
        }

        public void Dispose()
        {
            _toRemove.Clear();
            _toRemove = null;
        }

        public bool Disposed { get; set; }

        private double _next;

        public bool Requested;


        public bool OnCycle()
        {
            if (!Requested) return false;

            var time = Oblivion.GetUnixTimeStamp();

            if (time < _next)
                return false;

            if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    if (item == null) continue;

                    if (Room.GetRoomItemHandler().FloorItems.ContainsKey(item.Id))
                    {
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
                }
                while (_toRemove.Count > 0)
                {
                    var rI = _toRemove.Dequeue();
                    if (Items.Contains(rI))
                        Items.Remove(rI);
                }
            }

            _next = Oblivion.GetUnixTimeStamp() + Delay;
            Requested = false;
            return true;
        }
    }
}