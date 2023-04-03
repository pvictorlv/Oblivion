using System.Collections.Generic;
using System.Data;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Items.Datas
{
    /// <summary>
    ///     Class HighscoreData.
    /// </summary>
    internal class HighscoreData
    {
        /// <summary>
        ///     The last identifier
        /// </summary>
        internal int LastId;

        /// <summary>
        ///     The lines
        /// </summary>
        internal Dictionary<int, HighScoreLine> Lines;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HighscoreData" /> class.
        /// </summary>
        /// <param name="roomItem">The room item.</param>
        internal HighscoreData(RoomItem roomItem)
        {
            Lines = new Dictionary<int, HighScoreLine>();
            var itemId = roomItem.Id;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM items_highscores WHERE item_id='" + itemId +
                                      "' ORDER BY score DESC");

                var table = queryReactor.GetTable();

                if (table == null)
                    return;

                foreach (DataRow row in table.Rows)
                {
                    Lines.Add((int) row["id"], new HighScoreLine((string) row["username"], (int) row["score"]));
                    LastId = (int) row["id"];
                }
            }
        }

        public void Dispose()
        {
            Lines?.Clear();
            Lines = null;
        }

        /// <summary>
        ///     Generates the extra data.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="message">The message.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GenerateExtraData(RoomItem item, ServerMessage message)
        {
            message.AppendInteger(6);
            message.AppendString(item.ExtraData); //Ouvert/fermé

            if (item.GetBaseItem().Name.StartsWith("highscore_classic"))
                message.AppendInteger(2);
            else if (item.GetBaseItem().Name.StartsWith("highscore_mostwin"))
                message.AppendInteger(1);
            else if (item.GetBaseItem().Name.StartsWith("highscore_perteam"))
                message.AppendInteger(0);

            message.AppendInteger(0); //Time : ["alltime", "daily", "weekly", "monthly"]
            message.AppendInteger(Lines.Count); //Count

            /* TODO CHECK */
            foreach (var line in Lines)
            {
                message.AppendInteger(line.Value.Score);
                message.AppendInteger(1);
                message.AppendString(line.Value.Username);
            }

            return message;
        }

        /// <summary>
        ///     Adds the user score.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="username">The username.</param>
        /// <param name="score">The score.</param>
        internal async Task AddUserScore(RoomItem item, string username, int score)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                if (item.GetBaseItem().Name.Contains("highscore_classic"))
                {
                    queryReactor.SetQuery(
                        "SELECT id,score FROM items_highscores WHERE username = @username AND item_id = @itemid");
                    queryReactor.AddParameter("itemid", item.Id);
                    queryReactor.AddParameter("username", username);

                    var row = queryReactor.GetRow();

                    if (row != null)
                    {
                        score += (int) row["score"];
                        queryReactor.SetQuery(
                            "UPDATE items_highscores SET score = @score WHERE username = @username AND item_id = @itemid");
                        queryReactor.AddParameter("itemid", item.Id);
                        queryReactor.AddParameter("username", username);
                        queryReactor.AddParameter("score", score);
                        queryReactor.RunQuery();
                        Lines.Remove((int) row["id"]);
                    }
                    else
                    {
                        queryReactor.SetQuery(
                            "INSERT INTO items_highscores (item_id,username,score) VALUES (@itemid,@username,@score)");
                        queryReactor.AddParameter("itemid", item.Id);
                        queryReactor.AddParameter("username", username);
                        queryReactor.AddParameter("score", score);
                        queryReactor.RunQuery();
                    }
                }
                else if (item.GetBaseItem().Name.StartsWith("highscore_mostwin"))
                {
                    score = 1;
                    queryReactor.SetQuery(
                        "SELECT id,score FROM items_highscores WHERE username = @username AND item_id = @itemid");
                    queryReactor.AddParameter("itemid", item.Id);
                    queryReactor.AddParameter("username", username);

                    var row = queryReactor.GetRow();

                    if (row != null)
                    {
                        queryReactor.SetQuery(
                            "UPDATE items_highscores SET score = score + 1 WHERE username = @username AND item_id = @itemid");
                        queryReactor.AddParameter("itemid", item.Id);
                        queryReactor.AddParameter("username", username);
                        queryReactor.RunQuery();
                        Lines.Remove((int) row["id"]);
                        score = (int) row["score"] + 1;
                    }
                    else
                    {
                        queryReactor.SetQuery(
                            "INSERT INTO items_highscores (item_id,username,score) VALUES (@itemid,@username,@score)");
                        queryReactor.AddParameter("itemid", item.Id);
                        queryReactor.AddParameter("username", username);
                        queryReactor.AddParameter("score", score);
                        queryReactor.RunQuery();
                    }
                }

                LastId++;
                if (!Lines.ContainsKey(LastId))
                    Lines.Add(LastId, new HighScoreLine(username, score));
            }
        }
    }
}