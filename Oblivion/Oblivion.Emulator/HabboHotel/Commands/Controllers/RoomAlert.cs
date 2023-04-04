using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class RoomAlert : Command
    {
        public RoomAlert()
        {
            MinRank = -3;
            Description = "Alerts the Room.";
            Usage = ":roomalert [MESSAGE]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null) return true;
            var alert = string.Join(" ", pms);

            if (!room.CheckRights(session, true) && !session.GetHabbo().HasFuse("fuse_mod")) return false;
            await session.GetHabbo().CurrentRoom.SendMessage(GameClient.GetBytesNotif(alert));

            return true;
        }
    }
}