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
        internal void GetInventory()
        {
          Session?.GetHabbo()?.GetInventoryComponent()?.SerializeFloorItemInventory(Session);
          
        }
    }
}