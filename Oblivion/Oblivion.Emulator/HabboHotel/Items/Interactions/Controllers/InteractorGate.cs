using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorGate : FurniInteractorModel
    {
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            if (item?.GetBaseItem() == null || item.GetBaseItem().InteractionType != Interaction.Gate)
                return;

            var modes = item.GetBaseItem().Modes - 1;

            if (modes <= 0)
                await item.UpdateState(false, true);

            if (item.GetRoom() == null || item.GetRoom().GetGameMap() == null ||
                item.GetRoom().GetGameMap().SquareHasUsers(item.X, item.Y))
                return;

            if (!int.TryParse(item.ExtraData, out var currentMode))
            {
                currentMode = 0;
            }
            int newMode;

            if (currentMode <= 0)
                newMode = 1;
            else if (currentMode >= modes)
                newMode = 0;
            else
                newMode = currentMode + 1;

            if (newMode == 0 && item.GetRoom().GetGameMap().SquareHasUsers(item.X, item.Y))
                return;

            if (newMode == 0)
            {
                item.GetRoom().GetGameMap().GameMap[item.X, item.Y] = 0;
            }
            else
            {
                item.GetRoom().GetGameMap().GameMap[item.X, item.Y] = 1;

            }

            item.ExtraData = newMode.ToString();
            await  item.UpdateState();
           
            if (item.GetRoom().GotWireds())
                await item.GetRoom()
                    .GetWiredHandler()
                    .ExecuteWired(Interaction.TriggerStateChanged,
                        item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id), item);

            if (item.GetBaseItem().Width > 1 || item.GetBaseItem().Length > 1)
            {
                await item.GetRoom().GetGameMap().UpdateMapForItem(item);
            }
        }

        public override async Task OnWiredTrigger(RoomItem item)
        {
            var num = item.GetBaseItem().Modes - 1;

            if (num <= 0)
                await item.UpdateState(false, true);

            if (item.GetRoom() == null || item.GetRoom().GetGameMap() == null ||
                item.GetRoom().GetGameMap().SquareHasUsers(item.X, item.Y))
                return;

            if (!int.TryParse(item.ExtraData, out var num2))
            {
                num2 = 0;
            }

            int num3;

            if (num2 <= 0)
                num3 = 1;
            else
            {
                if (num2 >= num)
                    num3 = 0;
                else
                    num3 = num2 + 1;
            }

            if (num3 == 0 && item.GetRoom().GetGameMap().SquareHasUsers(item.X, item.Y))
                return;

            if (num3 == 0)
            {
                item.GetRoom().GetGameMap().GameMap[item.X, item.Y] = 0;
            }
            else
            {
                item.GetRoom().GetGameMap().GameMap[item.X, item.Y] = 1;

            }

            item.ExtraData = num3.ToString();
            await  item.UpdateState();
            if (item.GetBaseItem().Width > 1 || item.GetBaseItem().Length > 1)
            {
                await item.GetRoom().GetGameMap().UpdateMapForItem(item);
            }
        }
    }
}