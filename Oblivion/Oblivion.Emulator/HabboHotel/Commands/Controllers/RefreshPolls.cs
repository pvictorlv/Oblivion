using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshPolls. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshPolls : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshPolls" /> class.
        /// </summary>
        public RefreshPolls()
        {
            MinRank = 9;
            Description = "Refreshes Polls from Database.";
            Usage = ":refresh_polls";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
                Oblivion.GetGame().GetPollManager().Init(adapter);
            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_polls"));
            return true;
        }
    }
}