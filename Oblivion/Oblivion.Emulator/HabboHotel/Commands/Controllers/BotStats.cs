using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.RoomBots;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    /// Comando para visualizar estatísticas dos bots inteligentes
    /// </summary>
    internal sealed class BotStats : Command
    {
        public BotStats()
        {
            MinRank = 5; // Apenas para moderadores
            Description = "Mostra estatísticas dos bots inteligentes no quarto";
            Usage = ":botstats";
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

            // Encontrar bots no quarto
            var bots = room.GetRoomUserManager().GetRoomUsers()
                .Where(u => u.IsBot && u.BotData != null)
                .ToList();

            if (!bots.Any())
            {
                session.SendWhisper("Não há bots neste quarto!");
                return true;
            }

            session.SendWhisper($"📊 Estatísticas dos Bots ({bots.Count} encontrados):");

            foreach (var bot in bots)
            {
                var botAi = bot.BotAi as GenericBot;
                if (botAi == null) continue;

                var message = $"🤖 {bot.BotData.Name}:\n" +
                             $"• Tipo: {(bot.BotData.IsBartender ? "Bartender" : "Genérico")}\n" +
                             $"• Chat Automático: {(bot.BotData.AutomaticChat ? "Sim" : "Não")}\n" +
                             $"• Modo de Caminhada: {bot.BotData.WalkingMode}\n" +
                             $"• Frases: {bot.BotData.RandomSpeech?.Count ?? 0}";

                session.SendWhisper(message);
            }

            return true;
        }
    }
}