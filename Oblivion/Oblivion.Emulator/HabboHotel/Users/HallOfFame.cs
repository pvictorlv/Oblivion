using System.Collections.Generic;
using System.Data;

namespace Oblivion.HabboHotel.Users
{
    /// <summary>
    ///     Class HallOfFame.
    /// </summary>
    internal class HallOfFame
    {
        internal List<HallOfFameElement> Rankings;

        internal HallOfFame()
        {
            Rankings = new List<HallOfFameElement>();
            RefreshHallOfFame();
        }

        public void RefreshHallOfFame()
        {
            Rankings.Clear();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM users_rankings ORDER BY score DESC");
                var table = queryReactor.GetTable();

                if (table == null)
                    return;

                foreach (DataRow row in table.Rows)
                    Rankings.Add(new HallOfFameElement((uint) row["user_id"], (int) row["score"],
                        (string) row["competition"]));
            }
        }
    }

    /// <summary>
    ///     Class HallOfFameElement.
    /// </summary>
    internal class HallOfFameElement
    {
        internal string Competition;
        internal int Score;
        internal uint UserId;

        internal HallOfFameElement(uint userId, int score, string competition)
        {
            UserId = userId;
            Score = score;
            Competition = competition;
        }
    }
}