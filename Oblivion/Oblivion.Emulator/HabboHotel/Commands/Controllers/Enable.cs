using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Enable. This class cannot be inherited.
    /// </summary>
    internal sealed class Enable : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Enable" /> class.
        /// </summary>
        public Enable()
        {
            MinRank = 1;
            Description = "Enable/disable effect";
            Usage = ":enable";
            MinParams = 1;
        }

        public override Task<bool> Execute(GameClient session, string[] pms)
        {
            if (session?.GetHabbo() == null) return Task.FromResult(true);
            var user =
                session.GetHabbo()
                    .CurrentRoom.GetRoomUserManager()
                    .GetRoomUserByVirtualId(session.CurrentRoomUserId);

            if (user == null) return Task.FromResult(true);
            if (user.RidingHorse) return Task.FromResult(true);
            if (user.IsLyingDown) return Task.FromResult(true);

            if (!ushort.TryParse(pms[0], out var effect)) return Task.FromResult(true);
            if (effect == 178 && session.GetHabbo().Rank < 4) return Task.FromResult(true);
            if ((effect == 23 || effect == 24 || effect == 25 || effect == 26 || effect == 102) && !session.GetHabbo().HasFuse("fuse_mod")) return Task.FromResult(true);
            if (effect == 140 && !(session.GetHabbo().Vip || session.GetHabbo().HasFuse("fuse_vip_commands")))
                return Task.FromResult(true);

            session.GetHabbo()
                .GetAvatarEffectsInventoryComponent()
                .ActivateCustomEffect(effect);

            return Task.FromResult(true);
        }
    }
}