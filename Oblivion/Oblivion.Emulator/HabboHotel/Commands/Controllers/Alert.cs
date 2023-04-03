using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Alert. This class cannot be inherited.
    /// </summary>
    internal sealed class Alert : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Alert" /> class.
        /// </summary>
        public Alert()
        {
            MinRank = 5;
            Description = "Alerts a User.";
            Usage = ":alert [USERNAME] [MESSAGE]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var userName = pms[0];
            var msg = string.Join(" ", pms.Skip(1));

            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(userName);
            if (client == null)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            await client.SendNotif(string.Format("{0} \r\r-{1}", msg, session.GetHabbo().UserName));
            return true;
        }
    }
}