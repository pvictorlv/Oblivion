using System.Threading.Tasks;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class ToggleFurniState : IWiredItem
    {
        private long _mNext;

        public ToggleFurniState(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
            Delay = 0;
            _mNext = 0L;
        }

        public void Dispose()
        {
        }

        public bool Disposed { get; set; }

    
        public bool Requested { get; set; }

        public Interaction Type => Interaction.ActionToggleState;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return string.Empty; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public int Delay { get;
            set; }

        public async Task<bool> Execute(params object[] stuff)
        {
            if (Item == null || Items.Count <= 0)
                return false;

            var num = Oblivion.Now();

            if (_mNext > num)
            {
                await Task.Delay((int)(_mNext - num));
            }
            foreach (var current in Items.Where(
                current => current != null && Room.GetRoomItemHandler().FloorItems.Values.Contains(current)))
                current.Interactor.OnWiredTrigger(current);

            _mNext = Oblivion.Now() + Delay;

            return true;
        }
    }
}