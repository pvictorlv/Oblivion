using System.Threading.Tasks;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Gets the inventory.
        /// </summary>
        internal async Task GetInventory()
        {
            var comp = Session.GetHabbo().GetInventoryComponent();
            if (comp != null)
                await comp.SerializeFloorItemInventory(Session);
        }
    }
}