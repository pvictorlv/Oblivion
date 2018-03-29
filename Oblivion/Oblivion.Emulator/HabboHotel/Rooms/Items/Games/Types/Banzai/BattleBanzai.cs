#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Oblivion.Enclosure;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

#endregion

namespace Oblivion.HabboHotel.Rooms.Items.Games.Types.Banzai
{
    public class BattleBanzai
    {
        private List<RoomItem> _banzaiTiles;
        private List<RoomItem> _pucks;
        private Room _room;
        private GameField _field;
        private byte[,] _floorMap;
        private double _timestarted;

        public BattleBanzai(Room room)
        {
            _room = room;
            IsBanzaiActive = false;
            _timestarted = 0;
            _pucks = new List<RoomItem>();
            _banzaiTiles = new List<RoomItem>();
        }

        public bool IsBanzaiActive { get; private set; }

        public void AddTile(RoomItem item)
        {
            if (!_banzaiTiles.Contains(item))
                _banzaiTiles.Add(item);
        }

        public void RemoveTile(RoomItem item)
        {
            _banzaiTiles.Remove(item);
        }

        public void AddPuck(RoomItem item)
        {
            if (!_pucks.Contains(item))
                _pucks.Add(item);
        }

        public void RemovePuck(RoomItem item)
        {
            _pucks.Remove(item);
        }


        internal void OnUserWalk(RoomUser User)
        {
            if (User == null) return;
            /* TODO CHECK */
            foreach (var item in _pucks.ToList())
            {
                var num = User.X - item.X;
                var num2 = User.Y - item.Y;
                if (num <= 1 && num >= -1 && num2 <= 1 && num2 >= -1)
                {
                    var x = num * -1;
                    var y = num2 * -1;
                    x += item.X;
                    y += item.Y;
                    if (item.InteractingUser == User.UserId && _room.GetGameMap().ValidTile(x, y))
                    {
                        item.InteractingUser = 0;
                        MovePuck(item, User.GetClient(), User.Coordinate, item.Coordinate, 6, User.Team);
                    }
                    else if (_room.GetGameMap().ValidTile(x, y))
                    {
                        MovePuck(item, User.GetClient(), x, y, User.Team);
                    }
                }
            }

            if (IsBanzaiActive)
                HandleBanzaiTiles(User.Coordinate, User.Team, User);
        }

        public void BanzaiStart()
        {
            if (IsBanzaiActive)
                return;

            _floorMap = new byte[_room.GetGameMap().Model.MapSizeY, _room.GetGameMap().Model.MapSizeX];
            _field = new GameField(_floorMap, true);
            _timestarted = Oblivion.GetUnixTimeStamp();
            _room.GetGameManager().LockGates();
            for (var i = 1; i < 5; i++)
                _room.GetGameManager().Points[i] = 0;

            /* TODO CHECK */
            foreach (var tile in _banzaiTiles)
            {
                tile.ExtraData = "1";
                tile.Value = 0;
                tile.Team = Team.None;
                tile.UpdateState();
            }

            ResetTiles();
            IsBanzaiActive = true;

            if (_room.GotWireds())
                _room.GetWiredHandler().ExecuteWired(Interaction.TriggerGameStart);

            /* TODO CHECK */
            foreach (var user in _room.GetRoomUserManager().GetRoomUsers())
                user.LockedTilesCount = 0;
        }

        public void ResetTiles()
        {
            /* TODO CHECK */
            foreach (var item in _room.GetRoomItemHandler().FloorItems.Values)
            {
                var type = item.GetBaseItem().InteractionType;

                switch (type)
                {
                    case Interaction.BanzaiScoreBlue:
                    case Interaction.BanzaiScoreRed:
                    case Interaction.BanzaiScoreYellow:
                    case Interaction.BanzaiScoreGreen:
                        item.ExtraData = "0";
                        item.UpdateState();
                        break;
                }
            }
        }

