﻿using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Empty. This class cannot be inherited.
    /// </summary>
    internal sealed class Empty : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Empty" /> class.
        /// </summary>
        public Empty()
        {
            MinRank = 1;
            Description = "Empty's your Inventory.";
            Usage = ":empty";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            await session.GetHabbo().GetInventoryComponent().ClearItems();
            return true;
        }
    }
}