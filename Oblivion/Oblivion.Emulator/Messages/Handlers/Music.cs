using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.SoundMachine;
using Oblivion.HabboHotel.SoundMachine.Composers;
using Oblivion.HabboHotel.SoundMachine.Songs;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Retrieves the song identifier.
        /// </summary>
        internal async Task RetrieveSongId()
        {
            string text = Request.GetString();

            uint songId = SoundMachineSongManager.GetSongId(text);

            if (songId != 0u)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RetrieveSongIDMessageComposer"));
                await serverMessage.AppendStringAsync(text);
                await serverMessage.AppendIntegerAsync(songId);
                await Session.SendMessageAsync(serverMessage);
            }
        }

        /// <summary>
        /// Gets the music data.
        /// </summary>
        internal async Task GetMusicData()
        {
            int num = Request.GetInteger();

            var list = new List<SongData>();

            for (int i = 0; i < num; i++)
            {
                SongData song = SoundMachineSongManager.GetSong(Request.GetUInteger());

                if (song != null)
                    list.Add(song);
            }

            await Session.SendMessageAsync(SoundMachineComposer.Compose(list));

            list.Clear();
        }

        /// <summary>
        /// Adds the playlist item.
        /// </summary>
        internal async Task AddPlaylistItem()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            Room currentRoom = Session.GetHabbo().CurrentRoom;

            if (!currentRoom.CheckRights(Session, true))
                return;

            SoundMachineManager roomMusicController = currentRoom.GetRoomMusicController();

            if (roomMusicController.PlaylistSize >= roomMusicController.PlaylistCapacity)
                return;

            var num = Oblivion.GetGame().GetItemManager().GetRealId(Request.GetUInteger());

            UserItem item = Session.GetHabbo().GetInventoryComponent().GetItem(num);

            if (item == null || item.BaseItem.InteractionType != Interaction.MusicDisc)
                return;

            var songItem = new SongItem(item);

            int num2 = roomMusicController.AddDisk(songItem);

            if (num2 < 0)
                return;

            await songItem.SaveToDatabase(currentRoom.RoomId);

            await Session.GetHabbo().GetInventoryComponent().RemoveItem(num, true, currentRoom.RoomId);

            using (IQueryAdapter queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                await queryReactor.RunFastQueryAsync($"UPDATE items_rooms SET user_id=NULL WHERE id='{num}' LIMIT 1");

            await Session.SendMessageAsync(SoundMachineComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }

        /// <summary>
        /// Removes the playlist item.
        /// </summary>
        internal async Task RemovePlaylistItem()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            Room currentRoom = Session.GetHabbo().CurrentRoom;

            if (!currentRoom.GotMusicController())
                return;

            SoundMachineManager roomMusicController = currentRoom.GetRoomMusicController();

            SongItem songItem = roomMusicController.RemoveDisk(Request.GetInteger());

            if (songItem == null)
                return;

            await songItem.RemoveFromDatabase();

            var guid = Guid.NewGuid();
            ShortGuid itemId = guid;
            await Session.GetHabbo().GetInventoryComponent().AddNewItem(itemId, songItem.BaseItem.ItemId, songItem.ExtraData, 0u, false, true, 0, 0, songItem.SongCode);
            await Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            using (IQueryAdapter queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                await queryReactor.RunFastQueryAsync($"UPDATE items_rooms SET user_id='{Session.GetHabbo().Id}' WHERE id='{songItem.ItemId}' LIMIT 1;");

            await Session.SendMessageAsync(SoundMachineComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().GetDisks()));
            await Session.SendMessageAsync(SoundMachineComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }

        /// <summary>
        /// Gets the disks.
        /// </summary>
        internal async Task GetDisks()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            var disks = Session.GetHabbo().GetInventoryComponent().GetDisks();
            if (disks.Count == 0)
                return;

            await Session.SendMessageAsync(SoundMachineComposer.SerializeSongInventory(disks));
        }

        /// <summary>
        /// Gets the Play lists.
        /// </summary>
        internal async Task GetPlaylists()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            Room currentRoom = Session.GetHabbo().CurrentRoom;

            if (!currentRoom.GotMusicController())
                return;

            SoundMachineManager roomMusicController = currentRoom.GetRoomMusicController();
            await Session.SendMessageAsync(SoundMachineComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }
    }
}