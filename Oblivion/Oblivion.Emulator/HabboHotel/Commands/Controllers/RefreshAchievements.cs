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

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                Oblivion.GetGame().GetAchievementManager().LoadAchievements(dbClient);
            }

            session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_achievements"));
            return true;
        }
    }
}