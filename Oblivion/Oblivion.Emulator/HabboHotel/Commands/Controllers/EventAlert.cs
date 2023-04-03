using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class EventAlert : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventAlert" /> class.
        /// </summary>
        public EventAlert()
        {
            MinRank = 5;
            Description = "Alerts to all hotel a event.";
            Usage = ":eventha";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            await message.AppendStringAsync("events");
            await message.AppendIntegerAsync(4);
            await message.AppendStringAsync("title");
            await message.AppendStringAsync("Temos um novo Evento");
            await message.AppendStringAsync("message");
            await message.AppendStringAsync(
                "Tem um novo evento acontecendo agora mesmo!\n\nO evento está sendo feito por:    <b>" +
                session.GetHabbo().UserName +
                "</b>\n\nCorra para participar antes que o quarto seja fechado! Clique em " +
                "<i>Ir para o Evento</i>\n\nE o " +
                "evento vai ser:\n\n<b>" + string.Join(" ", pms) + "</b>\n\nEstamos esperando você lá em!<br><br><small>Digite :eventsoff para desativar o alerta!</small>");
            await message.AppendStringAsync("linkUrl");
            await message.AppendStringAsync("event:navigator/goto/" + session.GetHabbo().CurrentRoomId);
            await message.AppendStringAsync("linkTitle");
            await message.AppendStringAsync("Ir para o Evento");

            foreach (var client in Oblivion.GetGame().GetClientManager().Clients.Values)
            {
                if (client?.GetHabbo() == null)
                    continue;

                if (session.GetHabbo().Id == client.GetHabbo().Id)
                {
                    await client.SendWhisperAsync("O Alerta de Evento foi Enviado com Sucesso", true);
                    continue;
                }

                if (!client.GetHabbo().DisableEventAlert)
                {
                    await client.SendMessageAsync(message);
                    continue;
                }
                await client.SendWhisperAsync(
                    $"Um novo evento está acontecendo! Procure por {session.GetHabbo().CurrentRoom.RoomData.Owner} e venha ao evento!");
            }
            return true;
        }
    }
}