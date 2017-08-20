using System;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.PathFinding;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Kill. This class cannot be inherited.
    /// </summary>
    internal sealed class Kill : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Kill" /> class.
        /// </summary>
        public Kill()
        {
            MinRank = 1;
            Description = "Kill someone.";
            Usage = ":kill";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);
            if (room == null) return true;

            var user2 = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().LastSelectedUser);
            if (user2 == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            var user =
                room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().UserName);
            if (PathFinder.GetDistance(user.X, user.Y, user2.X, user2.Y) > 1)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("kil_command_error_1"));

                return true;
            }
            if (user2.IsLyingDown || user2.IsSitting)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("kil_command_error_2"));
                return true;
            }
            if (
                !string.Equals(user2.GetUserName(), session.GetHabbo().UserName,
                    StringComparison.CurrentCultureIgnoreCase))
            {
                user2.Statusses.Add("lay", "0.55");
                user2.IsLyingDown = true;
                user2.UpdateNeeded = true;
                user.Chat(user.GetClient(), $"Eu te matei {user2.GetUserName()}!", true, 0, 3);
                user2.Chat(user2.GetClient(), "Estou morto :(", true, 0,
                    3);
                return true;
            }
            return true;
        }
    }
}