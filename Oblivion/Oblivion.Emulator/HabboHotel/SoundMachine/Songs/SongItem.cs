using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.SoundMachine.Songs
{
    /// <summary>
    ///     Class SongItem.
    /// </summary>
    internal class SongItem
    {
        /// <summary>
        ///     The base item
        /// </summary>
        internal Item BaseItem;

        /// <summary>
        ///     The extra data
        /// </summary>
        internal string ExtraData;

        /// <summary>
        ///     The item identifier
        /// </summary>
        internal uint ItemId;

        
        /// <summary>
        ///     The song code
        /// </summary>
        internal string SongCode;

        /// <summary>
        ///     The song identifier
        /// </summary>
        internal uint SongId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SongItem" /> class.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="songId">The song identifier.</param>
        /// <param name="baseItem">The base item.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="songCode">The song code.</param>
        public SongItem(uint itemId, uint songId, int baseItem, string extraData, string songCode)
        {
            ItemId = itemId;
            SongId = songId;

            BaseItem = Oblivion.GetGame().GetItemManager().GetItem(((uint) baseItem));

            ExtraData = extraData;
            SongCode = songCode;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SongItem" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public SongItem(UserItem item)
        {
            ItemId = item.VirtualId;
            SongId = SoundMachineSongManager.GetSongId(item.SongCode);
            BaseItem = item.BaseItem;
            ExtraData = item.ExtraData;
            SongCode = item.SongCode;
        }

        /// <summary>
        ///     Saves to database.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        internal async Task SaveToDatabase(uint roomId)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var id = Oblivion.GetGame().GetItemManager().GetRealId(ItemId);
                queryReactor.RunFastQuery($"REPLACE INTO items_songs VALUES ('{id}', '{roomId}', '{SongId}')");
            }
        }

        /// <summary>
        ///     Removes from database.
        /// </summary>
        internal async Task RemoveFromDatabase()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var id = Oblivion.GetGame().GetItemManager().GetRealId(ItemId);

                queryReactor.RunFastQuery($"DELETE FROM items_songs WHERE itemid = '{id}'");
            }
        }
    }
}