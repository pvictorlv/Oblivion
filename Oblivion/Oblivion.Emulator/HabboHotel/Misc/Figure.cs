namespace Oblivion.HabboHotel.Misc
{
    /// <summary>
    ///     Struct Figure
    /// </summary>
    internal struct Figure
    {
        /// <summary>
        ///     The part
        /// </summary>
        internal string Part;

        /// <summary>
        ///     The part identifier
        /// </summary>
        internal string PartId;

        /// <summary>
        ///     The gender
        /// </summary>
        internal string Gender;

        /// <summary>
        ///     The colorable
        /// </summary>
        internal string Colorable;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Figure" /> struct.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="partId">The part identifier.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="colorable">The colorable.</param>
        public Figure(string part, string partId, string gender, string colorable)
        {
            Part = part;
            PartId = partId;
            Gender = gender;
            Colorable = colorable;
        }
    }
}