using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class SuperBan. This class cannot be inherited.
    /// </summary>
    internal sealed class SuperBan : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SuperBan" /> class.
        /// </summary>
        public SuperBan()
        {
            MinRank = 5;
            Description = "Super ban a user!";
            Usage = ":superban [USERNAME] [REASON]";
            MinParams = -1;
            BlockBad = true;

        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {

            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName, "Ban",
                    "User has received a Super ban.");
            Oblivion.GetGame()
                .GetBanManager()
                .BanUser(client, session.GetHabbo().UserName, 788922000.0, string.Join(" ", pms.Skip(1)),
                    false, false);
            return true;
        }
    }
}