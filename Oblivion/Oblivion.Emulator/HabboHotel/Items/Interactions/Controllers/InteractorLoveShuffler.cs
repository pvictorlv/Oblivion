using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorLoveShuffler : FurniInteractorModel
    {
        public override async Task OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "-1";
            item.UpdateNeeded = true;
        }

        public override async Task OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "-1";
        }

        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            if (item.ExtraData == "0")
                return;

            item.ExtraData = "0";
            await item.UpdateState(false, true);
            item.ReqUpdate(10, true);
        }

        public override async Task OnWiredTrigger(RoomItem item)
        {
            if (item.ExtraData == "0")
                return;

            item.ExtraData = "0";
            await item.UpdateState(false, true);
            item.ReqUpdate(10, true);
        }
    }
}