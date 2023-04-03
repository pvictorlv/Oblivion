using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Dance. This class cannot be inherited.
    /// </summary>
    internal sealed class Dance : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Dance" /> class.
        /// </summary>
        public Dance()
        {
            MinRank = 1;
            Description = "Makes you dance.";
            Usage = ":dance [danceId(0 - 4)]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            ushort.TryParse(pms[0], out var result);

            if (result > 4)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("command_dance_false"));
                result = 0;
            }
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            var message = new ServerMessage();
            message.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
            message.AppendInteger(session.CurrentRoomUserId);
            message.AppendInteger(result);
            session.GetHabbo().CurrentRoom.SendMessage(message);
            user.DanceId = result;

            return true;
        }
    }
}