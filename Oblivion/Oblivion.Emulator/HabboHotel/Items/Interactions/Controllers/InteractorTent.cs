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
        public override void OnUserWalkOff(GameClient session, RoomItem item, RoomUser user)
        {
            if (!user.IsBot && user.OnCampingTent)
            {
                var serverMessage = new ServerMessage();
                serverMessage.Init(
                    LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
                serverMessage.AppendInteger(0);
                serverMessage.AppendString("0");
                await user.GetClient().SendMessageAsync(serverMessage);
                user.OnCampingTent = false;
            }
        }

        public override void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (user.LastItem == item.Id)
            {
                user.OnCampingTent = true;
                return;
            }

            if (!user.IsBot && !user.OnCampingTent)
            {
                var serverMessage22 = new ServerMessage();
                serverMessage22.Init(
                    LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                serverMessage22.AppendString(item.Id.ToString());
                serverMessage22.AppendInteger(0);
                serverMessage22.AppendString("1");
                await user.GetClient().SendMessageAsync(serverMessage22);
                user.OnCampingTent = true;
                user.LastItem = item.Id;
                user.OnCampingTent = true;
            }
        }
    }
}