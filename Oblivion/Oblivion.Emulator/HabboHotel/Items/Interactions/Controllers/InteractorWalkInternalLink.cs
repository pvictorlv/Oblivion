using System;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorWalkInternalLink : FurniInteractorModel
    {
        public override void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (item == null || user == null)
                return;

            var data = item.ExtraData.Split(Convert.ToChar(9));

            if (item.ExtraData == "" || data.Length < 4)
                return;

            var message = new ServerMessage(LibraryParser.OutgoingRequest("InternalLinkMessageComposer"));

            message.AppendString(data[3]);
            session.SendMessage(message);
        }
    }
}