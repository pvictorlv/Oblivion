using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveDuckets. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveDuckets : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveDuckets" /> class.
        /// </summary>
        public GiveDuckets()
        {
            MinRank = 5;
            Description = "Gives user Duckets.";
            Usage = ":duckets [USERNAME] [AMOUNT]";
            MinParams = 2;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            int amount;
            if (!int.TryParse(pms[1], out amount))
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            client.GetHabbo().ActivityPoints += amount;
            client.GetHabbo().UpdateActivityPointsBalance();
            client.SendNotif(string.Format(Oblivion.GetLanguage().GetVar("staff_gives_duckets"),
                session.GetHabbo().UserName, amount));
            return true;
        }
    }
}