        public void BanzaiEnd(bool userTriggered = false)
        {
            IsBanzaiActive = false;
            _room.GetGameManager().StopGame();
            _floorMap = null;

            if (!userTriggered && _room.GotWireds())
                _room.GetWiredHandler().ExecuteWired(Interaction.TriggerGameEnd);

            var winners = _room.GetGameManager().GetWinningTeam();
            _room.GetGameManager().UnlockGates();
            /* TODO CHECK */
            foreach (var tile in _banzaiTiles.ToList())
                if (tile.Team == winners)
                {
                    tile.InteractionCount = 0;
                    tile.InteractionCountHelper = 0;
                    tile.UpdateNeeded = true;
                }
                else if (tile.Team == Team.None)
                {
                    tile.ExtraData = "0";
                    tile.UpdateState();
                }

            if (winners != Team.None)
            {
                var Winners = _room.GetRoomUserManager().GetRoomUsers();

                /* TODO CHECK */
                foreach (var User in Winners)
                {
                    if (User.Team != Team.None)
                        if (Oblivion.GetUnixTimeStamp() - _timestarted > 5)
                        {
                            Oblivion.GetGame()
                                .GetAchievementManager()
                                .ProgressUserAchievement(User.GetClient(), "ACH_BattleBallTilesLocked",
                                    User.LockedTilesCount);
                            Oblivion.GetGame()
                                .GetAchievementManager()
                                .ProgressUserAchievement(User.GetClient(), "ACH_BattleBallPlayer", 1);
                        }

                    if (Oblivion.GetUnixTimeStamp() - _timestarted > 5)
                        Oblivion.GetGame()
                            .GetAchievementManager()
                            .ProgressUserAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);


                    var waveAtWin = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
                    waveAtWin.AppendInteger(User.VirtualId);
                    waveAtWin.AppendInteger(1);
                    _room.SendMessage(waveAtWin);
                }

                _field?.Destroy();
            }
        }

