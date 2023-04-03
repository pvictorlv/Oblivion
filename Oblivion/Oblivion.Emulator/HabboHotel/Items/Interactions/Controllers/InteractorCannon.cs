﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorCannon : FurniInteractorModel
    {
        private HashSet<Point> _mCoords;
        private RoomItem _mItem;

        public override void OnPlace(GameClient session, RoomItem item)
        {
//            item.ExtraData = "0";
        }

        public override void OnRemove(GameClient session, RoomItem item)
        {
//            item.ExtraData = "0";
        }

        public override Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            var room = session?.GetHabbo()?.CurrentRoom;
            if (room == null) return;

            if (!hasRights) return;
            if (item.OnCannonActing)
                return;

            item.OnCannonActing = true;

            var coords = new HashSet<Point>();

            var itemX = item.X;
            var itemY = item.Y;

            switch (item.Rot)
            {
                case 0: // TESTEADO OK
                    var startingcoordX = itemX - 1;

                    for (var i = startingcoordX; i > 0; i--)
                        coords.Add(new Point(i, itemY));

                    break;

                case 4: // TESTEADO OK
                    var startingcoordX2 = itemX + 2;
                    var mapsizeX = item.GetRoom().GetGameMap().Model.MapSizeX;

                    for (var i = startingcoordX2; i < mapsizeX; i++)
                        coords.Add(new Point(i, itemY));

                    break;

                case 2: // TESTEADO OK
                    var startingcoordY = itemY - 1;

                    for (var i = startingcoordY; i > 0; i--)
                        coords.Add(new Point(itemX, i));

                    break;

                case 6: // OK!
                    var startingcoordY2 = itemY + 2;
                    var mapsizeY = item.GetRoom().GetGameMap().Model.MapSizeY;

                    for (var i = startingcoordY2; i < mapsizeY; i++)
                        coords.Add(new Point(itemX, i));

                    break;
            }

            item.ExtraData = (item.ExtraData == "0") ? "1" : "0";
            item.UpdateState();

            _mItem = item;
            _mCoords = coords;

            var explodeTimer = new Timer(1350);
            explodeTimer.Elapsed += ExplodeAndKick;
            explodeTimer.Enabled = true;
        }

        public override void OnWiredTrigger(RoomItem item)
        {
            if (item.OnCannonActing)
                return;

            item.OnCannonActing = true;

            var coords = new HashSet<Point>();

            var itemX = item.X;
            var itemY = item.Y;

            switch (item.Rot)
            {
                case 0: // TESTEADO OK
                    var startingcoordX = itemX - 1;

                    for (var i = startingcoordX; i > 0; i--)
                        coords.Add(new Point(i, itemY));

                    break;

                case 4: // TESTEADO OK
                    var startingcoordX2 = itemX + 2;
                    var mapsizeX = item.GetRoom().GetGameMap().Model.MapSizeX;

                    for (var i = startingcoordX2; i < mapsizeX; i++)
                        coords.Add(new Point(i, itemY));

                    break;

                case 2: // TESTEADO OK
                    var startingcoordY = itemY - 1;

                    for (var i = startingcoordY; i > 0; i--)
                        coords.Add(new Point(itemX, i));

                    break;

                case 6: // OK!
                    var startingcoordY2 = itemY + 2;
                    var mapsizeY = item.GetRoom().GetGameMap().Model.MapSizeY;

                    for (var i = startingcoordY2; i < mapsizeY; i++)
                        coords.Add(new Point(itemX, i));

                    break;
            }

            item.ExtraData = (item.ExtraData == "0") ? "1" : "0";
            item.UpdateState();

            _mItem = item;
            _mCoords = coords;

            var explodeTimer = new Timer(1350);
            explodeTimer.Elapsed += ExplodeAndKick;
            explodeTimer.Enabled = true;
        }

        private void ExplodeAndKick(object source, ElapsedEventArgs e)
        {
            var timer = (Timer)source;
            timer.Stop();

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            serverMessage.AppendString("room.kick.cannonball");
            serverMessage.AppendInteger(2);
            serverMessage.AppendString("link");
            serverMessage.AppendString("event:");
            serverMessage.AppendString("linkTitle");
            serverMessage.AppendString("ok");

            var room = _mItem.GetRoom();

            var toRemove = new HashSet<RoomUser>();

            /* TODO CHECK */ foreach (
                var user in
                    _mCoords.SelectMany(
                        coord =>
                            room.GetGameMap()
                                .GetRoomUsers(coord)
                                .Where(
                                    user =>
                                        user != null && !user.IsBot && !user.IsPet &&
                                        user.GetUserName() != room.RoomData.Owner)))
            {
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(4, false);
                toRemove.Add(user);
            }

            /* TODO CHECK */ foreach (var user in toRemove)
            {
                room.GetRoomUserManager().RemoveUserFromRoom(user, true, false);
                user.GetClient().SendMessage(serverMessage);
            }

            _mItem.OnCannonActing = false;
        }
    }
}