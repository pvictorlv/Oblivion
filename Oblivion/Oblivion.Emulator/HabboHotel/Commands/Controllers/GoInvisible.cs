﻿using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassEnable. This class cannot be inherited.
    /// </summary>
    internal sealed class GoInvisible : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassEnable" /> class.
        /// </summary>
        public GoInvisible()
        {
            MinRank = 7;
            Description = "Invisible";
            Usage = ":invisible";
            MinParams = 0;
            BlockBad = true;

        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().SpectatorMode = true;
            await session.SendNotif("In next room you enter you will be invisible.");

            return true;
        }
    }
}