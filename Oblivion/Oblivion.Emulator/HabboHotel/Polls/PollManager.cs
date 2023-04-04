using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

namespace Oblivion.HabboHotel.Polls
{
    /// <summary>
    ///     Class PollManager.
    /// </summary>
    internal class PollManager
    {
        /// <summary>
        ///     The polls
        /// </summary>
        internal Dictionary<uint, Poll> Polls;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PollManager" /> class.
        /// </summary>
        internal PollManager() => Polls = new Dictionary<uint, Poll>();

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="pollLoaded">The poll loaded.</param>
        internal async Task Init(IQueryAdapter dbClient, uint pollLoaded)
        {
           await Init(dbClient);
            pollLoaded = (uint) Polls.Count;
        }

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal Task Init(IQueryAdapter dbClient)
        {
            Polls.Clear();

            dbClient.SetQuery("SELECT * FROM polls_data WHERE enabled = '1'");

            var table = dbClient.GetTable();

            if (table == null)
                return Task.CompletedTask;

            /* TODO CHECK */ foreach (DataRow dataRow in table.Rows)
            {
                var num = uint.Parse(dataRow["id"].ToString());

                dbClient.SetQuery($"SELECT * FROM polls_questions WHERE poll_id = {num}");

                var table2 = dbClient.GetTable();

                var list = (from DataRow dataRow2 in table2.Rows select new PollQuestion(uint.Parse(dataRow2["id"].ToString()), (string) dataRow2["question"], int.Parse(dataRow2["answertype"].ToString()), dataRow2["answers"].ToString().Split('|'), (string) dataRow2["correct_answer"])).ToList();

                var value = new Poll(num, uint.Parse(dataRow["room_id"].ToString()), (string) dataRow["caption"], (string) dataRow["invitation"], (string) dataRow["greetings"], (string) dataRow["prize"], int.Parse(dataRow["type"].ToString()), list);

                Polls.Add(num, value);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Tries the get poll.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="poll">The poll.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryGetPoll(uint roomId, out Poll poll)
        {
            poll = Polls.Values.FirstOrDefault(x => x.RoomId == roomId);
            if (poll == null) return false;
            
            return true;
        }

        /// <summary>
        ///     Tries the get poll by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Poll.</returns>
        internal Poll TryGetPollById(uint id) => Polls.TryGetValue(id, out var poll) ? poll : null;
    }
}