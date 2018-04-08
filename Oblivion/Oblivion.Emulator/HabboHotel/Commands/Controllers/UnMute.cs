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

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client?.GetHabbo() == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().Rank >= 4)
            {
                session.SendWhisper("You are not allowed to mute that user.");
            }

            Oblivion.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Unmute", "Unmuted user");
            client.GetHabbo().UnMute();
            return true;
        }
    }
}