using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class WhisperRoom : Command
    {
        public WhisperRoom()
        {
            MinRank = 6;
            Description = "Susurrar para o Quarto Todo";
            Usage = ":whisperroom [MESSAGE]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var message = string.Join(" ", pms);
            /* TODO CHECK */ foreach (var client in Oblivion.GetGame().GetClientManager().Clients.Values)
            {
                var serverMessage = new ServerMessage();
                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                await serverMessage.AppendIntegerAsync(room.RoomId);
                await serverMessage.AppendStringAsync(message);
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendIntegerAsync(36);
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendIntegerAsync(-1);
                await client.SendMessage(serverMessage);
            }
            return true;
        }
    }
}