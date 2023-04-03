using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveDiamonds. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveDiamonds : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveDiamonds" /> class.
        /// </summary>
        public GiveDiamonds()
        {
            MinRank = 11;
            Description = "Gives user Diamonds.";
            Usage = ":diamonds [USERNAME] [AMOUNT]";
            MinParams = 2;
            BlockBad = true;

        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }


            if (!int.TryParse(pms[1], out var amount))
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            client.GetHabbo().Diamonds += amount;
            client.GetHabbo().UpdateSeasonalCurrencyBalance();
            client.SendNotif(string.Format(Oblivion.GetLanguage().GetVar("staff_gives_diamonds"),
                session.GetHabbo().UserName, amount));
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Diamonds", $"Diamonds given to user [{pms[0]}]");
            return true;
        }
    }
}