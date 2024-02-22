using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshAchievements. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshAchievements : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshAchievements" /> class.
        /// </summary>
        public RefreshAchievements()
        {
            MinRank = 9;
            Description = "Refreshes Achievements from Database.";
            Usage = ":refresh_achievements";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                Oblivion.GetGame().GetAchievementManager().LoadAchievements(dbClient);
            }

            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_achievements"));
            return true;
        }
    }
}