using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class DisableSex : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public DisableSex()
        {
            MinRank = 1;
            Description = "Pare de receber pedidos de sexo.";
            Usage = ":dsex";
            MinParams = 0;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            if (client.GetHabbo().AllowCustomCommands)
            {
                client.SendWhisper("Você não recebe mais pedidos de sexo!");
                client.GetHabbo().AllowCustomCommands = false;
            }
            else
            {
                client.SendWhisper("Você recebe pedidos de sexo!");
                client.GetHabbo().AllowCustomCommands = true;
            }

            return true;
        }
    }
}