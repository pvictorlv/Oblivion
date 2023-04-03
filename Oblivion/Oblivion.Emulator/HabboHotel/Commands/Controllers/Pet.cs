using System;
using System.Text;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class Pet : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public Pet()
        {
            MinRank = 1;
            Description = "Vire um pet!";
            Usage = ":pet [id]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            if (pms.Length < 1) return false;

            var room = client.GetHabbo().CurrentRoom;
            var habbo = client.GetHabbo();
            if (room == null) return true;

            var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);

            if (pms[0] == "habbo")
            {
                var messageRemove = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer"));
                await messageRemove.AppendStringAsync(roomUser.VirtualId.ToString());
                await room.SendMessage(messageRemove);

                var messageNormal = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
                await messageNormal.AppendIntegerAsync(1);
                roomUser.Serialize(messageNormal);
                await room.SendMessage(messageNormal);
                return true;
            }
            if (!int.TryParse(pms[0], out var petId))
            {
                await client.SendWhisperAsync("Digite um número de 0 a 47, caso queira voltar ao normal digite :pet habbo");
                return false;
            }
            if (petId == 13 || petId > 47 || petId < 0) return false;
            var random = new Random();
            var color = $"{random.Next(0x1000000):X6}";
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer"));
            await message.AppendStringAsync(roomUser.VirtualId.ToString());
            await room.SendMessage(message);

            var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
            await serverMessage.AppendIntegerAsync(1);
            await serverMessage.AppendIntegerAsync(habbo.Id);
            await serverMessage.AppendStringAsync(habbo.UserName);
            await serverMessage.AppendStringAsync("");
            await serverMessage.AppendStringAsync($"{petId} 0 {color} 2 2 -1 0 3 -1 0");
            await serverMessage.AppendIntegerAsync(roomUser.VirtualId);
            await serverMessage.AppendIntegerAsync(roomUser.X);
            await serverMessage.AppendIntegerAsync(roomUser.Y);
            await serverMessage.AppendStringAsync(TextHandling.GetString(roomUser.Z));
            await serverMessage.AppendIntegerAsync(0);
            await serverMessage.AppendIntegerAsync(2);
            await serverMessage.AppendIntegerAsync(petId);
            await serverMessage.AppendIntegerAsync(habbo.Id);
            await serverMessage.AppendStringAsync(habbo.UserName);
            await serverMessage.AppendIntegerAsync(1);
            serverMessage.AppendBool(false);
            serverMessage.AppendBool(false);
            await serverMessage.AppendIntegerAsync(0);
            await serverMessage.AppendIntegerAsync(0);
            await serverMessage.AppendStringAsync("");
            await room.SendMessage(serverMessage);
            return true;
        }
    }
}