using System;
using System.Collections.Generic;
using System.Linq;
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
        internal void RetrieveSongId()
        {
            string text = Request.GetString();

            uint songId = SoundMachineSongManager.GetSongId(text);

            if (songId != 0u)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RetrieveSongIDMessageComposer"));
                serverMessage.AppendString(text);
                serverMessage.AppendInteger(songId);
                Session.SendMessage(serverMessage);
            }
        }

        /// <summary>
        /// Gets the music data.
        /// </summary>
        internal void GetMusicData()
        {
            int num = Request.GetInteger();

            var list = new List<SongData>();

            for (int i = 0; i < num; i++)
            {
                SongData song = SoundMachineSongManager.GetSong(Request.GetUInteger());

                if (song != null)
                    list.Add(song);
            }

            Session.SendMessage(SoundMachineComposer.Compose(list));

            list.Clear();
        }

        /// <summary>
        /// Adds the playlist item.
        /// </summary>
        internal void AddPlaylistItem()
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

            songItem.SaveToDatabase(currentRoom.RoomId);

            Session.GetHabbo().GetInventoryComponent().RemoveItem(num, true, currentRoom.RoomId);

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery($"UPDATE items_rooms SET user_id='0' WHERE id='{num}' LIMIT 1");

            Session.SendMessage(SoundMachineComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }

        /// <summary>
        /// Removes the playlist item.
        /// </summary>
        internal void RemovePlaylistItem()
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

            songItem.RemoveFromDatabase();

            var guid = Guid.NewGuid();
            ShortGuid itemId = guid;
            Session.GetHabbo().GetInventoryComponent().AddNewItem(itemId, songItem.BaseItem.ItemId, songItem.ExtraData, 0u, false, true, 0, 0, songItem.SongCode);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            using (IQueryAdapter queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery($"UPDATE items_rooms SET user_id='{Session.GetHabbo().Id}' WHERE id='{songItem.ItemId}' LIMIT 1;");

            Session.SendMessage(SoundMachineComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().GetDisks()));
            Session.SendMessage(SoundMachineComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }

        /// <summary>
        /// Gets the disks.
        /// </summary>
        internal void GetDisks()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            var disks = Session.GetHabbo().GetInventoryComponent().GetDisks();
            if (disks.Count == 0)
                return;

            Session.SendMessage(SoundMachineComposer.SerializeSongInventory(disks));
        }

        /// <summary>
        /// Gets the Play lists.
        /// </summary>
        internal void GetPlaylists()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            Room currentRoom = Session.GetHabbo().CurrentRoom;

            if (!currentRoom.GotMusicController())
                return;

            SoundMachineManager roomMusicController = currentRoom.GetRoomMusicController();
            Session.SendMessage(SoundMachineComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }
    }
}