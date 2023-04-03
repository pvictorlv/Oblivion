using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class SetHeight : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public SetHeight()
        {
            MinRank = -2;
            Description = "Defina a altura do piso";
            Usage = ":setheight [v]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var currentRoom = client.GetHabbo().CurrentRoom;
            if (!int.TryParse(pms[0], out var height) || height >= 100)
            {
                client.SendWhisper("Insira um valor válido!");
                return true;
            }

            currentRoom.CustomHeight = height;
            return true;
        }
    }
}