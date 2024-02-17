using System;
using System.Collections.Generic;
using System.Data;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Achievements.Factorys
{
    /// <summary>
    ///     Class AchievementLevelFactory.
    /// </summary>
    internal class AchievementLevelFactory
    {
        /// <summary>
        ///     Gets the achievement levels.
        /// </summary>
        /// <param name="achievements">The achievements.</param>
        /// <param name="dbClient">The database client.</param>
        internal static void GetAchievementLevels(out Dictionary<string, Achievement> achievements,
            IQueryAdapter dbClient)
        {
            achievements = new Dictionary<string, Achievement>();
            dbClient.SetQuery("SELECT * FROM achievements_data");

            var table = dbClient.GetTable();

            foreach (DataRow dataRow in table.Rows)
            {
                var id = Convert.ToUInt32(dataRow["id"]);
                var category = (string)dataRow["category"];
                var text = (string)dataRow["group_name"];
                var level = (int)dataRow["level"];
                var rewardPixels = (int)dataRow["reward_pixels"];
                var rewardPoints = (int)dataRow["reward_points"];
                var requirement = (int)dataRow["progress_needed"];

                var level2 = new AchievementLevel(level, rewardPixels, rewardPoints, requirement);

                if (!achievements.TryGetValue(text, out var levelT))
                {
                    var achievement = new Achievement(id, text, category);
                    achievement.AddLevel(level2);
                    achievements.Add(text, achievement);
                }
                else
                {
                    if (!levelT.CheckLevel(level2))
                        levelT.AddLevel(level2);
                    else
                        Out.WriteLineSimple("Was Found a Duplicated Level for: " + text + ", Level: " + level2.Level,
                            "[Oblivion.Achievements]", ConsoleColor.Cyan);
                }
            }
        }
    }
}