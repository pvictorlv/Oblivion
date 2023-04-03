using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class PickPets. This class cannot be inherited.
    /// </summary>
    internal sealed class PickPets : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PickPets" /> class.
        /// </summary>
        public PickPets()
        {
            MinRank = -1;
            Description = "Picks up all the pets in your room.";
            Usage = ":pickpets";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
           foreach (
                var pet in
                    room.GetRoomUserManager().GetPets().Where(pet => pet.OwnerId == session.GetHabbo().Id))
            {
                session.GetHabbo().GetInventoryComponent().AddPet(pet);
                room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
            }
            await session.SendMessage(session.GetHabbo().GetInventoryComponent().SerializePetInventory());
            return true;
        }
    }
}