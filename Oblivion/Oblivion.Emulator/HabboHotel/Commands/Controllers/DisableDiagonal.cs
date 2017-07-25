﻿using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class DisableDiagonal. This class cannot be inherited.
    /// </summary>
    internal sealed class DisableDiagonal : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisableDiagonal" /> class.
        /// </summary>
        public DisableDiagonal()
        {
            MinRank = -2;
            Description = "Disable Diagonal walking.";
            Usage = ":disable_diagonal";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            room.GetGameMap().DiagonalEnabled = !room.GetGameMap().DiagonalEnabled;
            session.SendNotif(Oblivion.GetLanguage().GetVar("command_disable_diagonal"));

            return true;
        }
    }
}