using System;

namespace Oblivion.HabboHotel.Items.Handlers
{
    /// <summary>
    ///     Class HopperHandler.
    /// </summary>
    internal static class HopperHandler
    {
        /// <summary>
        ///     Gets a hopper.
        /// </summary>
        /// <param name="curRoom">The current room.</param>
        /// <returns>System.UInt32.</returns>
        internal static uint GetAHopper(uint curRoom)
        {
            uint result;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"SELECT room_id FROM items_hopper WHERE room_id <> '{curRoom}' ORDER BY RAND() LIMIT 1");

                var num = Convert.ToUInt32(queryReactor.GetInteger());
                result = num;
            }

            return result;
        }

        /// <summary>
        ///     Gets the hopper identifier.
        /// </summary>
        /// <param name="nextRoom">The next room.</param>
        /// <returns>System.UInt32.</returns>
        internal static string GetHopperId(uint nextRoom)
        {
            string result;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT hopper_id FROM items_hopper WHERE room_id = @room LIMIT 1");
                queryReactor.AddParameter("room", nextRoom);

                var theString = queryReactor.GetString();

                result = theString ?? "0u";
            }

            return result;
        }
    }
}