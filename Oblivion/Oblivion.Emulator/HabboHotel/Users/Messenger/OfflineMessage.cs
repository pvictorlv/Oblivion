using System.Collections.Generic;
using System.Data;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

namespace Oblivion.HabboHotel.Users.Messenger
{
    /// <summary>
    ///     Class OfflineMessage.
    /// </summary>
    internal class OfflineMessage
    {
        /// <summary>
        ///     From identifier
        /// </summary>
        internal uint FromId;

        /// <summary>
        ///     The message
        /// </summary>
        internal string Message;

        /// <summary>
        ///     The timestamp
        /// </summary>
        internal double Timestamp;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OfflineMessage" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="ts">The ts.</param>
        internal OfflineMessage(uint id, string msg, double ts)
        {
            FromId = id;
            Message = msg;
            Timestamp = ts;
        }

        /// <summary>
        ///     Initializes the offline messages.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal static void InitOfflineMessages(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM messenger_offline_messages");
            var table = dbClient.GetTable();

            /* TODO CHECK */ foreach (DataRow dataRow in table.Rows)
            {
                var key = (uint) dataRow[1];
                var id = (uint) dataRow[2];
                var msg = dataRow[3].ToString();
                var ts = (double) dataRow[4];

                if (Oblivion.OfflineMessages.TryGetValue(key, out var msgs))
                {
                    msgs.Add(new OfflineMessage(id, msg, ts));
                }
                else
                {
                    msgs = new List<OfflineMessage> {new OfflineMessage(id, msg, ts)};

                    Oblivion.OfflineMessages.Add(key, msgs);
                }

            }
        }

        /// <summary>
        ///     Saves the message.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="toId">To identifier.</param>
        /// <param name="fromId">From identifier.</param>
        /// <param name="message">The message.</param>
        internal static void SaveMessage(IQueryAdapter dbClient, uint toId, uint fromId, string message)
        {
            dbClient.SetQuery(
                "INSERT INTO messenger_offline_messages (to_id, from_id, Message, timestamp) VALUES (@tid, @fid, @msg, UNIX_TIMESTAMP())");
            dbClient.AddParameter("tid", toId);
            dbClient.AddParameter("fid", fromId);
            dbClient.AddParameter("msg", message);
            dbClient.RunQuery();
        }

        /// <summary>
        ///     Removes all messages.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="toId">To identifier.</param>
        internal static void RemoveAllMessages(IQueryAdapter dbClient, uint toId)
        {
            dbClient.RunFastQuery($"DELETE FROM messenger_offline_messages WHERE to_id={toId}");
        }
    }
}