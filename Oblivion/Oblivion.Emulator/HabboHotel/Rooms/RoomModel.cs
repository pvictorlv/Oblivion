using System;
using System.Globalization;
using Oblivion.HabboHotel.Rooms.Chat.Enums;

namespace Oblivion.HabboHotel.Rooms
{
    /// <summary>
    ///     Class RoomModel.
    /// </summary>
    internal class RoomModel
    {
        private const string Letters = "abcdefghijklmnopqrstuvw";

        /// <summary>
        ///     The club only
        /// </summary>
        internal bool ClubOnly;

        /// <summary>
        ///     The door orientation
        /// </summary>
        internal int DoorOrientation;

        /// <summary>
        ///     The door x
        /// </summary>
        internal int DoorX;

        /// <summary>
        ///     The door y
        /// </summary>
        internal int DoorY;

        /// <summary>
        ///     The door z
        /// </summary>
        internal double DoorZ;

        /// <summary>
        ///     The heightmap
        /// </summary>
        internal string Heightmap;

        /// <summary>
        ///     The map size x
        /// </summary>
        internal int MapSizeX;

        /// <summary>
        ///     The map size y
        /// </summary>
        internal int MapSizeY;

        /// <summary>
        ///     The sq character
        /// </summary>
        internal char[][] SqChar;

        /// <summary>
        ///     The sq floor height
        /// </summary>
        internal short[][] SqFloorHeight;

        /// <summary>
        ///     The sq seat rot
        /// </summary>
        internal byte[][] SqSeatRot;

        /// <summary>
        ///     The sq state
        /// </summary>
        internal SquareState[][] SqState;

        /// <summary>
        ///     The static furni map
        /// </summary>
        internal string StaticFurniMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomModel" /> class.
        /// </summary>
        /// <param name="doorX">The door x.</param>
        /// <param name="doorY">The door y.</param>
        /// <param name="doorZ">The door z.</param>
        /// <param name="doorOrientation">The door orientation.</param>
        /// <param name="heightmap">The heightmap.</param>
        /// <param name="staticFurniMap">The static furni map.</param>
        /// <param name="clubOnly">if set to <c>true</c> [club only].</param>
        internal RoomModel(int doorX, int doorY, double doorZ, int doorOrientation, string heightmap, string staticFurniMap, bool clubOnly)
        {
            try
            {
                DoorX = doorX;
                DoorY = doorY;
                DoorZ = doorZ;
                DoorOrientation = doorOrientation;
                Heightmap = heightmap.ToLower();
                StaticFurniMap = staticFurniMap;

                heightmap = heightmap.Replace($"{Convert.ToChar(10)}", string.Empty);

                var array = heightmap.Split(Convert.ToChar(13));

                MapSizeX = array[0].Length;
                MapSizeY = array.Length;
                ClubOnly = clubOnly;

                SqState = new SquareState[MapSizeX][];

                for (var i = 0; i < MapSizeX; i++)
                    SqState[i] = new SquareState[MapSizeY];

                SqFloorHeight = new short[MapSizeX][];

                for (var i = 0; i < MapSizeX; i++)
                    SqFloorHeight[i] = new short[MapSizeY];

                SqSeatRot = new byte[MapSizeX][];

                for (var i = 0; i < MapSizeX; i++)
                    SqSeatRot[i] = new byte[MapSizeY];

                SqChar = new char[MapSizeX][];

                for (var i = 0; i < MapSizeX; i++)
                    SqChar[i] = new char[MapSizeY];

                for (var y = 0; y < MapSizeY; y++)
                {
                    var text2 = array[y].Replace($"{Convert.ToChar(13)}", string.Empty).Replace($"{Convert.ToChar(10)}", string.Empty);

                    for (var x = 0; x < MapSizeX; x++)
                    {
                        char c = 'x';

                        if(x < text2.Length)
                            c = (char)text2[x];

                        if (x == doorX && y == doorY)
                        {
                            SqFloorHeight[x][y] = (short) DoorZ;
                            SqState[x][y] = SquareState.Open;

                            if (SqFloorHeight[x][y] > 9)
                                SqChar[x][y] = Letters[(SqFloorHeight[x][y] - 10)];
                            else
                                SqChar[x][y] = char.Parse(DoorZ.ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            if (c.Equals('x'))
                            {
                                SqFloorHeight[x][y] = -1;
                                SqState[x][y] = SquareState.Blocked;
                                SqChar[x][y] = c;
                            }
                            else
                            {
                                SqFloorHeight[x][y] = char.IsDigit(c)
                                    ? short.Parse(c.ToString())
                                    : Convert.ToInt16(Letters.IndexOf(char.ToLower(c)) + 10);
                                SqState[x][y] = SquareState.Open;
                                SqChar[x][y] = c;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Writer.Writer.LogCriticalException(e.ToString());
            }
        }
    }
}