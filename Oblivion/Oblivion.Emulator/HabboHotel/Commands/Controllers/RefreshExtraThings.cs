using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshExtraThings : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshExtraThings" /> class.
        /// </summary>
        public RefreshExtraThings()
        {
            MinRank = 5;
            Description = "Refresh Extra things cache.";
            Usage = ":refresh_extrathings";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Oblivion.GetGame().GetHallOfFame().RefreshHallOfFame();
            Oblivion.GetGame().GetRoomManager().GetCompetitionManager().RefreshCompetitions();
            Oblivion.GetGame().GetTargetedOfferManager().LoadOffer();
            return true;
        }
    }
}