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
            var msg = Session.GetHabbo().GetInventoryComponent().SerializeFloorItemInventory();
            if (msg == null) return;
            var queuedServerMessage = new QueuedServerMessage(Session.GetConnection());
            queuedServerMessage.AppendResponse(msg);
            queuedServerMessage.SendResponse();
        }
    }
}