using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Lay. This class cannot be inherited.
    /// </summary>
    internal sealed class Lay : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Lay" /> class.
        /// </summary>
        public Lay()
        {
            MinRank = 11;
            Description = "Makes you lay.";
            Usage = ":lay";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var currentRoom = session.GetHabbo().CurrentRoom;

            var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUserByHabbo == null) return true;

            if (roomUserByHabbo.IsSitting || roomUserByHabbo.RidingHorse || roomUserByHabbo.IsWalking ||
                roomUserByHabbo.Statusses.ContainsKey("lay"))
                return true;

            if (roomUserByHabbo.RotBody % 2 != 0) roomUserByHabbo.RotBody--;
            roomUserByHabbo.Statusses.TryAdd("lay", "0.55");
            roomUserByHabbo.IsLyingDown = true;
            roomUserByHabbo.UpdateNeeded = true;
            return true;
        }
    }
}