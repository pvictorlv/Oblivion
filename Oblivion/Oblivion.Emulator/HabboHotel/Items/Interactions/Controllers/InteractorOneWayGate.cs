using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorOneWayGate : FurniInteractorModel
    {
        public override Task OnPlace(GameClient session, RoomItem item)
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

            return Task.CompletedTask;
        }

        public override Task OnRemove(GameClient session, RoomItem item)
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

            return Task.CompletedTask;
        }

        public override Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null)
                return Task.CompletedTask;

            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null)
                return Task.CompletedTask;

            if (user.Coordinate != item.SquareInFront && user.CanWalk)
            {
                user.MoveTo(item.SquareInFront);
                return Task.CompletedTask;
            }

            if (!item.GetRoom().GetGameMap().CanWalk(item.SquareBehind.X, item.SquareBehind.Y, user.AllowOverride))
                return Task.CompletedTask;

            if (item.InteractingUser == 0)
            {
                item.InteractingUser = user.HabboId;

                user.CanWalk = false;

                if (user.IsWalking && (user.GoalX != item.SquareInFront.X || user.GoalY != item.SquareInFront.Y))
                    user.ClearMovement();

                user.AllowOverride = true;
                user.MoveTo(item.Coordinate);

                item.ReqUpdate(4, true);
            }

            return Task.CompletedTask;
        }
    }
}