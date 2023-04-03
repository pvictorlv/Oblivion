﻿using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Sit. This class cannot be inherited.
    /// </summary>
    internal sealed class Friends : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Sit" /> class.
        /// </summary>
        public Friends()
        {
            MinRank = 1;
            Description = "Enables/disables friends petitions";
            Usage = ":friends";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().HasFriendRequestsDisabled = !session.GetHabbo().HasFriendRequestsDisabled;
            return true;
        }
    }
}