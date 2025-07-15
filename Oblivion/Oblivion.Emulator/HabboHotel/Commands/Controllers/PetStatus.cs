using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Pets.AI;
using Oblivion.HabboHotel.RoomBots;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    /// Comando para visualizar o status inteligente dos pets
    /// </summary>
    internal sealed class PetStatus : Command
    {
        public PetStatus()
        {
            MinRank = 1;
            Description = "Mostra o status inteligente do seu pet";
            Usage = ":petstatus";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                session.SendWhisper("Você precisa estar em um quarto!");
                return true;
            }

            // Encontrar pets do usuário no quarto
            var userPets = room.GetRoomUserManager().GetRoomUsers()
                .Where(u => u.IsPet && u.PetData != null && u.PetData.OwnerId == session.GetHabbo().Id)
                .ToList();

            if (!userPets.Any())
            {
                session.SendWhisper("Você não tem pets neste quarto!");
                return true;
            }

            foreach (var pet in userPets)
            {
                var botAi = pet.BotAi as PetBot;
                if (botAi == null) continue;

                // Aqui você precisaria acessar o sistema de IA do pet
                // Por enquanto, vamos mostrar informações básicas
                var message = $"🐾 {pet.PetData.Name}:\n" +
                             $"• Energia: {pet.PetData.Energy}/100\n" +
                             $"• Nutrição: {pet.PetData.Nutrition}/100\n" +
                             $"• Experiência: {pet.PetData.Experience}\n" +
                             $"• Nível: {pet.PetData.Level}\n" +
                             $"• Respeito: {pet.PetData.Respect}";

                session.SendWhisper(message);
            }

            return true;
        }
    }
}