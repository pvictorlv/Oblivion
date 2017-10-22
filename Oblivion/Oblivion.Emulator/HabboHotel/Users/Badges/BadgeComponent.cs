using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Users.UserDataManagement;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Users.Badges
{
    /// <summary>
    ///     Class BadgeComponent.
    /// </summary>
    internal class BadgeComponent
    {
        /// <summary>
        ///     The _user identifier
        /// </summary>
        private readonly uint _userId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BadgeComponent" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="data">The data.</param>
        internal BadgeComponent(uint userId, UserData data)
        {
            BadgeList = new HybridDictionary();

            /* TODO CHECK */ foreach (var current in data.Badges.Where(current => !BadgeList.Contains(current.Code)))
                BadgeList.Add(current.Code, current);

            _userId = userId;
        }

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>The count.</value>
        internal int Count => BadgeList.Count;

        /// <summary>
        ///     Gets or sets the badge list.
        /// </summary>
        /// <value>The badge list.</value>
        internal HybridDictionary BadgeList { get; set; }

        /// <summary>
        ///     Gets the badge.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <returns>Badge.</returns>
        internal Badge GetBadge(string badge)
        {
            return BadgeList.Contains(badge) ? (Badge) BadgeList[badge] : null;
        }

        /// <summary>
        ///     Determines whether the specified badge has badge.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <returns><c>true</c> if the specified badge has badge; otherwise, <c>false</c>.</returns>
        internal bool HasBadge(string badge)
        {
            return BadgeList.Contains(badge);
        }

        /// <summary>
        ///     Gives the badge.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="inDatabase">if set to <c>true</c> [in database].</param>
        /// <param name="session">The session.</param>
        /// <param name="wiredReward">if set to <c>true</c> [wired reward].</param>
        internal void GiveBadge(string badge, bool inDatabase, GameClient session, bool wiredReward = false)
        {
            if (wiredReward)
                session.SendMessage(SerializeBadgeReward(!HasBadge(badge)));

            if (HasBadge(badge))
                return;

            if (inDatabase)
            {
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(
                        string.Concat("INSERT INTO users_badges (user_id,badge_id,badge_slot) VALUES (", _userId,
                            ",@badge,", 0, ")"));
                    queryReactor.AddParameter("badge", badge);
                    queryReactor.RunQuery();
                }
            }

            BadgeList.Add(badge, new Badge(badge, 0));

            session.SendMessage(SerializeBadge(badge));
            session.SendMessage(Update(badge));
        }

        /// <summary>
        ///     Serializes the badge.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeBadge(string badge)
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("ReceiveBadgeMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendString(badge);
            return serverMessage;
        }

        /// <summary>
        ///     Serializes the badge reward.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeBadgeReward(bool success)
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("WiredRewardAlertMessageComposer"));
            serverMessage.AppendInteger(success ? 7 : 1);
            return serverMessage;
        }

        /// <summary>
        ///     Resets the slots.
        /// </summary>
        internal void ResetSlots()
        {
            /* TODO CHECK */ foreach (Badge badge in BadgeList.Values)
                badge.Slot = 0;
        }

        /// <summary>
        ///     Removes the badge.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="session">The session.</param>
        internal void RemoveBadge(string badge, GameClient session)
        {
            if (!HasBadge(badge))
                return;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("DELETE FROM users_badges WHERE badge_id = @badge AND user_id = " + _userId);
                queryReactor.AddParameter("badge", badge);
                queryReactor.RunQuery();
            }

            BadgeList.Remove(GetBadge(badge));
            session.SendMessage(Serialize());
        }

        /// <summary>
        ///     Updates the specified badge identifier.
        /// </summary>
        /// <param name="badgeId">The badge identifier.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage Update(string badgeId)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(4);
            serverMessage.AppendInteger(1);
            serverMessage.AppendString(badgeId);
            return serverMessage;
        }

        /// <summary>
        ///     Serializes this instance.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage Serialize()
        {
            var list = new List<Badge>();
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadBadgesWidgetMessageComposer"));
            serverMessage.AppendInteger(Count);

            /* TODO CHECK */ foreach (Badge badge in BadgeList.Values)
            {
                serverMessage.AppendInteger(1);
                serverMessage.AppendString(badge.Code);

                if (badge.Slot > 0)
                    list.Add(badge);
            }

            serverMessage.AppendInteger(list.Count);

            /* TODO CHECK */ foreach (var current in list)
            {
                serverMessage.AppendInteger(current.Slot);
                serverMessage.AppendString(current.Code);
            }

            return serverMessage;
        }
        
    }
}