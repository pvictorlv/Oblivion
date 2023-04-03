using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Interfaces;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Models
{
    internal class FurniInteractorModel : IFurniInteractor
    {
        public virtual void OnPlace(GameClient session, RoomItem item)
        {
        }

        public virtual void OnRemove(GameClient session, RoomItem item)
        {
        }

        public virtual Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            return Task.CompletedTask;
        }

        public virtual void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public virtual void OnUserWalkOff(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public virtual void OnWiredTrigger(RoomItem item)
        {
        }
    }
}