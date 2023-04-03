using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Rooms.Items.Games.Handlers
{
    /// <summary>
    ///     Class GameItemHandler.
    /// </summary>
    internal class GameItemHandler
    {
        /// <summary>
        ///     The _banzai pyramids
        /// </summary>
        private ConcurrentDictionary<string, RoomItem> _banzaiPyramids;

        /// <summary>
        ///     The _banzai teleports
        /// </summary>
        private QueuedDictionary<string, RoomItem> _banzaiTeleports;

        /// <summary>
        ///     The _room
        /// </summary>
        private Room _room;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameItemHandler" /> class.
        /// </summary>
        /// <param name="room">The room.</param>
        public GameItemHandler(Room room)
        {
            _room = room;
            _banzaiPyramids = new ConcurrentDictionary<string, RoomItem>();
            _banzaiTeleports = new QueuedDictionary<string, RoomItem>();
        }

        /// <summary>
        ///     Called when [cycle].
        /// </summary>
        internal async void OnCycle()
        {
            try
            {
                await CyclePyramids();
                CycleRandomTeleports();
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Cycle - GameItemHandler");
            }
        }

        /// <summary>
        ///     Adds the pyramid.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="itemId">The item identifier.</param>
        internal async Task AddPyramid(RoomItem item, string itemId)
        {
            _banzaiPyramids[itemId] = item;
        }

        /// <summary>
        ///     Removes the pyramid.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        internal void RemovePyramid(string itemId)
        {
            _banzaiPyramids.TryRemove(itemId, out _);
        }

        /// <summary>
        ///     Adds the teleport.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="itemId">The item identifier.</param>
        internal void AddTeleport(RoomItem item, string itemId)
        {
            _banzaiTeleports.Inner[itemId] = item;
        }

        /// <summary>
        ///     Removes the teleport.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        internal void RemoveTeleport(string itemId)
        {
            _banzaiTeleports.Remove(itemId);
        }

        /// <summary>
        ///     Called when [teleport room user enter].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="item">The item.</param>
        internal async Task OnTeleportRoomUserEnter(RoomUser user, RoomItem item)
        {
            var items = _banzaiTeleports.Inner.Values.Where(p => p.Id != item.Id).ToList();

            if (items.Count <= 0)
                return;

            var countId = Oblivion.GetRandomNumber(0, items.Count);
            var countAmount = 0;

            /* TODO CHECK */
            foreach (var current in items)
            {
                if (current != null)
                {
                    if (countAmount != countId)
                    {
                        countAmount++;
                        continue;
                    }
                    current.ExtraData = "1";
                    current.UpdateNeeded = true;
                    await _room.GetGameMap().TeleportToItem(user, current);
                    item.ExtraData = "1";
                    item.UpdateNeeded = true;
                    await current.UpdateState();
                    await  item.UpdateState();

                    break;
                }
            }
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal async Task Destroy()
        {
            _banzaiTeleports?.Destroy();
            _banzaiPyramids?.Clear();
            _banzaiPyramids = null;
            _banzaiTeleports = null;
            _room = null;
        }

        /// <summary>
        ///     Cycles the pyramids.
        /// </summary>
        private async Task CyclePyramids()
        {
            /* TODO CHECK */
            foreach (var pyramid in _banzaiPyramids)
            {
                var item = pyramid.Value;
                if (item != null)
                {
                    if (item.InteractionCountHelper == 0 && item.ExtraData == "1")
                    {
                        await _room.GetGameMap().RemoveFromMap(item, false);
                        item.InteractionCountHelper = 1;
                    }

                    if (string.IsNullOrEmpty(item.ExtraData)) item.ExtraData = "0";

                    var randomNumber = Oblivion.GetRandomNumber(0, 30);
                    if (randomNumber <= 26) continue;
                    if (item.ExtraData == "0")
                    {
                        item.ExtraData = "1";
                        await item.UpdateState();
                        await _room.GetGameMap().RemoveFromMap(item, false);
                    }
                    else
                    {
                        if (!_room.GetGameMap().ItemCanBePlacedHere(item.X, item.Y)) continue;
                        item.ExtraData = "0";
                        await item.UpdateState();
                        _room.GetGameMap().AddItemToMap(item, false);
                    }
                }
            }
        }

        /// <summary>
        ///     Cycles the random teleports.
        /// </summary>
        private void CycleRandomTeleports()
        {
            _banzaiTeleports.OnCycle();
        }
    }
}