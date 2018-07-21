using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class InviteAll : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public InviteAll()
        {
            MinRank = 1;
            Description = "Chame todos seus amigos para sua sala!";
            Usage = ":inviteall [msg]";
            MinParams = 1;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var msg = pms[0];
            if (msg.Length <= 0 || msg.Length >= 255)
            {
                client.SendWhisper("Tamanho inválido, o tamanho máximo é 255 caracteres!");
                return false;
            }

            client?.GetHabbo()?.GetMessenger()?.SendInstantInviteForAll(msg);

            return true;
        }
    }
}