using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class JoinTeam : IWiredItem
    {
        public JoinTeam(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Delay = 0;
            OtherString = "1";
        }

        public Interaction Type => Interaction.ActionJoinTeam;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }
        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public async Task<bool> Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser?.GetClient()?.GetHabbo()?.CurrentRoom == null) return false;
            var furni = (Interaction) stuff[1];

            if (furni == Interaction.TriggerRepeater || furni == Interaction.TriggerLongRepeater) return false;

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