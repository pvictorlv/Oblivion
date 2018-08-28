using Oblivion.Util;

namespace Oblivion.HabboHotel.Items
{
    /// <summary>
    ///     Class WallCoordinate.
    /// </summary>
    internal class WallCoordinate
    {
        /// <summary>
        ///     The _length x
        /// </summary>
        private readonly int _lengthX;

        /// <summary>
        ///     The _length y
        /// </summary>
        private readonly int _lengthY;

        /// <summary>
        ///     The _side
        /// </summary>
        private readonly char _side;

        /// <summary>
        ///     The _width x
        /// </summary>
        private readonly int _widthX;

        /// <summary>
        ///     The _width y
        /// </summary>
        private readonly int _widthY;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WallCoordinate" /> class.
        /// </summary>
        /// <param name="wallPosition">The wall position.</param>
        public WallCoordinate(string wallPosition)
        {
            var posD = wallPosition.Split(' ');

            _side = posD[2] == "l" ? 'l' : 'r';

            var widD = posD[0].Substring(3).Split(',');

            _widthX = TextHandling.Parse(widD[0]);
            _widthY = TextHandling.Parse(widD[1]);

            var lenD = posD[1].Substring(2).Split(',');

            _lengthX = TextHandling.Parse(lenD[0]);
            _lengthY = TextHandling.Parse(lenD[1]);
        }

        

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() => ":w=" + _widthX + "," + _widthY + " " + "l=" + _lengthX + "," + _lengthY + " " + _side;

      
    }
}