        public void MovePuck(RoomItem item, GameClient mover, int newX, int newY, Team Team)
        {
            if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
                return;

            var oldRoomCoord = item.Coordinate;


            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return;

            item.ExtraData = Convert.ToInt32(Team).ToString();
            item.UpdateNeeded = true;
            item.UpdateState();

            double newZ = _room.GetGameMap().Model.SqFloorHeight[newX][newY];

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
            serverMessage.AppendInteger(oldRoomCoord.X);
            serverMessage.AppendInteger(oldRoomCoord.Y);
            serverMessage.AppendInteger(newX);
            serverMessage.AppendInteger(newY);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(item.VirtualId);
            serverMessage.AppendString(TextHandling.GetString(item.Z));
            serverMessage.AppendString(TextHandling.GetString(newZ));
            serverMessage.AppendInteger(-1);
            _room.SendMessage(serverMessage);

            _room.GetRoomItemHandler().SetFloorItem(mover, item, newX, newY, item.Rot, false, false, false);

            if (mover?.GetHabbo() == null)
                return;

            var user = mover.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id);
            if (IsBanzaiActive)
                HandleBanzaiTiles(new Point(newX, newY), Team, user);
        }

        internal void MovePuck(RoomItem item, GameClient client, Point user, Point ball, int length, Team Team)
        {
            var num = user.X - ball.X;
            var num2 = user.Y - ball.Y;
            if (num <= 1 && num >= -1 && num2 <= 1 && num2 >= -1)
            {
                var list = new List<Point>();
                var x = ball.X;
                var y = ball.Y;
                for (var i = 1; i < length; i++)
                {
                    x = num * (0 - i);
                    y = num2 * (0 - i);
                    x += item.X;
                    y += item.Y;
                    if (!_room.GetGameMap().ItemCanBePlacedHere(x, y))
                    {
                        if (i != 1)
                        {
                            if (i != length)
                                list.Add(new Point(x, y));
                            i--;
                            x = num * (0 - i);
                            y = num2 * (0 - i);
                            x += item.X;
                            y += item.Y;
                        }

                        break;
                    }

                    if (i != length)
                        list.Add(new Point(x, y));
                }

                if (client?.GetHabbo() != null)
                {
                    var roomUserByHabbo =
                        client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
                    /* TODO CHECK */
                    foreach (var point in list)
                        HandleBanzaiTiles(point, Team, roomUserByHabbo);
                    if (x != ball.X || y != ball.Y)
                        MovePuck(item, client, x, y, Team);
                }
            }
        }

        private void SetTile(RoomItem item, Team Team, RoomUser user)
        {
            if (item == null || user == null || _room == null) return;
            if (item.Team == Team)
            {
                if (item.Value < 3)
                {
                    item.Value++;
                    if (item.Value == 3)
                    {
                        var teamByte = Convert.ToByte(Team);
                        
                        user.LockedTilesCount++;
                        _room.GetGameManager().AddPointToTeam(item.Team, user);
                        if (_field == null) return;
                        _field.UpdateLocation(item.X, item.Y, teamByte);

                        var gfield = _field.DoUpdate();
                        if (gfield == null) return;
                        gfield = gfield.ToList();

                        foreach (var gameField in gfield)
                        {
                            if (gameField?.GetPoints() == null) continue;

                            var t = (Team) gameField.ForValue;

                            /* TODO CHECK */
                            var point = gameField.GetPoints().ToList();
                            foreach (var p in point)
                            {
                                HandleMaxBanzaiTiles(new Point(p.X, p.Y), t, user);
                                _floorMap[p.Y, p.X] = gameField.ForValue;
                            }

                            point.Clear();
                        }

                        gfield.Clear();
                    }
                }
            }
            else
            {
                if (item.Value < 3)
                {
                    item.Team = Team;
                    item.Value = 1;
                }
            }


            var newColor = item.Value + Convert.ToInt32(item.Team) * 3 - 1;
            item.ExtraData = newColor.ToString();
        }

        private void HandleBanzaiTiles(Point coord, Team Team, RoomUser user)
        {
            if (Team == Team.None)
                return;

            var i = 0;
            /* TODO CHECK */
            foreach (var _item in _banzaiTiles.ToList())
            {
                if (_item.GetBaseItem().InteractionType != Interaction.BanzaiFloor)
                {
                    user.Team = Team.None;
                    user.ApplyEffect(0);
                    continue;
                }

                if (_item.ExtraData.Equals("5") || _item.ExtraData.Equals("8") || _item.ExtraData.Equals("11") ||
                    _item.ExtraData.Equals("14"))
                {
                    i++;
                    continue;
                }

                if (_item.X != coord.X || _item.Y != coord.Y)
                    continue;

                SetTile(_item, Team, user);
                if (_item.ExtraData.Equals("5") || _item.ExtraData.Equals("8") || _item.ExtraData.Equals("11") ||
                    _item.ExtraData.Equals("14"))
                    i++;
                _item.UpdateState(false, true);
            }

            if (i == _banzaiTiles.Count)
                BanzaiEnd();
        }

        private void HandleMaxBanzaiTiles(Point coord, Team Team, RoomUser user)
        {
            if (Team == Team.None)
                return;

            /* TODO CHECK */
            foreach (
                var _item in
                _banzaiTiles.Where(
                    _item =>
                        _item.GetBaseItem().InteractionType == Interaction.BanzaiFloor && _item.X == coord.X &&
                        _item.Y == coord.Y))
            {
                SetMaxForTile(_item, Team);
                _room.GetGameManager().AddPointToTeam(Team, user);
                _item.UpdateState(false, true);
            }
        }

        private static void SetMaxForTile(RoomItem item, Team Team)
        {
            if (item.Value < 3)
            {
                item.Value = 3;
                item.Team = Team;
            }

            var newColor = item.Value + Convert.ToInt32(item.Team) * 3 - 1;
            item.ExtraData = newColor.ToString();
        }

        public void Destroy()
        {
            _banzaiTiles.Clear();
            _pucks.Clear();

            if (_floorMap != null)
                Array.Clear(_floorMap, 0, _floorMap.Length);

            _field?.Destroy();

            _room = null;
            _banzaiTiles = null;
            _pucks = null;
            _floorMap = null;
            _field = null;
        }
    }
}