﻿using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GoBoom. This class cannot be inherited.
    /// </summary>
    internal sealed class GoBoom : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GoBoom" /> class.
        /// </summary>
        public GoBoom()
        {
            MinRank = 5;
            Description = "BOOMMMMM";
            Usage = ":goboom";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);
            room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            foreach (var user in room.GetRoomUserManager().GetRoomUsers()) user.ApplyEffect(108);
            return true;
        }
    }
}