using System;
using System.Collections.Generic;
using System.Data;
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
        internal void AddNewEvent(uint roomId, string eventName, string eventDesc, GameClient session, int time = 7200,
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
                        queryReactor.RunQuery();
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
                    queryreactor2.RunQuery();
                }
                _events.Add(roomId, new RoomEvent(roomId, eventName, eventDesc));
                IL_17C:
                Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId).Event = _events[roomId];
                var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);
                if (room != null)
                {
                    room.RoomData.Event = _events[roomId];
                }
                if (session.GetHabbo().CurrentRoomId == roomId)
                {
                    SerializeEventInfo(roomId);
                }
            }
        }

        /// <summary>
        ///     Removes the event.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        internal void RemoveEvent(uint roomId)
        {
            _events.Remove(roomId);
            SerializeEventInfo(roomId);
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
        internal void SerializeEventInfo(uint roomId)
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
            serverMessage.Init(LibraryParser.OutgoingRequest("RoomEventMessageComposer"));
            serverMessage.AppendInteger(roomId);
            serverMessage.AppendInteger(room.RoomData.OwnerId);
            serverMessage.AppendString(room.RoomData.Owner);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(1);
            serverMessage.AppendString(@event.Name);
            serverMessage.AppendString(@event.Description);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(
                ((int) Math.Floor((@event.Time - Oblivion.GetUnixTimeStamp())/60.0)));

            serverMessage.AppendInteger(@event.Category);
            room.SendMessage(serverMessage);
        }

        /// <summary>
        ///     Updates the event.
        /// </summary>
        /// <param name="Event">The event.</param>
        internal void UpdateEvent(RoomEvent Event)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Concat("REPLACE INTO rooms_events VALUES (", Event.RoomId,
                    ", @name, @desc, ", Event.Time, ")"));
                queryReactor.AddParameter("name", Event.Name);
                queryReactor.AddParameter("desc", Event.Description);
                queryReactor.RunQuery();
            }
            SerializeEventInfo(Event.RoomId);
        }
    }
}