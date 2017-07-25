﻿using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class RoomAlert : Command
    {
        public RoomAlert()
        {
            MinRank = 5;
            Description = "Alerts the Room.";
            Usage = ":roomalert [MESSAGE]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var alert = string.Join(" ", pms);

            /*foreach (
                var user in
                    session.GetHabbo()
                        .CurrentRoom.GetRoomUserManager()
                        .GetRoomUsers()
                        .Where(user => !user.IsBot && user.GetClient() != null))
                user.GetClient().SendNotif(alert);*/

            session.GetHabbo().CurrentRoom.SendMessage(GameClient.GetBytesNotif(alert));

            return true;
        }
    }
}