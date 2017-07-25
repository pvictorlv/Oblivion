using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Kick. This class cannot be inherited.
    /// </summary>
    internal sealed class Kick : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Kick" /> class.
        /// </summary>
        public Kick()
        {
            MinRank = 5;
            Description = "Kick a selected user from room.";
            Usage = ":kick [USERNAME] [MESSAGE]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var userName = pms[0];
            var userSession = Oblivion.GetGame().GetClientManager().GetClientByUserName(userName);
            if (userSession == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (session.GetHabbo().Rank <= userSession.GetHabbo().Rank)
            {
                session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            if (userSession.GetHabbo().CurrentRoomId < 1)
            {
                session.SendNotif(Oblivion.GetLanguage().GetVar("command_kick_user_not_in_room"));
                return true;
            }
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(userSession.GetHabbo().CurrentRoomId);
            if (room == null) return true;

            room.GetRoomUserManager().RemoveUserFromRoom(userSession, true, false);
            userSession.CurrentRoomUserId = -1;
            if (pms.Length > 1)
            {
                userSession.SendNotif(
                    string.Format(Oblivion.GetLanguage().GetVar("command_kick_user_mod_default") + "{0}.",
                        string.Join(" ", pms.Skip(1))));
            }
            else userSession.SendNotif(Oblivion.GetLanguage().GetVar("command_kick_user_mod_default"));

            return true;
        }
    }
}