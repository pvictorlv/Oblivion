using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorBanzaiTele : FurniInteractorModel
    {
        public override async Task OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (user.IsWalking)
                user.GetRoom().GetGameItemHandler().OnTeleportRoomUserEnter(user, item);
        }


    }
}