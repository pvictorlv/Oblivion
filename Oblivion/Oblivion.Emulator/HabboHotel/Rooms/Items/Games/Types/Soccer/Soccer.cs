using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Soccer.Enums;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Rooms.Items.Games.Types.Soccer
{
    internal class Soccer
    {
//        private List<RoomItem> _balls;
        private RoomItem _ball;

        private RoomItem[] _gates;
        private Room _room;

        public Soccer(Room room)
        {
            _room = room;
            _gates = new RoomItem[4];
//            _balls = new List<RoomItem>();
        }

        public static int GetThreadTime(int i)
        {
            switch (i)
            {
                case 1:
                    return 75;

                case 2:
                    return 100;

                case 3:
                    return 125;

                case 4:
                    return 150;

                default:
                    if (i != 5)
                        return ((i != 6) ? 200 : 350);
                    return 200;
            }
        }

        internal bool GotBall() => _ball != null;

        internal void AddBall(RoomItem item)
        {
            _ball = item;
        }

        internal void Destroy()
        {
            Array.Clear(_gates, 0, _gates.Length);
            _gates = null;
            _room = null;
//            _balls.Clear();
            lock (_ball)
                _ball = null;
        }

        internal void OnCycle()
        {
            try
            {
                if (_ball == null)
                    return;

                lock (_ball)
                {
                    if (!_ball.BallIsMoving) return;

                    MoveBallProcess(_ball, _ball.InteractingBallUser);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Ball - OnCycle");
            }
        }

        internal void OnGateRemove(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.FootballGoalGreen:
                case Interaction.FootballCounterGreen:
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Green);
                    return;

                case Interaction.FootballGoalYellow:
                case Interaction.FootballCounterYellow:
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Yellow);
                    return;

                case Interaction.FootballGoalBlue:
                case Interaction.FootballCounterBlue:
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Blue);
                    return;

                case Interaction.FootballGoalRed:
                case Interaction.FootballCounterRed:
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Red);
                    return;

                default:
                    return;
            }
        }

        private IEnumerable<RoomItem> GetFootballItemsForAllTeams()
        {
            var items = _room.GetGameManager().GetItems(Team.Red).Values.ToList();
            items.AddRange(_room.GetGameManager().GetItems(Team.Green).Values);

            items.AddRange(_room.GetGameManager().GetItems(Team.Blue).Values);

            items.AddRange(_room.GetGameManager().GetItems(Team.Yellow).Values);

            return items;
        }

        private bool GameItemOverlaps(RoomItem gameItem)
        {
            var gameItemCoord = gameItem.Coordinate;
            return
                GetFootballItemsForAllTeams()
                    .Any(
                        item =>
                            item.AffectedTiles.Values.Any(
                                tile => tile.X == gameItemCoord.X && tile.Y == gameItemCoord.Y));
        }

        internal void OnUserWalk(RoomUser user)
        {
            if (user == null || _ball == null)
                return;

            lock (_ball)
            {
                if (user.SetX == _ball.X && user.SetY == _ball.Y && user.GoalX == _ball.X &&
                    user.GoalY == _ball.Y) // super chute.
                {
                    var userPoint = new Point(user.X, user.Y);
                    _ball.ExtraData = "55";
                    _ball.BallIsMoving = true;
                    _ball.BallValue = 1;
                    MoveBall(_ball, user.GetClient(), userPoint);
                }
                else if (user.X == _ball.X && user.Y == _ball.Y)
                {
                    var userPoint = new Point(user.SetX, user.SetY);
                    _ball.ExtraData = "55";
                    _ball.BallIsMoving = true;
                    _ball.BallValue = 1;
                    MoveBall(_ball, user.GetClient(), userPoint);
                }
                else
                {
                    if (user.HandelingBallStatus == 0 && user.GoalX == _ball.X && user.GoalY == _ball.Y)
                        return;

                    if (user.SetX == _ball.X && user.SetY == _ball.Y && user.IsWalking &&
                        (user.X != user.GoalX || user.Y != user.GoalY))
                    {
                        user.HandelingBallStatus = 1;
                        var comeDirection =
                            ComeDirection.GetComeDirection(new Point(user.X, user.Y), _ball.Coordinate);

                        if (comeDirection == IComeDirection.Null)
                            return;

                        var newX = user.SetX;
                        var newY = user.SetY;

                        if (!_ball.GetRoom().GetGameMap().ValidTile2(user.SetX, user.SetY) ||
                            !_ball.GetRoom().GetGameMap().ItemCanBePlacedHere(user.SetX, user.SetY))
                        {
                            comeDirection = ComeDirection.InverseDirections(_room, comeDirection, user.X, user.Y);
                            newX = _ball.X;
                            newY = _ball.Y;
                        }

                        ComeDirection.GetNewCoords(comeDirection, ref newX, ref newY);
                        _ball.ExtraData = "11";
//                        if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
//                            return;
                        MoveBall(_ball, user.GetClient(), newX, newY);
                    }
                }
            }
        }

        internal bool MoveBall(RoomItem item, GameClient mover, int newX, int newY)
        {
            if (item?.GetBaseItem() == null /*|| mover == null || mover.GetHabbo() == null*/)
                return false;

//            if (_room.GetGameMap().SquareHasFurni(newX, newY))
//            {
//                mover.SendWhisper("Has furni");
//                return false;
//            }
            if (item.BallIsMoving)
            {
                if (item.ExtraData == "55" || item.ExtraData == "44")
                {
                    var randomValue = new Random().Next(1, 7);
                    if (randomValue != 5)
                        if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
                            return false;
                }
            }
            else
            {
                if (!_room.GetGameMap().ValidTile(newX, newY))
                    return false;
            }

            var oldRoomCoord = item.Coordinate;
            var itemIsOnGameItem = GameItemOverlaps(item);
//            double  = _room.GetGameMap().Model.SqFloorHeight[newX][newY];
            var newZ = _room.GetGameMap().SqAbsoluteHeight(newX, newY);
            var mMessage = new ServerMessage();
            mMessage.Init(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer")); // Cf
            mMessage.AppendInteger(item.VirtualId);
            mMessage.AppendInteger(item.BaseItem);
            mMessage.AppendInteger(newX);
            mMessage.AppendInteger(newY);
            mMessage.AppendInteger(4);
            mMessage.AppendString(TextHandling.GetString(item.Z));
            mMessage.AppendString(TextHandling.GetString(newZ));
            mMessage.AppendInteger(0);
            mMessage.AppendInteger(0);
            mMessage.AppendString(item.ExtraData);
            mMessage.AppendInteger(-1);
            mMessage.AppendInteger(0);
            mMessage.AppendInteger(_room.RoomData.OwnerId);
            mMessage.AppendInteger(item.VirtualId);
            _room.SendMessage(mMessage);

            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return false;

//            _room.GetGameMap().SquareIsOpen();
            item.SetState(newX, newY, newZ,
                Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rot));
//            _room.GetGameMap().Model.SqFloorHeight[item.X][item.Y] = item.Z;
            if (itemIsOnGameItem || mover?.GetHabbo() == null)
                return false;
            HandleFootballGameItems(new Point(newX, newY),
                _room.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id));
            return false;
        }

        internal void MoveBall(RoomItem item, GameClient client, Point user)
        {
            try
            {
                item.ComeDirection = ComeDirection.GetComeDirection(user, item.Coordinate);

                if (item.ComeDirection != IComeDirection.Null)
                    MoveBallProcess(item, client);
            }
            catch
            {
            }
        }

        internal void MoveBallProcess(RoomItem item, GameClient client)
        {
            if (item == null) return;
//            if (!_balls.Contains(item)) return;

            var tryes = 0;
            var newX = item.Coordinate.X;
            var newY = item.Coordinate.Y;

            if (item.InteractingBallUser == null || item.InteractingBallUser != client)
                item.InteractingBallUser = client;


            while (tryes < 3)
            {
                if (_room?.GetGameMap() == null)
                    return;

//                var total = item.ExtraData == "55" ? 6 : 1;
//                for (var i = 0; i != total; i++)

                if (item.ComeDirection == IComeDirection.Null)
                {
                    item.BallIsMoving = false;
                    break;
                }

                var resetX = newX;
                var resetY = newY;

                ComeDirection.GetNewCoords(item.ComeDirection, ref newX, ref newY);

                var ignoreUsers = false;

//                if (_room.GetGameMap().SquareHasFurni(newX, newY))
//                {
//                    client.SendWhisper("Has furni");
//                    return;
//                }
                if (_room.GetGameMap().SquareHasUsers(newX, newY))
                {
                    if (item.ExtraData != "55" && item.ExtraData != "44")
                    {
                        item.BallIsMoving = false;
                        break;
                    }

                    ignoreUsers = true;
                }

                if (ignoreUsers == false)
                    if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
                    {
                        item.ComeDirection = ComeDirection.InverseDirections(_room, item.ComeDirection, newX, newY);
                        newX = resetX;
                        newY = resetY;
                        tryes++;
                        if (tryes > 2)
                            item.BallIsMoving = false;
                        continue;
                    }

                if (MoveBall(item, client, newX, newY))
                {
                    item.BallIsMoving = false;
                    break;
                }

                int.TryParse(item.ExtraData, out var number);
                if (number > 11)
                    item.ExtraData = (int.Parse(item.ExtraData) - 11).ToString();

                item.BallValue++;

                if (item.BallValue > 6)
                {
                    item.BallIsMoving = false;
                    item.BallValue = 1;
                    item.InteractingBallUser = null;
                }

                break;
                //break;
            }
        }

        internal void RegisterGate(RoomItem item)
        {
            if (_gates[0] == null)
            {
                item.Team = Team.Blue;
                _gates[0] = item;
                return;
            }

            if (_gates[1] == null)
            {
                item.Team = Team.Red;
                _gates[1] = item;
                return;
            }

            if (_gates[2] == null)
            {
                item.Team = Team.Green;
                _gates[2] = item;
                return;
            }

            if (_gates[3] != null)
                return;
            item.Team = Team.Yellow;
            _gates[3] = item;
        }

        internal void RemoveBall()
        {
            if (_room == null) return;
            if (_ball == null) return;


            lock (_ball)
            {
                _room.StopSoccer();
                _ball.BallIsMoving = false;
                _ball = null;
            }
        }

        internal void UnRegisterGate(RoomItem item)
        {
            switch (item.Team)
            {
                case Team.Red:
                    _gates[1] = null;
                    return;

                case Team.Green:
                    _gates[2] = null;
                    return;

                case Team.Blue:
                    _gates[0] = null;
                    return;

                case Team.Yellow:
                    _gates[3] = null;
                    return;

                default:
                    return;
            }
        }

        private void HandleFootballGameItems(Point ballItemCoord, RoomUser user)
        {
            if (user == null || _room?.GetGameManager() == null) return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
            //todo recode
            if (_room.GetGameManager()
                .GetItems(Team.Red)
                .Values.SelectMany(current => current.AffectedTiles.Values)
                .Any(current2 => current2.X == ballItemCoord.X && current2.Y == ballItemCoord.Y))
                _room.GetGameManager().AddPointToTeam(Team.Red, user);
            if (
                _room.GetGameManager()
                    .GetItems(Team.Green)
                    .Values.SelectMany(current3 => current3.AffectedTiles.Values)
                    .Any(current4 => current4.X == ballItemCoord.X && current4.Y == ballItemCoord.Y))
                _room.GetGameManager().AddPointToTeam(Team.Green, user);
            if (
                _room.GetGameManager()
                    .GetItems(Team.Blue)
                    .Values.SelectMany(current5 => current5.AffectedTiles.Values)
                    .Any(current6 => current6.X == ballItemCoord.X && current6.Y == ballItemCoord.Y))
                _room.GetGameManager().AddPointToTeam(Team.Blue, user);
            if (!_room.GetGameManager()
                .GetItems(Team.Yellow)
                .Values.SelectMany(current7 => current7.AffectedTiles.Values)
                .Any(current8 => current8.X == ballItemCoord.X && current8.Y == ballItemCoord.Y))
                _room.GetGameManager().AddPointToTeam(Team.Yellow, user);

            serverMessage.AppendInteger(user.VirtualId);
            serverMessage.AppendInteger(0);
            user.GetClient().GetHabbo().CurrentRoom.SendMessage(serverMessage);
        }
    }
}