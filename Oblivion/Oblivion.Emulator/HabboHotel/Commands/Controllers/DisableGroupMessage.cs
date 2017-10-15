using System;
using System.Linq;
using System.Text;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Groups.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class DisableGroupMessage. This class cannot be inherited.
    /// </summary>
    internal sealed class DisableGroupMessage : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisableGroupMessage" /> class.
        /// </summary>
        public DisableGroupMessage()
        {
            MinRank = 1;
            Description = "Pare de receber mensagens do grupo!";
            Usage = ":disablemessage";
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
            if (!gp.Members.TryGetValue(client.GetHabbo().Id, out var member))
            {
                client.SendWhisper("Você não está no grupo!");
                return false;
            }

            member.HasChat = !member.HasChat;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery(
                    $"UPDATE groups_members SET has_chat = '{Oblivion.BoolToEnum(member.HasChat)}' WHERE user_id = '{member.Id}' AND group_id = '{member.GroupId}'");
            }
            if (!member.HasChat)
            {
                client.GetHabbo().GetMessenger().OnDisableChat((int) gp.Id);
            }
            else
            {
                client.GetHabbo().GetMessenger().SerializeUpdate(gp);
            }
            client.SendWhisper(member.HasChat ? "O chat foi ativado" : "o chat foi desativado");
            return true;
        }
    }
}