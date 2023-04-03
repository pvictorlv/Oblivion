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
    internal sealed class HotelAlert : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HotelAlert" /> class.
        /// </summary>
        public HotelAlert()
        {
            MinRank = 5;
            Description = "Alerts the whole Hotel.";
            Usage = ":ha [MESSAGE]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {

            var str = string.Join(" ", pms);
           
            var message = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
            message.AppendString($"{str}\r\n- {session.GetHabbo().UserName}");
     
           Oblivion.GetGame().GetClientManager().SendMessageAsync(message);
            

             await Session.SendWhisperAsync("Enviado!");
            return true;
        }
    }
}