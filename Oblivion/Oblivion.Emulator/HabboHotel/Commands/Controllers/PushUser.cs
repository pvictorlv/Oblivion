using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.PathFinding;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class PushUser. This class cannot be inherited.
    /// </summary>
    internal sealed class PushUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PushUser" /> class.
        /// </summary>
        public PushUser()
        {
            MinRank = 1;
            Description = "Empurre o usuário na sua frente.";
            Usage = ":push";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.VirtualId);
            if (user == null) return true;

            if (room.RoomData.DisablePush)
            {
                session.SendWhisper("Realizar Push Foi Desativado pelo Dono do Quarto");
                return true;
            }


            var users = room.GetGameMap().GetRoomUsers(user.SquareInFront);
            if (users.Count <= 0)
            {
                return false;
                
            }
            var user2 = users[0];
            if (user2 == null) return true;
            if (user2.TeleportEnabled)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("command_error_teleport_enable"));
                return true;
            }

            if (PathFinder.GetDistance(user.X, user.Y, user2.X, user2.Y) > 2)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("command_pull_error_far_away"));
                return true;
            }

            switch (user.RotBody)
            {
                case 0:
                    user2.MoveTo(user2.X, user2.Y - 1);
                    break;

                case 1:
                    user2.MoveTo(user2.X + 1, user2.Y - 1);
                    break;

                case 2:
                    user2.MoveTo(user2.X + 1, user2.Y);
                    break;

                case 3:
                    user2.MoveTo(user2.X + 1, user2.Y + 1);
                    break;

                case 4:
                    user2.MoveTo(user2.X, user2.Y + 1);
                    break;

                case 5:
                    user2.MoveTo(user2.X - 1, user2.Y + 1);
                    break;

                case 6:
                    user2.MoveTo(user2.X - 1, user2.Y);
                    break;

                case 7:
                    user2.MoveTo(user2.X- 1, user2.Y - 1);
                    break;
            }
            user.Chat(session, $"Eu empurrei {user2.GetUserName()}!", true, -1);
            user2.UpdateNeeded = true;
            user2.SetRot(PathFinder.CalculateRotation(user2.X, user2.Y, user.GoalX, user.GoalY));
            return true;
        }
    }
}