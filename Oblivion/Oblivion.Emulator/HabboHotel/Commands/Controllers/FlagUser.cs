using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class FlagUser : Command
    {

        public FlagUser()
        {
            MinRank = 8;
            Description = "Enable name change for user";
            Usage = ":flaguser [name]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var name = pms[0];
            var user = Oblivion.GetGame().GetClientManager().GetClientByUserName(name);
            if (user?.GetHabbo() == null) return false;

            user.GetHabbo().LastChange = 0;
            return true;
        }
    }
}