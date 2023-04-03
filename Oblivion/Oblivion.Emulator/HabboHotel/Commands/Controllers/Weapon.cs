using System.Text;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class Weapon : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public Weapon()
        {
            MinRank = 1;
            Description = "Saque uma arma!";
            Usage = ":weapon [id]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var id = pms[0].ToLower();
            var room = client?.GetHabbo()?.CurrentRoom;
            if (room == null) return false;
            var user =
                room.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);

            switch (id)
            {
                case "uzi":
                    user.ApplyEffect(580);
                    break;
                case "shotgun":
                    user.ApplyEffect(581);
                    break;
                case "rifle":
                    user.ApplyEffect(582);
                    break;
                case "ak47":
                    user.ApplyEffect(583);
                    break;
                case "g36":
                    user.ApplyEffect(584);
                    break;
                case "glock":
                    user.ApplyEffect(585);
                    break;
                default:
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("Armas Disponíveis\r");
                    stringBuilder.Append("----------------------------\r");
                    stringBuilder.Append("Uzi\r");
                    stringBuilder.Append("Shotgun\r");
                    stringBuilder.Append("Rifle\r");
                    stringBuilder.Append("Ak47\r");
                    stringBuilder.Append("G36\r");
                    stringBuilder.Append("Glock\r");
                    stringBuilder.Append("\r\rComo usar? digite :weapon + arma");

                    await client.SendNotifWithScroll(stringBuilder.ToString());
                    return true;
            }

            user.Chat(client, "Saquei a minha arma, corram!", true, 0);

            return true;
        }
    }
}