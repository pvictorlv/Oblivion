using System.Threading.Tasks;
using System.Web;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class SendBubble : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public SendBubble()
        {
            MinRank = 1;
            Description = "Enviar alertas de balão";
            Usage = ":bubble [message]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var room = client?.GetHabbo()?.CurrentRoom;
            if (room == null)
            {
                return true;
            }

            var message = pms[0];
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("RoomNotificationMessageComposer"));
            message = System.Net.WebUtility.HtmlDecode(message);

            await msg.AppendStringAsync("micro");
            await msg.AppendIntegerAsync(5);
            await msg.AppendStringAsync("title");
            await msg.AppendStringAsync("Hotel");
            await msg.AppendStringAsync("message");
            await msg.AppendStringAsync(message);
            await msg.AppendStringAsync("linkUrl");
            await msg.AppendStringAsync("event:navigator/goto/" + client.GetHabbo().CurrentRoomId);
            await msg.AppendStringAsync("linkTitle");
            await msg.AppendStringAsync("");
            await msg.AppendStringAsync("display");
            await msg.AppendStringAsync("BUBBLE");

            
            if (client.GetHabbo().Rank < 6)
            {
                if (room.CheckRights(client, true))
                    await room.SendMessage(msg);
            }
            else
            {
                await Oblivion.GetGame().GetClientManager().SendMessageAsync(msg);
            }

            return true;
        }
    }
}