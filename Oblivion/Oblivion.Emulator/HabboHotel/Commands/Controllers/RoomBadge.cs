using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RoomBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class RoomBadge : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomBadge" /> class.
        /// </summary>
        public RoomBadge()
        {
            MinRank = 7;
            Description = "Gives just the whole room a badge.";
            Usage = ":roombadge [badgeCode]";
            MinParams = 1;
        }

        public override Task<bool> Execute(GameClient session, string[] pms)
        {
            if (pms[0].Length < 2) return Task.FromResult(true);
            var room = session.GetHabbo().CurrentRoom;
            /* TODO CHECK */ foreach (var current in room.GetRoomUserManager().UserList.Values)
            {
                try
                {
                    if (!current.IsBot && current.GetClient() != null &&
                        current.GetClient().GetHabbo() != null)
                    {
                        current.GetClient()
                            .GetHabbo()
                            .GetBadgeComponent()
                            .GiveBadge(pms[0], true, current.GetClient());
                    }
                }
                catch
                {
                }
            }
            Oblivion.GetGame().GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName,
                    string.Empty, "Badge",
                    string.Concat("Roombadge in room [", room.RoomId, "] with badge [", pms[0], "]"));
            return Task.FromResult(true);
        }
    }
}