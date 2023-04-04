using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Freeze.Enum;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Rooms.Items.Games.Types.Freeze
{
    internal class Freeze
    {
        private ConcurrentDictionary<string, RoomItem> _freezeTiles, _freezeBlocks;
        private Random _rnd;
        private Room _room;

        public Freeze(Room room)
        {
            _room = room;
            _freezeTiles = new ConcurrentDictionary<string, RoomItem>();
            _freezeBlocks = new ConcurrentDictionary<string, RoomItem>();
            ExitTeleport = null;
            _rnd = new Random();
            GameStarted = false;
        }

        public bool IsFreezeActive => GameStarted;

        internal bool GameStarted { get; private set; }

        internal RoomItem ExitTeleport { get; set; }

        public async Task CycleUser(RoomUser user)
        {
            if (user.Freezed)
            {
                {
                    ++user.FreezeCounter;
                }
                if (user.FreezeCounter > 10)
                {
                    user.Freezed = false;
                    user.FreezeCounter = 0;
                    await ActivateShield(user);
                }
            }
            if (!user.ShieldActive) return;

            {
                ++user.ShieldCounter;
            }
            if (user.ShieldCounter <= 10) return;
            user.ShieldActive = false;
            user.ShieldCounter = 10;
            await user.ApplyEffect((int) (user.Team + 39));
        }
        

        internal async Task StartGame()
        {
            GameStarted = true;
            await CountTeamPoints();
            await ResetGame();
//            _room.GetGameManager().LockGates();
            await _room.GetGameManager().StartGame();

            if (ExitTeleport == null) return;

            /* TODO CHECK */ foreach (
                var user in
                    _freezeTiles.Values.Select(
                        tile => _room.GetGameMap().GetRoomUsers(new Point(tile.X, tile.Y)))
                        .SelectMany(users => users.Where(user => user != null && user.Team == Team.None)))
                await _room.GetGameMap().TeleportToItem(user, ExitTeleport);
        }

        internal async Task StopGame()
        {
            GameStarted = false;
            _room.GetGameManager().UnlockGates();
            await _room.GetGameManager().StopGame();
            var winningTeam = _room.GetGameManager().GetWinningTeam();
            /* TODO CHECK */ foreach (var avatar in _room.GetRoomUserManager().UserList.Values)
            {
                avatar.FreezeLives = 0;
                if (avatar.Team != winningTeam) continue;
                await avatar.UnIdle();
                avatar.DanceId = 0;
                var waveAtWin = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
                await waveAtWin.AppendIntegerAsync(avatar.VirtualId);
                await waveAtWin.AppendIntegerAsync(1);
                await _room.SendMessage(waveAtWin);
            }
        }

        internal async Task ResetGame()
        {
            /* TODO CHECK */
            foreach (var roomItem in _freezeBlocks.Values)
            {
                if (!string.IsNullOrEmpty(roomItem.ExtraData) && !roomItem.GetBaseItem().InteractionType.Equals(Interaction.FreezeBlueGate) && (!roomItem.GetBaseItem().InteractionType.Equals(Interaction.FreezeRedGate) && !roomItem.GetBaseItem().InteractionType.Equals(Interaction.FreezeGreenGate)) && !roomItem.GetBaseItem().InteractionType.Equals(Interaction.FreezeYellowGate))
                {
                    roomItem.ExtraData = string.Empty;
                    await roomItem.UpdateState(false, true);
                    _room.GetGameMap().AddItemToMap(roomItem, false);
                }
            }
        }

        internal async Task OnUserWalk(RoomUser user)
        {
            if (!GameStarted || user == null || user.Team == Team.None) return;

            if (user.X == user.GoalX && user.GoalY == user.Y && user.ThrowBallAtGoal)
            {
                /* TODO CHECK */
                foreach (var roomItem in _freezeTiles.Values)
                {
                    if (roomItem.InteractionCountHelper == 0 && (roomItem.X == user.X && roomItem.Y == user.Y))
                    {
                        roomItem.InteractionCountHelper = 1;
                        roomItem.ExtraData = "1000";
                        await roomItem.UpdateState();
                        roomItem.InteractingUser = user.UserId;
                        roomItem.FreezePowerUp = user.BanzaiPowerUp;
                        roomItem.ReqUpdate(4, true);
                        switch (user.BanzaiPowerUp)
                        {
                            case FreezePowerUp.GreenArrow:
                            case FreezePowerUp.OrangeSnowball:
                                user.BanzaiPowerUp = FreezePowerUp.None;
                                break;
                        }
                    }
                }
            }
            /* TODO CHECK */
            foreach (var roomItem in _freezeBlocks.Values)
            {
                if (roomItem != null && user.X == roomItem.X && user.Y == roomItem.Y && roomItem.FreezePowerUp != FreezePowerUp.None) 
                    await PickUpPowerUp(roomItem, user);
            }
        }

        internal async Task OnFreezeTiles(RoomItem item, FreezePowerUp powerUp, uint userId)
        {
            if (_room.GetRoomUserManager().GetRoomUserByHabbo(userId) == null || item == null) return;

            List<RoomItem> items;
            switch (powerUp)
            {
                case FreezePowerUp.BlueArrow:
                    items = await GetVerticalItems(item.X, item.Y, 5);
                    break;

                case FreezePowerUp.GreenArrow:
                    items = await GetDiagonalItems(item.X, item.Y, 5);
                    break;

                case FreezePowerUp.OrangeSnowball:
                    items = await GetVerticalItems(item.X, item.Y, 5);
                    items.AddRange(await GetDiagonalItems(item.X, item.Y, 5));
                    break;

                default:
                    items = await GetVerticalItems(item.X, item.Y, 3);
                    break;
            }
            await HandleBanzaiFreezeItems(items);
        }

        internal void AddFreezeTile(RoomItem item)
        {
            _freezeTiles.AddOrUpdate(item.Id, item, (k, v) => item);
        }

        internal void RemoveFreezeTile(string itemId)
        {
            _freezeTiles.TryRemove(itemId, out _);
        }

        internal void AddFreezeBlock(RoomItem item)
        {
            _freezeBlocks.AddOrUpdate(item.Id, item, (k, v) => item);
        }

        internal void RemoveFreezeBlock(string itemId)
        {
            _freezeBlocks.TryRemove(itemId, out _);
        }

        internal void Destroy()
        {
            _freezeBlocks.Clear();
            _freezeTiles.Clear();
            _room = null;
            _freezeTiles = null;
            _freezeBlocks = null;
            ExitTeleport = null;
            _rnd = null;
        }
        

        private static async Task ActivateShield(RoomUser user)
        {
            await user.ApplyEffect((int) (user.Team + 48));
            user.ShieldActive = true;
            user.ShieldCounter = 0;
        }

        private static bool SquareGotFreezeTile(IEnumerable<RoomItem> items)
        {
            return items.Any(roomItem => roomItem != null && roomItem.GetBaseItem().InteractionType == Interaction.FreezeTile);
        }

        private static bool SquareGotFreezeBlock(IEnumerable<RoomItem> items)
        {
            return
                items.Any(
                    roomItem =>
                        roomItem != null && roomItem.GetBaseItem().InteractionType == Interaction.FreezeTileBlock);
        }

        private async Task CountTeamPoints()
        {
            await _room.GetGameManager().Reset();
            /* TODO CHECK */
            foreach (var roomUser in _room.GetRoomUserManager()
                         .UserList.Values)
            {
                if (!roomUser.IsBot && roomUser.Team != Team.None && roomUser.GetClient() != null)
                {
                    roomUser.BanzaiPowerUp = FreezePowerUp.None;
                    roomUser.FreezeLives = 3;
                    roomUser.ShieldActive = false;
                    roomUser.ShieldCounter = 11;
                    await _room.GetGameManager().AddPointToTeam(roomUser.Team, 30, null);
                    var serverMessage = new ServerMessage();
                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("UpdateFreezeLivesMessageComposer"));
                    await serverMessage.AppendIntegerAsync(roomUser.InternalRoomId);
                    await serverMessage.AppendIntegerAsync(roomUser.FreezeLives);
                    await roomUser.GetClient().SendMessageAsync(serverMessage);
                }
            }
        }

        private async Task HandleBanzaiFreezeItems(IEnumerable<RoomItem> items)
        {
            /* TODO CHECK */ foreach (var roomItem in items)
            {
                switch (roomItem.GetBaseItem().InteractionType)
                {
                    case Interaction.FreezeTileBlock:
                        await SetRandomPowerUp(roomItem);
                        await roomItem.UpdateState(false, true);
                        continue;
                    case Interaction.FreezeTile:
                        roomItem.ExtraData = "11000";
                        await roomItem.UpdateState(false, true);
                        continue;
                    default:
                        continue;
                }
            }
        }

        private async Task SetRandomPowerUp(RoomItem item)
        {
            if (!string.IsNullOrEmpty(item.ExtraData)) return;
            switch (_rnd.Next(1, 14))
            {
                case 2:
                    item.ExtraData = "2000";
                    item.FreezePowerUp = FreezePowerUp.BlueArrow;
                    break;

                case 3:
                    item.ExtraData = "3000";
                    item.FreezePowerUp = FreezePowerUp.Snowballs;
                    break;

                case 4:
                    item.ExtraData = "4000";
                    item.FreezePowerUp = FreezePowerUp.GreenArrow;
                    break;

                case 5:
                    item.ExtraData = "5000";
                    item.FreezePowerUp = FreezePowerUp.OrangeSnowball;
                    break;

                case 6:
                    item.ExtraData = "6000";
                    item.FreezePowerUp = FreezePowerUp.Heart;
                    break;

                case 7:
                    item.ExtraData = "7000";
                    item.FreezePowerUp = FreezePowerUp.Shield;
                    break;

                default:
                    item.ExtraData = "1000";
                    item.FreezePowerUp = FreezePowerUp.None;
                    break;
            }
            await _room.GetGameMap().RemoveFromMap(item, false);
            await  item.UpdateState(false, true);
        }

        private async Task PickUpPowerUp(RoomItem item, RoomUser user)
        {
            switch (item.FreezePowerUp)
            {
                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:
                    user.BanzaiPowerUp = item.FreezePowerUp;
                    break;

                case FreezePowerUp.Shield:
                    await ActivateShield(user);
                    break;

                case FreezePowerUp.Heart:
                    if (user.FreezeLives < 5)
                    {
                        {
                            ++user.FreezeLives;
                        }
                        await _room.GetGameManager().AddPointToTeam(user.Team, 10, user);
                    }
                    var serverMessage = new ServerMessage();
                    await serverMessage.InitAsync(LibraryParser.OutgoingRequest("UpdateFreezeLivesMessageComposer"));
                    await serverMessage.AppendIntegerAsync(user.InternalRoomId);
                    await serverMessage.AppendIntegerAsync(user.FreezeLives);
                    await user.GetClient().SendMessageAsync(serverMessage);
                    break;
            }
            item.FreezePowerUp = FreezePowerUp.None;
            item.ExtraData = $"1{item.ExtraData}";
            await  item.UpdateState(false, true);
        }

        private async Task HandleUserFreeze(Point point)
        {
            var users = _room.GetGameMap().GetRoomUsers(point);
            if (users.Count <= 0)
            {
                return;
                
            }

            var user = users[0];

            if (user == null || user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                return;

            await FreezeUser(user);
        }

        private async Task FreezeUser(RoomUser user)
        {
            if (user.IsBot || user.ShieldActive || user.Team == Team.None || user.Freezed)
                return;

            user.Freezed = true;
            user.FreezeCounter = 0;
            --user.FreezeLives;

            if (user.FreezeLives <= 0)
            {
                var serverMessage = new ServerMessage();
                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("UpdateFreezeLivesMessageComposer"));
                await serverMessage.AppendIntegerAsync(user.InternalRoomId);
                await serverMessage.AppendIntegerAsync(user.FreezeLives);
                await user.GetClient().SendMessageAsync(serverMessage);
                await user.ApplyEffect(-1);
                await _room.GetGameManager().AddPointToTeam(user.Team, -10, user);
                var managerForFreeze = _room.GetTeamManagerForFreeze();
                managerForFreeze.OnUserLeave(user);
                user.Team = Team.None;
                if (ExitTeleport != null) await _room.GetGameMap().TeleportToItem(user, ExitTeleport);
                user.Freezed = false;
                user.SetStep = false;
                user.IsWalking = false;
                user.UpdateNeeded = true;
                if (!managerForFreeze.BlueTeam.Any() && !managerForFreeze.RedTeam.Any() &&
                    !managerForFreeze.GreenTeam.Any() && managerForFreeze.YellowTeam.Any())
                    await StopGame();
                else if (managerForFreeze.BlueTeam.Any() && !managerForFreeze.RedTeam.Any() &&
                         !managerForFreeze.GreenTeam.Any() && !managerForFreeze.YellowTeam.Any())
                    await StopGame();
                else if (!managerForFreeze.BlueTeam.Any() && managerForFreeze.RedTeam.Any() &&
                         !managerForFreeze.GreenTeam.Any() && !managerForFreeze.YellowTeam.Any())
                    await StopGame();
                else
                {
                    if (managerForFreeze.BlueTeam.Any() || managerForFreeze.RedTeam.Any() ||
                        !managerForFreeze.GreenTeam.Any() || managerForFreeze.YellowTeam.Any())
                        return;
                    await StopGame();
                }
            }
            else
            {
                await _room.GetGameManager().AddPointToTeam(user.Team, -10, user);
                await user.ApplyEffect(12);
                var serverMessage = new ServerMessage();
                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("UpdateFreezeLivesMessageComposer"));
                await serverMessage.AppendIntegerAsync(user.InternalRoomId);
                await serverMessage.AppendIntegerAsync(user.FreezeLives);
                await user.GetClient().SendMessageAsync(serverMessage);
            }
        }

        private async Task<List<RoomItem>> GetVerticalItems(int x, int y, int length)
        {
            var list = new List<RoomItem>();
            var num1 = 0;
            Point point;
            while (num1 < length)
            {
                point = new Point((x + num1), y);

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                   await HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num1;
                    else break;
                }
                else break;
            }
            var num2 = 1;
            while (num2 < length)
            {
                point = new Point(x, (y + num2));

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                    await HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num2;
                    else break;
                }
                else break;
            }
            var num3 = 1;
            while (num3 < length)
            {
                point = new Point((x - num3), y);

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                    await HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num3;
                    else break;
                }
                else break;
            }
            var num4 = 1;
            while (num4 < length)
            {
                point = new Point(x, (y - num4));

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                    await HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num4;
                    else break;
                }
                else break;
            }
            return list;
        }

        private async Task<List<RoomItem>> GetDiagonalItems(int x, int y, int length)
        {
            var list = new List<RoomItem>();
            var num1 = 0;
            Point point;
            while (num1 < length)
            {
                point = new Point((x + num1), checked(y + num1));

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                  await  HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num1;
                    else break;
                }
                else break;
            }
            var num2 = 0;
            while (num2 < length)
            {
                point = new Point((x - num2), checked(y - num2));

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                    await HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num2;
                    else break;
                }
                else break;
            }
            var num3 = 0;
            while (num3 < length)
            {
                point = new Point((x - num3), checked(y + num3));

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                    await HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num3;
                    else break;
                }
                else break;
            }
            var num4 = 0;
            while (num4 < length)
            {
                point = new Point((x + num4), checked(y - num4));

                var itemsForSquare = GetItemsForSquare(point);
                if (SquareGotFreezeTile(itemsForSquare))
                {
                    await HandleUserFreeze(point);
                    list.AddRange(itemsForSquare);
                    if (!SquareGotFreezeBlock(itemsForSquare)) ++num4;
                    else break;
                }
                else break;
            }
            return list;
        }

        private ConcurrentList<RoomItem> GetItemsForSquare(Point point) => _room.GetGameMap().GetCoordinatedItems(point);
    }
}