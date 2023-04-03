using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Achievements.Composers;
using Oblivion.HabboHotel.Achievements.Factorys;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Achievements
{
    /// <summary>
    ///     Class AchievementManager.
    /// </summary>
    public class AchievementManager
    {
        /// <summary>
        ///     The achievement data cached
        /// </summary>
        internal ServerMessage AchievementDataCached;

        /// <summary>
        ///     The achievements
        /// </summary>
        internal Dictionary<string, Achievement> Achievements;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AchievementManager" /> class.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="loadedAchs">The loaded achs.</param>
        internal AchievementManager()
        {
            Achievements = new Dictionary<string, Achievement>();
          //  LoadAchievements(dbClient);
//            loadedAchs = (uint)Achievements.Count;
        }

        /// <summary>
        ///     Loads the achievements.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal async Task LoadAchievements(IQueryAdapter dbClient)
        {
            Achievements.Clear();

            AchievementLevelFactory.GetAchievementLevels(out Achievements, dbClient);

            AchievementDataCached =
                new ServerMessage(LibraryParser.OutgoingRequest("SendAchievementsRequirementsMessageComposer"));
            await AchievementDataCached.AppendIntegerAsync(Achievements.Count);

            /* TODO CHECK */
            foreach (var ach in Achievements.Values)
            {
                await AchievementDataCached.AppendStringAsync(ach.GroupName.Replace("ACH_", string.Empty));
                await AchievementDataCached.AppendIntegerAsync(ach.Levels.Count);

                var i = 1;
                foreach (var level in ach.Levels.Values)
                {
                    await AchievementDataCached.AppendIntegerAsync(i);
                    await AchievementDataCached.AppendIntegerAsync(level.Requirement);
                    i++;
                }
            }

            await AchievementDataCached.AppendIntegerAsync(0);
        }

        /// <summary>
        ///     Gets the list.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        internal async Task GetList(GameClient session, ClientMessage message)
        {
            await session.SendMessage(AchievementListComposer.Compose(session, Achievements.Values));
        }

        /// <summary>
        ///     Tries the progress login achievements.
        /// </summary>
        /// <param name="session">The session.</param>
        internal async Task TryProgressLoginAchievements(GameClient session)
        {
            if (session.GetHabbo() == null)
                return;

            var loginAch = session.GetHabbo().GetAchievementData("ACH_Login");

            if (loginAch == null)
            {
                await ProgressUserAchievement(session, "ACH_Login", 1, true);
                return;
            }

            var daysBtwLastLogin = Oblivion.GetUnixTimeStamp() - session.GetHabbo().PreviousOnline;

            if (daysBtwLastLogin >= 51840 && daysBtwLastLogin <= 112320)
                await ProgressUserAchievement(session, "ACH_Login", 1);
        }

        /// <summary>
        ///     Tries the progress registration achievements.
        /// </summary>
        /// <param name="session">The session.</param>
        internal async Task TryProgressRegistrationAchievements(GameClient session)
        {
            if (session.GetHabbo() == null)
                return;

            var regAch = session.GetHabbo().GetAchievementData("ACH_RegistrationDuration");

            if (regAch == null)
            {
                await ProgressUserAchievement(session, "ACH_RegistrationDuration", 1, true);
                return;
            }

            if (regAch.Level == 5)
                return;

            double sinceMember = Oblivion.GetUnixTimeStamp() - (int)session.GetHabbo().CreateDate;

            var daysSinceMember = Convert.ToInt32(Math.Round(sinceMember / 86400));

            if (daysSinceMember == regAch.Progress)
                return;

            var days = daysSinceMember - regAch.Progress;

            if (days < 1)
                return;

            await ProgressUserAchievement(session, "ACH_RegistrationDuration", days);
        }

        /// <summary>
        ///     Tries the progress habbo club achievements.
        /// </summary>
        /// <param name="session">The session.</param>
        internal async Task TryProgressHabboClubAchievements(GameClient session)
        {
            if (session.GetHabbo() == null || !session.GetHabbo().GetSubscriptionManager().HasSubscription)
                return;

            var clubAch = session.GetHabbo().GetAchievementData("ACH_VipHC");

            if (clubAch == null)
            {
                await ProgressUserAchievement(session, "ACH_VipHC", 1, true);
                await ProgressUserAchievement(session, "ACH_BasicClub", 1, true);
                return;
            }

            if (clubAch.Level == 5)
                return;

            var subscription = session.GetHabbo().GetSubscriptionManager().GetSubscription();

            var sinceActivation = Oblivion.GetUnixTimeStamp() - subscription.ActivateTime;

            if (sinceActivation < 31556926)
                return;

            for (int i = 1; i < 5; i++)
            {
                if (sinceActivation >= 31556926 * i)
                {
                    await ProgressUserAchievement(session, "ACH_VipHC", 1);
                    await ProgressUserAchievement(session, "ACH_BasicClub", 1);
                }
                else
                    break;
            }
        }

        public async Task<bool> ProgressUserAchievement(GameClient Session, string AchievementGroup, int ProgressAmount,
            bool FromZero = false, bool talentComplete = false)
        {
            if (Session?.GetHabbo()?.Data?.Achievements == null) return false;

            if (!Achievements.TryGetValue(AchievementGroup, out var AchievementData))
                return false;


            var UserData = Session.GetHabbo().GetAchievementData(AchievementGroup);
            if (UserData == null)
            {
                UserData = new UserAchievement(AchievementGroup, 0, 0);
                Session.GetHabbo().Data.Achievements.Add(AchievementGroup, UserData);
                FromZero = true;
            }
            else
            {
                FromZero = false;
            }

            var TotalLevels = AchievementData.Levels.Count;

            if (UserData.Level >= TotalLevels)
                return false; // done, no more.

            var TargetLevel = UserData.Level + 1;

            if (TargetLevel > TotalLevels)
                TargetLevel = TotalLevels;

            var TargetLevelData = AchievementData.Levels[TargetLevel];
            int NewProgress;
            if (FromZero)
                NewProgress = ProgressAmount;
            else
                NewProgress = UserData.Progress + ProgressAmount;

            var NewLevel = UserData.Level;
            var NewTarget = NewLevel + 1;

            if (NewTarget > TotalLevels)
                NewTarget = TotalLevels;

            if (NewProgress >= TargetLevelData.Requirement)
            {
                NewLevel++;
                NewTarget++;


                NewProgress = 0;

                var userBadgeComponent = Session.GetHabbo().GetBadgeComponent();
                if (TargetLevel != 1)
                    await userBadgeComponent.RemoveBadge(Convert.ToString($"{AchievementGroup}{TargetLevel - 1}"), Session);

                await userBadgeComponent.GiveBadge($"{AchievementGroup}{TargetLevel}", true, Session);


                if (NewTarget > TotalLevels)
                    NewTarget = TotalLevels;

                await Session.SendMessageAsync(AchievementUnlockedComposer.Compose(AchievementData, TargetLevel,
                    TargetLevelData.RewardPoints, TargetLevelData.RewardPixels));

                var levels = new KeyValuePair<int, int>(NewLevel, NewProgress);

                if (Session.GetHabbo()?.AchievementsToUpdate != null)
                    Session.GetHabbo().AchievementsToUpdate[AchievementGroup] = levels;

                UserData.Level = NewLevel;
                UserData.Progress = NewProgress;

                Session.GetHabbo().ActivityPoints += TargetLevelData.RewardPixels;
                Session.GetHabbo().AchievementPoints += TargetLevelData.RewardPoints;
                await Session.GetHabbo().UpdateActivityPointsBalance();
                //                Session.SendMessage(new AchievementScoreComposer(Session.GetHabbo().GetStats().AchievementPoints));
                await Session.SendMessageAsync(AchievementScoreUpdateComposer.Compose(Session.GetHabbo().AchievementPoints));

                var NewLevelData = AchievementData.Levels[NewTarget];

                await Session.SendMessageAsync(AchievementProgressComposer.Compose(AchievementData, NewTarget, NewLevelData,
                    TotalLevels, Session.GetHabbo().GetAchievementData(AchievementGroup)));

                if (!talentComplete &&
                    Oblivion.GetGame().GetTalentManager().TryGetTalent(AchievementGroup, out var talent))
                    await Oblivion.GetGame().GetTalentManager().CompleteUserTalent(Session, talent);
                return true;
            }

            UserData.Level = NewLevel;
            UserData.Progress = NewProgress;
            var levelsN = new KeyValuePair<int, int>(NewLevel, NewProgress);

            Session.GetHabbo().AchievementsToUpdate[AchievementGroup] = levelsN;

            await Session.SendMessageAsync(AchievementProgressComposer.Compose(AchievementData, TargetLevel, TargetLevelData,
                TotalLevels, Session.GetHabbo().GetAchievementData(AchievementGroup)));

            return false;
        }


        /// <summary>
        ///     Gets the achievement.
        /// </summary>
        /// <param name="achievementGroup">The achievement group.</param>
        /// <returns>Achievement.</returns>
        internal Achievement GetAchievement(string achievementGroup) =>
            Achievements.TryGetValue(achievementGroup, out var ach)
                ? ach
                : new Achievement();
    }
}