using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshSettings. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshSettings : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshSettings" /> class.
        /// </summary>
        public RefreshSettings()
        {
            MinRank = 9;
            Description = "Refreshes Settings from Database.";
            Usage = ":refresh_settings";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            using (var adapter = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                Oblivion.ConfigData = new ConfigData(adapter);
            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_settings"));
            return true;
        }
    }
}