using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Quests.Composer
{
    /// <summary>
    ///     Class QuestStartedComposer.
    /// </summary>
    internal class QuestStartedComposer
    {
        /// <summary>
        ///     Composes the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="quest">The quest.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(GameClient session, Quest quest)
        {
            var msg = session.GetMessageHandler().GetResponse();
            msg.Init(LibraryParser.OutgoingRequest("QuestStartedMessageComposer"));
            QuestListComposer.SerializeQuest(msg, session, quest, quest.Category);
            return msg;
        }
    }
}