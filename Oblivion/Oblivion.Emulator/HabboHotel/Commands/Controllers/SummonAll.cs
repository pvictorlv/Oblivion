using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class SummonAll. This class cannot be inherited.
    /// </summary>
    internal sealed class SummonAll : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SummonAll" /> class.
        /// </summary>
        public SummonAll()
        {
            MinRank = 7;
            Description = "Summon all users online to the room you are in.";
            Usage = ":summonall [reason]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var reason = string.Join(" ", pms);

            var messageBytes =
                GameClient.GetBytesNotif(string.Format("Você foi puxado por \r- {0}:\r\n{1}",
                    session.GetHabbo().UserName, reason));
            /* TODO CHECK */ foreach (var client in Oblivion.GetGame().GetClientManager().Clients.Values)
            {
                if (session.GetHabbo().CurrentRoom == null ||
                    session.GetHabbo().CurrentRoomId == client.GetHabbo().CurrentRoomId)
                    continue;

                await client.GetMessageHandler()
                    .PrepareRoomForUser(session.GetHabbo().CurrentRoom.RoomId,
                        session.GetHabbo().CurrentRoom.RoomData.PassWord);
                await client.SendMessage(messageBytes);
            }
            return true;
        }
    }
}