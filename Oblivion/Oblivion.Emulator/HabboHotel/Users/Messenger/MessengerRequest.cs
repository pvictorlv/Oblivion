using Oblivion.Messages;

namespace Oblivion.HabboHotel.Users.Messenger
{
    /// <summary>
    ///     Class MessengerRequest.
    /// </summary>
    internal class MessengerRequest
    {
        /// <summary>
        ///     The _user look
        /// </summary>
        private string _look;

        /// <summary>
        ///     The _user name
        /// </summary>
        private string _userName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessengerRequest" /> class.
        /// </summary>
        /// <param name="toUser">To user.</param>
        /// <param name="fromUser">From user.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="look"></param>
        internal MessengerRequest(ulong toUser, ulong fromUser, string userName, string look)
        {
            To = toUser;
            From = fromUser;
            _userName = userName;
            _look = look;
        }

        /// <summary>
        ///     Gets to.
        /// </summary>
        /// <value>To.</value>
        internal ulong To { get; private set; }

        /// <summary>
        ///     Gets from.
        /// </summary>
        /// <value>From.</value>
        internal ulong From { get; }

        /// <summary>
        ///     Serializes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        internal void Serialize(ServerMessage request)
        {
            var virtualId = Oblivion.GetGame().GetClientManager().GetVirtualId(From);

            request.AppendInteger(virtualId);
            request.AppendString(_userName);
            request.AppendString(_look);
        }

        public void Dispose()
        {
            _look = null;
            _userName = null;
        }
    }
}