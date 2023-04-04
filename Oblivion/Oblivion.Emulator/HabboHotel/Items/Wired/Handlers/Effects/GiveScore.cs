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
    public class GiveScore : IWiredItem
    {
        public GiveScore(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = "10,1";
            OtherExtraString = "0";
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionGiveScore;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Task<bool> Execute(params object[] stuff)
        {
            

            if (stuff[0] == null)
                return Task.FromResult(false);

            if ((Interaction) stuff[1] == Interaction.TriggerScoreAchieved)
                return Task.FromResult(false);


            var roomUser = (RoomUser) stuff[0];

            if (roomUser == null)
                return Task.FromResult(false);

            if (roomUser.Team == Team.None)
                return Task.FromResult(false);

            int timesDone;
            int.TryParse(OtherExtraString, out timesDone);

            var scoreToAchieve = 10;
            var maxTimes = 1;

            if (!string.IsNullOrWhiteSpace(OtherString))
            {
                var integers = OtherString.Split(',');
                scoreToAchieve = int.Parse(integers[0]);
                maxTimes = int.Parse(integers[1]);
            }

            if (timesDone >= maxTimes)
                return Task.FromResult(false);

            Room.GetGameManager().AddPointToTeam(roomUser.Team, scoreToAchieve, roomUser);
            timesDone++;

            OtherExtraString = timesDone.ToString();
            return Task.FromResult(true);
        }
    }
}