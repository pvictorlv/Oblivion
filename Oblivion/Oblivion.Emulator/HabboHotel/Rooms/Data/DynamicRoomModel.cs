using System;
using System.Text;
using Oblivion.HabboHotel.Rooms.Chat.Enums;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Rooms.Data
{
    /// <summary>
    ///     Class DynamicRoomModel.
    /// </summary>
    internal class DynamicRoomModel
    {
        /// <summary>
        ///     The _m room
        /// </summary>
        private Room _mRoom;

        /// <summary>
        ///     The _serialized heightmap
        /// </summary>
        private ServerMessage _serializedHeightmap;

        /// <summary>
        ///     The _static model
        /// </summary>
        private RoomModel _staticModel;

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
        ///     The heightmap serialized
        /// </summary>
        internal bool HeightmapSerialized;

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
        ///     Initializes a new instance of the <see cref="DynamicRoomModel" /> class.
        /// </summary>
        /// <param name="pModel">The p model.</param>
        /// <param name="room">The room.</param>
        internal DynamicRoomModel(RoomModel pModel, Room room)
        {
            _staticModel = pModel;
            DoorX = _staticModel.DoorX;
            DoorY = _staticModel.DoorY;
            DoorZ = _staticModel.DoorZ;
            DoorOrientation = _staticModel.DoorOrientation;
            Heightmap = _staticModel.Heightmap;
            MapSizeX = _staticModel.MapSizeX;
            MapSizeY = _staticModel.MapSizeY;
            ClubOnly = _staticModel.ClubOnly;
            _mRoom = room;
            Generate();
        }

        /// <summary>
        ///     Generates this instance.
        /// </summary>
        internal void Generate()
        {
            SqState = new SquareState[MapSizeX][];
            for (var i = 0; i < MapSizeX; i++) SqState[i] = new SquareState[MapSizeY];
            SqFloorHeight = new short[MapSizeX][];
            for (var i = 0; i < MapSizeX; i++) SqFloorHeight[i] = new short[MapSizeY];
            SqSeatRot = new byte[MapSizeX][];
            for (var i = 0; i < MapSizeX; i++) SqSeatRot[i] = new byte[MapSizeY];
            SqChar = new char[MapSizeX][];
            for (var i = 0; i < MapSizeX; i++) SqChar[i] = new char[MapSizeY];
            for (var i = 0; i < MapSizeY; i++)
            {
                for (var j = 0; j < MapSizeX; j++)
                {
                    if (j > _staticModel.MapSizeX - 1 || i > _staticModel.MapSizeY - 1)
                        SqState[j][i] = SquareState.Blocked;
                    else
                    {
                        SqState[j][i] = _staticModel.SqState[j][i];
                        SqFloorHeight[j][i] = _staticModel.SqFloorHeight[j][i];
                        SqSeatRot[j][i] = _staticModel.SqSeatRot[j][i];
                        SqChar[j][i] = _staticModel.SqChar[j][i];
                    }
                }
            }
            HeightmapSerialized = false;
        }

        /// <summary>
        ///     Refreshes the arrays.
        /// </summary>
        internal void RefreshArrays()
        {
            var newSqState = new SquareState[MapSizeX + 1][];
            for (var i = 0; i < MapSizeX; i++) newSqState[i] = new SquareState[MapSizeY + 1];
            var newSqFloorHeight = new short[MapSizeX + 1][];
            for (var i = 0; i < MapSizeX; i++) newSqFloorHeight[i] = new short[MapSizeY + 1];
            var newSqSeatRot = new byte[MapSizeX + 1][];
            for (var i = 0; i < MapSizeX; i++) newSqSeatRot[i] = new byte[MapSizeY + 1];
            var newSqChar = new char[MapSizeX + 1][];
            for (var i = 0; i < MapSizeX; i++) newSqChar[i] = new char[MapSizeY + 1];
            for (var i = 0; i < MapSizeY; i++)
            {
                for (var j = 0; j < MapSizeX; j++)
                {
                    if (j > _staticModel.MapSizeX - 1 || i > _staticModel.MapSizeY - 1)
                        newSqState[j][i] = SquareState.Blocked;
                    else
                    {
                        newSqState[j][i] = _staticModel.SqState[j][i];
                        newSqFloorHeight[j][i] = _staticModel.SqFloorHeight[j][i];
                        newSqSeatRot[j][i] = _staticModel.SqSeatRot[j][i];
                        newSqChar[j][i] = _staticModel.SqChar[j][i];
                    }
                }
            }
            SqState = newSqState;
            SqFloorHeight = newSqFloorHeight;
            SqSeatRot = newSqSeatRot;
        }
     

        /// <summary>
        ///     Sets the state of the update.
        /// </summary>
        internal void SetUpdateState()
        {
            HeightmapSerialized = false;
        }

        /// <summary>
        ///     Gets the heightmap.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GetHeightmap()
        {
            if (HeightmapSerialized) return _serializedHeightmap;
            _serializedHeightmap = SerializeHeightmap();
            HeightmapSerialized = true;
            return _serializedHeightmap;
        }

        /// <summary>
        ///     Adds the x.
        /// </summary>
        internal void AddX()
        {
            {
                MapSizeX++;
                RefreshArrays();
            }
        }

        /// <summary>
        ///     Opens the square.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        internal void OpenSquare(int x, int y, double z)
        {
            if (z > 9.0) z = 9.0;
            if (z < 0.0) z = 0.0;
            SqFloorHeight[x][y] = ((short) z);

            SqState[x][y] = SquareState.Open;
        }

        /// <summary>
        ///     Adds the y.
        /// </summary>
        internal void AddY()
        {
            MapSizeY++;
            RefreshArrays();
        }

        /// <summary>
        ///     Sets the mapsize.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        internal void SetMapsize(int x, int y)
        {
            MapSizeX = x;
            MapSizeY = y;
            RefreshArrays();
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            Array.Clear(SqState, 0, SqState.Length);
            Array.Clear(SqFloorHeight, 0, SqFloorHeight.Length);
            Array.Clear(SqSeatRot, 0, SqSeatRot.Length);
            _staticModel = null;
            _mRoom = null;
            if (_serializedHeightmap != null)
            {
                _serializedHeightmap.Disposable = true;
                _serializedHeightmap.Dispose();
                _serializedHeightmap = null;
            }

            Heightmap = null;
            SqState = null;
            SqFloorHeight = null;
            SqSeatRot = null;
        }

        /// <summary>
        ///     Serializes the heightmap.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        private ServerMessage SerializeHeightmap()
        {
            var serverMessage =
                new ServerMessage(LibraryParser.OutgoingRequest("FloorMapMessageComposer")) {Disposable = false};
            serverMessage.AppendBool(false);
            serverMessage.AppendInteger(_mRoom.RoomData.WallHeight);
            var FloorMap = new StringBuilder();

            for (var y = 0; y < MapSizeY; y++)
            {
                for (var x = 0; x < MapSizeX; x++)
                {
                    if (x == DoorX && y == DoorY)
                    {
                        FloorMap.Append(DoorZ > 9 ? ((char)(87 + DoorZ)).ToString() : DoorZ.ToString());
                        continue;
                    }

                    if (SqState[x][y] == SquareState.Blocked)
                    {
                        FloorMap.Append('x');
                        continue;
                    }

                    double Height = SqFloorHeight[x][y];
                    var Val = Height > 9 ? ((char)(87 + Height)).ToString() : Height.ToString();
                    FloorMap.Append(Val);
                }
                FloorMap.Append(Convert.ToChar(13));
            }

            serverMessage.AppendString(FloorMap.ToString());
            return serverMessage;
        }
    }
}