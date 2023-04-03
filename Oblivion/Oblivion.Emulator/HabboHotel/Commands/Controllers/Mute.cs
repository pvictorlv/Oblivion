using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Mute. This class cannot be inherited.
    /// </summary>
    internal sealed class Mute : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Mute" /> class.
        /// </summary>
        public Mute()
        {
            MinRank = 4;
            Description = "Mute a selected user.";
            Usage = ":mute [USERNAME]";
            MinParams = 1;
            BlockBad = true;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client?.GetHabbo() == null)
            {
                await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            if (client.GetHabbo().Rank >= 4)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
            }

            await Oblivion.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Mute", "Muted user");
            client.GetHabbo().Mute();
            return true;
        }
    }
}