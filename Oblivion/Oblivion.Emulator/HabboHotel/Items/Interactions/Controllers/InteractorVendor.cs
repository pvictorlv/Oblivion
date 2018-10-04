using System.Linq;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Rooms.User.Path;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorVendor : FurniInteractorModel
    {
        public override void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            item.UpdateNeeded = true;

            if (item.InteractingUser > 0u)
            {
                var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (roomUserByHabbo != null)
                    roomUserByHabbo.CanWalk = true;
            }
        }

        public override void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser <= 0u)
                return;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

            if (roomUserByHabbo != null)
                roomUserByHabbo.CanWalk = true;
        }

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item.ExtraData == "1" || !item.GetBaseItem().VendingIds.Any() || item.InteractingUser != 0u ||
                session == null)
                return;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (roomUserByHabbo == null)
                return;

            if (!Gamemap.TilesTouching(roomUserByHabbo.X, roomUserByHabbo.Y, item.X, item.Y))
            {
                roomUserByHabbo.MoveTo(item.SquareInFront);
                return;
            }

            item.InteractingUser = session.GetHabbo().Id;
            roomUserByHabbo.CanWalk = false;
            roomUserByHabbo.ClearMovement();

            roomUserByHabbo.SetRot(PathFinder.CalculateRotation(roomUserByHabbo.X, roomUserByHabbo.Y, item.X, item.Y));

            item.ReqUpdate(2, true);
            item.ExtraData = "1";
            item.UpdateState(false, true);
        }
    }
}