using System.Collections.Generic;
using Oblivion.HabboHotel.Achievements.Interfaces;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Relationships;
using Oblivion.HabboHotel.Users.Subscriptions;

namespace Oblivion.HabboHotel.Users.UserDataManagement
{
    /// <summary>
    ///     Class UserData.
    /// </summary>
    internal class UserData
    {
        /// <summary>
        ///     The achievements
        /// </summary>
        internal Dictionary<string, UserAchievement> Achievements;

      
        

        /// <summary>
        ///     The effects
        /// </summary>
//        internal List<AvatarEffect> Effects;

        /// <summary>
        ///     The favourited rooms
        /// </summary>
        internal List<uint> FavouritedRooms;

        /// <summary>
        ///     The friends
        /// </summary>
        internal Dictionary<uint, MessengerBuddy> Friends;

        /// <summary>
        ///     The ignores
        /// </summary>
        internal List<uint> Ignores;
        

        /// <summary>
        ///     The mini mail count
        /// </summary>
        internal uint MiniMailCount;
        

        /// <summary>
        ///     The quests
        /// </summary>
        internal Dictionary<uint, int> Quests;

        /// <summary>
        ///     The relations
        /// </summary>
        internal Dictionary<int, Relationship> Relations;

        /// <summary>
        ///     The requests
        /// </summary>
        internal Dictionary<uint, MessengerRequest> Requests;

        /// <summary>
        ///     The rooms
        /// </summary>
        internal List<RoomData> Rooms;

        /// <summary>
        ///     The subscriptions
        /// </summary>
        internal Subscription Subscriptions;

        /// <summary>
        ///     The suggested polls
        /// </summary>
        internal HashSet<uint> SuggestedPolls;

        /// <summary>
        ///     The tags
        /// </summary>
        internal List<string> Tags;

        /// <summary>
        ///     The talents
        /// </summary>
        internal Dictionary<int, UserTalent> Talents;

        /// <summary>
        ///     The user
        /// </summary>
        internal Habbo User;

        /// <summary>
        ///     The user identifier
        /// </summary>
        internal uint UserId;

        ///<summary>
        /// User blockeds commands
        /// </summary>
        internal List<string> BlockedCommands;


        /// <summary>
        /// Opened gifts (xmas calendar)
        /// </summary>
        public List<int> OpenedGifts;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserData" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="achievements">The achievements.</param>
        /// <param name="talents">The talents.</param>
        /// <param name="favouritedRooms">The favourited rooms.</param>
        /// <param name="ignores">The ignores.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="sub">The sub.</param>
        /// <param name="friends">The friends.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="rooms">The rooms.</param>
        /// <param name="quests">The quests.</param>
        /// <param name="user">The user.</param>
        /// <param name="relations">The relations.</param>
        /// <param name="suggestedPolls">The suggested polls.</param>
        /// <param name="miniMailCount">The mini mail count.</param>
        public UserData(uint userId, Dictionary<string, UserAchievement> achievements,
            Dictionary<int, UserTalent> talents, List<uint> favouritedRooms, List<uint> ignores, List<string> tags,
            Subscription sub,
            Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests,
            List<RoomData> rooms, Dictionary<uint, int> quests, Habbo user,
            Dictionary<int, Relationship> relations, HashSet<uint> suggestedPolls,
            uint miniMailCount, List<string> blockedCommands, List<int> openedGifts)
        {
            UserId = userId;
            Achievements = achievements;
            Talents = talents;
            FavouritedRooms = favouritedRooms;
            Ignores = ignores;
            Tags = tags;
            Subscriptions = sub;
            Friends = friends;
            Requests = requests;
            Rooms = rooms;
            Quests = quests;
            User = user;
            Relations = relations;
            SuggestedPolls = suggestedPolls;
            MiniMailCount = miniMailCount;
            BlockedCommands = blockedCommands;
            OpenedGifts = openedGifts;
        }

        public void Dispose()
        {
            BlockedCommands.Clear();
            BlockedCommands = null;
            Rooms.Clear();
            Rooms = null;
            Quests.Clear();
            Quests = null;
            User = null;
            Relations.Clear();
            Relations = null;
            SuggestedPolls.Clear();
            SuggestedPolls = null;
            MiniMailCount = 0;
            FavouritedRooms.Clear();
            FavouritedRooms = null;
            Ignores.Clear();
            Ignores = null;
            Tags.Clear();
            Tags = null;
            Subscriptions = null;
            Friends.Clear();
            Friends = null;
            Requests.Clear();
            Requests = null;
            Talents.Clear();
            Talents = null;
            Achievements.Clear();
            Achievements = null;
            OpenedGifts.Clear();
            OpenedGifts = null;
        }
    }
}