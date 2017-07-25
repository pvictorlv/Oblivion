using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class PickAll. This class cannot be inherited.
    /// </summary>
    internal sealed class PickAll : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PickAll" /> class.
        /// </summary>
        public PickAll()
        {
            MinRank = -2;
            Description = "Picks up all the items in your room.";
            Usage = ":pickall";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var roomItemList = room.GetRoomItemHandler().RemoveAllFurniture(session);
            if (session.GetHabbo().GetInventoryComponent() == null)
            {
                return true;
            }
            // session.GetHabbo().GetInventoryComponent().AddItemArray(roomItemList);
            //session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            room.GetRoomItemHandler().RemoveItemsByOwner(ref roomItemList, ref session);
            return true;
        }
    }
}