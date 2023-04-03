using System;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.Messages;

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
        internal async Task Save(uint roomId, IQueryAdapter dbClient)
        {
            if (IsSaved)
                return;

//            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {

                dbClient.SetQuery("INSERT INTO users_chatlogs (user_id, room_id, timestamp, message) VALUES (@user, @room, @time, @message)");
                dbClient.AddParameter("user", UserId);
                dbClient.AddParameter("room", roomId);
                dbClient.AddParameter("time", Oblivion.DateTimeToUnix(TimeStamp));
                dbClient.AddParameter("message", Message);
                dbClient.RunQuery();
            }
            IsSaved = true;
        }

        internal async Task Serialize(ref ServerMessage message)
        {
            var habbo = Oblivion.GetHabboById(UserId);
            message.AppendString(TimeStamp.ToString("h:mm:ss"));
            message.AppendInteger(UserId);
            message.AppendString(habbo == null ? "*User not found*" : habbo.UserName);
            message.AppendString(Message);
            message.AppendBool(_globalMessage);
        }
    }
}