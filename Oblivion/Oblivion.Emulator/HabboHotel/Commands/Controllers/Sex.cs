﻿using System;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class Sex : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public Sex()
        {
            MinRank = 1;
            Description = "Have sex with someone";
            Usage = ":sex [user]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var room = client?.GetHabbo()?.CurrentRoom;
            if (room == null) return true;

            var fuckedName = pms[0];
            var fuckedUser = room.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
            var fuckerUser = room.GetRoomUserManager().GetRoomUserByHabbo(fuckedName);
            if (fuckerUser == null)
            {
                await client.SendWhisperAsync("Usuário não encontrado!");
                return true;
            }

            if (client.GetHabbo().LastCustomCommand + 30 >= Oblivion.GetUnixTimeStamp())
            {
                await client.SendWhisperAsync("Espere um pouco!");
                return true;
            }

            if (!fuckerUser.GetClient().GetHabbo().AllowCustomCommands)
            {
                await client.SendWhisperAsync("O usuário não aceita esse tipo de brincadeira!");
                return true;
            }

            fuckedUser.InteractingUser = fuckerUser.UserId;

            if (fuckerUser.InteractingUser != fuckedUser.UserId)
            {
                await fuckerUser.GetClient().SendWhisperAsync($"O usuário {fuckedUser.GetUserName()} deseja fazer sexo com você!");
                return true;
            }


            if (!(Math.Abs(fuckerUser.X - fuckedUser.X) >= 2) || (Math.Abs(fuckerUser.Y - fuckedUser.Y) >= 2))
            {
                await Task.Run(async () =>
                {
                    await fuckerUser.Chat(client, "*Virando " + fuckedUser.GetUserName() + " pra começar a fazer sexo*", false,0);
                    await Task.Delay(1000);
                    await fuckedUser.Chat(client, "Gostei da ideia, vamos fazer um sexo gostoso!", false, 0);
                    await Task.Delay(1000);
                    await fuckerUser.ApplyEffect(507);
                    await fuckedUser.ApplyEffect(104);
                    await fuckerUser.Chat(client, "*Agarra, beija e chupa " + fuckedUser.GetUserName() + " deliciosamente*", false, 0);
                    await Task.Delay(2000);
                    await fuckedUser.Chat(client, "*Ai, ai, ai, to quase lá, vai com força, vaai*", false, 0);
                    await Task.Delay(1500);
                    await fuckedUser.Chat(client, "Isso, mais forte, mais forte, AAAAAAAH *GOZEI*", false, 0);
                    await Task.Delay(1000);
                    await fuckerUser.Chat(client, "*Ai que dlç, gozei, vamos outra?!*", false, 0);
                });
                await fuckerUser.ApplyEffect(0);
                await fuckedUser.ApplyEffect(0);
            }
            else
            {
                await client.SendWhisperAsync("Chegue mais perto da pessoa ou aguarde mais tempo para fazer novamente.");
            }

            fuckedUser.InteractingUser = 0;
            fuckerUser.InteractingUser = 0;

            fuckerUser.GetClient().GetHabbo().LastCustomCommand = Oblivion.GetUnixTimeStamp();
            client.GetHabbo().LastCustomCommand = Oblivion.GetUnixTimeStamp();

            return true;
        }
    }
}