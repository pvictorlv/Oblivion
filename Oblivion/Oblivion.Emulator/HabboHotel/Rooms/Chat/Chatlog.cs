using System;
using Oblivion.Messages;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Rooms.Chat
{
    /// <summary>
    ///     Class Chatlog.
    /// </summary>
    internal class Chatlog
    {
        /// <summary>
        ///     The is saved
        /// </summary>
        internal bool IsSaved, GlobalMessage;

        /// <summary>
        ///     The message
        /// </summary>
        internal string Message;

        /// <summary>
        ///     The timestamp
        /// </summary>
        internal DateTime TimeStamp;

        private readonly bool _globalMessage;

        /// <summary>
        ///     The user identifier
        /// </summary>
        internal uint UserId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Chatlog" /> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="time">The time.</param>
        /// <param name="globalMessage"></param>
        /// <param name="fromDatabase">if set to <c>true</c> [from database].</param>
        internal Chatlog(uint user, string msg, DateTime time, bool globalMessage, bool fromDatabase = false)
        {
            UserId = user;
            Message = msg;
            TimeStamp = time;
            _globalMessage = globalMessage;
            GlobalMessage = true;
            IsSaved = fromDatabase;
        }

        /// <summary>
        ///     Saves the specified room identifier.
        /// </summary>
        /// <param name="queryChunk"></param>
        /// <param name="id">Auto increment</param>
        internal void Save(QueryChunk queryChunk, uint id)
        {
            if (IsSaved)
                return;

            queryChunk.AddQuery("INSERT INTO users_chatlogs (user_id, room_id, timestamp, message) VALUES (@user" + id + ", @room, @time" + id + ", @message" + id + ")");
            queryChunk.AddParameter("user" + id, UserId);
            queryChunk.AddParameter("time" + id, Oblivion.DateTimeToUnix(TimeStamp));
            queryChunk.AddParameter("message" + id, Message);
        }

        internal void Serialize(ref ServerMessage message)
        {
            var habbo = Oblivion.GetHabboById(UserId);
            message.AppendInteger(Oblivion.DifferenceInMilliSeconds(TimeStamp, DateTime.Now));
            message.AppendInteger(UserId);
            message.AppendString(habbo == null ? "*User not found*" : habbo.UserName);
            message.AppendString(Message);
            message.AppendBool(GlobalMessage);
        }
    }
}