using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorScoreboard : FurniInteractorModel
    {
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            // Request 1 - Decrease value with red button
            // Request 2 - Increase value with green button
            // Request 3 - Reset with UI/Wired/Double click

            if (!int.TryParse(item.ExtraData, out var oldValue))
            {
                oldValue = 0;
            }

            if (oldValue >= 0 && oldValue <= 99 && request == 1)
            {
                if (oldValue > 0)
                    oldValue--;
                else if (oldValue == 0)
                    oldValue = 99;
            }

            else if (oldValue >= 0 && oldValue <= 99 && request == 2)
            {
                if (oldValue < 99)
                    oldValue++;
                else if (oldValue == 99)
                    oldValue = 0;
            }

            else if (request == 3)
            {
                oldValue = 0;
                item.PendingReset = true;
            }

            item.ExtraData = oldValue.ToString();
            await  item.UpdateState();
        }

        public override async Task OnWiredTrigger(RoomItem item)
        {
            item.ExtraData = "0";
            await  item.UpdateState();
        }
    }
}