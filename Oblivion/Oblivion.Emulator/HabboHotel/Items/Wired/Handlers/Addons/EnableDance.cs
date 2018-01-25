using System.Collections.Generic;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Addons
{
    public class EnableDance : IWiredItem
    {
        public EnableDance(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }
        public void Dispose()
        {

        }

        public bool Disposed { get; set; }

        public Interaction Type => Interaction.ActionEnableDance;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];

            if (roomUser?.GetClient() == null) return false;

            if (int.TryParse(OtherString, out var danceId))
            {
                if (danceId > 4)
                    danceId = 4;

                var message = new ServerMessage();
                message.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                message.AppendInteger(roomUser.GetClient().CurrentRoomUserId);
                message.AppendInteger(danceId);
                roomUser.GetClient().GetHabbo().CurrentRoom.SendMessage(message);
                roomUser.DanceId = danceId;

                return true;
            }

            return false;
        }
    }
}