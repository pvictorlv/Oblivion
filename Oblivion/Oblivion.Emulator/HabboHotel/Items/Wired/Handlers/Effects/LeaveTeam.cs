using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class LeaveTeam : IWiredItem
    {
        public LeaveTeam(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionLeaveTeam;

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

        public Task<bool> Execute(params object[] stuff)
        {
            

            var roomUser = (RoomUser) stuff[0];
            if (roomUser?.GetClient()?.GetHabbo() == null) return false;
            var room = roomUser.GetRoom();
            var delay = Delay / 500;
            
            while (roomUser.Team != Team.None)
            {
                delay--;
                if (delay >= 0) continue;

                var t = room.GetTeamManagerForFreeze();
                var t2 = room.GetTeamManagerForBanzai();
                t.OnUserLeave(roomUser);
                t2.OnUserLeave(roomUser);
                roomUser.Team = Team.None;
            }
            /*if (roomUser.Team != Team.None)
            {
               
            }*/
            roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateEffect(0);


            return true;
        }
    }
}