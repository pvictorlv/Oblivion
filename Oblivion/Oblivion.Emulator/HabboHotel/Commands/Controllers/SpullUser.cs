using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class PullUser. This class cannot be inherited.
    /// </summary>
    internal sealed class SpullUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PullUser" /> class.
        /// </summary>
        public SpullUser()
        {
            MinRank = -3;
            Description = "Puxe um usuário com força!";
            Usage = ":spull [username]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.VirtualId);
            if (user == null) return true;

            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.VirtualId == session.VirtualId)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("command_pull_error_own"));
                return true;
            }
            var user2 = room.GetRoomUserManager().GetRoomUserByHabbo(client.VirtualId);
            if (user2 == null) return true;
            if (user2.TeleportEnabled)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("command_error_teleport_enable"));
                return true;
            }

            if ((user.RotBody % 2) != 0) user.RotBody--;
            switch (user.RotBody)
            {
                case 0:
                    user2.MoveTo(user.X, user.Y - 1);
                    break;

                case 2:
                    user2.MoveTo(user.X + 1, user.Y);
                    break;

                case 4:
                    user2.MoveTo(user.X, user.Y + 1);
                    break;

                case 6:
                    user2.MoveTo(user.X - 1, user.Y);
                    break;
            }
            return true;
        }
    }
}