using System.Collections.Generic;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Addons
{
    public class FreezeUser : IWiredItem
    {
        public FreezeUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ActionFreezeUser;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            if (stuff[0] == null)
                return false;

            var roomUser = (RoomUser) stuff[0];

            roomUser.Frozen = !roomUser.Frozen;
            if (roomUser.Frozen)
            {
                roomUser.ApplyEffect(12);
                roomUser.GetClient().SendWhisper("Você foi congelado");
            }
            else
            {
                roomUser.GetClient().SendWhisper("Você foi descongelado");
            }
            return true;
        }
    }
}