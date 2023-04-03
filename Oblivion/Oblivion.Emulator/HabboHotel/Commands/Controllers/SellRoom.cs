using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class SellRoom : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public SellRoom()
        {
            MinRank = -2;
            Description = "Venda a sua sala!";
            Usage = ":sellroom [PRICE]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var currentRoom = client.GetHabbo().CurrentRoom;
            var user = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
            if (user == null) return false;

            if (currentRoom.RoomData.OwnerId != user.HabboId)
            {
                client.SendWhisper("Você deve ser dono do quarto!");
                return false;
            }
            var actualInput = pms[0];
            var roomCostType = actualInput.Substring(actualInput.Length - 1);
            if (!int.TryParse(actualInput.Substring(0, actualInput.Length - 1), out var roomCost))
            {
                client.SendWhisper("Valor inválido");
                return false;
            }

            if (roomCost < 50)
            {
                currentRoom.RoomData.RoomForSale = false;
                client.SendWhisper("A sala não está mais a venda! (Valor mínimo: 50)");
                return true;
            }
            if (roomCost > 1000000)
            {
                client.SendWhisper("Valor muito grande, o máximo permitido é 1000000");
                return false;
            }
            if (actualInput.EndsWith("c") || actualInput.EndsWith("d"))
            {
                currentRoom.RoomData.RoomForSale = true;
                currentRoom.RoomData.RoomSaleCost = roomCost;
                currentRoom.RoomData.RoomSaleType = roomCostType;
            }
            else
            {
                client.SendWhisper("Insira um valor válido no final do preço! (c para créditos e d para diamantes)");
                return false;
            }
            /* TODO CHECK */ foreach (var userInRoom in currentRoom.GetRoomUserManager().GetRoomUsers())
                userInRoom?.GetClient()?
                    .SendWhisper($"A sala está a venda pelo preço de {roomCost}{roomCostType}");

            return true;
        }
    }
}