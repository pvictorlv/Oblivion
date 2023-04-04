using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveBadge : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveBadge" /> class.
        /// </summary>
        public GiveBadge()
        {
            MinRank = 5;
            Description = "Give user a badge.";
            Usage = ":givebadge [USERNAME] [badgeCode]";
            MinParams = 2;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            await client.GetHabbo().GetBadgeComponent().GiveBadge(pms[1], true, client);
            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_badge_give_done"));
            await Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Badge", $"Badge given to user [{pms[1]}]");
            return true;
        }
    }
}