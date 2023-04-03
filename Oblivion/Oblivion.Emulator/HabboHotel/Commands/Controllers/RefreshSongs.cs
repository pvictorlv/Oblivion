using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.SoundMachine;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshSongs. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshSongs : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshSongs" /> class.
        /// </summary>
        public RefreshSongs()
        {
            MinRank = 9;
            Description = "Refreshes Songs from Database.";
            Usage = ":refresh_songs";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            SoundMachineSongManager.Initialize();
            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_songs"));
            return true;
        }
    }
}