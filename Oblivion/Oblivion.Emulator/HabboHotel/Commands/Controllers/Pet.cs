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
                messageRemove.AppendString(roomUser.VirtualId.ToString());
                await room.SendMessage(messageRemove);

                var messageNormal = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
                messageNormal.AppendInteger(1);
                roomUser.Serialize(messageNormal);
                await room.SendMessage(messageNormal);
                return true;
            }
            if (!int.TryParse(pms[0], out var petId))
            {
                client.SendWhisper("Digite um número de 0 a 47, caso queira voltar ao normal digite :pet habbo");
                return false;
            }
            if (petId == 13 || petId > 47 || petId < 0) return false;
            var random = new Random();
            var color = $"{random.Next(0x1000000):X6}";
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer"));
            message.AppendString(roomUser.VirtualId.ToString());
            await room.SendMessage(message);

            var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(habbo.Id);
            serverMessage.AppendString(habbo.UserName);
            serverMessage.AppendString("");
            serverMessage.AppendString($"{petId} 0 {color} 2 2 -1 0 3 -1 0");
            serverMessage.AppendInteger(roomUser.VirtualId);
            serverMessage.AppendInteger(roomUser.X);
            serverMessage.AppendInteger(roomUser.Y);
            serverMessage.AppendString(TextHandling.GetString(roomUser.Z));
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(2);
            serverMessage.AppendInteger(petId);
            serverMessage.AppendInteger(habbo.Id);
            serverMessage.AppendString(habbo.UserName);
            serverMessage.AppendInteger(1);
            serverMessage.AppendBool(false);
            serverMessage.AppendBool(false);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(0);
            serverMessage.AppendString("");
            await room.SendMessage(serverMessage);
            return true;
        }
    }
}