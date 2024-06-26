using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorHopper : FurniInteractorModel
    {
        public override Task OnPlace(GameClient session, RoomItem item)
        {
            item.GetRoom().GetRoomItemHandler().HopperCount++;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("INSERT INTO items_hopper (hopper_id, room_id) VALUES (@hopperid, @roomid);");
                queryReactor.AddParameter("hopperid", item.Id);
                queryReactor.AddParameter("roomid", item.RoomId);
                queryReactor.RunQuery();
            }

            if (item.InteractingUser == 0u)
                return Task.CompletedTask;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

            if (roomUserByHabbo != null)
            {
                roomUserByHabbo.ClearMovement();
                roomUserByHabbo.AllowOverride = false;
                roomUserByHabbo.CanWalk = true;
            }

            item.InteractingUser = 0u;
            return Task.CompletedTask;
        }

        public override Task OnRemove(GameClient session, RoomItem item)
        {
            item.GetRoom().GetRoomItemHandler().HopperCount--;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"DELETE FROM items_hopper WHERE item_id=@hid OR room_id={item.GetRoom().RoomId} LIMIT 1");
                queryReactor.AddParameter("hid", item.Id);
                queryReactor.RunQuery();
            }

            if (item.InteractingUser == 0u)
                return Task.CompletedTask;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

            roomUserByHabbo?.UnlockWalking();

            item.InteractingUser = 0u;
            return Task.CompletedTask;
        }

        public override Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item?.GetRoom() == null || session?.GetHabbo() == null)
                return Task.CompletedTask;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (roomUserByHabbo == null)
                return Task.CompletedTask;

            if (!(roomUserByHabbo.Coordinate == item.Coordinate) && !(roomUserByHabbo.Coordinate == item.SquareInFront))
            {
                if (roomUserByHabbo.CanWalk)
                    roomUserByHabbo.MoveTo(item.SquareInFront);
                return Task.CompletedTask;
            }

            if (item.InteractingUser != 0u)
                return Task.CompletedTask;

            roomUserByHabbo.TeleDelay = 2;
            item.InteractingUser = roomUserByHabbo.GetClient().GetHabbo().Id;
            return Task.CompletedTask;
        }
    }
}