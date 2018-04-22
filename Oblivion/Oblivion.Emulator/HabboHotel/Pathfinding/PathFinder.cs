using System;
using System.Collections.Generic;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;

namespace Oblivion.HabboHotel.PathFinding
{
    /// <summary>
    ///     Class PathFinder.
    /// </summary>
    internal class PathFinder
    {
        /// <summary>
        ///     The diag move points
        /// </summary>
        public static Vector2D[] DiagMovePoints =
        {
            new Vector2D(-1, -1),
            new Vector2D(0, -1),
            new Vector2D(1, -1),
            new Vector2D(1, 0),
            new Vector2D(1, 1),
            new Vector2D(0, 1),
            new Vector2D(-1, 1),
            new Vector2D(-1, 0)
        };

        /// <summary>
        ///     The no diag move points
        /// </summary>
        public static Vector2D[] NoDiagMovePoints =
        {
            new Vector2D(0, -1),
            new Vector2D(1, 0),
            new Vector2D(0, 1),
            new Vector2D(-1, 0)
        };

        /// <summary>
        ///     Finds the path.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="diag">if set to <c>true</c> [diag].</param>
        /// <param name="map">The map.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>List&lt;Vector2D&gt;.</returns>
        public static List<Vector2D> FindPath(RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
        {
            try
            {
                var list = new List<Vector2D>();
                var pathFinderNode = FindPathReversed(user, diag, map, start, end);

                if (pathFinderNode != null)
                {
                    list.Add(end);

                    while (pathFinderNode.Next != null)
                    {
                        list.Add(pathFinderNode.Next.Position);
                        pathFinderNode = pathFinderNode.Next;
                    }
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Finds the path reversed.
        /// </summary>
        /// <param name="roomUserable">The user.</param>
        /// <param name="whatIsDiag">if set to <c>true</c> [diag].</param>
        /// <param name="gameLocalMap">The map.</param>
        /// <param name="startMap">The start.</param>
        /// <param name="endMap">The end.</param>
        /// <returns>PathFinderNode.</returns>
        public static PathFinderNode FindPathReversed(RoomUser roomUserable, bool whatIsDiag, Gamemap gameLocalMap, Vector2D startMap, Vector2D endMap)
        {
            try
            {
                var minSpanTreeCost = new MinHeap<PathFinderNode>(256);
                var pathFinderMap = new PathFinderNode[gameLocalMap.Model.MapSizeX, gameLocalMap.Model.MapSizeY];
                var pathFinderStart = new PathFinderNode(startMap) {Cost = 0};
                var pathFinderEnd = new PathFinderNode(endMap);
                if (pathFinderMap.GetLength(0) <= pathFinderStart.Position.X ||
                    pathFinderMap.GetLength(1) <= pathFinderStart.Position.Y) return null;
                pathFinderMap[pathFinderStart.Position.X, pathFinderStart.Position.Y] = pathFinderStart;
                minSpanTreeCost.Add(pathFinderStart);

                while (minSpanTreeCost.Count > 0)
                {
                    pathFinderStart = minSpanTreeCost.ExtractFirst();
                    pathFinderStart.InClosed = true;

                    for (var index = 0;
                        (whatIsDiag
                            ? (index < DiagMovePoints.Length ? 1 : 0)
                            : (index < NoDiagMovePoints.Length ? 1 : 0)) != 0;
                        index++)
                    {
                        var realEndPosition = pathFinderStart.Position +
                                              (whatIsDiag ? DiagMovePoints[index] : NoDiagMovePoints[index]);

                        var isEndOfPath = ((realEndPosition.X == endMap.X) && (realEndPosition.Y == endMap.Y));

                        if (gameLocalMap.IsValidStep(roomUserable,
                            new Vector2D(pathFinderStart.Position.X, pathFinderStart.Position.Y), realEndPosition,
                            isEndOfPath, roomUserable.AllowOverride, true, true))
                        {
                            PathFinderNode pathFinderSecondNodeCalculation =
                                pathFinderMap[realEndPosition.X, realEndPosition.Y];

                            if (pathFinderSecondNodeCalculation == null)
                            {
                                pathFinderSecondNodeCalculation = new PathFinderNode(realEndPosition);
                                pathFinderMap[realEndPosition.X, realEndPosition.Y] = pathFinderSecondNodeCalculation;
                            }

                            if (!pathFinderSecondNodeCalculation.InClosed)
                            {
                                var internalSpanTreeCost = 0;

                                if (pathFinderStart.Position.X != pathFinderSecondNodeCalculation.Position.X)
                                    internalSpanTreeCost++;

                                if (pathFinderStart.Position.Y != pathFinderSecondNodeCalculation.Position.Y)
                                    internalSpanTreeCost++;

                                var loopTotalCost = pathFinderStart.Cost + internalSpanTreeCost +
                                                    pathFinderSecondNodeCalculation.Position.GetDistanceSquared(endMap);

                                if (loopTotalCost < pathFinderSecondNodeCalculation.Cost)
                                {
                                    pathFinderSecondNodeCalculation.Cost = loopTotalCost;
                                    pathFinderSecondNodeCalculation.Next = pathFinderStart;
                                }

                                if (!pathFinderSecondNodeCalculation.InOpen)
                                {
                                    if (pathFinderSecondNodeCalculation.Equals(pathFinderEnd))
                                    {
                                        pathFinderSecondNodeCalculation.Next = pathFinderStart;

                                        return pathFinderSecondNodeCalculation;
                                    }

                                    pathFinderSecondNodeCalculation.InOpen = true;

                                    minSpanTreeCost.Add(pathFinderSecondNodeCalculation);
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Calculates the rotation.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>System.Int32.</returns>
        internal static int CalculateRotation(int x1, int y1, int x2, int y2)
        {
            var dX = x2 - x1;
            var dY = y2 - y1;

            var d = Math.Atan2(dY, dX)*180/Math.PI;
            return ((int) d + 90)/45;
        }

        /// <summary>
        ///     Gets the distance.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="toX">To x.</param>
        /// <param name="toY">To y.</param>
        /// <returns>System.Int32.</returns>
        public static int GetDistance(int x, int y, int toX, int toY) => Convert.ToInt32(Math.Sqrt(Math.Pow(toX - x, 2) + Math.Pow(toY - y, 2)));
    }
}