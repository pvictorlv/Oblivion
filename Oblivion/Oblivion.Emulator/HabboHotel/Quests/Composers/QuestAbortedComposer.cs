using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Quests.Composer
{
    /// <summary>
    ///     Class QuestAbortedComposer.
    /// </summary>
    internal class QuestAbortedComposer
    {
        /// <summary>
        ///     Composes this instance.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("QuestAbortedMessageComposer"));
            serverMessage.AppendBool(false);
            return serverMessage;
        }
    }
}