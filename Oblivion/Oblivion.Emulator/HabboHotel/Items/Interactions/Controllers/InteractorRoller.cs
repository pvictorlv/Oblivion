using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorRoller : FurniInteractorModel
    {
        public override async Task OnRemove(GameClient session, RoomItem item)
        {
            item.GetRoom().GetRoomItemHandler().Rollers.Remove(item);
            if (item.GetRoom().GetRoomItemHandler().Rollers.Count <= 0)
            {
                item.GetRoom().GetRoomItemHandler().GotRollers = false;
            }
        }
    }
}