using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorTent : FurniInteractorModel
    {
        public override async Task OnUserWalkOff(GameClient session, RoomItem item, RoomUser user)
        {
            if (!user.IsBot && user.OnCampingTent)
            {
                var serverMessage = new ServerMessage();
                await serverMessage.InitAsync(
                    LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                await serverMessage.AppendStringAsync(item.Id.ToString());
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendStringAsync("0");
                await user.GetClient().SendMessageAsync(serverMessage);
                user.OnCampingTent = false;
            }
        }

        public override async Task OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (user.LastItem == item.Id)
            {
                user.OnCampingTent = true;
                return;
            }

            if (!user.IsBot && !user.OnCampingTent)
            {
                var serverMessage22 = new ServerMessage();
                await serverMessage22.InitAsync(
                    LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                await serverMessage22.AppendStringAsync(item.Id.ToString());
                await serverMessage22.AppendIntegerAsync(0);
                await serverMessage22.AppendStringAsync("1");
                await user.GetClient().SendMessageAsync(serverMessage22);
                user.OnCampingTent = true;
                user.LastItem = item.Id;
                user.OnCampingTent = true;
            }
        }
    }
}