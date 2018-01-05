using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class TelePort. This class cannot be inherited.
    /// </summary>
    internal sealed class TelePort : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TelePort" /> class.
        /// </summary>
        public TelePort()
        {
            MinRank = 2;
            Description = "Teleport around the room, like a kingorooo.";
            Usage = ":teleport";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (!room.CheckRights(session, false))
            {
                return false;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;
            if (!user.RidingHorse)
            {
                user.TeleportEnabled = !user.TeleportEnabled;
                room.GetGameMap().GenerateMaps();
                return true;
            }
            session.SendWhisper(Oblivion.GetLanguage().GetVar("command_error_teleport_enable"));
            return true;
        }
    }
}