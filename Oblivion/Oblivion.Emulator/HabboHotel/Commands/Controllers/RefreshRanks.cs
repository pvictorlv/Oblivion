﻿using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshRanks. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshRanks : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshRanks" /> class.
        /// </summary>
        public RefreshRanks()
        {
            MinRank = 9;
            Description = "Refreshes Ranks from Database.";
            Usage = ":refresh_ranks";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
                Oblivion.GetGame().GetRoleManager().LoadRights(adapter);
            CommandsManager.UpdateInfo();
            await session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_ranks"));
            return true;
        }
    }
}