using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Handlers;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Rooms.Chat.Enums;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Rooms.User.Path
{
    /// <summary>
    ///     Class Gamemap.
    /// </summary>
    internal class Gamemap
    {
        /// <summary>
        ///     The _room
        /// </summary>
        private Room _room;

        /// <summary>
        ///     The _user map
        /// </summary>
        private ConcurrentDictionary<int, ConcurrentList<RoomUser>> _userMap;

        /// <summary>
        ///     The diagonal enabled
        /// </summary>
        internal bool DiagonalEnabled;

        /// <summary>
        ///     The guild gates
        /// </summary>
        internal Dictionary<Point, RoomItem> GuildGates;

        /// <summary>
        ///     The serialized floormap
        /// </summary>
        internal ServerMessage SerializedFloormap;

        /// <summary>
        ///     The walkable list
        /// </summary>
        internal HashSet<Point> WalkableList;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Gamemap" /> class.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <exception cref="System.Exception"></exception>
        public Gamemap(Room room)
        {
            _room = room;
            DiagonalEnabled = true;
        
            CoordinatedItems = new ConcurrentDictionary<Point, ConcurrentList<RoomItem>>();
         
            _userMap = new ConcurrentDictionary<int, ConcurrentList<RoomUser>>();
            GuildGates = new Dictionary<Point, RoomItem>();
        }

        public async Task Init()
        {
            var room = _room;
            StaticModel = await Oblivion.GetGame().GetRoomManager().GetModel(room.RoomData.ModelName, room.RoomId);

            if (StaticModel == null)
                throw new ArgumentNullException($"No modeldata found for roomID {room.RoomId}");

            Model = new DynamicRoomModel(StaticModel, room);
            GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
            ItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

            WalkableList = GetWalkablePoints();
        }

        /// <summary>
        ///     Gets the model.
        /// </summary>
        /// <value>The model.</value>
        internal DynamicRoomModel Model { get; private set; }


        public bool ItemCanMove(RoomItem item, Point MoveTo)
        {
            if (item.X == MoveTo.X && item.Y == MoveTo.Y) return true;

            var points = GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, MoveTo.X, MoveTo.Y,
                    item.Rot)
                .Values;

            if (points.Count == 0)
                return true;

            foreach (var Coord in points)
            {
                if (Coord.X >= Model.MapSizeX || Coord.Y >= Model.MapSizeY)
                    return false;

                if (!SquareIsOpen(Coord.X, Coord.Y, false))
                    return false;
                if (!CanRollItemHere(Coord.X, Coord.Y))
                    return false;
                if (SquareHasUsers(Coord.X, Coord.Y))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Gets the static model.
        /// </summary>
        /// <value>The static model.</value>
        internal RoomModel StaticModel { get; private set; }

        /// <summary>
        ///     Gets the effect map.
        /// </summary>
        /// <value>The effect map.</value>
//        internal byte[,] EffectMap { get; private set; }
        /// <summary>
        ///     Gets the coordinated items.
        /// </summary>
        /// <value>The coordinated items.</value>
        internal ConcurrentDictionary<Point, ConcurrentList<RoomItem>> CoordinatedItems { get; private set; }

        /// <summary>
        ///     Gets the game map.
        /// </summary>
        /// <value>The game map.</value>
        internal byte[,] GameMap { get; private set; }

        /// <summary>
        ///     Gets the item height map.
        /// </summary>
        /// <value>The item height map.</value>
        internal double[,] ItemHeightMap { get; private set; }

        /// <summary>
        ///     Determines whether this instance can walk the specified p state.
        /// </summary>
        /// <param name="pState">State of the p.</param>
        /// <param name="pOverride">if set to <c>true</c> [p override].</param>
        /// <returns><c>true</c> if this instance can walk the specified p state; otherwise, <c>false</c>.</returns>
        internal static bool CanWalk(byte pState, bool pOverride) => pOverride || pState > 0;

        /// <summary>
        ///     Gets the affected tiles.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="width">The width.</param>
        /// <param name="posX">The position x.</param>
        /// <param name="posY">The position y.</param>
        /// <param name="rotation">The rotation.</param>g
        /// <returns>Dictionary&lt;System.Int32, ThreeDCoord&gt;.</returns>
        internal static Dictionary<int, ThreeDCoord> GetAffectedTiles(int length, int width,
            int posX, int posY, int rotation)
        {
            var x = 0;
            var pointList = new Dictionary<int, ThreeDCoord>();
            if (length == 1 && width == 1)
                pointList.Add(x++, new ThreeDCoord(posX, posY, 0));
            if (length > 1)
                switch (rotation)
                {
                    case 4:
                    case 0:
                        for (var i = 0; i < length; i++)
                        {
                            pointList.Add(x++, new ThreeDCoord(posX, posY + i, i));
                            for (var j = 1; j < width; j++)
                                pointList.Add(x++, new ThreeDCoord(posX + j, posY + i, (i < j) ? j : i));
                        }

                        break;

                    case 6:
                    case 2:
                        for (var i = 0; i < length; i++)
                        {
                            pointList.Add(x++, new ThreeDCoord(posX + i, posY, i));
                            for (var j = 1; j < width; j++)
                                pointList.Add(x++, new ThreeDCoord(posX + i, posY + j, (i < j) ? j : i));
                        }

                        break;
                }
            if (width <= 1)
                return pointList;
            switch (rotation)
            {
                case 4:
                case 0:
                    for (var i = 0; i < width; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(posX + i, posY, i));
                        for (var j = 1; j < length; j++)
                            pointList.Add(x++, new ThreeDCoord(posX + i, posY + j, (i < j) ? j : i));
                    }

                    break;

                case 6:
                case 2:
                    for (var i = 0; i < width; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(posX, posY + i, i));
                        for (var j = 1; j < length; j++)
                            pointList.Add(x++, new ThreeDCoord(posX + j, posY + i, (i < j) ? j : i));
                    }

                    break;
            }

            return pointList;
        }

        /// <summary>
        ///     Tileses the touching.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool TilesTouching(int x1, int y1, int x2, int y2) => Math.Abs(x1 - x2) <= 1 &&
                                                                              Math.Abs(y1 - y2) <= 1 ||
                                                                              (x1 == x2 && y1 == y2);

        /// <summary>
        ///     Tiles the distance.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>System.Int32.</returns>
        internal static int TileDistance(int x1, int y1, int x2, int y2) => (Math.Abs(x1 - x2) + Math.Abs(y1 - y2));

        /// <summary>
        ///     Adds the user to map.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="coord">The coord.</param>
        internal void AddUserToMap(RoomUser user, Point coord)
        {
            var coordKey = Formatter.PointToInt(coord);
            if (_userMap.TryGetValue(coordKey, out var users))
            {
                users.Add(user);
                //                _userMap[coordKey] = users;
            }
            else
            {
                users = new ConcurrentList<RoomUser> {user};
                _userMap.TryAdd(coordKey, users);
            }
        }

        /// <summary>
        ///     Teleports to item.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="item">The item.</param>
        internal async Task TeleportToItem(RoomUser user, RoomItem item, bool fromWired = false)
        {
            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.X, user.Y),
                new Point(item.X, item.Y), user);
            user.X = item.X;
            user.Y = item.Y;
            user.Z = item.Z;
            user.SqState = GameMap[item.X, item.Y];
            GameMap[item.X, item.Y] = 1;
            user.RotBody = item.Rot;
            user.RotHead = item.Rot;
            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
            if (!fromWired)
                await item.UserWalksOnFurni(user);
        }

        /// <summary>
        ///     Updates the user movement.
        /// </summary>
        /// <param name="oldCoord">The old coord.</param>
        /// <param name="newCoord">The new coord.</param>
        /// <param name="user">The user.</param>
        internal void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        /// <summary>
        ///     Removes the user from map.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="coord">The coord.</param>
        internal void RemoveUserFromMap(RoomUser user, Point coord)
        {
            var coordKey = Formatter.PointToInt(coord);
            if (_userMap.TryGetValue(coordKey, out var users))
            {
                if (users.Contains(user))
                    users.Remove(user);

                if (users.Count <= 0)
                {
                    _userMap.TryRemove(coordKey, out _);
                }
            }
        }

        /// <summary>
        ///     Gets the room users.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>List&lt;RoomUser&gt;.</returns>
        internal ConcurrentList<RoomUser> GetRoomUsers(Point coord)
        {
            var coordKey = Formatter.PointToInt(coord);
            if (_userMap.TryGetValue(coordKey, out var users))
            {
                return users;
            }

            return new ConcurrentList<RoomUser>();
        }


        /// <summary>
        ///     Gets the random walkable square.
        /// </summary>
        /// <returns>Point.</returns>
        internal Point GetRandomWalkableSquare()
        {
            if (WalkableList.Count <= 0)
                return new Point(0, 0);

            var randomNumber = Oblivion.GetRandomNumber(0, WalkableList.Count);
            var num = 0;
            /* TODO CHECK */
            foreach (var current in WalkableList)
            {
                if (num == randomNumber)
                    return current;
                num++;
            }

            return new Point(0, 0);
        }


        /// <summary>
        ///     Adds to map.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void AddToMap(RoomItem item)
        {
            AddItemToMap(item);
        }

        /// <summary>
        ///     Updates the map for item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal async Task UpdateMapForItem(RoomItem item)
        {
            await RemoveFromMap(item, false);
            AddToMap(item);
        }

        /// <summary>
        ///     Generates the maps.
        /// </summary>
        /// <param name="checkLines">if set to <c>true</c> [check lines].</param>
        internal void GenerateMaps(bool checkLines = true)
        {
            try
            {
                while (true)
                {
                   // if (Model == null)
                   //     return;

                    var xMap = 0;
                    var yMap = 0;

                    CoordinatedItems.Clear();
                    var floorItems = _room.GetRoomItemHandler().FloorItems.Values;
                    if (checkLines)
                    {
                        /* TODO CHECK */
                        foreach (var roomItems in floorItems)
                        {
                            if (roomItems.X > Model.MapSizeX && roomItems.X > xMap)
                                xMap = roomItems.X;
                            if (roomItems.Y > Model.MapSizeY && roomItems.Y > yMap)
                                yMap = roomItems.Y;
                        }
                    }

                    if (yMap > Model.MapSizeY - 1 || xMap > Model.MapSizeX - 1)
                    {
                        if (xMap < Model.MapSizeX)
                            xMap = Model.MapSizeX;
                        if (yMap < Model.MapSizeY)
                            yMap = Model.MapSizeY;
                        Model.SetMapsize(xMap + 7, yMap + 7);

                        checkLines = false;

                        continue;
                    }

                    if (xMap != StaticModel.MapSizeX || yMap != StaticModel.MapSizeY)
                    {
                        GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
                        ItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                        for (var j = 0; j < Model.MapSizeY; j++)
                        for (var k = 0; k < Model.MapSizeX; k++)
                        {
                            //3 = door; 2 = seat; 1 = open; 0 = block
                            if (k == Model.DoorX && j == Model.DoorY)
                                GameMap[k, j] = 3;
                            else if (Model.SqState[k][j] == SquareState.Open)
                                GameMap[k, j] = 1;
                            else if (Model.SqState[k][j] == SquareState.Seat)
                                GameMap[k, j] = 2;
                            else
                                GameMap[k, j] = 0;
                        }
                    }

                    else
                    {
                        GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
                        ItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                        for (var n = 0; n < Model.MapSizeY; n++)
                        {
                            for (var num3 = 0; num3 < Model.MapSizeX; num3++)
                            {
                                if (num3 == Model.DoorX && n == Model.DoorY)
                                {
                                    GameMap[num3, n] = 3;
                                }
                                else if (Model.SqState[num3][n] == SquareState.Open)
                                    GameMap[num3, n] = 1;
                                else if (Model.SqState[num3][n] == SquareState.Seat)
                                {
                                    GameMap[num3, n] = 2;
                                }
                                else
                                    GameMap[num3, n] = 0;
                            }
                        }
                    }


                    var doors = new List<RoomItem>();
                    foreach (var item in floorItems)
                    {
                        AddItemToMap(item);
                        if (item.GetBaseItem().InteractionType == Interaction.Gate)
                        {
                            doors.Add(item);
                        }
                    }

                    foreach (var item in doors)
                    {
                        if (item.ExtraData == "0")
                            GameMap[item.X, item.Y] = 0;
                    }

                    if (!_room.RoomData.AllowWalkThrough)
                    {
                        /* TODO CHECK */
                        foreach (var current in _room.GetRoomUserManager().UserList.Values)
                        {
                            current.SqState = GameMap[current.X, current.Y];
                            GameMap[current.X, current.Y] = 0;
                        }
                    }

                    break;
                }
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception RoomData Loading on (GenerateMaps): " + ex);
                Logging.HandleException(ex, "Oblivion.HabboHotel.Rooms.Gamemap");
            }
        }

        /// <summary>
        ///     Adds the coordinated item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="coord">The coord.</param>
        internal void AddCoordinatedItem(RoomItem item, Point coord)
        {
            if (item == null) return;
            if (!CoordinatedItems.TryGetValue(coord, out var items))
            {
                items = new ConcurrentList<RoomItem> {item};

                CoordinatedItems.TryAdd(coord, items);
            }
            else
            {
                if (items.Count > 0)
                {
                    if (items.Contains(item))
                        return;
                }

                items.Add(item);
            }
        }

        /// <summary>
        ///     Gets the coordinated items.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal ConcurrentList<RoomItem> GetCoordinatedItems(Point coord) =>
            !CoordinatedItems.TryGetValue(coord, out var items) ? new ConcurrentList<RoomItem>() : items;


        internal ConcurrentList<RoomItem> GetCoordinatedHeighestItems(Point coord)
        {
            if (!CoordinatedItems.TryGetValue(coord, out var items))
            {
                return new ConcurrentList<RoomItem>();
            }


            if (items.Count == 1)
                return items;
            var returnItems = new ConcurrentList<RoomItem>();
            double heighest = -1;

            /* TODO CHECK */
            foreach (var i in items)
            {
                if (i == null) continue;
                if (i.TotalHeight > heighest)
                {
                    heighest = i.Z;
                    returnItems.Clear();
                    returnItems.Add(i);
                }
                else if (i.TotalHeight == heighest)
                    returnItems.Add(i);
            }

            return returnItems;
        }

        internal bool HasHeightestItem(Point coord, double itemZ)
        {
            if (!CoordinatedItems.TryGetValue(coord, out var items))
            {
                return false;
            }


            if (items.Count == 1)
                return false;

            foreach (var item in items)
            {
                if (item.Z > itemZ)
                    return true;
            }

            return false;
        }


        internal async Task<bool> RemoveFromMap(RoomItem item, bool removed)
        {
            RemoveSpecialItem(item, removed);
            if (_room.GotSoccer())
                _room.GetSoccer().OnGateRemove(item);
            var result = false;
            var hybridDictionary = new Dictionary<Point, ConcurrentList<RoomItem>>();
            foreach (var current2 in item.GetCoords())
            {
                if (CoordinatedItems.TryGetValue(current2, out var value))
                {
                    if (value != null)
                    {
                        value.Remove(item);
                        result = true;
                    }

                    if (!hybridDictionary.ContainsKey(current2))
                        hybridDictionary.Add(current2, value);
                }

                SetDefaultValue(current2.X, current2.Y);
            }

            foreach (var point2 in hybridDictionary)
            {
                var list = point2.Value;
                foreach (var current3 in list)
                    ConstructMapForItem(current3, point2.Key);
            }

            GuildGates.Remove(item.Coordinate);
            await _room.GetRoomItemHandler().OnHeightMapUpdate(hybridDictionary.Keys);
            hybridDictionary.Clear();

            return result;
        }

        /// <summary>
        ///     Removes from map.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal async Task<bool> RemoveFromMap(RoomItem item) => await RemoveFromMap(item, true);

        /// <summary>
        ///     Adds the item to map.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="handleGameItem">if set to <c>true</c> [handle game item].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool AddItemToMap(RoomItem item, bool handleGameItem = true)
        {
            if (handleGameItem)
            {
                AddSpecialItems(item);
                var interactionType = item.GetBaseItem().InteractionType;
                if (interactionType != Interaction.Roller)
                {
                    if (interactionType == Interaction.FootballGoalGreen || interactionType ==
                                                                         Interaction.FootballGoalYellow
                                                                         || interactionType ==
                                                                         Interaction.FootballGoalBlue
                                                                         || interactionType ==
                                                                         Interaction.FootballGoalRed)
                    {
                        if (_room.GotSoccer())
                        {
                            _room.GetSoccer().RegisterGate(item);
                        }
                    }

                    switch (interactionType)
                    {
                        case Interaction.FootballGoalGreen:
                        case Interaction.FootballCounterGreen:
                        case Interaction.BanzaiGateGreen:
                        case Interaction.BanzaiScoreGreen:
                        case Interaction.FreezeGreenCounter:
                        case Interaction.FreezeGreenGate:
                            _room.GetGameManager().AddFurnitureToTeam(item, Team.Green);
                            break;

                        case Interaction.FootballGoalYellow:
                        case Interaction.FootballCounterYellow:
                        case Interaction.BanzaiGateYellow:
                        case Interaction.BanzaiScoreYellow:
                        case Interaction.FreezeYellowCounter:
                        case Interaction.FreezeYellowGate:
                            _room.GetGameManager().AddFurnitureToTeam(item, Team.Yellow);
                            break;

                        case Interaction.FootballGoalBlue:
                        case Interaction.FootballCounterBlue:
                        case Interaction.BanzaiGateBlue:
                        case Interaction.BanzaiScoreBlue:
                        case Interaction.FreezeBlueCounter:
                        case Interaction.FreezeBlueGate:
                            _room.GetGameManager().AddFurnitureToTeam(item, Team.Blue);
                            break;

                        case Interaction.FootballGoalRed:
                        case Interaction.FootballCounterRed:
                        case Interaction.BanzaiGateRed:
                        case Interaction.BanzaiScoreRed:
                        case Interaction.FreezeRedCounter:
                        case Interaction.FreezeRedGate:

                            _room.GetGameManager().AddFurnitureToTeam(item, Team.Red);
                            break;

                        case Interaction.FreezeExit:
                            _room.GetFreeze().ExitTeleport = item;
                            break;

                        case Interaction.GuildGate:
                        {
                            if (!GuildGates.ContainsKey(item.Coordinate))
                            {
                                GuildGates.Add(item.Coordinate, item);
                                GameMap[item.X, item.Y] = 0;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    if (!_room.GetRoomItemHandler().Rollers.Contains(item))
                        _room.GetRoomItemHandler().Rollers.Add(item);
                }
            }

            if (item.GetBaseItem().Type != 's')
                return true;


            if (item.X > Model.MapSizeX - 1)
            {
                Model.AddX();
                GenerateMaps();
                return false;
            }

            if (item.Y > Model.MapSizeY - 1)
            {
                Model.AddY();
                GenerateMaps();
                return false;
            }

            var retVal = true;

            foreach (var current in item.GetCoords())
            {
                //                var coord = new Point(current.X, current.Y);
                AddCoordinatedItem(item, current);
                retVal = ConstructMapForItem(item, current);
            }

            return retVal;
        }

        /// <summary>
        ///     Determines whether this instance can walk the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="Override">if set to <c>true</c> [override].</param>
        /// <param name="horseId">The horse identifier.</param>
        /// <returns><c>true</c> if this instance can walk the specified x; otherwise, <c>false</c>.</returns>
        internal bool CanWalk(int x, int y, bool Override, uint horseId = 0u) =>
            _room.RoomData.AllowWalkThrough || Override ||
            GetRoomUsers(new Point(x, y)).Count <= 0;

        /// <summary>
        ///     Gets the floor status.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>System.Byte.</returns>
        internal byte GetFloorStatus(Point coord)
        {
            if (coord.X > GameMap.GetUpperBound(0) || coord.Y > GameMap.GetUpperBound(1))
                return 1;
            return GameMap[coord.X, coord.Y];
        }

        /// <summary>
        ///     Gets the height for square from data.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>System.Double.</returns>
        internal double GetHeightForSquareFromData(Point coord)
        {
            try
            {
                var high = Model.SqFloorHeight.GetUpperBound(0);
                if (coord.X > high || coord.Y > Model.SqFloorHeight[high].GetUpperBound(0))
                    return 1.0;
                return Model.SqFloorHeight[coord.X][coord.Y];
            }
            catch (Exception)
            {
                return 1.0;
            }
        }

        /// <summary>
        ///     Determines whether this instance [can roll item here] the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if this instance [can roll item here] the specified x; otherwise, <c>false</c>.</returns>
        internal bool CanRollItemHere(int x, int y) => ValidTile(x, y) && Model.SqState[x][y] != SquareState.Blocked;


        /// <summary>
        ///     Squares the is open.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="pOverride">if set to <c>true</c> [p override].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SquareIsOpen(int x, int y, bool pOverride) =>
            (Model.MapSizeX - 1 >= x && Model.MapSizeY - 1 >= y) && CanWalk(GameMap[x, y], pOverride) &&
            (pOverride || _room.RoomData.AllowWalkThrough || GetRoomUsers(new Point(x, y)).Count <= 0);


        /// <summary>
        ///     Gets the random valid walkable square.
        /// </summary>
        /// <returns>Point.</returns>
        internal Point GetRandomValidWalkableSquare()
        {
            var walkableSquares = new List<Point>();
            for (var y = 0; y < GameMap.GetUpperBound(1) - 1; y++)
            for (var x = 0; x < GameMap.GetUpperBound(0) - 1; x++)
                if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                    walkableSquares.Add(new Point(x, y));
            var randomNumber = Oblivion.GetRandomNumber(0, walkableSquares.Count);
            var i = 0;
            foreach (var coord in walkableSquares)
            {
                if (i == randomNumber)
                    return coord;
                i++;
            }

            return new Point(0, 0);
        }

        /// <summary>
        ///     Determines whether [is valid step] [the specified user].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="endOfPath">if set to <c>true</c> [end of path].</param>
        /// <param name="Override">if set to <c>true</c> [override].</param>
        /// <returns><c>true</c> if [is valid step] [the specified user]; otherwise, <c>false</c>.</returns>
        internal async Task<bool> IsValidStep(RoomUser user, Vector2D from, Vector2D to, bool endOfPath, bool Override,
            bool checkDiag, bool generating)
        {
            if (user == null)
                return false;

            if (Override)
                return true;

            if (!ValidTile(to.X, to.Y))
                return false;


            var square = new Point(to.X, to.Y);
            if (user.IsBot == false && user.GetClient() != null)
            {
                if (GuildGates.TryGetValue(square, out var guild))
                {
                    var guildId = guild.GroupId;
                    if (guildId > 0 &&
                        user.GetClient()
                            .GetHabbo()
                            .UserGroups.Any(member => member != null && member.GroupId == guildId))
                        return true;
                }
            }

            var xLen = GameMap.GetLength(0);
            var yLen = GameMap.GetLength(1);

            if (checkDiag)
            {
                int xValue = to.X - from.X;
                int yValue = to.Y - from.Y;

                if (xValue == -1 && yValue == -1)
                {
                    if (xLen <= to.X + 1 || yLen <= to.Y + 1)
                    {
                        return false;
                    }

                    var sqState = GameMap[to.X + 1, to.Y + 1];
                    if (sqState != 1 && sqState != 2)
                        return false;
                }
                else if (xValue == 1 && yValue == -1)
                {
                    if (xLen <= to.X - 1 || yLen <= to.Y + 1)
                    {
                        return false;
                    }

                    var sqState = GameMap[to.X - 1, to.Y + 1];

                    if (sqState != 1 && sqState != 2)
                        return false;
                }
                else if (xValue == 1 && yValue == 1)
                {
                    if (xLen <= to.X - 1 || yLen <= to.Y - 1)
                    {
                        return false;
                    }

                    var sqState = GameMap[to.X - 1, to.Y - 1];

                    if (sqState != 1 && sqState != 2)
                        return false;
                }
                else if (xValue == -1 && yValue == 1)
                {
                    if (xLen <= to.X + 1 || yLen <= to.Y - 1)
                    {
                        return false;
                    }

                    var sqState = GameMap[to.X + 1, to.Y - 1];

                    if (sqState != 1 && sqState != 2)
                        return false;
                }
            }

            if (!TileIsWalkable(to.X, to.Y, endOfPath))
            {
                if (!generating && !endOfPath)
                    user.PathRecalcNeeded = true;
            }


            if (xLen > to.X && yLen > to.Y)
            {
                var sqState = GameMap[to.X, to.Y];
                if ((sqState == 2 && !endOfPath) || sqState == 0)
                {
                    if (sqState == 0)
                    {
                        return false;
                    }

                    return false;
                }
            }

            var squaseHasUser = GetRoomUsers(new Point(to.X, to.Y)).Count > 0;

            if (squaseHasUser && endOfPath && !_room.RoomData.AllowWalkThrough)
            {
                user.Path.Clear();
                user.IsWalking = false;
                user.RemoveStatus("mv");
                await _room.GetRoomUserManager().UpdateUserStatus(user, false);
                if (!user.RidingHorse || user.IsPet || user.IsBot)
                    return generating;
                var roomUserByVirtualId =
                    _room.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(user.HorseId));
                using (var message =
                    new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer")))
                {
                    message.AppendInteger(1);
                    if (roomUserByVirtualId != null)
                    {
                        roomUserByVirtualId.IsWalking = false;
                        roomUserByVirtualId.ClearMovement();
                        await roomUserByVirtualId.SerializeStatus(message, "");
                    }

                    await user.GetClient().GetHabbo().CurrentRoom.SendMessage(message);
                }
            }
            else if (squaseHasUser && !_room.RoomData.AllowWalkThrough)
            {
                return false;
            }

            return SqAbsoluteHeight(to.X, to.Y) - SqAbsoluteHeight(from.X, from.Y) <= 1.5;
        }

        public bool TileIsWalkable(int pX, int pY, bool endPath = false)
        {
            if (SquareHasUsers(pX, pY))
            {
                if (endPath)
                    return false;

                if (_room?.RoomData == null) return false;

                if (!_room.RoomData.AllowWalkThrough)
                    return false;
            }

            if (Model.SqState == null) return false;

            if (pX >= Model.SqState.Length)
            {
                return false;
            }

            if (pY >= Model.SqState[pX].Length)
            {
                return false;
            }

            return Model.SqState[pX][pY] == SquareState.Open;
        }


        /// <summary>
        ///     Valids the tile2.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ValidTile2(int x, int y)
        {
            if (!ValidTile(x, y))
                return false;
            bool result;
            try
            {
                result = (Model.SqState[x][y] == SquareState.Open);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        ///     Items the can be placed here.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ItemCanBePlacedHere(int x, int y)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y) return false;
            
            if (x == Model.DoorX && y == Model.DoorY) return false;
            
            if (x >= GameMap.GetLength(0)) return false;
            
            return y <= GameMap.GetLength(1) &&
                   GameMap[x, y] == 1;
        }


        /// <summary>
        ///     Sqs the height of the absolute.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>System.Double.</returns>
        internal double SqAbsoluteHeight(int x, int y)
        {
            try
            {
                if (GameMap == null) return 0;

                if (x >= GameMap.GetUpperBound(0) || y >= GameMap.GetUpperBound(1))
                    return 0;

                if (x >= Model.MapSizeX || y >= Model.MapSizeY)
                    return 0.0;

                var point = new Point(x, y);
                if (CoordinatedItems.TryGetValue(point, out var itemsOnSquare))
                {
                    return SqAbsoluteHeight(x, y, itemsOnSquare);
                }

                return Model.SqFloorHeight[x][y];
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception RoomData Loading on (SqAbsoluteHeight): " + ex);
                Logging.HandleException(ex, "Oblivion.HabboHotel.Rooms.Gamemap");
                return 0.0;
            }
        }

        /// <summary>
        ///     Sqs the height of the absolute.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="itemsOnSquare">The items on square.</param>
        /// <returns>System.Double.</returns>
        internal double SqAbsoluteHeight(int x, int y, ConcurrentList<RoomItem> itemsOnSquare)
        {
            try
            {
                double[] highestStack = {Model.SqFloorHeight[x][y]};
                var deductable = 0.0;

                /* TODO CHECK */
                if (highestStack.Length <= 0)
                {
                    return 0.0;
                }

                if (itemsOnSquare != null && itemsOnSquare.Count > 0)
                {
                    foreach (var item in itemsOnSquare)
                    {
                        if ((item?.GetBaseItem() == null || !(item.TotalHeight > highestStack[0]))) continue;
                        if (item.GetBaseItem().IsSeat || item.GetBaseItem().InteractionType == Interaction.Bed)
                            deductable = item.GetBaseItem().Height;
                        highestStack[0] = item.TotalHeight;
                    }
                }

                highestStack[0] -= deductable;
                return highestStack[0] < 0 ? 0 : highestStack[0];
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SqAbsoluteHeight");
                return 0.0;
            }
        }

        /// <summary>
        ///     Valids the tile.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ValidTile(int x, int y) => x > 0 && y > 0 && x < Model.MapSizeX && y < Model.MapSizeY;

        /// <summary>
        ///     Gets the room item for square.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetRoomItemForSquare(int x, int y)
        {
            var point = new Point(x, y);
            var list = new List<RoomItem>();
            if (!CoordinatedItems.TryGetValue(point, out var list2))
                return list;

            list.AddRange(list2.Where(roomItem => roomItem.Coordinate.X == x && roomItem.Coordinate.Y == y));

            return list;
        }


        /// <summary>
        ///     Squares the has furni.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SquareHasFurni(int x, int y, Interaction type)
        {
            var point = new Point(x, y);
            if (!CoordinatedItems.TryGetValue(point, out var list))
                return false;

            return
                list.Any(
                    item =>
                        item.Coordinate.X == x && item.Coordinate.Y == y &&
                        item.GetBaseItem().InteractionType == type);
        }

        /// <summary>
        ///     Gets all room item for square.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal ConcurrentList<RoomItem> GetAllRoomItemForSquare(int pX, int pY)
        {
            var point = new Point(pX, pY);
            var list = new ConcurrentList<RoomItem>();
            return CoordinatedItems.TryGetValue(point, out var list2) ? list2 : list;
        }

        /// <summary>
        ///     Squares the has users.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SquareHasUsers(int x, int y) => GetRoomUsers(new Point(x, y)).Count > 0;

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Dispose()
        {
            _userMap?.Clear();
            CoordinatedItems?.Clear();
            WalkableList?.Clear();
            GuildGates?.Clear();
            if (GameMap != null)
                Array.Clear(GameMap, 0, GameMap.Length);

            if (ItemHeightMap != null)
                Array.Clear(ItemHeightMap, 0, ItemHeightMap.Length);
            _userMap = null;
            GameMap = null;
            GuildGates = null;
            ItemHeightMap = null;
            CoordinatedItems = null;
            SerializedFloormap?.Dispose();
            SerializedFloormap = null;
            _room = null;
            Model = null;
            StaticModel = null;
            WalkableList = null;
        }


        /// <summary>
        ///     Gets the new heightmap.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GetNewHeightmap()
        {
            if (SerializedFloormap != null)
                return SerializedFloormap;
            SerializedFloormap = NewHeightMap();
            return SerializedFloormap;
        }

        /// <summary>
        ///     Sets the default value.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        private void SetDefaultValue(int x, int y)
        {
            try
            {
                GameMap[x, y] = 0;

                ItemHeightMap[x, y] = 0.0;
                if (x == Model.DoorX && y == Model.DoorY)
                {
                    GameMap[x, y] = 3;
                }
                else if (Model.SqState[x][y] == SquareState.Open)
                {
                    GameMap[x, y] = 1;
                }
                else if (Model.SqState[x][y] == SquareState.Seat)
                {
                    GameMap[x, y] = 2;
                }
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }
        }

        /// <summary>
        ///     Gets the walkable points.
        /// </summary>
        /// <returns>HashSet&lt;Point&gt;.</returns>
        private HashSet<Point> GetWalkablePoints()
        {
            var list = new HashSet<Point>();

            {
                for (var i = 0; i < GameMap.GetUpperBound(1) - 1; i++)
                for (var j = 0; j < GameMap.GetUpperBound(0) - 1; j++)
                    if (StaticModel.DoorX != j && StaticModel.DoorY != i && GameMap[j, i] == 0)
                        list.Add(new Point(j, i));
                return list;
            }
        }

        /// <summary>
        ///     Constructs the map for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="coord">The coord.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ConstructMapForItem(RoomItem item, Point coord)
        {
            try
            {
                if (item?.GetBaseItem() == null || Model == null || ItemHeightMap == null ||
                    Model.SqFloorHeight == null)
                    return false;

                if (coord.X > Model.MapSizeX - 1)
                {
                    Model.AddX();
                    GenerateMaps();
                    return false;
                }

                if (coord.Y > Model.MapSizeY - 1)
                {
                    Model.AddY();
                    GenerateMaps();
                    return false;
                }

                if (Model.SqState[coord.X][coord.Y] == SquareState.Blocked)
                {
                    Model.OpenSquare(coord.X, coord.Y, item.Z);
                    Model.SetUpdateState();
                }


                if ((ItemHeightMap.GetLength(0) - 1 >= coord.X && ItemHeightMap.GetLength(1) - 1 >= coord.Y) &&
                    ItemHeightMap[coord.X, coord.Y] <= item.TotalHeight)
                {
                    ItemHeightMap[coord.X, coord.Y] = item.TotalHeight - Model.SqFloorHeight[item.X][item.Y];

                    if (GameMap.GetLength(0) >= coord.X && GameMap.GetLength(1) >= coord.Y)
                    {
                        if (item.GetBaseItem().Walkable)
                        {
                            if (GameMap[coord.X, coord.Y] != 2)
                                GameMap[coord.X, coord.Y] = 1;
                        }
                        else if (item.Z <= Model.SqFloorHeight[item.X][item.Y] + 0.1 &&
                                 item.GetBaseItem().InteractionType == Interaction.Gate && item.ExtraData == "1")
                        {
                            if (GameMap[coord.X, coord.Y] != 2)
                                GameMap[coord.X, coord.Y] = 1;
                        }
                        else if (item.GetBaseItem().IsSeat || item.GetBaseItem().InteractionType == Interaction.Bed ||
                                 item.GetBaseItem().InteractionType == Interaction.Guillotine ||
                                 item.GetBaseItem().InteractionType == Interaction.BedTent)
                        {
                            //if (coord.X == item.X && coord.Y == item.Y)
                            GameMap[coord.X, coord.Y] = 2;
                        }
                        else if (GameMap[coord.X, coord.Y] != 2)
                            GameMap[coord.X, coord.Y] = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(string.Concat("Error during map generation for room ", _room.RoomId,
                    ". Exception: ", ex.ToString()));
                Logging.HandleException(ex, "Oblivion.HabboHotel.Rooms.Gamemap");
            }

            return true;
        }

        /// <summary>
        ///     Adds the special items.
        /// </summary>
        /// <param name="item">The item.</param>
        private void AddSpecialItems(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.BanzaiFloor:
                    _room.GetBanzai().AddTile(item);
                    break;

                case Interaction.BanzaiTele:
                    _room.GetGameItemHandler().AddTeleport(item, item.Id);
                    item.ExtraData = string.Empty;
                    break;

                case Interaction.BanzaiPuck:
                    _room.GetBanzai().AddPuck(item);
                    break;

                case Interaction.BanzaiPyramid:
                    _room.GetGameItemHandler().AddPyramid(item, item.Id);
                    break;

                case Interaction.FreezeExit:
                    var exitTeleport = _room.GetFreeze().ExitTeleport;
                    if (exitTeleport == null || item.Id != exitTeleport.Id)
                        break;
                    _room.GetFreeze().ExitTeleport = null;
                    break;

                case Interaction.FreezeTileBlock:
                    _room.GetFreeze().AddFreezeBlock(item);
                    break;

                case Interaction.FreezeTile:
                    _room.GetFreeze().AddFreezeTile(item);
                    break;

                case Interaction.Football:
                    _room.GetSoccer().AddBall(item);
                    break;
            }
        }

        /// <summary>
        ///     Removes the special item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void RemoveSpecialItem(RoomItem item, bool removed)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.BanzaiFloor:
                    _room.GetBanzai().RemoveTile(item);
                    break;

                case Interaction.BanzaiTele:
                    _room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;

                case Interaction.BanzaiPuck:
                    _room.GetBanzai().RemovePuck(item);
                    break;

                case Interaction.BanzaiPyramid:
                    _room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;

                case Interaction.FreezeTileBlock:
                    _room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;

                case Interaction.FreezeTile:
                    _room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                /*
                                case Interaction.FootballGate:
                //                       _room.GetSoccer().UnRegisterGate(item);
                                    break;*/

                case Interaction.Football:
                    _room.GetSoccer().RemoveBall(item.Id);
                    break;
            }
        }

        /// <summary>
        ///     News the height map.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        private ServerMessage NewHeightMap()
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("HeightMapMessageComposer"));
            serverMessage.AppendInteger(Model.MapSizeX);
            serverMessage.AppendInteger(Model.MapSizeX * Model.MapSizeY);
            for (var i = 0; i < Model.MapSizeY; i++)
            {
                for (var j = 0; j < Model.MapSizeX; j++)
                {
                    if (Model.SqState[j][i] == SquareState.Blocked)
                    {
                        serverMessage.AppendShort(-1);
                    }
                    else
                    {
                        serverMessage.AppendShort((short)(SqAbsoluteHeight(j, i) * 256));
                    }
                }
            }

            //  serverMessage.AppendShort(this.Model.SqFloorHeight[j, i] * 256);
            return serverMessage;
        }

        internal MovementState GetChasingMovement(int X, int Y)
        {
            try
            {
                bool moveToLeft = true;
                bool moveToRight = true;
                bool moveToUp = true;
                bool moveToDown = true;

                for (int i = 1; i < 4; i++)
                {
                    // Left
                    if (moveToLeft)
                    {
                        if (SquareHasUsers(X - i, Y))
                            return MovementState.Left;
                        else if (i == 1 && !TileIsWalkable(X - i, Y, false))
                            moveToLeft = false;
                    }

                    // Right
                    if (moveToRight)
                    {
                        if (SquareHasUsers(X + i, Y))
                            return MovementState.Right;
                        else if (i == 1 && !TileIsWalkable(X + i, Y, false))
                            moveToRight = false;
                    }

                    // Up
                    if (moveToUp)
                    {
                        if (SquareHasUsers(X, Y - i))
                            return MovementState.Up;
                        else if (i == 1 && !TileIsWalkable(X, Y - i, false))
                            moveToUp = false;
                    }

                    // Down
                    if (moveToDown)
                    {
                        if (SquareHasUsers(X, Y + i))
                            return MovementState.Down;
                        else if (i == 1 && !TileIsWalkable(X, Y + i, false))
                            moveToDown = false;
                    }

                    // Breaking bucle
                    if (i == 3 && !moveToLeft && !moveToRight && !moveToUp && !moveToDown)
                        return MovementState.Random;
                }

                List<MovementState> movements = new List<MovementState>();
                if (moveToLeft)
                    movements.Add(MovementState.Left);
                if (moveToRight)
                    movements.Add(MovementState.Right);
                if (moveToUp)
                    movements.Add(MovementState.Up);
                if (moveToDown)
                    movements.Add(MovementState.Down);

                var rnd = Oblivion.GetRandomNumber(0, movements.Count);
                return movements[rnd];
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "GetChasingMovement");
                return MovementState.Random;
            }
        }

        public Point GetChaseMovement(RoomItem Item)
        {
            var Distance = 1000;
            var Coord = new Point(0, 0);
            var iX = Item.X;
            var iY = Item.Y;
            var X = false;

            foreach (var User in _room.GetRoomUserManager().GetRoomUsers())
                if (User.X == Item.X)
                {
                    var Difference = Math.Abs(User.Y - Item.Y);
                    if (Difference >= Distance)
                        continue;
                    Distance = Difference;
                    Coord = User.Coordinate;
                    X = false;
                }
                else if (User.Y == Item.Y)
                {
                    var Difference = Math.Abs(User.X - Item.X);
                    if (Difference >= Distance)
                        continue;
                    Distance = Difference;
                    Coord = User.Coordinate;
                    X = true;
                }

            if (Distance > 5)
                return Item.GetSides().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (X && Distance < 99)
                if (iX > Coord.X)
                {
                    iX--;
                    return new Point(iX, iY);
                }
                else
                {
                    iX++;
                    return new Point(iX, iY);
                }

            if (!X && Distance < 99)
                if (iY > Coord.Y)
                {
                    iY--;
                    return new Point(iX, iY);
                }
                else
                {
                    iY++;
                    return new Point(iX, iY);
                }

            return Item.Coordinate;
        }

        public Point GetInverseChaseMovement(RoomItem Item)
        {
            var Distance = 99;
            var Coord = new Point(0, 0);
            var iX = Item.X;
            var iY = Item.Y;
            var X = false;

            foreach (var User in _room.GetRoomUserManager().GetRoomUsers())
                if (User.X == Item.X)
                {
                    var Difference = Math.Abs(User.Y - Item.Y);
                    if (Difference >= Distance)
                        continue;
                    Distance = Difference;

                    Coord = User.Coordinate;
                    X = false;
                }
                else if (User.Y == Item.Y)
                {
                    var Difference = Math.Abs(User.X - Item.X);
                    if (Difference >= Distance)
                        continue;
                    Distance = Difference;
                    Coord = User.Coordinate;
                    X = true;
                }

            if (Distance > 5)
            {
                var sides = Item.GetSides();
                if (sides.Count <= 0) return Coord;
                return sides[Oblivion.GetRandomNumber(0, sides.Count - 1)];
            }

            if (X && Distance < 99)
                if (iX > Coord.X)
                {
                    iX++;
                    return new Point(iX, iY);
                }
                else
                {
                    iX--;
                    return new Point(iX, iY);
                }

            if (!X && Distance < 99)
                if (iY > Coord.Y)
                {
                    iY++;
                    return new Point(iX, iY);
                }
                else
                {
                    iY--;
                    return new Point(iX, iY);
                }

            return Item.Coordinate;
        }


        internal bool IsValidValueItem(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Model.MapSizeX || y >= Model.MapSizeY)
                return false;

            if (SquareHasUsers(x, y))
                return true;

            if (GetCoordinatedItems(new Point(x, y)).Count > 0 && !SquareIsOpen(x, y, false))
                return false;

            return Model.SqState[x][y] == SquareState.Open;
        }
    }
}