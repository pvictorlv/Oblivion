using System;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.RoomBots
{
    /// <summary>
    ///     Class BotAI.
    /// </summary>
    internal abstract class BotAi : IDisposable
    {
        /// <summary>
        ///     The _room
        /// </summary>
        private Room _room;

        /// <summary>
        /// Set when bot is disposed
        /// </summary>
        protected bool Disposed;


        /// <summary>
        ///     The _room user
        /// </summary>
        private RoomUser _roomUser;



        /// <summary>
        ///     The base identifier
        /// </summary>
        internal uint BaseId;

        /// <summary>
        ///     Initializes the specified base identifier.
        /// </summary>
        /// <param name="baseId">The base identifier.</param>
        /// <param name="user">The user.</param>
        /// <param name="room">The room.</param>
        internal void Init(uint baseId, int roomUserId, uint roomId, RoomUser user, Room room)
        {
            BaseId = baseId;
            _roomUser = user;
            _room = room;
        }

        /// <summary>
        ///     Gets the room.
        /// </summary>
        /// <returns>Room.</returns>
        internal Room GetRoom()
        {
            if (_room == null)
            {
                StopTimerTick();
            }
            return _room;
        }

        /// <summary>
        ///     Gets the room user.
        /// </summary>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetRoomUser() => _roomUser;

        /// <summary>
        ///     Gets the bot data.
        /// </summary>
        /// <returns>RoomBot.</returns>
        internal RoomBot GetBotData() => GetRoomUser()?.BotData;

        /// <summary>
        ///     Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Disposed = true;
            GetBotData()?.Dispose();
            StopTimerTick();
            _room = null;
            _roomUser = null;
        }

        /// <summary>
        ///     Called when [self enter room].
        /// </summary>
        internal abstract void OnSelfEnterRoom();

        /// <summary>
        ///     Called when [self leave room].
        /// </summary>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal abstract void OnSelfLeaveRoom(bool kicked);

        /// <summary>
        ///     Called when [user enter room].
        /// </summary>
        /// <param name="user">The user.</param>
        internal abstract Task OnUserEnterRoom(RoomUser user);

        /// <summary>
        ///     Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal abstract void OnUserLeaveRoom(GameClient client);

        /// <summary>
        ///     Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="msg">The MSG.</param>
        internal abstract Task OnUserSay(RoomUser user, string msg);

        /// <summary>
        ///     Called when [user shout].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal abstract void OnUserShout(RoomUser user, string message);

        /// <summary>
        ///     Called when [timer tick].
        /// </summary>
        internal abstract Task OnTimerTick();

        internal abstract void OnChatTick();

        /// <summary>
        ///     Modifieds this instance.
        /// </summary>
        internal abstract void Modified();

        internal abstract void StopTimerTick();
    }
}