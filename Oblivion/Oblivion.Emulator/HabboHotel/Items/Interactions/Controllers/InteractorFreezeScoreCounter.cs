using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFreezeScoreCounter : FurniInteractorModel
    {
        public override async Task OnPlace(GameClient session, RoomItem item)
        {
            if (item.Team == Team.None)
                return;

            item.ExtraData = item.GetRoom().GetGameManager().Points[(int)item.Team].ToString();
            await item.UpdateState(false, true);
        }

        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            item.GetRoom().GetGameManager().Points[(int)item.Team] = 0;
            item.ExtraData = "0";
            await  item.UpdateState();
        }
    }
}