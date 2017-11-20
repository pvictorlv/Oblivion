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
                queryReactor.SetQuery("SELECT id,username,look,vip_points FROM users WHERE rank < 3 ORDER BY vip_points DESC LIMIT 10");
                var table = queryReactor.GetTable();

                if (table == null)
                    return;

                foreach (DataRow row in table.Rows)
                    Rankings.Add(new HallOfFameElement((uint) row["id"], (int) row["vip_points"], row["username"].ToString(), row["look"].ToString()));
            }
        }
    }

    /// <summary>
    ///     Class HallOfFameElement.
    /// </summary>
    internal class HallOfFameElement
    {
        internal int Score;
        internal uint UserId;
        internal string Username;
        internal string Look;

        internal HallOfFameElement(uint userId, int score, string username, string look)
        {
            UserId = userId;
            Score = score;
            Username = username;
            Look = look;
        }
    }
}