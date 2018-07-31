using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorRollerSkates : FurniInteractorModel
    {
        public override void OnUserWalkOff(GameClient session, RoomItem item, RoomUser user)
        {
            if (user.LastRollerDate + 60 < Oblivion.GetUnixTimeStamp())
            {
                Oblivion.GetGame().GetAchievementManager()
                    .ProgressUserAchievement(user.GetClient(), "ACH_RbTagC", 1);
                user.LastRollerDate = Oblivion.GetUnixTimeStamp();
            }

            Oblivion.GetGame().GetAchievementManager()
                .ProgressUserAchievement(user.GetClient(), "ACH_TagB", 1);

        }
    }
}