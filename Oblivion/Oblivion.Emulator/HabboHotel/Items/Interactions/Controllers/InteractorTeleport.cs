using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Handlers;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorTeleport : FurniInteractorModel
    {
        public override void OnPlace(GameClient session, RoomItem item)
        {

            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                var user1 = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user1 != null)
                {
                    user1.ClearMovement();
                    user1.AllowOverride = false;
                    user1.CanWalk = true;
                }

                item.InteractingUser = 0;
            }

            if (item.InteractingUser2 != 0)
            {
                var user2 = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);

                if (user2 != null)
                {
                    user2.ClearMovement();
                    user2.AllowOverride = false;
                    user2.CanWalk = true;
                }

                item.InteractingUser2 = 0;
            }

            

        }

        public override void OnRemove(GameClient session, RoomItem item)
        {
            item.TeleporterId = "0";
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                user?.UnlockWalking();

                item.InteractingUser = 0;
            }

            if (item.InteractingUser2 != 0)
            {
                var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);

                user?.UnlockWalking();

                item.InteractingUser2 = 0;
            }
        }

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item?.GetRoom() == null || session?.GetHabbo() == null)
                return;

            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.VirtualId);

            if (user != null)
            {
                if (user.Coordinate == item.Coordinate || user.Coordinate == item.SquareInFront)
                {
                    if (item.InteractingUser != 0)
                        return;

                    item.InteractingUser = user.GetClient().VirtualId;
                }
                else if (user.CanWalk)
                    user.MoveTo(item.SquareInFront);
            }
            

        }
    }
}