using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<string, RoomItem> _balls;

        private RoomItem[] _gates;
        private Room _room;

        public Soccer(Room room)
        {
            _room = room;
            _gates = new RoomItem[4];
            _balls = new ConcurrentDictionary<string, RoomItem>();
            //            _balls = new List<RoomItem>();
        }


        internal bool GotBall() => _balls?.Count > 0;

        public void AddBall(RoomItem item)
        {
            this._balls[item.Id] = item;
        }

        public void RemoveBall(string itemID)
        {
            if (_balls.TryRemove(itemID, out var ball))
            {
                ball.BallIsMoving = false;
            }

            if (_balls.Count <= 0)
            {
                _room?.StopSoccer();
            }
        }

        internal void Destroy()
        {
            Array.Clear(_gates, 0, _gates.Length);
            _gates = null;
            _room = null;
            _balls.Clear();
            _balls = null;
        }

        internal async Task<bool> OnCycle()
        {
            try
            {
                if (_balls == null || _balls.IsEmpty)
                    return false;


                foreach (var _ball in _balls.Values)
                {
                    if (!_ball.BallIsMoving)
                    {
                        await Task.Delay(175).ConfigureAwait(false);
                        return true;
                    }

                        await MoveBallProcess(_ball, _ball.InteractingBallUser);
                    

                    if (_ball.ExtraData == "33")
                    {
                        await Task.Delay(150);

                    }
                    else if (_ball.ExtraData == "22")
                    {
                        await Task.Delay(175);
                    }
                    else if (_ball.ExtraData == "11")
                    {
                        await Task.Delay(200);
                    }
                    else
                    {
                        await Task.Delay(250);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Ball - OnCycle");
                return false;
            }

            return true;
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

        internal async Task OnUserWalk(RoomUser user)
        {
            foreach (var _ball in _balls.Values)
            {
                if (user == null || _ball == null)
                    continue;

                if (user.SetX == _ball.X && user.SetY == _ball.Y && user.GoalX == _ball.X &&
                    user.GoalY == _ball.Y && user.HandelingBallStatus == 0) // super chute.
                {
                    var userPoint = new Point(user.X, user.Y);
                    _ball.ExtraData = "55";
                    _ball.BallIsMoving = true;
                    _ball.BallValue = 1;
                    await MoveBall(_ball, user.GetClient(), userPoint);
                }
                else if (user.SetX == _ball.X && user.SetY == _ball.Y && user.GoalX == _ball.X &&
                         user.GoalY == _ball.Y && user.HandelingBallStatus == 1) // super chute quando para de andar
                {
                    user.HandelingBallStatus = 0;
                    var _comeDirection = ComeDirection.GetComeDirection(new Point(user.X, user.Y), _ball.Coordinate);
                    if (_comeDirection != IComeDirection.Null)
                    {
                        int NewX = user.SetX;
                        int NewY = user.SetY;

                        ComeDirection.GetNewCoords(_comeDirection, ref NewX, ref NewY);
                        if (_ball.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                        {
                            Point userPoint = new Point(user.X, user.Y);
                            _ball.ExtraData = "55";
                            _ball.BallIsMoving = true;
                            _ball.BallValue = 1;
                            _ball.InteractingBallUser = user.GetClient();
                            await MoveBall(_ball, user.GetClient(), userPoint);
                        }
                    }
                }
                else if (user.X == _ball.X && user.Y == _ball.Y && user.HandelingBallStatus == 0)
                {
                    var userPoint = new Point(user.SetX, user.SetY);
                    _ball.ExtraData = "55";
                    _ball.BallIsMoving = true;
                    _ball.BallValue = 1;
                    await MoveBall(_ball, user.GetClient(), userPoint);
                }
                else
                {
                    if (user.HandelingBallStatus == 0 && user.GoalX == _ball.X && user.GoalY == _ball.Y)
                        continue;

                    if (user.SetX == _ball.X && user.SetY == _ball.Y && user.IsWalking &&
                        (user.X != user.GoalX || user.Y != user.GoalY))
                    {
                        user.HandelingBallStatus = 1;
                        var comeDirection =
                            ComeDirection.GetComeDirection(new Point(user.X, user.Y), _ball.Coordinate);

                        if (comeDirection == IComeDirection.Null)
                            continue;

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
                        await MoveBall(_ball, user.GetClient(), newX, newY);
                    }
                }
            }
        }

        internal async Task<bool> MoveBall(RoomItem item, GameClient mover, int newX, int newY)
        {
            if (item?.GetBaseItem() == null /*|| mover == null || mover.GetHabbo() == null*/)
                return false;
            if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
                return false;

            if (mover?.GetHabbo() == null)
                return false;
            
            var roomUser = _room.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id);

            if (roomUser != null && roomUser.HandelingBallStatus == 1)
            {
                if (_room.GetGameMap().SquareHasUsers(newX, newY) && item.BallValue > 1)
                    return false;
            }

            var oldRoomCoord = item.Coordinate;
            var itemIsOnGameItem = GameItemOverlaps(item);
//            double  = _room.GetGameMap().Model.SqFloorHeight[newX][newY];
            var newZ = _room.GetGameMap().SqAbsoluteHeight(newX, newY);
            using (var mMessage = new ServerMessage())
            {
                await mMessage.InitAsync(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer")); // Cf
                await mMessage.AppendIntegerAsync(item.VirtualId);
                await mMessage.AppendIntegerAsync(item.BaseItem.ItemId);
                await mMessage.AppendIntegerAsync(newX);
                await mMessage.AppendIntegerAsync(newY);
                await mMessage.AppendIntegerAsync(4);
                await mMessage.AppendStringAsync($"{TextHandling.GetString(item.Z):0.00}");
                await mMessage.AppendStringAsync($"{TextHandling.GetString(item.Z):0.00}");

                await mMessage.AppendIntegerAsync(0);
                await mMessage.AppendIntegerAsync(0);
                await mMessage.AppendStringAsync(item.ExtraData);
                await mMessage.AppendIntegerAsync(-1);
                await mMessage.AppendIntegerAsync(0);
                await mMessage.AppendIntegerAsync(_room.RoomData.OwnerId);
                await mMessage.AppendIntegerAsync(item.VirtualId);
                await _room.SendMessage(mMessage);

                if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                    return false;

//            _room.GetGameMap().SquareIsOpen();
                item.SetState(newX, newY, newZ,
                    Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY,
                        item.Rot));
//            _room.GetGameMap().Model.SqFloorHeight[item.X][item.Y] = item.Z;
                if (itemIsOnGameItem || mover?.GetHabbo() == null)
                    return true;

                await HandleFootballGameItems(new Point(newX, newY), roomUser);
                return false;
            }
        }

        internal async Task MoveBall(RoomItem item, GameClient client, Point user)
        {
            try
            {
                item.ComeDirection = ComeDirection.GetComeDirection(user, item.Coordinate);

                if (item.ComeDirection != IComeDirection.Null)
                    await MoveBallProcess(item, client);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "MoveBall");
            }
        }

        internal async Task MoveBallProcess(RoomItem item, GameClient client)
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

                if (await MoveBall(item, client, newX, newY))
                {
                    item.BallIsMoving = false;
                    break;
                }

                int.TryParse(item.ExtraData, out var number);
                if (number > 11)
                    item.ExtraData = (number - 11).ToString();

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

        internal void UnRegisterGate(RoomItem item)
        {
            switch (item.Team)
            {
                case Team.Red:
                    _gates[1] = null;
                    return ;

                case Team.Green:
                    _gates[2] = null;
                    return ;

                case Team.Blue:
                    _gates[0] = null;
                    return ;

                case Team.Yellow:
                    _gates[3] = null;
                    return ;

                default:
                    return ;
            }
        }

        private async Task HandleFootballGameItems(Point ballItemCoord, RoomUser user)
        {
            if (user == null || _room?.GetGameManager() == null) return;
            using (var serverMessage =
                   new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer")))
            {
                //todo recode
                foreach (var current in _room.GetGameManager()
                             .GetItems(Team.Red)
                             .Values)
                foreach (var value in current.AffectedTiles.Values)
                    if (value.X == ballItemCoord.X && value.Y == ballItemCoord.Y)
                    {
                        await _room.GetGameManager().AddPointToTeam(Team.Red, user);
                        break;
                    }

                foreach (var current3 in _room.GetGameManager()
                             .GetItems(Team.Green)
                             .Values)
                foreach (var value in current3.AffectedTiles.Values)
                {
                    if (value.X != ballItemCoord.X || value.Y != ballItemCoord.Y) continue;
                    await _room.GetGameManager().AddPointToTeam(Team.Green, user);
                    break;
                }

                foreach (var current5 in _room.GetGameManager()
                             .GetItems(Team.Blue)
                             .Values)
                foreach (var value in current5.AffectedTiles.Values)
                    if (value.X == ballItemCoord.X && value.Y == ballItemCoord.Y)
                    {
                        await _room.GetGameManager().AddPointToTeam(Team.Blue, user);
                        break;
                    }

                foreach (var current5 in _room.GetGameManager()
                             .GetItems(Team.Yellow)
                             .Values)
                foreach (var value in current5.AffectedTiles.Values)
                    if (value.X == ballItemCoord.X && value.Y == ballItemCoord.Y)
                    {
                        await _room.GetGameManager().AddPointToTeam(Team.Yellow, user);
                        break;
                    }

                await serverMessage.AppendIntegerAsync(user.VirtualId);
                await serverMessage.AppendIntegerAsync(0);
                await user.GetClient().GetHabbo().CurrentRoom.SendMessage(serverMessage);
            }
        }
    }
}