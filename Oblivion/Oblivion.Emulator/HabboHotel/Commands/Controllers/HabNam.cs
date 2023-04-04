using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HabNam. This class cannot be inherited.
    /// </summary>
    internal sealed class HabNam : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HabNam" /> class.
        /// </summary>
        public HabNam()
        {
            MinRank = 1;
            Description = "Enable/disable habnam";
            Usage = ":habnam";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            await session.GetHabbo()
                .GetAvatarEffectsInventoryComponent()
                .ActivateCustomEffect(user != null && user.CurrentEffect != 140 ? 140 : 0);

            return true;
        }
    }
}