using System.Collections.Generic;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Achievements.Composers
{
    /// <summary>
    ///     Class AchievementListComposer.
    /// </summary>
    internal class AchievementListComposer
    {
        /// <summary>
        ///     Composes the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="achievements">The achievements.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(GameClient session, ICollection<Achievement> achievements)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AchievementListMessageComposer"));
            serverMessage.AppendInteger(achievements.Count);

            /* TODO CHECK */
            foreach (var achievement in achievements)
            {
                var achievementData = session.GetHabbo().GetAchievementData(achievement.GroupName);

                var i = achievementData?.Level + 1 ?? 1;

                var count = achievement.Levels.Count;

                if (i > count)
                    i = count;

                var achievementLevel = achievement.Levels[i];

                var oldLevel = (achievement.Levels.TryGetValue(i - 1, out var level)) ? level : achievementLevel;

                serverMessage.AppendInteger(achievement.Id);
                serverMessage.AppendInteger(i);
                serverMessage.AppendString($"{achievement.GroupName}{i}");
                serverMessage.AppendInteger(oldLevel.Requirement);
                serverMessage.AppendInteger(achievementLevel.Requirement);
                serverMessage.AppendInteger(achievementLevel.RewardPoints);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(achievementData?.Progress ?? 0);

                if (achievementData == null)
                    serverMessage.AppendBool(false);
                else if (achievementData.Level >= count)
                    serverMessage.AppendBool(true);
                else
                    serverMessage.AppendBool(false);

                serverMessage.AppendString(achievement.Category);
                serverMessage.AppendString(string.Empty);
                serverMessage.AppendInteger(count);
                serverMessage.AppendInteger(0);
            }

            serverMessage.AppendString(string.Empty);

            return serverMessage;
        }
    }
}