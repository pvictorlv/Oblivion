
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

//            StackTrace stackTrace = new StackTrace();

            // Get calling method name
//            Out.WriteLine(stackTrace.GetFrame(1).GetMethod().Name);
            var msg = Session.GetHabbo().GetInventoryComponent().SerializeFloorItemInventory();
            if (msg == null) return;
            var queuedServerMessage = new QueuedServerMessage(Session.GetConnection());
            queuedServerMessage.AppendResponse(msg);
            queuedServerMessage.SendResponse();
        }
    }
}