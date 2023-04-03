using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorGroupForumTerminal : FurniInteractorModel
    {
        public override Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            uint.Parse(item.ExtraData);
        }
    }
}