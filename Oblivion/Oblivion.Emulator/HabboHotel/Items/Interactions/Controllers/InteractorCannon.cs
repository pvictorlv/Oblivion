using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Oblivion.Configuration;
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

        

        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
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
            await  item.UpdateState();

            _mItem = item;
            _mCoords = coords;

            var explodeTimer = new Timer(1350);
            explodeTimer.Elapsed += ExplodeAndKick;
            explodeTimer.Enabled = true;
        }

        public override async Task OnWiredTrigger(RoomItem item)
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
            await  item.UpdateState();

            _mItem = item;
            _mCoords = coords;

            var explodeTimer = new Timer(1350);
            explodeTimer.Elapsed += ExplodeAndKick;
            explodeTimer.Enabled = true;
        }

        private async void ExplodeAndKick(object source, ElapsedEventArgs e)
        {
            try
            {
                var timer = (Timer)source;
                timer.Stop();

                var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                await serverMessage.AppendStringAsync("room.kick.cannonball");
                await serverMessage.AppendIntegerAsync(2);
                await serverMessage.AppendStringAsync("link");
                await serverMessage.AppendStringAsync("event:");
                await serverMessage.AppendStringAsync("linkTitle");
                await serverMessage.AppendStringAsync("ok");

                var room = _mItem.GetRoom();

                var toRemove = new HashSet<RoomUser>();

                /* TODO CHECK */
                foreach (Point coord in _mCoords)
                foreach (RoomUser user in room.GetGameMap()
                             .GetRoomUsers(coord))
                {
                    if (user != null && !user.IsBot && !user.IsPet && user.GetUserName() != room.RoomData.Owner)
                    {
                        await user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent()
                            .ActivateCustomEffect(4, false);
                        toRemove.Add(user);
                    }
                }

                /* TODO CHECK */
                foreach (var user in toRemove)
                {
                    await room.GetRoomUserManager().RemoveUserFromRoom(user, true, false);
                    await user.GetClient().SendMessageAsync(serverMessage);
                }

                _mItem.OnCannonActing = false;
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "InteractorCannon.ExplodeAndKick()");
            }
        }
    }
}