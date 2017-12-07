﻿using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    internal class InverseChase : IWiredItem, IWiredCycler
    {
        public InverseChase(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            Delay = 0;
        }

        public Interaction Type => Interaction.ActionInverseChase;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

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

        public bool Execute(params object[] stuff)
        {
            if (Room == null) return false;

            if (_next < 1 || _next < Oblivion.GetUnixTimeStamp())
                _next = Oblivion.GetUnixTimeStamp() + Delay;

            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }

            return true;
        }

        private double _next;

        public bool Requested;

        public bool OnCycle()
        {
            if (!Requested || _next < 1)
                return false;
            var time = Oblivion.GetUnixTimeStamp();
            if (_next <= time)
            {
                if (Items?.Count > 0)
                    foreach (var Item in Items)
                    {
                        if (Room.GetRoomItemHandler().FloorItems.ContainsKey(Item.Id))
                        {
                            if (Room.GetWiredHandler().OtherBoxHasItem(this, Item))
                                Items.Remove(Item);

                            var Point = Room.GetGameMap().GetInverseChaseMovement(Item);

                            if (!Room.GetGameMap().ItemCanMove(Item, Point))
                                continue;

                            if (Room.GetGameMap().CanRollItemHere(Point.X, Point.Y) &&
                                !Room.GetGameMap().SquareHasUsers(Point.X, Point.Y))
                            {
                                var NewZ = Item.Z;
                                var CanBePlaced = true;

                                var Items = Room.GetGameMap().GetCoordinatedItems(Point);
                                foreach (var IItem in Items.Where(IItem => IItem != null && IItem.Id != Item.Id))
                                {
                                    if (!IItem.GetBaseItem().Walkable)
                                    {
                                        _next = 0;
                                        CanBePlaced = false;
                                        break;
                                    }

                                    if (IItem.TotalHeight > NewZ)
                                        NewZ = IItem.TotalHeight;

                                    if (CanBePlaced && !IItem.GetBaseItem().Stackable)
                                        CanBePlaced = false;
                                }

                                if (CanBePlaced && Point != Item.Coordinate)
                                {
                                    var serverMessage =
                                        new ServerMessage(
                                            LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                                    serverMessage.AppendInteger(Item.X);
                                    serverMessage.AppendInteger(Item.Y);
                                    serverMessage.AppendInteger(Point.X);
                                    serverMessage.AppendInteger(Point.Y);
                                    serverMessage.AppendInteger(1);
                                    serverMessage.AppendInteger(Item.VirtualId);
                                    serverMessage.AppendString(Item.Z.ToString(Oblivion.CultureInfo));
                                    serverMessage.AppendString(NewZ.ToString(Oblivion.CultureInfo));
                                    serverMessage.AppendInteger(0);
                                    Room.SendMessage(serverMessage);
                                    Room.GetRoomItemHandler().SetFloorItem(Item, Point.X, Point.Y, NewZ);
                                }
                            }
                        }
                    }

                _next = 0;
                return true;
            }
            return false;
        }
    }
}