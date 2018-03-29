using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFreezeTile : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session?.GetHabbo() == null || item.InteractingUser > 0U)
                return;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().CurrentRoomId);
            if (roomUserByHabbo != null)
            {
                roomUserByHabbo.GoalX = item.X;
                roomUserByHabbo.GoalY = item.Y;

                if (roomUserByHabbo.Team != Team.None)
                    roomUserByHabbo.ThrowBallAtGoal = true;
            }
        }
    }
}