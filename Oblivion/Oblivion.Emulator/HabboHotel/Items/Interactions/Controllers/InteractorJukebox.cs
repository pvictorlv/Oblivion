using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorJukebox : FurniInteractorModel
    {
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            if (item.ExtraData == "1")
            {
                item.GetRoom().GetRoomMusicController().Stop();
                item.ExtraData = "0";
            }
            else
            {
                item.GetRoom().GetRoomMusicController().Start();
                item.ExtraData = "1";
            }

            await  item.UpdateState();
        }

        public override async Task OnWiredTrigger(RoomItem item)
        {
            if (item.ExtraData == "1")
            {
                item.GetRoom().GetRoomMusicController().Stop();
                item.ExtraData = "0";
            }
            else
            {
                item.GetRoom().GetRoomMusicController().Start();
                item.ExtraData = "1";
            }

            await  item.UpdateState();
        }
    }
}