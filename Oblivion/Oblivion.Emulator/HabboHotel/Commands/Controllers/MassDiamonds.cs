using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassDiamonds. This class cannot be inherited.
    /// </summary>
    internal sealed class MassDiamonds : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassDiamonds" /> class.
        /// </summary>
        public MassDiamonds()
        {
            MinRank = 8;
            Description = "Gives all the users online Diamonds.";
            Usage = ":massdiamonds [AMOUNT]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            int amount;
            if (!int.TryParse(pms[0], out amount))
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            /* TODO CHECK */ foreach (var client in Oblivion.GetGame().GetClientManager().Clients.Values)
            {
                if (client?.GetHabbo() == null) continue;
                var habbo = client.GetHabbo();
                habbo.Diamonds += amount;
                client.GetHabbo().UpdateSeasonalCurrencyBalance();
                client.SendNotif(Oblivion.GetLanguage().GetVar("command_diamonds_one_give") + amount +
                                 (Oblivion.GetLanguage().GetVar("command_diamonds_two_give")));
            }
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, "ALL ONLINES",
                    "Diamonds", $"Diamonds given to everyone");
            return true;
        }
    }
}