using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorSwitch : FurniInteractorModel
    {
        
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            RoomUser roomUser = null;

            if (session != null)
                roomUser = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (roomUser == null)
                return;

            if (Gamemap.TilesTouching(item.X, item.Y, roomUser.X, roomUser.Y))
            {
                var num = item.GetBaseItem().Modes - 1;
                int num3;
                int.TryParse(item.ExtraData, out var num2);

                if (num2 <= 0)
                    num3 = 1;
                else
                {
                    if (num2 >= num)
                        num3 = 0;
                    else
                        num3 = num2 + 1;
                }

                item.ExtraData = num3.ToString();
                await  item.UpdateState();
                if (item.GetRoom().GotWireds())
                    item.GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerStateChanged, roomUser, item);

                return;
            }

            await roomUser.MoveTo(item.SquareInFront);
        }
    }
}