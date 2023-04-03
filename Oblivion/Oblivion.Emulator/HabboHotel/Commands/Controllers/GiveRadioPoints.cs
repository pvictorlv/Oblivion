using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveRadioPoints : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public GiveRadioPoints()
        {
            MinRank = 5;
            Description = "Dê pontos a um usuário!";
            Usage = ":rpoints [user]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {

            var room = client.GetHabbo().CurrentRoom;
            if (room == null) return false;

            var target = pms[0];

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(target);

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery($"UPDATE users SET rpoints = rpoints + 1 WHERE id = '{targetUser.UserId}'");
            }

            client.GetHabbo().Diamonds += 5;
            client.GetHabbo().UpdateSeasonalCurrencyBalance();
            client.SendWhisper("Enviado com sucesso!");
            targetUser.GetClient().SendWhisper("Você ganhou um ponto de rádio!");
            return true;
        }
    }
}