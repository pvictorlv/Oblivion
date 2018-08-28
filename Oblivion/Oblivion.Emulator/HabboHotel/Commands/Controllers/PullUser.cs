using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class PullUser. This class cannot be inherited.
    /// </summary>
    internal sealed class PullUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PullUser" /> class.
        /// </summary>
        public PullUser()
        {
            MinRank = 1;
            Description = "Pull User.";
            Usage = ":pulluser";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;
            

            if (room.RoomData.DisablePull)
            {
                session.SendWhisper("Realizar Pull Foi Desativado pelo Dono do Quarto");
                return true;
            }

            var users = room.GetGameMap().GetRoomUsers(user.SquaresInFront(2));
            if (users.Count <= 0) return true;

            var user2 = users[0];

            if (user2 == null) return true;
            if (user2.TeleportEnabled)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("command_error_teleport_enable"));
                return true;
            }
            if (user2.TeleportEnabled)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("command_error_teleport_enable"));
                return true;
            }
            
//            if ((user.RotBody % 2) != 0) user.RotBody--;
            switch (user.RotBody)
            {
                case 0:
                    user2.MoveTo(user.X, user.Y - 1);
                    break;
                case 1:
                    user2.MoveTo(user2.X - 1, user2.Y + 1);
                    break;

                case 2:
                    user2.MoveTo(user.X + 1, user.Y);
                    break;


                case 3:
                    user2.MoveTo(user2.X - 1, user2.Y - 1);
                    break;

                case 4:
                    user2.MoveTo(user.X, user.Y + 1);
                    break;
                    

                case 5:
                    user2.MoveTo(user2.X + 1, user2.Y - 1);
                    break;
                    

                case 6:
                    user2.MoveTo(user.X - 1, user.Y);
                    break;

                case 7:
                    user2.MoveTo(user2.X + 1, user2.Y + 1);
                    break;
            }
            user.Chat(session, $"Eu puxei {user2.GetUserName()}!", true, -1);

            return true;
        }
    }
}