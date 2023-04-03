using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class WhisperHotel : Command
    {
        public WhisperHotel()
        {
            MinRank = 7;
            Description = "Susurrar a Todo el Hotel";
            Usage = ":whisperhotel [MESSAGE]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var message = string.Join(" ", pms);
            if (string.IsNullOrEmpty(message)) return true;
            /* TODO CHECK */ foreach (var client in Oblivion.GetGame().GetClientManager().Clients.Values)
            {
                var serverMessage = new ServerMessage();
                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                await serverMessage.AppendIntegerAsync(client.CurrentRoomUserId);
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