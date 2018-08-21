using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveDiamonds. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveEmeralds : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveDiamonds" /> class.
        /// </summary>
        public GiveEmeralds()
        {
            MinRank = 11;
            Description = "Gives user emeralds.";
            Usage = ":emeralds [USERNAME] [AMOUNT]";
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
            if (session.GetHabbo().UserName != "Dark" && session.GetHabbo().UserName != "Yagam" && session.GetHabbo().UserName != "Roberta") return false;
            if (!int.TryParse(pms[1], out var amount))
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            client.GetHabbo().Emeralds += amount;
            client.GetHabbo().UpdateSeasonalCurrencyBalance();
            client.SendNotif(string.Format(Oblivion.GetLanguage().GetVar("staff_gives_emeralds"),
                session.GetHabbo().UserName, amount));
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Emeralds", $"Emeralds given to user [{pms[0]}]");
            return true;
        }
    }
}