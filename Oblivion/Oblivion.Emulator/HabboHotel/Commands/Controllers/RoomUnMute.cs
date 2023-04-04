using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RoomUnMute. This class cannot be inherited.
    /// </summary>
    internal sealed class RoomUnMute : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomUnMute" /> class.
        /// </summary>
        public RoomUnMute()
        {
            MinRank = 5;
            Description = "UnMutes the whole room.";
            Usage = ":roomunmute";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (!session.GetHabbo().CurrentRoom.RoomMuted)
            {
                 await session.SendWhisperAsync("Room isn't muted.");
                return true;
            }

            session.GetHabbo().CurrentRoom.RoomMuted = false;

            /*
            var message = new ServerMessage();
            message.Init(LibraryParser.OutgoingRequest("AlertNotificationMessageComposer"));
            message.AppendString("Room is now UnMuted.");
            message.AppendString("");
            await room.SendMessage(message);*/

            
            await room.SendMessage(GameClient.GetBytesNotif("Este quarto foi des-desmutado."));

            await Oblivion.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, string.Empty,
                    "Room Unmute", "Room UnMuted");
            return true;
        }
    }
}