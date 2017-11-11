using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class SetMax. This class cannot be inherited.
    /// </summary>
    internal sealed class FixRoom : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetMax" /> class.
        /// </summary>
        public FixRoom()
        {
            MinRank = -1;
            Description = "Sua sala bugou?";
            Usage = ":fixroom";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().CurrentRoom.GetGameMap().GenerateMaps();
            return true;
        }
    }
}