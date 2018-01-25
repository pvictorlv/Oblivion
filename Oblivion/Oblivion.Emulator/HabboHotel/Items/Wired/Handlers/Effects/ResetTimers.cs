using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class ResetTimers : IWiredItem
    {
        public ResetTimers(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionResetTimer;

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }


        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }


        private long _mNext;


        public int Delay { get; set; }


        public async Task<bool> Execute(params object[] stuff)
        {
            var num = Oblivion.Now();

            if (_mNext > num)
                await Task.Delay((int)(_mNext - num));

            Room.GetWiredHandler().ExecuteWired(Interaction.TriggerTimer);

            _mNext = Oblivion.Now() + Delay;


            return true;

        }
    }
}