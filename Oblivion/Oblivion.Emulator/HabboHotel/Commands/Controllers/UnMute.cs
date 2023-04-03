using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class UnMute. This class cannot be inherited.
    /// </summary>
    internal sealed class UnMute : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnMute" /> class.
        /// </summary>
        public UnMute()
        {
            MinRank = 4;
            Description = "UnMutes the selected user.";
            Usage = ":unmute [USERNAME]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client?.GetHabbo() == null)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().Rank >= 4)
            {
                 await Session.SendWhisperAsync("You are not allowed to mute that user.");
            }

            client.GetHabbo().BobbaFiltered = 0;

            Oblivion.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Unmute", "Unmuted user");
            client.GetHabbo().UnMute();

            Oblivion.MutedUsersByFilter.Remove(session.GetHabbo().Id);

            return true;
        }
    }
}