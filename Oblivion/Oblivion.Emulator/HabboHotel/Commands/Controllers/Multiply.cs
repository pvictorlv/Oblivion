using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class Multiply : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public Multiply()
        {
            MinRank = 8;
            Description = "...";
            Usage = ":multipy [value]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var numb = pms[0];
            if (!int.TryParse(numb, out int value))
            {
                await client.SendWhisperAsync("Valor inválido");
                return false;
            }
            Oblivion.Multipy = value;
            return true;
        }
    }
}