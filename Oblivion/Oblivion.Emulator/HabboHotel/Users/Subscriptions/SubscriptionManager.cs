using System;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Users.UserDataManagement;

namespace Oblivion.HabboHotel.Users.Subscriptions
{
    /// <summary>
    ///     Class SubscriptionManager.
    /// </summary>
    internal class SubscriptionManager
    {
        /// <summary>
        ///     The _user identifier
        /// </summary>
        private readonly uint _userId;

        /// <summary>
        ///     The _subscription
        /// </summary>
        private Subscription _subscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SubscriptionManager" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userData">The user data.</param>
        internal SubscriptionManager(uint userId, UserData userData)
        {
            _userId = userId;
            _subscription = userData.Subscriptions;
        }

        /// <summary>
        ///     Gets a value indicating whether this instance has subscription.
        /// </summary>
        /// <value><c>true</c> if this instance has subscription; otherwise, <c>false</c>.</value>
        internal bool HasSubscription => _subscription != null && _subscription.IsValid;

        /// <summary>
        ///     Gets the subscription.
        /// </summary>
        /// <returns>Subscription.</returns>
        internal Subscription GetSubscription()
        {
            return _subscription;
        }

        /// <summary>
        ///     Adds the subscription.
        /// </summary>
        /// <param name="dayLength">Length of the day.</param>
        internal async Task AddSubscription(double dayLength)
        {
            var num = ((int) Math.Round(dayLength));

            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(_userId);
            DateTime target;
            int num2;
            int num3;

            if (_subscription != null)
            {
                target = Oblivion.UnixToDateTime(_subscription.ExpireTime).AddDays(num);
                num2 = _subscription.ActivateTime;
                num3 = _subscription.LastGiftTime;
            }
            else
            {
                target = DateTime.Now.AddDays(num);
                num2 = Oblivion.GetUnixTimeStamp();
                num3 = Oblivion.GetUnixTimeStamp();
            }

            var num4 = Oblivion.DateTimeToUnix(target);
            _subscription = new Subscription(2, num2, num4, num3);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(string.Concat("REPLACE INTO users_subscriptions VALUES (", _userId, ", 2, ",
                    num2, ", ", num4, ", ", num3, ");"));

            await clientByUserId.GetHabbo().SerializeClub();
            await Oblivion.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(clientByUserId);
        }

        /// <summary>
        ///     Reloads the subscription.
        /// </summary>
        internal async Task ReloadSubscription()
        {
            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery(
                    "SELECT * FROM users_subscriptions WHERE user_id=@id AND timestamp_expire > UNIX_TIMESTAMP() ORDER BY subscription_id DESC LIMIT 1");
                queryReactor.AddParameter("id", _userId);

                var row = queryReactor.GetRow();

                _subscription = row == null
                    ? null
                    : new Subscription((int) row[1], (int) row[2], (int) row[3], (int) row[4]);
            }
            
        }


        public void Dispose()
        {
            _subscription = null;
        }
    }
}