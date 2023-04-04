using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFreezeTile : FurniInteractorModel
    {
        public override Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null || session.GetHabbo() == null || item.InteractingUser > 0U)
                return Task.CompletedTask;

            var pName = session.GetHabbo().UserName;
            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(pName);
            if (roomUserByHabbo != null)
            {
                roomUserByHabbo.GoalX = item.X;
                roomUserByHabbo.GoalY = item.Y;

                if (roomUserByHabbo.Team != Team.None)
                    roomUserByHabbo.ThrowBallAtGoal = true;
            }

            return Task.CompletedTask;
        }
    }
}