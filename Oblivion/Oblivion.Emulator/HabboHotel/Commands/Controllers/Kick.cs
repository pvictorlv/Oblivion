using System.Linq;
using System.Threading.Tasks;
using Oblivion.Connection;
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
            MinRank = -2;
            Description = "Kick a selected user from room.";
            Usage = ":kick [USERNAME] [MESSAGE]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var userName = pms[0];
            var usersession = Oblivion.GetGame().GetClientManager().GetClientByUserName(userName);
            if (usersession == null)
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (session.GetHabbo().Rank <= usersession.GetHabbo().Rank)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            if (usersession.GetHabbo().CurrentRoomId < 1)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("command_kick_user_not_in_room"));
                return true;
            }
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(usersession.GetHabbo().CurrentRoomId);
            if (room == null) return true;

            await room.GetRoomUserManager().RemoveUserFromRoom(usersession, true, false);
            usersession.CurrentRoomUserId = -1;
            if (pms.Length > 1)
            {
                await usersession.SendNotif(
                    string.Format(Oblivion.GetLanguage().GetVar("command_kick_user_mod_default") + "{0}.",
                        string.Join(" ", pms.Skip(1))));
            }
            else await usersession.SendNotif(Oblivion.GetLanguage().GetVar("command_kick_user_mod_default"));

            return true;
        }
    }
}