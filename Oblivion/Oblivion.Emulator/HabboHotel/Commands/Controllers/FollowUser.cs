﻿using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FollowUser. This class cannot be inherited.
    /// </summary>
    internal sealed class FollowUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FollowUser" /> class.
        /// </summary>
        public FollowUser()
        {
            MinRank = 1;
            Description = "Follow the selected user.";
            Usage = ":follow [USERNAME]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null || client.GetHabbo() == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().CurrentRoom == null ||
                client.GetHabbo().CurrentRoom == session.GetHabbo().CurrentRoom)
                return false;
            var roomFwd =
                new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            roomFwd.AppendInteger(client.GetHabbo().CurrentRoom.RoomId);
            session.SendMessage(roomFwd);

            return true;
        }
    }
}