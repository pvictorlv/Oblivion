using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Sit. This class cannot be inherited.
    /// </summary>
    internal sealed class DisableEvent : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Sit" /> class.
        /// </summary>
        public DisableEvent()
        {
            MinRank = 1;
            Description = "Desativa as mensagens de Eventos do Hotel";
            Usage = ":disableevent";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().DisableEventAlert = !session.GetHabbo().DisableEventAlert;
             await Session.SendWhisperAsync("Sucesso!");
            return true;
        }
    }
}