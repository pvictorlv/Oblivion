﻿using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms.RoomInvokedItems;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RoomKickUsers. This class cannot be inherited.
    /// </summary>
    internal sealed class RoomKickUsers : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomKickUsers" /> class.
        /// </summary>
        public RoomKickUsers()
        {
            MinRank = 5;
            Description = "Mutes the whole room.";
            Usage = ":roomkick [reason]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            var alert = string.Join(" ", pms);
            var kick = new RoomKick(alert, (int)session.GetHabbo().Rank);
            Oblivion.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, string.Empty,
                    "Room kick", "Kicked the whole room");
            room.QueueRoomKick(kick);

            return true;
        }
    }
}