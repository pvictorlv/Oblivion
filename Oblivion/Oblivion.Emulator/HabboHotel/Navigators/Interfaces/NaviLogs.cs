﻿namespace Oblivion.HabboHotel.Navigators.Interfaces
{
    /// <summary>
    ///     Struct NaviLogs
    /// </summary>
    internal struct NaviLogs
    {
        /// <summary>
        ///     The identifier
        /// </summary>
        internal int Id;

        /// <summary>
        ///     The value1
        /// </summary>
        internal string Value1;

        /// <summary>
        ///     The value2
        /// </summary>
        internal string Value2;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NaviLogs" /> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        internal NaviLogs(int id, string value1, string value2)
        {
            Id = id;
            Value1 = value1;
            Value2 = value2;
        }
    }
}