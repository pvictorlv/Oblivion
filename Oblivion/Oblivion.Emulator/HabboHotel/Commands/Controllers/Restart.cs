using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Shutdown. This class cannot be inherited.
    /// </summary>
    internal sealed class Restart : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Shutdown" /> class.
        /// </summary>
        public Restart()
        {
            MinRank = 11;
            Description = "Restart the Server.";
            Usage = ":restart";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            if (session.GetHabbo().UserName != "Dark") return false;

            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, string.Empty, "Restart",
                    "Issued Restart command!");
            new Task(Oblivion.PerformRestart).Start();
            return true;
        }
    }
}