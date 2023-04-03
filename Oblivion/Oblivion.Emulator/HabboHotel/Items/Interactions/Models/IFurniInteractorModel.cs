using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Interfaces;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Models
{
    internal class FurniInteractorModel : IFurniInteractor
    {
        public virtual Task OnPlace(GameClient session, RoomItem item)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnRemove(GameClient session, RoomItem item)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUserWalkOff(GameClient session, RoomItem item, RoomUser user)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnWiredTrigger(RoomItem item)
        {
            return Task.CompletedTask;
        }
    }
}