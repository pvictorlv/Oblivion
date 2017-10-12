using System.Collections.Generic;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class JoinTeam : IWiredItem
    {
        public JoinTeam(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Delay = 0;
        }

        public Interaction Type => Interaction.ActionJoinTeam;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get { return new List<RoomItem>(); }
            set { }
        }

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
            if (!int.TryParse(OtherString, out var team))
            {
                return false;
            }

            var t = roomUser.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();

            if (roomUser.Team != Team.None)
            {
                t.OnUserLeave(roomUser);
                roomUser.Team = Team.None;
            }

            roomUser.Team = (Team) team;
            t.AddUser(roomUser);
            roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(team + 39);

            return true;
        }
    }
}