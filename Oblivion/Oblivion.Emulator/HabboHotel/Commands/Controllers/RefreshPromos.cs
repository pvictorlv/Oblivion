using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshPromos : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshPromos" /> class.
        /// </summary>
        public RefreshPromos()
        {
            MinRank = 5;
            Description = "Refresh promos cache.";
            Usage = ":refresh_promos";
            MinParams = 0;
        }

        public override Task<bool> Execute(GameClient session, string[] pms)
        {
            Oblivion.GetGame().GetHotelView().RefreshPromoList();
            return Task.FromResult(true);
        }
    }
}