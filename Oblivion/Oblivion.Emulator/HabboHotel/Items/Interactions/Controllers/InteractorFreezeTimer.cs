using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFreezeTimer : FurniInteractorModel
    {
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!item.GetRoom().CheckRights(session))
                return;

            var num = 0;

            if (!string.IsNullOrEmpty(item.ExtraData))
                num = int.Parse(item.ExtraData);

            if (request == 2)
            {
                if (item.PendingReset && num > 0)
                {
                    num = 0;
                    item.PendingReset = false;
                }
                else
                {
                    if (num == 0 || num == 30 || num == 60 || num == 120 || num == 180 || num == 300 || num == 600)
                    {
                        switch (num)
                        {
                            case 0:
                                num = 30;
                                break;

                            case 30:
                                num = 60;
                                break;

                            case 60:
                                num = 120;
                                break;

                            case 120:
                                num = 180;
                                break;

                            case 180:
                                num = 300;
                                break;

                            case 300:
                                num = 600;
                                break;

                            case 600:
                                num = 0;
                                break;
                        }
                    }
                    else
                        num = 0;

                    item.UpdateNeeded = false;
                }
            }
            else
            {
                if (request == 1 && !item.GetRoom().GetFreeze().GameStarted)
                {
                    item.UpdateNeeded = !item.UpdateNeeded;

                    if (item.UpdateNeeded)
                        await item.GetRoom().GetFreeze().StartGame();
//                    item.GetRoom().GetSoccer().StartGame();
                    item.PendingReset = true;
                }
            }

            item.ExtraData = num.ToString();
            await  item.UpdateState();
        }
    }
}