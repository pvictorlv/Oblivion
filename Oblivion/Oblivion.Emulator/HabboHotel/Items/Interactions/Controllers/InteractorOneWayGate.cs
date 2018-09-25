using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorOneWayGate : FurniInteractorModel
    {
        public override void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null)
                {
                    user.ClearMovement();
                    user.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public override void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null)
                {
                    user.ClearMovement();
                    user.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null)
                return;

            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.VirtualId);

            if (user == null)
                return;

            if (user.Coordinate != item.SquareInFront && user.CanWalk)
            {
                user.MoveTo(item.SquareInFront);
                return;
            }

            if (!item.GetRoom().GetGameMap().CanWalk(item.SquareBehind.X, item.SquareBehind.Y, user.AllowOverride))
                return;

            if (item.InteractingUser == 0)
            {
                item.InteractingUser = user.UserId;

                user.CanWalk = false;

                if (user.IsWalking && (user.GoalX != item.SquareInFront.X || user.GoalY != item.SquareInFront.Y))
                    user.ClearMovement();

                user.AllowOverride = true;
                user.MoveTo(item.Coordinate);

                item.ReqUpdate(4, true);
            }
        }
    }
}