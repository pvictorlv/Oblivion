using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Interfaces
{
    internal interface IFurniInteractor
    {
        Task OnPlace(GameClient session, RoomItem item);

        Task OnRemove(GameClient session, RoomItem item);

        Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights);

        Task OnUserWalk(GameClient session, RoomItem item, RoomUser user);
        Task OnUserWalkOff(GameClient session, RoomItem item, RoomUser user);

        Task OnWiredTrigger(RoomItem item);
    }
}