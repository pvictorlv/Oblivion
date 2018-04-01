using System;
using System.Collections.Generic;
using System.Data;
using Oblivion.Configuration;
using Oblivion.Util;

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
                queryReactor.SetQuery("SELECT id,username,look,diamonds FROM users WHERE rank < 3 ORDER BY diamonds DESC LIMIT 10");
                var table = queryReactor.GetTable();

                if (table == null)
                    return;
               
                    foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        if (!int.TryParse(row["diamonds"].ToString(), out var diamonds))
                        {
                            Out.WriteLine($"User {row["id"]} has invalid diamond amount");
                            continue;
                        }

                        Rankings.Add(new HallOfFameElement((uint) row["id"], diamonds,
                            row["username"].ToString(), row["look"].ToString()));
                    }
                    catch (Exception e)
                    {
                        Logging.HandleException(e, $"Hall of fame user: {row["id"]}");
                    }
                }
                
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