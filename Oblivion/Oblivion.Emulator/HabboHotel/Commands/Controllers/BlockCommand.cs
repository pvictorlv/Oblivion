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
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var type = pms[0];
            if (type == "room")
            {
                var cmd = pms[1];

                if (!CommandsManager.CommandsDictionary.ContainsKey(cmd))
                {
                    session.SendWhisper("Comando não encontrado!");
                    return false;
                }
                if (!session.GetHabbo().CurrentRoom.RoomData.BlockedCommands.Contains(cmd))
                {
                    session.GetHabbo().CurrentRoom.RoomData.BlockedCommands.Add(cmd);
                    session.SendWhisper("Sucesso!");
                    return true;
                }
                session.SendWhisper("Comando já bloqueado");
                return false;
            }
            if (session.GetHabbo().Rank < 7)
            {
                session.SendWhisper("Comando incorreto, digite :block room [comando] para bloquear um comando.");
                return false;
            }
            var name = pms[1];
            var word = pms[2];

            if (!CommandsManager.CommandsDictionary.ContainsKey(word))
            {
                session.SendWhisper("Comando não encontrado!");
                return false;
            }
            var user = Oblivion.GetHabboForName(name);
            if (user == null)
            {
                session.SendWhisper("Usuário não encontrado!");
                return false;
            }
            if (user.Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper("Não bloqueie um superior!");
                return false;
            }
            if (user.Data.BlockedCommands.Contains(word))
            {
                session.SendWhisper("Comando já bloqueado!");
                return false;
            }
            user.Data.BlockedCommands.Add(word);
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO user_blockcmd (user_id, command_name) VALUES (@user, @command)");
                dbClient.AddParameter("user", user.Id);
                dbClient.AddParameter("command", word);
                dbClient.RunQuery();
            }
            session.SendWhisper("Sucesso!");

            return true;
        }
    }
}