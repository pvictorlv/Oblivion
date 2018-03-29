using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Navigators.Interfaces;

namespace Oblivion.HabboHotel.Users.Authenticator
{
    /// <summary>
    ///     Class HabboFactory.
    /// </summary>
    internal static class HabboFactory
    {
        /// <summary>
        ///     Generates the habbo.
        /// </summary>
        /// <param name="dRow">The d row.</param>
        /// <param name="mRow">The m row.</param>
        /// <param name="group">The group.</param>
        /// <returns>Habbo.</returns>
        internal static Habbo GenerateHabbo(DataRow dRow, DataRow mRow, List<GroupMember> group)
        {
            var id = uint.Parse(dRow["id"].ToString());
            var userName = (string) dRow["username"];
            var ras = uint.Parse(dRow["rank"].ToString());
            var motto = (string) dRow["motto"];
            var look = (string) dRow["look"];
            var gender = (string) dRow["gender"];
            var lastOnline = int.Parse(dRow["last_online"].ToString());
            var credits = (int) dRow["credits"];
            var vipPoints = (int) dRow["vip_points"];
            var activityPoints = (int) dRow["activity_points"];
            var muted = Oblivion.EnumToBool(dRow["is_muted"].ToString());
            var homeRoom = Convert.ToUInt32(dRow["home_room"]);

            int lastTotem = 0,
                respect = 0,
                dailyRespectPoints = 3,
                dailyPetRespectPoints = 3,
                achievementPoints = 0,
                dailyCompetitionVotes = 3;
            uint currentQuestId = 0, favId = 0;
            try
            {
                respect = (int) mRow["respect"];
                dailyRespectPoints = (int) mRow["daily_respect_points"];
                dailyPetRespectPoints = (int) mRow["daily_pet_respect_points"];
                currentQuestId = Convert.ToUInt32(mRow["quest_id"]);
                achievementPoints = (int) mRow["achievement_score"];
                favId = uint.Parse(mRow["favourite_group"].ToString());
                dailyCompetitionVotes = (int) mRow["daily_competition_votes"];
                lastTotem = Convert.ToInt32(mRow["last_totem"]);
            }
            catch (Exception)
            {
            }

            //AQUI
            var hasFriendRequestsDisabled = Oblivion.EnumToBool(dRow["block_newfriends"].ToString());
            var appearOffline = Oblivion.EnumToBool(dRow["hide_online"].ToString());
            var hideInRoom = Oblivion.EnumToBool(dRow["hide_inroom"].ToString());
            // TERMINA
            var vip = Oblivion.EnumToBool(dRow["vip"].ToString());
            var createDate = Convert.ToDouble(dRow["account_created"]);
            var citizenship = dRow["talent_status"].ToString();
            var diamonds = int.Parse(dRow["diamonds"].ToString());
            var lastChange = (int) dRow["last_name_change"];
            var tradeLocked = Oblivion.EnumToBool(dRow["trade_lock"].ToString());
            var tradeLockExpire = int.Parse(dRow["trade_lock_expire"].ToString());

            /* builders club */
            var buildersExpire = (int) dRow["builders_expire"];
            var buildersItemsMax = (int) dRow["builders_items_max"];
            var buildersItemsUsed = (int) dRow["builders_items_used"];

            /* guides */
            var onDuty = Convert.ToBoolean(dRow["OnDuty"]);
            var dutyLevel = uint.Parse(dRow["DutyLevel"].ToString());
            var disableAlert = Oblivion.EnumToBool(dRow["disabled_alert"].ToString());
            var navilogs = new Dictionary<int, NaviLogs>();
            var navilogstring = (string) dRow["navilogs"];
            if (navilogstring.Length <= 0)
                return new Habbo(id, userName, ras, motto, look, gender, credits, activityPoints, muted, homeRoom,
                    respect, dailyRespectPoints, dailyPetRespectPoints,
                    hasFriendRequestsDisabled, currentQuestId, achievementPoints,
                    lastOnline, appearOffline, hideInRoom, vip, createDate, citizenship, diamonds, group, favId,
                    lastChange, tradeLocked, tradeLockExpire, buildersExpire, buildersItemsMax,
                    buildersItemsUsed, onDuty, navilogs, dailyCompetitionVotes, dutyLevel, disableAlert, lastTotem,
                    vipPoints)
                {
                    LoadedGroups = true
                };
            /* TODO CHECK */
            foreach (var naviLogs in navilogstring.Split(';').Where(value => navilogstring.Contains(",")).Select(value => new NaviLogs(int.Parse(value.Split(',')[0]), value.Split(',')[1], value.Split(',')[2])).Where(naviLogs => !navilogs.ContainsKey(naviLogs.Id)))
            {
                navilogs.Add(naviLogs.Id, naviLogs);
            }

            return new Habbo(id, userName, ras, motto, look, gender, credits, activityPoints, muted, homeRoom, respect,
                dailyRespectPoints, dailyPetRespectPoints,
                hasFriendRequestsDisabled, currentQuestId, achievementPoints,
                lastOnline, appearOffline, hideInRoom, vip, createDate, citizenship, diamonds, group, favId,
                lastChange, tradeLocked, tradeLockExpire, buildersExpire, buildersItemsMax,
                buildersItemsUsed, onDuty, navilogs, dailyCompetitionVotes, dutyLevel, disableAlert, lastTotem,
                vipPoints)
            {
                LoadedGroups = true
            };
        }

        internal static Habbo GenerateCachedHabbo(DataRow dRow, List<GroupMember> group)
        {
            var id = uint.Parse(dRow["id"].ToString());
            var userName = (string) dRow["username"];
            var motto = (string) dRow["motto"];
            var look = (string) dRow["look"];
            var gender = (string) dRow["gender"];
            var lastOnline = int.Parse(dRow["last_online"].ToString());
            var achievementPoints = (int) dRow["achievement_score"];
            var favId = uint.Parse(dRow["favourite_group"].ToString());


            //AQUI
            var hasFriendRequestsDisabled = Oblivion.EnumToBool(dRow["block_newfriends"].ToString());

            // TERMINA
            var createDate = Convert.ToDouble(dRow["account_created"]);


            return new Habbo(id, userName, 0, motto, look, gender, 0, 0, false, 0,
                0, 3, 3,
                hasFriendRequestsDisabled, 0, achievementPoints,
                lastOnline, false, false, false, createDate, "citizenship", 0, group, favId,
                0, false, 0, 0, 0,
                0, false, null, 0, 0, false, 0, 0);
        }
    }
}