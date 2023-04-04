using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HandItem. This class cannot be inherited.
    /// </summary>
    internal sealed class HandItem : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandItem" /> class.
        /// </summary>
        public HandItem()
        {
            MinRank = 1;
            Description = "Lets you pick a hand item, e.g. A drink";
            Usage = ":handitem [itemId]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            if (!ushort.TryParse(pms[0], out var itemId)) return true;

            var user = session.GetHabbo()
                .CurrentRoom.GetRoomUserManager()
                .GetRoomUserByVirtualId(session.CurrentRoomUserId);
            if (user.RidingHorse)
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("horse_handitem_error"));
                return true;
            }
            if (user.IsLyingDown)
                return true;

            user.CarryItem(itemId);
            return true;
        }
    }
}