using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorScoreboard : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            int num;
            int.TryParse(item.ExtraData, out num);
            Out.WriteLine(request.ToString());
            switch (request)
            {
                case 1:
                    if (item.PendingReset && num > 0)
                    {
                        num = 0;
                        item.PendingReset = false;
                    }
                    else
                    {
                        num = num + 60;
                        item.UpdateNeeded = false;
                    }
                    break;

                case 2:
                    item.ExtraData = (num + 1).ToString();
                    item.UpdateNeeded = true;
                    item.PendingReset = true;
                    break;
                case 3:
                    break;
            }

            item.ExtraData = num.ToString();
            item.UpdateState();
        }

        public override void OnWiredTrigger(RoomItem item)
        {
            int num;
            int.TryParse(item.ExtraData, out num);

            num += 60;

            item.UpdateNeeded = false;
            item.ExtraData = num.ToString();
            item.UpdateState();
        }
    }
}