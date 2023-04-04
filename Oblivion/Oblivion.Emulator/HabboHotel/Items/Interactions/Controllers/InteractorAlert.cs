using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorAlert : FurniInteractorModel
    {
        public override  Task OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            item.UpdateNeeded = true;
            return Task.CompletedTask;
        }

        public override  Task OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            return Task.CompletedTask;
        }

        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            if (item.ExtraData != "0")
                return;

            item.ExtraData = "1";
            await item.UpdateState(false, true);
            item.ReqUpdate(4, true);
        }

        public override async Task OnWiredTrigger(RoomItem item)
        {
            if (item.ExtraData != "0")
                return;

            item.ExtraData = "1";
            await item.UpdateState(false, true);
            item.ReqUpdate(4, true);
        }
    }
}