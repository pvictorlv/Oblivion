using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class DisableGroupChat. This class cannot be inherited.
    /// </summary>
    internal sealed class DisableGroupChat : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public DisableGroupChat()
        {
            MinRank = -1;
            Description = "Desative o chat do grupo";
            Usage = ":disablechat";
            MinParams = 0;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var room = client?.GetHabbo()?.CurrentRoom;
            if (room == null)
            {
                return false;
            }
            var gp = room.RoomData.Group;
            if (gp == null)
            {
                client.SendWhisper("Você deve estar no quarto do grupo para usar esse comando!");
                return false;
            }
            if (!gp.Admins.ContainsKey(client.GetHabbo().Id))
            {
                client.SendWhisper("Você não tem direitos para isso!");
                return false;
            }

            gp.HasChat = !gp.HasChat;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery(
                    $"UPDATE groups_data SET has_chat = '{Oblivion.BoolToEnum(gp.HasChat)}' WHERE id = '{gp.Id}'");
            }
            foreach (var userClient in gp.Members.Values.Select(user => Oblivion.GetGame().GetClientManager().GetClientByUserName(user.Name)).Where(userClient => userClient?.GetHabbo()?.GetMessenger() != null))
            {
                userClient.GetHabbo().GetMessenger().SerializeUpdate(gp, gp.HasChat);
            }
            client.SendWhisper(gp.HasChat ? "O chat foi ativado" : "o chat foi desativado");
            return true;
        }
    }
}