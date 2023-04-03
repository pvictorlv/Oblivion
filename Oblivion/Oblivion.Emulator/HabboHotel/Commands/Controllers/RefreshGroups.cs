using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshGroups. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshGroups : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshGroups" /> class.
        /// </summary>
        public RefreshGroups()
        {
            MinRank = 9;
            Description = "Refreshes Groups from Database.";
            Usage = ":refresh_groups";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            Oblivion.GetGame().GetGroupManager().InitGroups();
            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_groups"));
            return true;
        }
    }
}