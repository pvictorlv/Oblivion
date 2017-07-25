using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Security;
using Oblivion.Security.BlackWords;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshBannedHotels. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshBannedHotels : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshBannedHotels" /> class.
        /// </summary>
        public RefreshBannedHotels()
        {
            MinRank = 9;
            Description = "Refreshes BlackWords filter from Database.";
            Usage = ":refresh_banned_hotels";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Filter.Reload();
            BlackWordsManager.Reload();

            session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_banned_hotels"));
            return true;
        }
    }
}