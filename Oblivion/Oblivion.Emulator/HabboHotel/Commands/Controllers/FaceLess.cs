using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FaceLess. This class cannot be inherited.
    /// </summary>
    internal sealed class FaceLess : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FaceLess" /> class.
        /// </summary>
        public FaceLess()
        {
            MinRank = 1;
            Description = "No Face.";
            Usage = ":faceless";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            if (!session.GetHabbo().Look.Contains("hd-"))
                return true;

            var head = session.GetHabbo().Look.Split('.').FirstOrDefault(element => element.StartsWith("hd-"));
            var color = "1";
            if (!string.IsNullOrEmpty(head))
            {
                color = head.Split('-')[2];
                session.GetHabbo().Look = session.GetHabbo().Look.Replace('.' + head, string.Empty);
            }
            session.GetHabbo().Look += ".hd-99999-" + color;

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE users SET look = @look WHERE id = " + session.GetHabbo().Id);
                dbClient.AddParameter("look", session.GetHabbo().Look);
                await dbClient.RunQueryAsync();
            }
            var room = session.GetHabbo().CurrentRoom;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;

            var roomUpdate = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            await roomUpdate.AppendIntegerAsync(user.VirtualId);
            await roomUpdate.AppendStringAsync(session.GetHabbo().Look);
            await roomUpdate.AppendStringAsync(session.GetHabbo().Gender.ToLower());
            await roomUpdate.AppendStringAsync(session.GetHabbo().Motto);
            await roomUpdate.AppendIntegerAsync(session.GetHabbo().AchievementPoints);
            await room.SendMessage(roomUpdate);

            return true;
        }
    }
}