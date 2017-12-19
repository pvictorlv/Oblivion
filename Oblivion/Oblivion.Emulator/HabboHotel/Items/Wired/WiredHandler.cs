using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Handlers.Addons;
using Oblivion.HabboHotel.Items.Wired.Handlers.Conditions;
using Oblivion.HabboHotel.Items.Wired.Handlers.Effects;
using Oblivion.HabboHotel.Items.Wired.Handlers.Triggers;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items.Wired
{
    public class WiredHandler
    {
        private Room _room;

        private List<IWiredItem> _wiredItems;

        public WiredHandler(Room room)
        {
            //todo: change _wiredItems to triggers only
            _wiredItems = new List<IWiredItem>();
            Effects = new ConcurrentDictionary<int, List<IWiredItem>>();
            Conditions = new ConcurrentDictionary<int, List<IWiredItem>>();
            _room = room;
        }

        public static void OnEvent(IWiredItem item)
        {
            if (item.Item == null || item.Item.ExtraData == "1")
                return;

            item.Item.ExtraData = "1";
            item.Item.UpdateState(false, true);
            item.Item.ReqUpdate(1, true);
        }

        public IWiredItem GetRandomEffect(ICollection<IWiredItem> EffectList)
            => EffectList.OrderBy(x => Guid.NewGuid()).FirstOrDefault();


        public IWiredItem LoadWired(IWiredItem fItem)
        {
            if (fItem?.Item == null)
            {
                if (_wiredItems.Contains(fItem))
                    _wiredItems.Remove(fItem);
                return null;
            }

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT string,bool,delay,extra_string,extra_string_2,items FROM items_wireds WHERE id=@id LIMIT 1");
                queryReactor.AddParameter("id", fItem.Item.Id);

                var row = queryReactor.GetRow();

                if (row == null)
                {
                    var wiredItem = GenerateNewItem(fItem.Item);
                    AddWired(wiredItem);
                    SaveWired(wiredItem);

                    return wiredItem;
                }

                fItem.OtherString = row["string"].ToString();
                fItem.OtherBool = (row["bool"].ToString() == "1");
                fItem.Delay = (int) row["delay"];
                fItem.OtherExtraString = row["extra_string"].ToString();
                fItem.OtherExtraString2 = row["extra_string_2"].ToString();

                var array = row["items"].ToString().Split(';');

                /* TODO CHECK */
                foreach (var s in array)
                {
                    if (!int.TryParse(s, out var value))
                        continue;

                    var item = _room.GetRoomItemHandler().GetItem(Convert.ToUInt32(value));

                    fItem.Items.Add(item);
                }

                AddWired(fItem);
            }

            return fItem;
        }


        public bool OtherBoxHasItem(IWiredItem Box, RoomItem boxItem)
        {
            bool any = (from item in GetEffects(Box)
                where item.Item.Id != Box.Item.Id
                where item.Type == Interaction.ActionMoveRotate || item.Type == Interaction.ActionMoveToDir ||
                      item.Type == Interaction.ActionChase || item.Type == Interaction.ActionInverseChase
                where item.Items != null && item.Items.Count != 0
                select item).Any(item => item.Items.Contains(boxItem));

            return any;
        }

        public static void SaveWired(IWiredItem fItem)
        {
            if (fItem == null)
                return;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var text = string.Empty;
                var num = 0;

                /* TODO CHECK */
                foreach (var current in fItem.Items)
                {
                    if (num != 0) text += ";";
                    text += current.Id;
                    num++;
                }

                if (fItem.OtherString == null)
                    fItem.OtherString = string.Empty;
                if (fItem.OtherExtraString == null)
                    fItem.OtherExtraString = string.Empty;
                if (fItem.OtherExtraString2 == null)
                    fItem.OtherExtraString2 = string.Empty;

                queryReactor.SetQuery(
                    "REPLACE INTO items_wireds VALUES (@id, @items, @delay, @string, @bool, @extrastring, @extrastring2)");
                queryReactor.AddParameter("id", fItem.Item.Id);
                queryReactor.AddParameter("items", text);
                queryReactor.AddParameter("delay", fItem.Delay);
                queryReactor.AddParameter("string", fItem.OtherString);
                queryReactor.AddParameter("bool", Oblivion.BoolToEnum(fItem.OtherBool));
                queryReactor.AddParameter("extrastring",
                    (fItem.OtherExtraString.Length > 255)
                        ? fItem.OtherExtraString.Substring(0, 255)
                        : fItem.OtherExtraString);
                queryReactor.AddParameter("extrastring2",
                    (fItem.OtherExtraString2.Length > 255)
                        ? fItem.OtherExtraString2.Substring(0, 255)
                        : fItem.OtherExtraString2);
                queryReactor.RunQuery();
            }
        }

        public void ReloadWired(IWiredItem item)
        {
            SaveWired(item);
            RemoveWired(item);
            AddWired(item);
        }

        public void ResetExtraString(Interaction type)
        {
            lock (_wiredItems)
            {
                /* TODO CHECK */
                foreach (var current in _wiredItems.Where(current => current != null && current.Type == type))
                    current.OtherExtraString = "0";
            }
        }

        public bool ExecuteWired(Interaction type, params object[] stuff)
        {
            try
            {
                if (!IsTrigger(type) || stuff == null)
                    return false;

                var b = false;
                foreach (var current in _wiredItems)
                {
                    if (current != null && current.Type == type && current.Execute(stuff))
                        b = true;
                }
                return b;
            }
            catch (Exception e)
            {
                Writer.Writer.HandleException(e, "WiredHandler.cs:ExecuteWired Type: " + type);
            }

            return false;
        }

        public void OnCycle()
        {
            if (_wiredItems == null || _room == null || _wiredItems.Count <= 0)
                return;

            var wireds = _wiredItems.ToList();
            foreach (var item in wireds)
            {
                try
                {
                    if (item?.Item == null)
                        continue;

                    var selectedItem = _room.GetRoomItemHandler().GetItem(item.Item.Id);

                    if (selectedItem == null)
                    {
                        RemoveWired(item);
                        continue;
                    }


                    var cycle = item as IWiredCycler;
                    if (cycle == null) continue;
                    //todo: queue
                    if (cycle.TickCount <= 0)
                    {
                        cycle.OnCycle();
                    }
                    else
                        cycle.TickCount--;
                }
                catch (Exception e)
                {

                    Writer.Writer.HandleException(e, "WiredHandler.cs:OnCycle, ROOM ID: " + _room.RoomId);
                    if (item != null)
                        _wiredItems?.Remove(item);
                }
            }
            wireds.Clear();
        }


        public void AddWired(IWiredItem item)
        {
            if (item == null) return;

            if (_wiredItems.Contains(item))
                return;
            _wiredItems.Add(item);
            var coord = new Point(item.Item.X, item.Item.Y);
            var point = Formatter.PointToInt(coord);
            if (IsEffect(item.Type))
            {
                if (Effects.TryGetValue(point, out var items))
                {
                    items.Add(item);
                    Effects[point] = items;
                }
                else
                {
                    items = new List<IWiredItem> {item};
                    Effects.TryAdd(point, items);
                }
            }
            else if (IsCondition(item.Type))
            {
                if (Conditions.TryGetValue(point, out var items))
                {
                    items.Add(item);
                    Conditions[point] = items;
                }
                else
                {
                    items = new List<IWiredItem> { item };
                    Conditions.TryAdd(point, items);
                }
            }
        }

        public void RemoveWired(IWiredItem item)
        {
//            var point = new Point(item.Item.oldX, item.Item.oldY);
            var coord = new Point(item.Item.X, item.Item.Y);
            var point = Formatter.PointToInt(coord);

            if (_wiredItems.Contains(item))
                _wiredItems.Remove(item);

            if (IsEffect(item.Type))
            {
                if (Effects.TryGetValue(point, out var items))
                {
                    items.Remove(item);
                    if (items.Count <= 0)
                    {
                        Effects.TryRemove(point, out _);
                    }
                    else
                    {
                        Effects[point] = items;
                    }
                }
            }
            else if (IsCondition(item.Type))
            {
                if (Conditions.TryGetValue(point, out var items))
                {
                    items.Remove(item);
                    if (items.Count <= 0)
                    {
                        Conditions.TryRemove(point, out _);
                    }
                    else
                    {
                        Conditions[point] = items;
                    }
                }
            }
        }

        public IWiredItem ReloadWired(RoomItem item)
        {
            IWiredItem current = _wiredItems?.FirstOrDefault(x => x?.Item?.Id == item.Id);
            if (current == null)
            {
                var wired = GenerateNewItem(item);
                AddWired(wired);
                return null;
            }
            
            var coord = new Point(current.Item.X, current.Item.Y);
            var point = Formatter.PointToInt(coord);

            if (_wiredItems.Contains(current))
                _wiredItems.Remove(current);

            if (IsEffect(current.Type))
            {
                if (Effects.TryGetValue(point, out var items))
                {
                    items.Remove(current);
                    if (items.Count <= 0)
                    {
                        Effects.TryRemove(point, out _);
                    }
                    else
                    {
                        Effects[point] = items;
                    }
                }
            }
            else if (IsCondition(current.Type))
            {
                if (Conditions.TryGetValue(point, out var items))
                {
                    items.Remove(current);
                    if (items.Count <= 0)
                    {
                        Conditions.TryRemove(point, out _);
                    }
                    else
                    {
                        Conditions[point] = items;
                    }
                }
            }

            return current;
        }

        public void RemoveWired(RoomItem item)
        {
            var current = _wiredItems?.FirstOrDefault(x => x?.Item?.Id == item.Id);
            if (current == null)
                return;
//            _wiredItems.Remove(current);
            RemoveWired(current);

            current.Items.Clear();
            current.Items = null;
            current.Item = null;
            current.Room = null;
        }

        public IWiredItem GenerateNewItem(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.TriggerTimer:
                    return new TimerTrigger(item, _room);

                case Interaction.TriggerRoomEnter:
                    return new UserEntersRoom(item, _room);

                case Interaction.TriggerGameEnd:
                    return new GameEnds(item, _room);

                case Interaction.TriggerGameStart:
                    return new GameStarts(item, _room);

                case Interaction.TriggerRepeater:
                    return new Repeater(item, _room);

                case Interaction.TriggerLongRepeater:
                    return new LongRepeater(item, _room);

                case Interaction.TriggerOnUserSay:
                    return new SaysKeyword(item, _room);
                case Interaction.TriggerOnUserSayCommand:
                    return new UserSaysCommand(item, _room);
                case Interaction.TriggerScoreAchieved:
                    return new ScoreAchieved(item, _room);

                case Interaction.TriggerStateChanged:
                    return new FurniStateToggled(item, _room);

                case Interaction.TriggerWalkOnFurni:
                    return new WalksOnFurni(item, _room);

                case Interaction.TriggerWalkOffFurni:
                    return new WalksOffFurni(item, _room);

                case Interaction.TriggerCollision:
                    return new Collision(item, _room);

                case Interaction.ActionMoveRotate:
                    return new MoveRotateFurni(item, _room);

                case Interaction.ActionMoveToDir:
                    return new MoveToDir(item, _room);

                case Interaction.ActionShowMessage:
                    return new ShowMessage(item, _room);

                case Interaction.ActionEffectUser:
                    return new EffectUser(item, _room);

                case Interaction.ActionEnableDance:
                    return new EnableDance(item, _room);

                case Interaction.ActionHandItem:
                    return new GiveHandItem(item, _room);

                case Interaction.ActionFreezeUser:
                    return new FreezeUser(item, _room);

                case Interaction.ActionRegenMap:
                    return new RegenMap(item, _room);

                case Interaction.ActionRollerSpeed:
                    return new RollerSpeed(item, _room);

                case Interaction.ActionTeleportTo:
                    return new TeleportToFurni(item, _room);

                case Interaction.ActionToggleState:
                    return new ToggleFurniState(item, _room);

                case Interaction.ActionResetTimer:
                    return new ResetTimers(item, _room);

                case Interaction.ActionKickUser:
                    return new KickUser(item, _room);

                case Interaction.ConditionFurnisHaveUsers:
                    return new FurniHasUsers(item, _room);

                // Condi��es Novas


                case Interaction.ConditionUserIsInTeam:
                    return new UserIsInTeam(item, _room);

                case Interaction.ConditionUserIsNotInTeam:
                    return new UserIsNotInTeam(item, _room);


                case Interaction.ConditionItemsMatches:
                    return new ItemsCoincide(item, _room);

                case Interaction.ConditionFurniTypeMatches:
                    return new ItemsTypeMatches(item, _room);

                case Interaction.ConditionHowManyUsersInRoom:
                    return new HowManyUsers(item, _room);

                case Interaction.ConditionGroupMember:
                    return new IsGroupMember(item, _room);

                case Interaction.ConditionTriggerOnFurni:
                    return new TriggererOnFurni(item, _room);

                case Interaction.ConditionFurniHasFurni:
                    return new FurniHasFurni(item, _room);

                case Interaction.ConditionUserWearingEffect:
                    return new UserIsWearingEffect(item, _room);

                case Interaction.ConditionUserWearingBadge:
                    return new UserIsWearingBadge(item, _room);

                case Interaction.ConditionUserHasFurni:
                    return new UserHasFurni(item, _room);

                case Interaction.ConditionDateRangeActive:
                    return new DateRangeActive(item, _room);

                case Interaction.ConditionTimeMoreThan:
                    return new TimeMoreThan(item, _room);

                case Interaction.ConditionTimeLessThan:
                    return new TimeLessThan(item, _room);

                // Condi��es Negativas
                case Interaction.ConditionTriggererNotOnFurni:
                    return new TriggererNotOnFurni(item, _room);

                case Interaction.ConditionFurniHasNotFurni:
                    return new FurniHasNotFurni(item, _room);

                case Interaction.ConditionFurnisHaveNotUsers:
                    return new FurniHasNotUsers(item, _room);

                case Interaction.ConditionItemsDontMatch:
                    return new ItemsNotCoincide(item, _room);

                case Interaction.ConditionFurniTypeDontMatch:
                    return new ItemsTypeDontMatch(item, _room);

                case Interaction.ConditionNotGroupMember:
                    return new IsNotGroupMember(item, _room);

                case Interaction.ConditionNegativeHowManyUsers:
                    return new NotHowManyUsersInRoom(item, _room);

                case Interaction.ConditionUserNotWearingEffect:
                    return new UserIsNotWearingEffect(item, _room);

                case Interaction.ConditionUserNotWearingBadge:
                    return new UserIsNotWearingBadge(item, _room);

                // Efeitos Novos
                case Interaction.ActionGiveReward:
                    return new GiveReward(item, _room);

                case Interaction.ActionPosReset:
                    return new ResetPosition(item, _room);

                case Interaction.ActionGiveScore:
                    return new GiveScore(item, _room);

                case Interaction.ActionMuteUser:
                    return new MuteUser(item, _room);

                case Interaction.ActionJoinTeam:
                    return new JoinTeam(item, _room);

                case Interaction.ActionLeaveTeam:
                    return new LeaveTeam(item, _room);

                case Interaction.ActionChase:
                    return new Chase(item, _room);

                case Interaction.ActionInverseChase:
                    return new InverseChase(item, _room);

                case Interaction.ActionCallStacks:
                    return new CallStacks(item, _room);

                // Bots
                case Interaction.TriggerBotReachedStuff:
                    return new BotReachedStuff(item, _room);

                case Interaction.TriggerBotReachedAvatar:
                    return new BotReachedAvatar(item, _room);

                case Interaction.ActionBotClothes:
                    return new BotClothes(item, _room);

                case Interaction.ActionBotFollowAvatar:
                    return new BotFollowAvatar(item, _room);

                case Interaction.ActionBotGiveHanditem:
                    return new BotGiveHanditem(item, _room);

                case Interaction.ActionBotMove:
                    return new BotMove(item, _room);

                case Interaction.ActionBotTalk:
                    return new BotTalk(item, _room);

                case Interaction.ActionBotTalkToAvatar:
                    return new BotTalkToAvatar(item, _room);

                case Interaction.ActionBotTeleport:
                    return new BotTeleport(item, _room);

                case Interaction.ConditionUserHasHanditem:
                    return new UserHasHanditem(item, _room);
                case Interaction.SpecialRandom:
                    return new SpecialRandom(item, _room);
            }

            return null;
        }

        public List<IWiredItem> GetConditions(IWiredItem item)
        {
            var point = new Point(item.Item.X, item.Item.Y);
            var coord = Formatter.PointToInt(point);
            if (!Conditions.TryGetValue(coord, out var items))
            {
                return new List<IWiredItem>();
            }

            return items;
        }

        public ConcurrentDictionary<int, List<IWiredItem>> Effects;
        public ConcurrentDictionary<int, List<IWiredItem>> Conditions;

        public bool OnUserFurniCollision(Room Instance, RoomItem Item)
        {
            if (Instance == null || Item == null)
                return false;

            foreach (var point in Item.GetSides())
            {
                if (Instance.GetGameMap().SquareHasUsers(point.X, point.Y))
                {
                    List<RoomUser> users = Instance.GetGameMap().GetRoomUsers(point);
                    foreach (RoomUser User in users)
                        ExecuteWired(Interaction.TriggerCollision, User, Item);
                }
            }


            return true;
        }

        public List<IWiredItem> GetEffects(IWiredItem item)
        {
            var point = new Point(item.Item.X, item.Item.Y);
            var coord = Formatter.PointToInt(point);

            if (!Effects.TryGetValue(coord, out var items))
            {
                return new List<IWiredItem>();
            }

            return items;
        }

        public IWiredItem GetWired(RoomItem item)
        {
            return _wiredItems.FirstOrDefault(current => current != null && item.Id == current.Item.Id);
        }

        public List<IWiredItem> GetWiredsByType(Interaction type, Point coord) => _wiredItems
            .Where(item => item != null && item.Type == type && item.Item.X == coord.X && item.Item.Y == coord.Y)
            .ToList();

        public List<IWiredItem> GetWiredsByTypes(GlobalInteractions type) => _wiredItems
            .Where(item => item != null && InteractionTypes.AreFamiliar(type, item.Item.GetBaseItem().InteractionType))
            .ToList();

        public void Destroy()
        {
            foreach (var current in _wiredItems.ToList())
            {
                if (current == null) continue;
                current.Items?.Clear();
                current.Items = null;
                current.Item = null;
                current.Room = null;
            }

            _wiredItems.Clear();
            _wiredItems = null;
            _room = null;
            Effects.Clear();
            Conditions.Clear();
            Effects = null;
            Conditions = null;
        }

        private static bool IsTrigger(Interaction type) => InteractionTypes.AreFamiliar(GlobalInteractions.WiredTrigger,
            type);

        private static bool IsEffect(Interaction type) => InteractionTypes.AreFamiliar(GlobalInteractions.WiredEffect,
            type);

        private static bool IsCondition(Interaction type) => InteractionTypes.AreFamiliar(
            GlobalInteractions.WiredCondition, type);
    }
}