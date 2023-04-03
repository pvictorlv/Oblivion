using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshItems. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshItems : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshItems" /> class.
        /// </summary>
        public RefreshItems()
        {
            MinRank = 9;
            Description = "Refreshes Items from Database.";
            Usage = ":refresh_items";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            Oblivion.GetGame().ReloadItems();
            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_items"));
            return true;
        }
    }
}