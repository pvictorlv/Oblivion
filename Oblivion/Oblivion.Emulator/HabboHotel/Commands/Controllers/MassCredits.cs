using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassCredits. This class cannot be inherited.
    /// </summary>
    internal sealed class MassCredits : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassCredits" /> class.
        /// </summary>
        public MassCredits()
        {
            MinRank = 8;
            Description = "Gives all the users online credits.";
            Usage = ":masscredits [AMOUNT]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            if (!int.TryParse(pms[0], out var amount))
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
           foreach (var client in Oblivion.GetGame().GetClientManager().Clients.Values)
            {
                if (client?.GetHabbo() == null) continue;
                client.GetHabbo().Credits += amount;
                client.GetHabbo().UpdateCreditsBalance();
                client.SendNotif(Oblivion.GetLanguage().GetVar("command_mass_credits_one_give") + amount +
                                 (Oblivion.GetLanguage().GetVar("command_mass_credits_two_give")));
            }
            return true;
        }
    }
}