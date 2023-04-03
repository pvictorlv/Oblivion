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
          Session?.GetHabbo()?.GetInventoryComponent()?.SerializeFloorItemInventory(Session);
          
        }
    }
}