using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Achievements.Composers
{
    /// <summary>
    ///     Class AchievementScoreUpdateComposer.
    /// </summary>
    internal class AchievementScoreUpdateComposer
    {
        /// <summary>
        ///     Composes the specified score.
        /// </summary>
        /// <param name="score">The score.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(int score)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AchievementPointsMessageComposer"));
            serverMessage.AppendInteger(score);
            return serverMessage;
        }
    }
}