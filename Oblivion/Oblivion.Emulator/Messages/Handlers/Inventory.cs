﻿namespace Oblivion.Messages.Handlers
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
            var queuedServerMessage = new QueuedServerMessage(Session.GetConnection());
          
            queuedServerMessage.AppendResponse(Session.GetHabbo().GetInventoryComponent().SerializeFloorItemInventory());
            queuedServerMessage.SendResponse();
        }
    }
}