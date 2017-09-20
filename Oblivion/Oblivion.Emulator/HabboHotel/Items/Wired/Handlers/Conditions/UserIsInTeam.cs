using System.Collections.Generic;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class UserIsInTeam : IWiredItem
    {
        public UserIsInTeam(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            OtherString = "1";
        }

        public Interaction Type => Interaction.ConditionUserIsInTeam;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return ""; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            var roomUser = stuff?[0] as RoomUser;
            if (roomUser == null)
                return false;

            if (!int.TryParse(OtherString, out int team))
            {
                return false;
            }
            return (Team) team == roomUser.Team;
        }
    }
}