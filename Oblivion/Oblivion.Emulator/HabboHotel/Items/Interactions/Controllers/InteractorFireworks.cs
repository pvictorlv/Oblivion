using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFireworks : FurniInteractorModel
    {
        public override async Task OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "1";
        }

        public override async Task OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "1";
        }

        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item.ExtraData == "" || item.ExtraData == "0")
            {
                item.ExtraData = "1";
                await  item.UpdateState();
                return;
            }
            if (item.ExtraData == "1")
                item.ExtraData = "2";
        }
    }
}