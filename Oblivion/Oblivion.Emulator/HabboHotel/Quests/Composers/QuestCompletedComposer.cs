using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Quests.Composer
{
    /// <summary>
    ///     Class QuestCompletedComposer.
    /// </summary>
    internal class QuestCompletedComposer
    {
        /// <summary>
        ///     Composes the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="quest">The quest.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(GameClient session, Quest quest)
        {
            var amountOfQuestsInCategory = Oblivion.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(quest.Category);
            var i = quest.Number;
            var i2 = session.GetHabbo().GetQuestProgress(quest.Id);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("QuestCompletedMessageComposer"));
            serverMessage.AppendString(quest.Category);
            serverMessage.AppendInteger(i);
            serverMessage.AppendInteger(quest.Name.Contains("xmas2012") ? 1 : amountOfQuestsInCategory);
            serverMessage.AppendInteger(quest.RewardType);
            serverMessage.AppendInteger(quest.Id);
            serverMessage.AppendBool(session.GetHabbo().CurrentQuestId == quest.Id);
            serverMessage.AppendString(quest.ActionName);
            serverMessage.AppendString(quest.DataBit);
            serverMessage.AppendInteger(quest.Reward);
            serverMessage.AppendString(quest.Name);
            serverMessage.AppendInteger(i2);
            serverMessage.AppendInteger(quest.GoalData);
            serverMessage.AppendInteger(quest.TimeUnlock);
            serverMessage.AppendString("");
            serverMessage.AppendString("");
            serverMessage.AppendBool(true);
            serverMessage.AppendBool(true);
            return serverMessage;
        }
    }
}