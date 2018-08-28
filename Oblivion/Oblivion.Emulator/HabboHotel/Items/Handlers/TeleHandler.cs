using System;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Items.Handlers
{
    /// <summary>
    ///     Class TeleHandler.
    /// </summary>
    internal static class TeleHandler
    {
        /// <summary>
        ///     Gets the linked tele.
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <returns>System.UInt32.</returns>
        internal static string GetLinkedTele(string teleId, Room pRoom)
        {
            string result;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT tele_two_id FROM items_teleports WHERE tele_one_id = {teleId}");
                queryReactor.RunQuery();
                var row = queryReactor.GetRow();

                result = row == null ? "0" : Convert.ToString(row[0]);
            }

            return result;
        }

        /// <summary>
        ///     Gets the tele room identifier.
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns>System.UInt32.</returns>
        internal static uint GetTeleRoomId(string teleId, Room pRoom)
        {
            if (pRoom.GetRoomItemHandler().GetItem(teleId) != null)
                return pRoom.RoomId;

            uint result;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT room_id FROM items_rooms WHERE id = {teleId} LIMIT 1");
                queryReactor.RunQuery();
                var row = queryReactor.GetRow();

                result = row == null ? 0 : Convert.ToUInt32(row[0]);
            }

            return result;
        }

        /// <summary>
        ///     Determines whether [is tele linked] [the specified tele identifier].
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns><c>true</c> if [is tele linked] [the specified tele identifier]; otherwise, <c>false</c>.</returns>
        internal static bool IsTeleLinked(string teleId, Room pRoom, RoomItem item)
        {
            if (item.TeleporterId == "0") return false;
            return GetTeleRoomId(item.TeleporterId, pRoom) != 0u;
        }
    }
}