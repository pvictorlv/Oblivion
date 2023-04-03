using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Rooms.Data
{
    /// <summary>
    ///     Class RoomEvents.
    /// </summary>
    internal class RoomEvents
    {
        /// <summary>
        ///     The _events
        /// </summary>
        private readonly Dictionary<uint, RoomEvent> _events;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomEvents" /> class.
        /// </summary>
        internal RoomEvents()
        {
            _events = new Dictionary<uint, RoomEvent>();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM rooms_events WHERE `expire` > UNIX_TIMESTAMP()");
                var table = queryReactor.GetTable();
                /* TODO CHECK */ foreach (DataRow dataRow in table.Rows)
                {
                    _events.Add((uint) dataRow[0],
                        new RoomEvent((uint) dataRow[0], dataRow[1].ToString(), dataRow[2].ToString(), (int) dataRow[3],
                            (int) dataRow[4]));
                }
            }
        }

        /// <summary>
        ///     Adds the new event.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="eventDesc">The event desc.</param>
        /// <param name="session">The session.</param>
        /// <param name="time">The time.</param>
        /// <param name="category">The category.</param>
        internal async Task AddNewEvent(uint roomId, string eventName, string eventDesc, GameClient session, int time = 7200,
            int category = 1)
        {
            {
                if (_events.ContainsKey(roomId))
                {
                    var roomEvent = _events[roomId];
                    roomEvent.Name = eventName;
                    roomEvent.Description = eventDesc;
                    if (roomEvent.HasExpired)
                    {
                        roomEvent.Time = Oblivion.GetUnixTimeStamp() + time;
                    }
                    else
                    {
                        roomEvent.Time += time;
                    }
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery(
                            "REPLACE INTO rooms_events VALUES ('@id','@name','@desc','@time','@category')");
                        queryReactor.AddParameter("id", roomId);
                        queryReactor.AddParameter("name", eventName);
                        queryReactor.AddParameter("desc", eventDesc);
                        queryReactor.AddParameter("time", roomEvent.Time);
                        queryReactor.AddParameter("category", category);
                        await queryReactor.RunQueryAsync();
                        goto IL_17C;
                    }
                }
                using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.SetQuery(string.Concat("REPLACE INTO rooms_events VALUES (", roomId,
                        ", @name, @desc, ", Oblivion.GetUnixTimeStamp() + 7200, ", @category)"));
                    queryreactor2.AddParameter("name", eventName);
                    queryreactor2.AddParameter("desc", eventDesc);
                    queryreactor2.AddParameter("category", category);
                    await queryreactor2.RunQueryAsync();
                }
                _events.Add(roomId, new RoomEvent(roomId, eventName, eventDesc));
                IL_17C:
                (await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId)).Event = _events[roomId];
                var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);
                if (room != null)
                {
                    room.RoomData.Event = _events[roomId];
                }
                if (session.GetHabbo().CurrentRoomId == roomId)
                {
                    await SerializeEventInfo(roomId);
                }
            }
        }

        /// <summary>
        ///     Removes the event.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        internal async Task RemoveEvent(uint roomId)
        {
            _events.Remove(roomId);
            await SerializeEventInfo(roomId);
        }

        /// <summary>
        ///     Gets the events.
        /// </summary>
        /// <returns>Dictionary&lt;System.UInt32, RoomEvent&gt;.</returns>
        internal Dictionary<uint, RoomEvent> GetEvents()
        {
            return _events;
        }

        /// <summary>
        ///     Gets the event.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>RoomEvent.</returns>
        internal RoomEvent GetEvent(uint roomId)
        {
            return _events.ContainsKey(roomId) ? _events[roomId] : null;
        }

        /// <summary>
        ///     Rooms the has events.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RoomHasEvents(uint roomId)
        {
            return _events.ContainsKey(roomId);
        }

        /// <summary>
        ///     Serializes the event information.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        internal async Task SerializeEventInfo(uint roomId)
        {
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);
            if (room == null)
            {
                return;
            }
            var @event = GetEvent(roomId);
            if (@event == null || @event.HasExpired)
            {
                return;
            }
            if (!RoomHasEvents(roomId))
            {
                return;
            }
            var serverMessage = new ServerMessage();
            await serverMessage.InitAsync(LibraryParser.OutgoingRequest("RoomEventMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomId);
            await serverMessage.AppendIntegerAsync(room.RoomData.OwnerId);
            await serverMessage.AppendStringAsync(room.RoomData.Owner);
            await serverMessage.AppendIntegerAsync(1);
            await serverMessage.AppendIntegerAsync(1);
            await serverMessage.AppendStringAsync(@event.Name);
            await serverMessage.AppendStringAsync(@event.Description);
            await serverMessage.AppendIntegerAsync(0);
            await serverMessage.AppendIntegerAsync(
                ((int) Math.Floor((@event.Time - Oblivion.GetUnixTimeStamp())/60.0)));

            await serverMessage.AppendIntegerAsync(@event.Category);
            await room.SendMessage(serverMessage);
        }

        /// <summary>
        ///     Updates the event.
        /// </summary>
        /// <param name="Event">The event.</param>
        internal async Task UpdateEvent(RoomEvent Event)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Concat("REPLACE INTO rooms_events VALUES (", Event.RoomId,
                    ", @name, @desc, ", Event.Time, ")"));
                queryReactor.AddParameter("name", Event.Name);
                queryReactor.AddParameter("desc", Event.Description);
                await queryReactor.RunQueryAsync();
            }
            await SerializeEventInfo(Event.RoomId);
        }
    }
}