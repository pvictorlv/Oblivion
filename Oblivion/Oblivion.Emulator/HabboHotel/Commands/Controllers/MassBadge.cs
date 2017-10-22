using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class MassBadge : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassBadge" /> class.
        /// </summary>
        public MassBadge()
        {
            MinRank = 7;
            Description = "Give Badges to the users that is Online.";
            Usage = ":massbadge [badgeId]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            /* TODO CHECK */ foreach (
                var client in
                    Oblivion.GetGame()
                        .GetClientManager()
                        .Clients.Values.Where(client => client != null && client.GetHabbo() != null))
                client.GetHabbo().GetBadgeComponent().GiveBadge(pms[0], true, client);

            session.SendNotif(Oblivion.GetLanguage().GetVar("command_badge_give_done"));
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, "All",
                    "Badge", "Badge [" + pms[0] + "] given to all online users ATM");
            return true;
        }
    }
}