﻿using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Override. This class cannot be inherited.
    /// </summary>
    internal sealed class Override : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Override" /> class.
        /// </summary>
        public Override()
        {
            MinRank = -2;
            Description = "Makes you can transpase items.";
            Usage = ":override";
            MinParams = 0;
        }

        public override Task<bool> Execute(GameClient session, string[] pms)
        {

            var currentRoom = session.GetHabbo().CurrentRoom;

            if (!currentRoom.CheckRights(session, false, true)) return Task.FromResult(false);

            var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUserByHabbo == null) return Task.FromResult(true);
            roomUserByHabbo.AllowOverride = !roomUserByHabbo.AllowOverride;

            return Task.FromResult(true);
        }
    }
}