using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Enable. This class cannot be inherited.
    /// </summary>
    internal sealed class BlockCommand : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AddBlackWord" /> class.
        /// </summary>
        public BlockCommand()
        {
            MinRank = -2;
            Description = "Destaivar um comando na sala ou de um usuário";
            Usage = ":block room [cmd]";
            MinParams = 2;
            BlockBad = true;

        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var name = pms[0];
            if (name == "room")
            {
                var cmd = pms[1];

                if (!CommandsManager.CommandsDictionary.ContainsKey(cmd))
                {
                     await Session.SendWhisperAsync("Comando não encontrado!");
                    return false;
                }
                if (!session.GetHabbo().CurrentRoom.RoomData.BlockedCommands.Contains(cmd))
                {
                    session.GetHabbo().CurrentRoom.RoomData.BlockedCommands.Add(cmd);
                     await Session.SendWhisperAsync("Sucesso!");
                    return true;
                }
                 await Session.SendWhisperAsync("Comando já bloqueado");
                return false;
            }
            if (session.GetHabbo().Rank < 7)
            {
                 await Session.SendWhisperAsync("Comando incorreto, digite :block room [comando] para bloquear um comando.");
                return false;
            }
            var word = pms[1];

            if (!CommandsManager.CommandsDictionary.ContainsKey(word))
            {
                 await Session.SendWhisperAsync("Comando não encontrado!");
                return false;
            }
            var user = Oblivion.GetHabboForName(name);
            if (user == null)
            {
                 await Session.SendWhisperAsync("Usuário não encontrado!");
                return false;
            }
            if (user.Rank >= session.GetHabbo().Rank)
            {
                 await Session.SendWhisperAsync("Não bloqueie um superior!");
                return false;
            }
            if (user.Data.BlockedCommands.Contains(word))
            {
                 await Session.SendWhisperAsync("Comando já bloqueado!");
                return false;
            }
            user.Data.BlockedCommands.Add(word);
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO user_blockcmd (user_id, command_name) VALUES (@user, @command)");
                dbClient.AddParameter("user", user.Id);
                dbClient.AddParameter("command", word);
                await dbClient.RunQueryAsync();
            }
             await Session.SendWhisperAsync("Sucesso!");

            return true;
        }
    }
}