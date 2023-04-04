using System.Threading.Tasks;
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

        public override Task<bool> Execute(GameClient session, string[] pms)
        {
            Task.Factory.StartNew(async () =>
            {
                var room = session.GetHabbo().CurrentRoom;
                await room.GetRoomItemHandler().RemoveAllFurniture(session);
               
//                room.GetRoomItemHandler().RemoveItemsByOwner(ref roomItemList, ref session);
                return true;
            });

            return Task.FromResult(true);
        }
    }
}