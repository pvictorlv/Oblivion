using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class GivePoints : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveBadge" /> class.
        /// </summary>
        public GivePoints()
        {
            MinRank = 6;
            Description = "Give points for user";
            Usage = ":epoints [username]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendNotif(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            client.GetHabbo().Diamonds += 10;
            client.GetHabbo().UpdateSeasonalCurrencyBalance();

            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(client, "JU", 1, true);

            client.SendNotif(string.Format(Oblivion.GetLanguage().GetVar("staff_gives_diamonds"),
                session.GetHabbo().UserName, 20));
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Diamonds", $"Diamonds given to user [{pms[0]}]");
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery($"UPDATE users SET epoints = epoints + 1 WHERE id = {client.GetHabbo().Id}");
            }
            return true;
        }
    }
}