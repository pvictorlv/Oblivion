using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MoonWalk. This class cannot be inherited.
    /// </summary>
    internal sealed class MoonWalk : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MoonWalk" /> class.
        /// </summary>
        public MoonWalk()
        {
            MinRank = 1;
            Description = "Enable/disable Moonwalk";
            Usage = ":moonwalk";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;
            user.IsMoonwalking = !user.IsMoonwalking;

            return true;
        }
    }
}