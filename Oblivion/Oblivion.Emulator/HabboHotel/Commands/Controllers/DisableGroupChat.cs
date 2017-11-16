﻿using Oblivion.HabboHotel.Commands.Interfaces;
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


            /* TODO CHECK */ foreach (var member in gp.Members.Keys)
            {
                var user = Oblivion.GetGame().GetClientManager().GetClientByUserId(member);
                if (user?.GetHabbo() == null) continue;
                if (!gp.HasChat)
                    user.GetHabbo().GetMessenger().OnDisableChat((int) gp.Id);
                else
                    user.GetHabbo().GetMessenger().SerializeUpdate(gp);
            }
            client.SendWhisper(gp.HasChat ? "O chat foi ativado" : "o chat foi desativado");

            return true;
        }
    }
}