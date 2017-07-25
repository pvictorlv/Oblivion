using System.Text;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class About : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public About()
        {
            MinRank = 1;
            Description = "Shows information about the server.";
            Usage = ":about";
            MinParams = 0;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var message =
                new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

            message.AppendString("Oblivion");
            message.AppendInteger(4);
            message.AppendString("title");
            message.AppendString("Oblivion Emulator v2");
            message.AppendString("message");
            var info = new StringBuilder();
            info.Append("<h5><b>Oblivion Emulator v2 - A new world!</b><h5></br></br>");
            info.Append("<br />");
             info.AppendFormat(
                "<b>Créditos:</b> <br />Dark, Claudio Santoro, Kessiler, Boris, <b>Lucca (Droppy)</b>, Antoine, IhToN<br /> <br /> ");
            info.AppendFormat("<b>Estatisticas:</b> <br />");
            var userCount = Oblivion.GetGame().GetClientManager().Clients.Count;
            var roomsCount = Oblivion.GetGame().GetRoomManager().LoadedRooms.Count;
            info.AppendFormat("<b>Usuários:</b> {0} em {1}{2}.<br /><br /><br />", userCount, roomsCount,
                (roomsCount == 1) ? " Quarto" : " Quartos");
            message.AppendString(info.ToString());
            message.AppendString("linkUrl");
            message.AppendString("event:");
            message.AppendString("linkTitle");
            message.AppendString("ok");
            client.SendMessage(message);

            return true;
        }
    }
}