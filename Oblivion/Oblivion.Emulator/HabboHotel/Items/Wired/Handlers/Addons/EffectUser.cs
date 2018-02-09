using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Addons
{
    public class EffectUser : IWiredItem
    {
        public EffectUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new ConcurrentList<RoomItem>();
        }

        public void Dispose()
        {

        }

        public bool Disposed { get; set; }
        public Interaction Type => Interaction.ActionEffectUser;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];
            if (roomUser == null) return false;

            if (!int.TryParse(OtherString, out var effectId)) return false;

            var session = roomUser.GetClient();
            if (effectId == 178 && session.GetHabbo().Rank < 4) return true;
            if ((effectId == 23 || effectId == 24 || effectId == 25 || effectId == 26 || effectId == 102) && !session.GetHabbo().HasFuse("fuse_mod")) return true;
            if (effectId == 140 && !(session.GetHabbo().Vip || session.GetHabbo().HasFuse("fuse_vip_commands")))
                return true;

            if (roomUser != null && !string.IsNullOrEmpty(OtherString))
                roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);
            return true;
        }
    }
}