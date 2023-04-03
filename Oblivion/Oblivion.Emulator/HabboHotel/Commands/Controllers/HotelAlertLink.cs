using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class HotelAlertLink : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HotelAlert" /> class.
        /// </summary>
        public HotelAlertLink()
        {
            MinRank = 5;
            Description = "send a message with a link.";
            Usage = ":hal [url] [message]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var messageUrl = pms[0];
            var messageStr = string.Join(" ", pms.Skip(1));

           await Oblivion.GetGame()
                .GetClientManager()
                .SendSuperNotif("${catalog.alert.external.link.title}", messageStr, "game_promo_small", session,
                    messageUrl, "${facebook.create_link_in_web}", true, false);
            return true;
        }
    }
}