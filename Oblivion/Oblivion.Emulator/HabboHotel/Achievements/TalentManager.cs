using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Achievements
{
    /// <summary>
    ///     Class TalentManager.
    /// </summary>
    internal class TalentManager
    {
        /// <summary>
        ///     The talents
        /// </summary>
        internal Dictionary<int, Talent> Talents = new Dictionary<int, Talent>();

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM achievements_talents ORDER BY `order_num` ASC");

            DataTable table = dbClient.GetTable();

             foreach (Talent talent in from DataRow dataRow in table.Rows select new Talent(
                (int)dataRow["id"], 
                (string)dataRow["type"], 
                (int)dataRow["parent_category"],
                (int)dataRow["level"], 
                (string)dataRow["achievement_group"], 
                (int)dataRow["achievement_level"],
                (string)dataRow["prize"], 
                (uint)dataRow["prize_baseitem"]))
                Talents.Add(talent.Id, talent);
        }

        /// <summary>
        ///     Gets the talent.
        /// </summary>
        /// <param name="talentId">The talent identifier.</param>
        /// <returns>Talent.</returns>
        internal Talent GetTalent(int talentId) => Talents[talentId];

        /// <summary>
        ///     Levels the is completed.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="trackType">Type of the track.</param>
        /// <param name="talentLevel">The talent level.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool LevelIsCompleted(GameClient session, string trackType, int talentLevel) => 
            GetTalents(trackType, talentLevel).All(
                current =>
                session.GetHabbo().GetAchievementData(current.AchievementGroup) == null || 
                session.GetHabbo().GetAchievementData(current.AchievementGroup).Level < current.AchievementLevel);

        /// <summary>
        ///     Completes the user talent.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="talent">The talent.</param>
        internal async Task CompleteUserTalent(GameClient session, Talent talent)
        {
            if (session?.GetHabbo() == null || session.GetHabbo().CurrentTalentLevel < talent.Level || session.GetHabbo().Data.Talents.ContainsKey(talent.Id))
                return;

            if (!LevelIsCompleted(session, talent.Type, talent.Level))
                return;

            if (!string.IsNullOrEmpty(talent.Prize) && talent.PrizeBaseItem > 0u)
               await Oblivion.GetGame().GetCatalog().DeliverItems(session, Oblivion.GetGame().GetItemManager().GetItem(talent.PrizeBaseItem), 1, string.Empty, 0, 0, string.Empty);

            session.GetHabbo().Data.Talents.Add(talent.Id, new UserTalent(talent.Id, 1));

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery($"REPLACE INTO users_talents VALUES ('{session.GetHabbo().Id}', '{talent.Id}', '1');");

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TalentLevelUpMessageComposer"));

            serverMessage.AppendString(talent.Type);
            serverMessage.AppendInteger(talent.Level);
            serverMessage.AppendInteger(0);

            if (talent.Type == "citizenship" && talent.Level == 4)
            {
                serverMessage.AppendInteger(2);
                serverMessage.AppendString("HABBO_CLUB_VIP_7_DAYS");
                serverMessage.AppendInteger(7);
                serverMessage.AppendString(talent.Prize);
                serverMessage.AppendInteger(0);
            }
            else
            {
                serverMessage.AppendInteger(1);
                serverMessage.AppendString(talent.Prize);
                serverMessage.AppendInteger(0);
            }

            session.SendMessage(serverMessage);

            if (talent.Type == "citizenship" && talent.Level == 3)
                Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(session, "ACH_Citizenship", 1, false, true);
            else if (talent.Type == "citizenship" && talent.Level == 4)
            {
                session.GetHabbo().GetSubscriptionManager().AddSubscription(7);

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunFastQuery($"UPDATE users SET talent_status = 'helper' WHERE id = '{session.GetHabbo().Id}'");
            }
        }

        /// <summary>
        ///     Tries the get talent.
        /// </summary>
        /// <param name="achGroup">The ach group.</param>
        /// <param name="talent">The talent.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryGetTalent(string achGroup, out Talent talent)
        {
            foreach (var current in Talents.Values)
            {
                if (current.AchievementGroup != achGroup) continue;
                talent = current;
                return true;
            }

            talent = new Talent();
            return false;
        }

        /// <summary>
        ///     Gets all talents.
        /// </summary>
        /// <returns>Dictionary&lt;System.Int32, Talent&gt;.</returns>
        internal Dictionary<int, Talent> GetAllTalents() => Talents;

        /// <summary>
        ///     Gets the talents.
        /// </summary>
        /// <param name="trackType">Type of the track.</param>
        /// <param name="parentCategory">The parent category.</param>
        /// <returns>List&lt;Talent&gt;.</returns>
        internal List<Talent> GetTalents(string trackType, int parentCategory) => Talents.Values.Where(current => current.Type == trackType && current.ParentCategory == parentCategory).ToList();
    }
}