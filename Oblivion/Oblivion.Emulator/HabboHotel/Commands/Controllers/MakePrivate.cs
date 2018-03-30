using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class MakePrivate : Command
    {
        public MakePrivate()
        {
            MinRank = 7;
            Description = "Haz una sala privada.";
            Usage = ":makeprivate";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery($"UPDATE rooms_data SET roomtype = 'private' WHERE id = {room.RoomId}");
            var roomId = session.GetHabbo().CurrentRoom.RoomId;
            var users = new List<RoomUser>(session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values);

            Oblivion.GetGame().GetRoomManager().UnloadRoom(session.GetHabbo().CurrentRoom, "");

            Oblivion.GetGame().GetRoomManager().LoadRoom(roomId);

            var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            roomFwd.AppendInteger(roomId);

            var data = roomFwd.GetReversedBytes();

            /* TODO CHECK */ foreach (var user in users.Where(user => user?.GetClient() != null))
                user.GetClient().SendMessage(data);

            return true;
        }
    }
}