using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveCredits. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveCredits : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveCredits" /> class.
        /// </summary>
        public GiveCredits()
        {
            MinRank = 5;
            Description = "Gives user credits.";
            Usage = ":credits [USERNAME] [AMOUNT]";
            MinParams = 2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            int amount;
            if (!int.TryParse(pms[1], out amount))
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            client.GetHabbo().Credits += amount;
            client.GetHabbo().UpdateCreditsBalance();
            client.SendNotif(string.Format(Oblivion.GetLanguage().GetVar("staff_gives_credits"),
                session.GetHabbo().UserName, amount));
            return true;
        }
    }
}