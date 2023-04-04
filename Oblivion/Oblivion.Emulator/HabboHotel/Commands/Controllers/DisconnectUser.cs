using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class DisconnectUser. This class cannot be inherited.
    /// </summary>
    internal sealed class DisconnectUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisconnectUser" /> class.
        /// </summary>
        public DisconnectUser()
        {
            MinRank = 7;
            Description = "dc user.";
            Usage = ":dc [username]";
            MinParams = 1;
            BlockBad = true;

        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var user = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (user?.GetHabbo() == null)
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (user.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            try
            {
                user.Dispose();
                Oblivion.GetGame()
                    .GetModerationTool()
                    .LogStaffEntry(session.GetHabbo().UserName, user.GetHabbo().UserName, "dc",
                        $"Disconnect User[{pms[1]}]");
            }
            catch
            {
            }

            return true;
        }
    }
}