using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class EmptyUser. This class cannot be inherited.
    /// </summary>
    internal sealed class EmptyUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EmptyUser" /> class.
        /// </summary>
        public EmptyUser()
        {
            MinRank = 7;
            Description = "Clears all the items from a users inventory.";
            Usage = ":empty_user [USERNAME]";
            MinParams = -1;
            BlockBad = true;

        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null || client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            client.GetHabbo().GetInventoryComponent().ClearItems();
            return true;
        }
    }
}