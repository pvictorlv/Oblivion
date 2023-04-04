using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FastWalk. This class cannot be inherited.
    /// </summary>
    internal sealed class FastWalk : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FastWalk" /> class.
        /// </summary>
        public FastWalk()
        {
            MinRank = -3;
            Description = "Enable/Disable Fast Walk.";
            Usage = ":fastwalk";
            MinParams = 0;
        }

        public override Task<bool> Execute(GameClient session, string[] pms)
        {
            var user =
                Oblivion.GetGame()
                    .GetRoomManager()
                    .GetRoom(session.GetHabbo().CurrentRoomId)
                    .GetRoomUserManager()
                    .GetRoomUserByHabbo(session.GetHabbo().Id);
            user.FastWalking = !user.FastWalking;
            return Task.FromResult(true);
        }
    }
}