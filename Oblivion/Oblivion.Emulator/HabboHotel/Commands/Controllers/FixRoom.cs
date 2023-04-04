using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class FixRoom : Command
    {
        public FixRoom()
        {
            MinRank = -1;
            Description = "Sua sala bugou?";
            Usage = ":fixroom";
            MinParams = 0;
        }

        public override Task<bool> Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().CurrentRoom.GetGameMap().GenerateMaps();
            return Task.FromResult(true);
        }
    }
}