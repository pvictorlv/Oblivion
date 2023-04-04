using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFreezeGate : FurniInteractorModel
    {
        public override async Task OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (item == null) return;

            var num4 = (int) (item.Team + 39);
            var teamManagerForFreeze =
                user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();
            var avatarEffectsInventoryComponent2 =
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();
            if (user.Team != item.Team)
            {
                if (teamManagerForFreeze.CanEnterOnTeam(item.Team))
                {
                    if (user.Team != Team.None) teamManagerForFreeze.OnUserLeave(user);
                    user.Team = item.Team;
                    teamManagerForFreeze.AddUser(user);
                    if (avatarEffectsInventoryComponent2.CurrentEffect != num4)
                        await avatarEffectsInventoryComponent2.ActivateCustomEffect(num4);
                }
            }
            else
            {
                teamManagerForFreeze.OnUserLeave(user);
                if (avatarEffectsInventoryComponent2.CurrentEffect == num4)
                    await avatarEffectsInventoryComponent2.ActivateCustomEffect(0);
                user.Team = Team.None;
            }

            var serverMessage33 =
                new ServerMessage(
                    LibraryParser.OutgoingRequest("UserIsPlayingFreezeMessageComposer"));
            serverMessage33.AppendBool(user.Team != Team.None);
            await user.GetClient().SendMessageAsync(serverMessage33);
        }
    }
}