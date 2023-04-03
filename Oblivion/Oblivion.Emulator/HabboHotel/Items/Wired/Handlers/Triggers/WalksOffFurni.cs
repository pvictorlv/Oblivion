using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class WalksOffFurni : IWiredItem
    {

        public WalksOffFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }


        public Interaction Type => Interaction.TriggerWalkOffFurni;

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

        public async Task<bool> Execute(params object[] stuff)
        {
            if (stuff.Length < 2) return false;

            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null || roomUser.IsBot || roomUser.IsPet)
                return false;

            var roomItem = (RoomItem) stuff[1];
            if (roomItem == null) return false;


            if (!Items.Contains(roomItem)) return false;

            if (roomUser.LastItem != roomItem.Id)
                return false;

            if (roomItem.AffectedTiles.Values.Any(
                current => current.X == roomUser.X && current.Y == roomUser.Y ||
                           roomUser.X == roomItem.X && roomUser.Y == roomItem.Y))
                return false;

        

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Count > 0)
                /* TODO CHECK */
                foreach (var current in conditions)
                {
                    if (!current.Execute(roomUser, roomItem))
                        return false;

                    await WiredHandler.OnEvent(current);
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

                        await WiredHandler.OnEvent(specialBox);
                        await WiredHandler.OnEvent(selectedBox);
                    }
                }
                else
                {
                    foreach (var current3 in effects)
                    {
                        current3.Execute(roomUser, Type);
                        await WiredHandler.OnEvent(current3);
                    }
                }
            }

            await WiredHandler.OnEvent(this);



            return true;
        }
    }
}