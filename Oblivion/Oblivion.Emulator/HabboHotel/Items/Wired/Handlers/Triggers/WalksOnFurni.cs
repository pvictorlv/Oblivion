using System.Drawing;
using System.Threading.Tasks;
using System.Linq;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class WalksOnFurni : IWiredItem
    {
        public WalksOnFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public Interaction Type => Interaction.TriggerWalkOnFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString
        {
            get { return ""; }
            set { }
        }

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


        public void Dispose()
        {
        }

        public bool Disposed { get; set; }


        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null || roomUser.IsBot || roomUser.IsPet)
                return false;
            var roomItem = (RoomItem) stuff[1];
            if (roomItem == null)
                return false;

            if (!Items.Contains(roomItem)) return false;


            var userPosition = roomUser.X;
            var lastUserPosition = roomUser.CopyX;

            if (roomUser.LastItem != 0 && roomUser.LastItem == roomItem.Id &&
                userPosition == lastUserPosition)
                return false;

            if (Room.GetGameMap().HasHeightestItem(new Point(roomItem.X, roomItem.Y), roomItem.Z))
            {
                return false;
            }
            

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Count > 0)
                /* TODO CHECK */
                foreach (var current in conditions)
                {
                    if (!current.Execute(roomUser, roomItem))
                        return false;

                    WiredHandler.OnEvent(current);
                }

            if (effects.Count > 0)
            {
                var specials = Room.GetWiredHandler().GetSpecials(this);
                if (specials.Count > 0)
                {
                    var specialBox = specials[0];
                    if (specialBox != null)
                    {
                        var selectedBox = specialBox.Type == Interaction.SpecialRandom
                            ? Room.GetWiredHandler().GetRandomEffect(effects)
                            : Room.GetWiredHandler().GetRandomUnseenEffect(effects);

                        if (selectedBox == null || !selectedBox.Execute())
                            return false;

                        WiredHandler.OnEvent(specialBox);
                        WiredHandler.OnEvent(selectedBox);
                    }
                }
                else
                {
                    foreach (var current3 in effects)
                    {
                        if (current3 == null) continue;
                        current3.Execute(roomUser, Type);
                        WiredHandler.OnEvent(current3);
                    }
                }
            }

            WiredHandler.OnEvent(this);


            return true;
        }
    }
}