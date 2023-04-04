using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class SetMax. This class cannot be inherited.
    /// </summary>
    internal sealed class SetMax : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetMax" /> class.
        /// </summary>
        public SetMax()
        {
            MinRank = -1;
            Description = "Set max users in a room.";
            Usage = ":setmax [Number from 1 to 200. If 10 < num > 100 requires VIP]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (!room.CheckRights(session, true))
            {
                return false;
            }
            if (!ushort.TryParse(pms[0], out var maxUsers) || maxUsers == 0 || maxUsers > (150 * Oblivion.Multipy))
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("command_setmax_error_number"));
                return true;
            }

            if (maxUsers > 75 * Oblivion.Multipy && !(session.GetHabbo().Vip || session.GetHabbo().HasFuse("fuse_vip_commands")))
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("command_setmax_error_max"));
                return true;
            }
            if (maxUsers < 10 && !(session.GetHabbo().Vip || session.GetHabbo().HasFuse("fuse_vip_commands")))
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("command_setmax_error_min"));
                return true;
            }

            session.GetHabbo().CurrentRoom.SetMaxUsers(maxUsers);
            return true;
        }
    }
}