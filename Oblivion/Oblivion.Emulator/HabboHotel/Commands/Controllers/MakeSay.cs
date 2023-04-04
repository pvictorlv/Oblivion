using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class MakeSay : Command
    {
        public MakeSay()
        {
            MinRank = 9;
            Description = "Makes a selected user shout.";
            Usage = ":makesay [USERNAME] [MESSAGE]";
            MinParams = -1;
        }

        public override Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room?.GetRoomUserManager().GetRoomUserByHabbo(pms[0]);
            if (user == null) return Task.FromResult(true);

            var msg = string.Join(" ", pms.Skip(1));

            if (string.IsNullOrEmpty(msg)) return Task.FromResult(true);

            if (msg.StartsWith(":"))
            {
                return Task.FromResult(true);
            }

            user.Chat(user.GetClient(), msg, false, 0);
            return Task.FromResult(true);
        }
    }
}