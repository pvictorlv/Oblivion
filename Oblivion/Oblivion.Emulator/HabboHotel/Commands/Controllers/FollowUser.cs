using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FollowUser. This class cannot be inherited.
    /// </summary>
    internal sealed class FollowUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FollowUser" /> class.
        /// </summary>
        public FollowUser()
        {
            MinRank = 1;
            Description = "Follow the selected user.";
            Usage = ":follow [USERNAME]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client?.GetHabbo()?.CurrentRoom == null)
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
           
            if (client.GetHabbo().CurrentRoom == session.GetHabbo().CurrentRoom)
                return false;

            var roomFwd =
                new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            await roomFwd.AppendIntegerAsync(client.GetHabbo().CurrentRoom.RoomId);
            await session.SendMessage(roomFwd);

            return true;
        }
    }
}