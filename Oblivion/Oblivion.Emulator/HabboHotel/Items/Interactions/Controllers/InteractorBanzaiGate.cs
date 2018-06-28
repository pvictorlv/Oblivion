using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorBanzaiGate : FurniInteractorModel
    {
        public override void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            var effect = (int) item.Team + 32;
            var teamManagerForBanzai =
                user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();
            var avatarEffectsInventoryComponent =
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();
            if (user.Team == Team.None)
            {
                if (!teamManagerForBanzai.CanEnterOnTeam(item.Team)) return;
                user.Team = item.Team;
                teamManagerForBanzai.AddUser(user);
                if (avatarEffectsInventoryComponent.CurrentEffect != effect)
                    avatarEffectsInventoryComponent.ActivateCustomEffect(effect);
                return;
            }

            if (user.Team != Team.None && user.Team != item.Team)
            {
                teamManagerForBanzai.OnUserLeave(user);
                user.Team = Team.None;
                avatarEffectsInventoryComponent.ActivateCustomEffect(0);
                return;
            }

            teamManagerForBanzai.OnUserLeave(user);

            if (avatarEffectsInventoryComponent.CurrentEffect == effect)
                avatarEffectsInventoryComponent.ActivateCustomEffect(0);
            user.Team = Team.None;
        }
    }
}