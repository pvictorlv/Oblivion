using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassDance. This class cannot be inherited.
    /// </summary>
    internal sealed class MassDance : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassDance" /> class.
        /// </summary>
        public MassDance()
        {
            MinRank = 7;
            Description = "Enable dance Id for the whole room.";
            Usage = ":massdance [danceId(0 - 4)]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            ushort danceId;
            ushort.TryParse(pms[0], out danceId);

            if (danceId > 4)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("command_dance_wrong_syntax"));
                return true;
            }
            var room = session.GetHabbo().CurrentRoom;
            var roomUsers = room.GetRoomUserManager().GetRoomUsers();

            /* TODO CHECK */ foreach (var roomUser in roomUsers)
            {

                var message =
                    new ServerMessage(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                await message.AppendIntegerAsync(roomUser.VirtualId);
                await message.AppendIntegerAsync(danceId);
                await room.SendMessage(message);
                roomUser.DanceId = danceId;
            }
            return true;
        }
    }
}