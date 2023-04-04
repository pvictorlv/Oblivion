using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshRanks. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshVirtual : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshRanks" /> class.
        /// </summary>
        public RefreshVirtual()
        {
            MinRank = 11;
            Description = "PLEASE DON'T USE THIS";
            Usage = ":refresh_virtual";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            Oblivion.GetGame().GetItemManager().ResetVirtualIds();
             await session.SendWhisperAsync("foi");
            return true;
        }
    }
}