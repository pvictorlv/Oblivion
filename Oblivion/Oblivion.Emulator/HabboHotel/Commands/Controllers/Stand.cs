using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Stand. This class cannot be inherited.
    /// </summary>
    internal sealed class Stand : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Stand" /> class.
        /// </summary>
        public Stand()
        {
            MinRank = 1;
            Description = "Stand";
            Usage = ":stand";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;

            if (user.IsSitting)
            {
                user.Statusses.TryRemove("sit", out _);
                user.IsSitting = false;
                user.UpdateNeeded = true;
            }
            else if (user.IsLyingDown)
            {
                user.Statusses.TryRemove("lay", out _);
                user.IsLyingDown = false;
                user.UpdateNeeded = true;
            }
            return true;
        }
    }
}