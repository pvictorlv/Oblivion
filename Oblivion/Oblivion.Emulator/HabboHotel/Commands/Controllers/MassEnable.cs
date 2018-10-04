using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassEnable. This class cannot be inherited.
    /// </summary>
    internal sealed class MassEnable : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassEnable" /> class.
        /// </summary>
        public MassEnable()
        {
            MinRank = 7;
            Description = "Mass enable.";
            Usage = ":massenable [id]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            if (!ushort.TryParse(pms[0], out var effectId)) return true;

            var room = session.GetHabbo().CurrentRoom;
            room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            /* TODO CHECK */ foreach (var user in room.GetRoomUserManager().GetRoomUsers().Where(user => !user.RidingHorse))
                user.ApplyEffect(effectId);
            return true;
        }
    }
}