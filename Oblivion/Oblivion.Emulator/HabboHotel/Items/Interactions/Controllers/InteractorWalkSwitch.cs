using System;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorWalkSwitch : FurniInteractorModel
    {
        public override async Task OnUserWalkOff(GameClient session, RoomItem item, RoomUser user)
        {
            item.ExtraData = "0";
            await  item.UpdateState();
        }

        public override async Task OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            var num = item.GetBaseItem().Modes - 1;

            if (session == null || num <= 0)
                return;

            int.TryParse(item.ExtraData, out var num2);
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

            item.ExtraData = num3.ToString();
            await  item.UpdateState();
        }
    }
}