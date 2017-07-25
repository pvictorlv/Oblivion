using System;

namespace Oblivion.HabboHotel.Users.UserDataManagement
{
    /// <summary>
    ///     Class UserDataNotFoundException.
    /// </summary>
    internal class UserDataNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserDataNotFoundException" /> class.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public UserDataNotFoundException(string reason) : base(reason)
        {
        }
    }
}