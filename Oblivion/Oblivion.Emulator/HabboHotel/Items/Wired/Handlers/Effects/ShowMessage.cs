using System.Collections.Generic;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class ShowMessage : IWiredItem
    {
        private List<Interaction> _mBanned;

        public ShowMessage(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater
            };
        }

        public void Dispose()
        {
            _mBanned.Clear();
            _mBanned = null;
        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ActionShowMessage;

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

        public async Task<bool> Execute(params object[] stuff)
        {
            

            if (stuff[0] == null)
                return false;

            var roomUser = (RoomUser) stuff[0];
            var item = (Interaction) stuff[1];

            if (_mBanned.Contains(item))
                return false;

            if (OtherString.ToUpper().Contains("%USERNAME%"))
                OtherString = OtherString.Replace("%USERNAME%", roomUser.GetUserName());

            if (OtherString.ToUpper().Contains("%ROOMNAME%"))
                OtherString = OtherString.Replace("%ROOMNAME%", roomUser.GetRoom().RoomData.Name);

            if (OtherString.ToUpper().Contains("%USERCOUNT%"))
                OtherString = OtherString.Replace("%USERCOUNT%", roomUser.GetRoom().UserCount.ToString());

            if (OtherString.ToUpper().Contains("%USERSONLINE%"))
                OtherString = OtherString.Replace("%USERSONLINE%",
                    Oblivion.GetGame().GetClientManager().ClientCount().ToString());


            if (roomUser?.GetClient() != null && !string.IsNullOrEmpty(OtherString))
                await roomUser.GetClient().SendWhisperAsync(OtherString, true);

            return true;
        }
    }
}