using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Unload. This class cannot be inherited.
    /// </summary>
    internal sealed class Unload : Command
    {
        /// <summary>
        ///     The _re enter
        /// </summary>
        private readonly bool _reEnter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Unload" /> class.
        /// </summary>
        /// <param name="reEnter">if set to <c>true</c> [re enter].</param>
        public Unload(bool reEnter = false)
        {
            MinRank = -1;
            Description = "Unloads the current room!";
            Usage = reEnter ? ":reload" : ":unload";
            MinParams = 0;
            _reEnter = reEnter;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var roomId = session.GetHabbo().CurrentRoom.RoomId;
            var users = new List<RoomUser>(session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values);

            await Oblivion.GetGame().GetRoomManager().UnloadRoom(session.GetHabbo().CurrentRoom, "");

            if (!_reEnter)
                return true;
            await Oblivion.GetGame().GetRoomManager().LoadRoom(roomId);

            var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            await roomFwd.AppendIntegerAsync(roomId);


            /* TODO CHECK */ foreach (var user in users.Where(user => user?.GetClient() != null))
                await user.GetClient().SendMessageAsync(roomFwd);
            return true;
        }
    }
}