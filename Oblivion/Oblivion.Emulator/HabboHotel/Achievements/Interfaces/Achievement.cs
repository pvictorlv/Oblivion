using System.Collections.Generic;

namespace Oblivion.HabboHotel.Achievements.Interfaces
{
    /// <summary>
    ///     Class Achievement.
    /// </summary>
    internal struct Achievement
    {
        /// <summary>
        ///     The identifier
        /// </summary>
        internal readonly uint Id;

        /// <summary>
        ///     The group name
        /// </summary>
        internal readonly string GroupName;

        /// <summary>
        ///     The category
        /// </summary>
        internal readonly string Category;

        /// <summary>
        ///     The levels
        /// </summary>
        internal readonly Dictionary<int, AchievementLevel> Levels;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Achievement" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="category">The category.</param>
        public Achievement(uint id, string groupName, string category)
        {
            Id = id;
            GroupName = groupName;
            Category = category;

            Levels = new Dictionary<int, AchievementLevel>();
        }

        /// <summary>
        ///     Adds the level.
        /// </summary>
        /// <param name="level">The level.</param>
        public void AddLevel(AchievementLevel level)
        {
            Levels.Add(level.Level, level);
        }

        public bool CheckLevel(AchievementLevel level)
        {
            return Levels.ContainsKey(level.Level);
        }
    }
}