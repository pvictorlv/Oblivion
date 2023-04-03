using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Items.Interactions;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Rooms.Items.Games
{
    internal class GameManager
    {
        private QueuedDictionary<string, RoomItem> _blueTeamItems;
        private QueuedDictionary<string, RoomItem> _greenTeamItems;
        private QueuedDictionary<string, RoomItem> _redTeamItems;
        private Room _room;
        private QueuedDictionary<string, RoomItem> _yellowTeamItems;
        internal int[] TeamPoints;

        public GameManager(Room room)
        {
            TeamPoints = new int[5];
            _redTeamItems = new QueuedDictionary<string, RoomItem>();
            _blueTeamItems = new QueuedDictionary<string, RoomItem>();
            _greenTeamItems = new QueuedDictionary<string, RoomItem>();
            _yellowTeamItems = new QueuedDictionary<string, RoomItem>();
            _room = room;
        }
        internal void UnlockGates()
        {
            /* TODO CHECK */
            foreach (var current in _redTeamItems.Values) UnlockGate(current);
            /* TODO CHECK */
            foreach (var current2 in _greenTeamItems.Values) UnlockGate(current2);
            /* TODO CHECK */
            foreach (var current3 in _blueTeamItems.Values) UnlockGate(current3);
            /* TODO CHECK */
            foreach (var current4 in _yellowTeamItems.Values) UnlockGate(current4);
        }
        private void UnlockGate(RoomItem item)
        {
            var interactionType = item.GetBaseItem().InteractionType;
            if (!InteractionTypes.AreFamiliar(GlobalInteractions.GameGate, interactionType)) return;

            /* TODO CHECK */
            foreach (var current in _room.GetGameMap().GetRoomUsers(new Point(item.X, item.Y)))
                current.SqState = 1;
            _room.GetGameMap().GameMap[item.X, item.Y] = 1;
        }

        internal int[] Points
        {
            get { return TeamPoints; }
            set { TeamPoints = value; }
        }


        internal async void OnCycle()
        {
            try
            {
                await Task.Yield();
                _redTeamItems.OnCycle();
                _blueTeamItems.OnCycle();
                _greenTeamItems.OnCycle();
                _yellowTeamItems.OnCycle();
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "GameManager Cycle");
            }
        }

        internal QueuedDictionary<string, RoomItem> GetItems(Team team)
        {
            switch (team)
            {
                case Team.Red:
                    return _redTeamItems;

                case Team.Green:
                    return _greenTeamItems;

                case Team.Blue:
                    return _blueTeamItems;

                case Team.Yellow:
                    return _yellowTeamItems;

                default:
                    return new QueuedDictionary<string, RoomItem>();
            }
        }

        internal Team GetWinningTeam()
        {
            var result = 1;
            var num = 0;
            for (var i = 1; i < 5; i++)
            {
                if (TeamPoints[i] <= num) continue;
                num = TeamPoints[i];
                result = i;
            }

            return (Team) result;
        }

        internal async Task AddPointToTeam(Team team, RoomUser user)
        {
            await AddPointToTeam(team, 1, user);
        }

        internal async Task AddPointToTeam(Team team, int points, RoomUser user)
        {
            /* TODO CHECK */
            foreach (
                var current in
                GetFurniItems(team).Values.Where(current => !IsSoccerGoal(current.GetBaseItem().InteractionType)))
            {
                current.ExtraData = TeamPoints[(int) team].ToString();
                await current.UpdateState();
            }

            await _room.GetWiredHandler().ExecuteWired(Interaction.TriggerScoreAchieved, user);
        }

        internal async Task Reset()
        {
            {
               await AddPointToTeam(Team.Blue, GetScoreForTeam(Team.Blue) * -1, null);
                await AddPointToTeam(Team.Green, GetScoreForTeam(Team.Green) * -1, null);
                await AddPointToTeam(Team.Red, GetScoreForTeam(Team.Red) * -1, null);
                await AddPointToTeam(Team.Yellow, GetScoreForTeam(Team.Yellow) * -1, null);
            }
        }

        internal void AddFurnitureToTeam(RoomItem item, Team team)
        {
            switch (team)
            {
                case Team.Red:
                    _redTeamItems.Add(item.Id, item);
                    break;

                case Team.Green:
                    _greenTeamItems.Add(item.Id, item);
                    break;

                case Team.Blue:
                    _blueTeamItems.Add(item.Id, item);
                    break;

                case Team.Yellow:
                    _yellowTeamItems.Add(item.Id, item);
                    break;

                default:
                    return;
            }

            UnlockGate(item);

        }

        internal void RemoveFurnitureFromTeam(RoomItem item, Team team)
        {
            switch (team)
            {
                case Team.Red:
                    _redTeamItems.Remove(item.Id);
                    return;

                case Team.Green:
                    _greenTeamItems.Remove(item.Id);
                    return;

                case Team.Blue:
                    _blueTeamItems.Remove(item.Id);
                    return;

                case Team.Yellow:
                    _yellowTeamItems.Remove(item.Id);
                    return;

                default:
                    return;
            }
        }

        internal RoomItem GetFirstScoreBoard(Team team)
        {
            QueuedDictionary<string, RoomItem> gameItems;
            Interaction interaction;

            switch (team)
            {
                case Team.Red:
                    interaction = Interaction.FreezeRedCounter;
                    gameItems = _redTeamItems;
                    break;
                case Team.Green:
                    interaction = Interaction.FreezeGreenCounter;
                    gameItems = _greenTeamItems;
                    break;
                case Team.Blue:
                    interaction = Interaction.FreezeBlueCounter;
                    gameItems = _blueTeamItems;
                    break;
                case Team.Yellow:
                    interaction = Interaction.FreezeYellowCounter;
                    gameItems = _yellowTeamItems;
                    break;
                case Team.None:
                default:
                    return null;
            }

            return gameItems.Values.FirstOrDefault(x => x.GetBaseItem().InteractionType == interaction);
        }
        

        internal async Task StopGame()
        {
            if (_room == null) return;
            var team = GetWinningTeam();
            var winners = new List<RoomUser>();
            switch (team)
            {
                case Team.Blue:
                    winners = GetRoom().GetTeamManagerForFreeze().BlueTeam;
                    break;

                case Team.Red:
                    winners = GetRoom().GetTeamManagerForFreeze().RedTeam;
                    break;

                case Team.Yellow:
                    winners = GetRoom().GetTeamManagerForFreeze().YellowTeam;
                    break;

                case Team.Green:
                    winners = GetRoom().GetTeamManagerForFreeze().GreenTeam;
                    break;
            }

            var item = GetFirstHighscore();
            if (item == null) return;
            var score = GetScoreForTeam(team);
            /* TODO CHECK */
            foreach (var winner in winners) 
                await item.HighscoreData.AddUserScore(item, winner.GetUserName(), score);
            await  item.UpdateState(false, true);
        }

        internal async Task StartGame()
        {
            GetRoom().GetWiredHandler().ResetExtraString(Interaction.ActionGiveScore);
        }

        internal Room GetRoom() => _room;

        internal async Task Destroy()
        {
            Array.Clear(TeamPoints, 0, TeamPoints.Length);
            _redTeamItems.Destroy();
            _blueTeamItems.Destroy();
            _greenTeamItems.Destroy();
            _yellowTeamItems.Destroy();
            TeamPoints = null;
            _redTeamItems = null;
            _blueTeamItems = null;
            _greenTeamItems = null;
            _yellowTeamItems = null;
            _room = null;
        }

        private static bool IsSoccerGoal(Interaction type) =>
            type == Interaction.FootballGoalBlue || type == Interaction.FootballGoalGreen ||
            type == Interaction.FootballGoalRed || type == Interaction.FootballGoalYellow;

        private int GetScoreForTeam(Team team) => TeamPoints[(int) team];

        private QueuedDictionary<string, RoomItem> GetFurniItems(Team team)
        {
            switch (team)
            {
                case Team.Red:
                    return _redTeamItems;

                case Team.Green:
                    return _greenTeamItems;

                case Team.Blue:
                    return _blueTeamItems;

                case Team.Yellow:
                    return _yellowTeamItems;

                default:
                    return new QueuedDictionary<string, RoomItem>();
            }
        }
        
        internal RoomItem GetFirstHighscore()
        {
            return _room.GetRoomItemHandler().FloorItems.Values.FirstOrDefault(current2 =>
                current2?.GetBaseItem().InteractionType == Interaction.WiredHighscore);
        }
    }
}