using System.Linq;
using System.Text.RegularExpressions;
using Oblivion.Collections;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Wired
{
    public static class WiredSaver
    {
        public static void SaveWired(GameClient session, RoomItem item, ClientMessage request)
        {
            if (item == null || !item.IsWired)
                return;

            var room = item.GetRoom();

            var wiredHandler = room?.GetWiredHandler();

            var wired = wiredHandler?.GetWired(item);
            if (wired == null)
            {
                return;
            }
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.TriggerTimer:
                case Interaction.ActionLeaveTeam:
                {
                    request.GetInteger();
                    var delay = (request.GetInteger() / 2);
                    wired.Delay = delay;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.TriggerRoomEnter:
                {
                    request.GetInteger();
                    var otherString = request.GetString();
                    wired.OtherString = otherString;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.TriggerLongRepeater:
                {
                    request.GetInteger();
                    var delay = request.GetInteger() * 5000;
                    wired.Delay = delay;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.TriggerRepeater:
                {
                    request.GetInteger();
                    var delay = (request.GetInteger() / 2) * 1000;
                    wired.Delay = delay;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.TriggerOnUserSay:
                case Interaction.TriggerOnUserSayCommand:
                {
                    request.GetInteger();
                    var num = request.GetInteger();
                    var otherString2 = request.GetString();
                    wired.OtherString = otherString2;
                    wired.OtherBool = (num == 1);
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.TriggerStateChanged:
                {
                    request.GetInteger();
                    request.GetString();
                    var furniItems = GetFurniItems(request, room);
                    var num2 = request.GetInteger();
                    wired.Delay = num2 * 500;
                    wired.Items = furniItems;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.TriggerWalkOnFurni:
                case Interaction.ActionChase:
                case Interaction.ActionInverseChase:
                case Interaction.ActionResetTimer:
                {
                    request.GetInteger();
                    request.GetString();
                    var furniItems2 = GetFurniItems(request, room);
                    var num3 = request.GetInteger();
                    wired.Delay = num3;
                    wired.Items = furniItems2;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.TriggerWalkOffFurni:
                {
                    request.GetInteger();
                    request.GetString();
                    var furniItems3 = GetFurniItems(request, room);
                    var num4 = request.GetInteger();
                    wired.Delay = num4 * 500;
                    wired.Items = furniItems3;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionMoveRotate:
                case Interaction.ActionMoveToDir:
                {
                    request.GetInteger();
                    var dir = request.GetInteger();
                    var rot = request.GetInteger();
                    request.GetString();
                    var furniItems4 = GetFurniItems(request, room);
                    var delay = request.GetInteger();
                    wired.Items = furniItems4;
                    wired.Delay = delay * 500;
                    wired.OtherString = $"{dir};{rot}";
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionShowMessage:
                case Interaction.ActionRollerSpeed:
                case Interaction.ActionKickUser:
                {
                    request.GetInteger();
                    var otherString3 = request.GetString();
                    request.GetInteger();
                    var delay = request.GetInteger();
                    wired.Delay = delay;
                    wired.OtherString = otherString3;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionHandItem:
                case Interaction.ActionEffectUser:
                {
                    request.GetInteger();
                    var otherString3 = Regex.Match(request.GetString(), @"\d+").Value;
                    wired.OtherString = otherString3;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ActionEnableDance:
                {
                    request.GetInteger();
                    var otherString3 = Regex.Match(request.GetString(), @"\d+").Value;
                    if (!int.TryParse(otherString3, out int numb))
                    {
                        numb = 0;
                    }
                    if (numb > 4)
                    {
                        otherString3 = "4";
                    }
                    else if (numb < 0)
                    {
                        otherString3 = "0";
                    }
                    wired.OtherString = otherString3;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionTeleportTo:
                {
                    request.GetInteger();
                    request.GetString();
                    var furniItems5 = GetFurniItems(request, room);
                    var num8 = request.GetInteger();
                    wired.Items = furniItems5;
                    wired.Delay = num8 * 500;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionToggleState:
                {
                    request.GetInteger();
                    request.GetString();
                    var furniItems6 = GetFurniItems(request, room);
                    var num9 = request.GetInteger();
                    wired.Items = furniItems6;
                    wired.Delay = num9 * 500;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ActionBotFollowAvatar:
                {
                    request.GetInteger();
                    wired.OtherBool = request.GetInteger() == 1;
                    wired.OtherString = request.GetString();
                    request.GetInteger();
                    wired.Delay = request.GetInteger();
                   
                    break;
                }
                case Interaction.ActionGiveReward:
                {
                    if (!session.GetHabbo().HasFuse("fuse_use_superwired"))
                        return;

                    request.GetInteger();
                    var often = request.GetInteger();
                    var unique = request.GetIntegerAsBool();
                    var limit = request.GetInteger();
                    request.GetInteger();
                    var extrainfo = request.GetString();
                    var furniItems7 = GetFurniItems(request, room);
                    wired.Items = furniItems7;
                    wired.Delay = 0;
                    wired.OtherBool = unique;
                    wired.OtherString = extrainfo;
                    wired.OtherExtraString = often.ToString();
                    wired.OtherExtraString2 = limit.ToString();
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionMuteUser:
                {
                    request.GetInteger();
                    var minutes = request.GetInteger() * 500;
                    var message = request.GetString();
                    var furniItems7 = GetFurniItems(request, room);
                    wired.Items = furniItems7;
                    wired.Delay = minutes;
                    wired.OtherBool = false;
                    wired.OtherString = message;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.TriggerScoreAchieved:
                {
                    request.GetInteger();
                    var pointsRequired = request.GetInteger();

                    wired.Delay = 0;
                    wired.OtherString = pointsRequired.ToString();
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ConditionItemsMatches:
                case Interaction.ConditionItemsDontMatch:
                case Interaction.ActionPosReset:
                {
                    request.GetInteger();
                    var actualExtraData = request.GetIntegerAsBool();
                    var actualRot = request.GetIntegerAsBool();
                    var actualPosition = request.GetIntegerAsBool();

                    var booleans = $"{actualExtraData},{actualRot},{actualPosition}".ToLower();

                    request.GetString();
                    var items = GetFurniItems(request, room);

                    var delay = request.GetInteger() * 500;
                    var dataToSave = string.Empty;
                    var extraStringForWi = string.Empty;

                    /* TODO CHECK */
                    foreach (var aItem in items)
                    {
//                        if (aItem.GetBaseItem().InteractionType == Interaction.Dice)
//                        {
                        // Why have a RETURN here?
//                            dataToSave += string.Format("0|0|0|0,0,0", aItem.Id, aItem.ExtraData, aItem.Rot, aItem.X,
//                                aItem.Y, aItem.Z);
//                            extraStringForWi +=
//                                $"{aItem.Id},{((actualExtraData) ? aItem.ExtraData : "N")},{((actualRot) ? aItem.Rot.ToString() : "N")},{((actualPosition) ? aItem.X.ToString() : "N")},{((actualPosition) ? aItem.Y.ToString() : "N")}";
//                        }

                        dataToSave += $"{aItem.Id}|{aItem.ExtraData}|{aItem.Rot}|{aItem.X},{aItem.Y},{aItem.Z}";
                        extraStringForWi +=
                            $"{aItem.Id},{((actualExtraData) ? aItem.ExtraData : "N")},{((actualRot) ? aItem.Rot.ToString() : "N")},{((actualPosition) ? aItem.X.ToString() : "N")},{((actualPosition) ? aItem.Y.ToString() : "N")}";

                        if (aItem == items.Last())
                            continue;

                        dataToSave += "/";
                        extraStringForWi += ";";
                    }

                    wired.Items = items;
                    wired.Delay = delay;
                    wired.OtherBool = true;
                    wired.OtherString = booleans;
                    wired.OtherExtraString = dataToSave;
                    wired.OtherExtraString2 = extraStringForWi;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ConditionGroupMember:
                case Interaction.ConditionNotGroupMember:
                case Interaction.TriggerCollision:
                {
                    break;
                }

                case Interaction.ConditionHowManyUsersInRoom:
                case Interaction.ConditionNegativeHowManyUsers:
                {
                    request.GetInteger();
                    var minimum = request.GetInteger();
                    var maximum = request.GetInteger();

                    var ei = $"{minimum},{maximum}";
                    wired.Items = new ConcurrentList<RoomItem>();
                    wired.OtherString = ei;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ConditionUserNotWearingEffect:
                case Interaction.ConditionUserWearingEffect:
                {
                    request.GetInteger();
                    var effect = request.GetInteger();
                    wired.Items = new ConcurrentList<RoomItem>();
                    wired.OtherString = effect.ToString();
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ConditionUserWearingBadge:
                case Interaction.ConditionUserNotWearingBadge:
                case Interaction.ConditionUserHasFurni:
                {
                    request.GetInteger();
                    var badge = request.GetString();
                    wired.Items = new ConcurrentList<RoomItem>();
                    wired.OtherString = badge;
                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ConditionDateRangeActive:
                {
                    request.GetInteger();

                    var startDate = request.GetInteger();
                    var endDate = request.GetInteger();

                    wired.Items = new ConcurrentList<RoomItem>();
                    wired.OtherString = $"{startDate},{endDate}";

                    if (startDate == 0)
                    {
                        wired.OtherString = string.Empty;
                        session.SendNotif(Oblivion.GetLanguage().GetVar("user_wired_con_date_range"));
                    }

                    wiredHandler.ReloadWired(wired);
                    break;
                }

                case Interaction.ConditionTriggerOnFurni:
                case Interaction.ConditionFurniTypeMatches:
                case Interaction.ConditionTriggererNotOnFurni:
                case Interaction.ConditionFurniTypeDontMatch:
                {
                    request.GetInteger();
                    request.GetString();
                    var furniItems = GetFurniItems(request, room);

                    wired.Items = furniItems;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ConditionFurnisHaveUsers:
                case Interaction.ConditionFurnisHaveNotUsers:
                case Interaction.ConditionFurniHasFurni:
                case Interaction.ConditionFurniHasNotFurni:
                {
                    request.GetInteger();
                    var allItems = request.GetIntegerAsBool();
                    request.GetString();

                    var furniItems = GetFurniItems(request, room);


                    wired.OtherBool = allItems;
                    wired.Items = furniItems;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionGiveScore:
                {
                    request.GetInteger();
                    var scoreToGive = request.GetInteger();
                    var maxTimesPerGame = request.GetInteger();

                    var newExtraInfo = $"{scoreToGive},{maxTimesPerGame}";

                    var furniItems8 = GetFurniItems(request, room);
                    wired.Items = furniItems8;
                    wired.OtherString = newExtraInfo;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionBotTalk:
                {
                    request.GetInteger();
                    var type = request.GetIntegerAsBool();
                    var data = request.GetString().Split((char) 9);

                    wired.OtherBool = type;
                    wired.OtherString = data[0];
                    wired.OtherExtraString = data[1];
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionBotClothes:
                {
                    request.GetInteger();
                    var data = request.GetString().Split((char) 9);

                    wired.OtherString = data[0];
                    wired.OtherExtraString = data[1];
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionBotTeleport:
                {
                    request.GetInteger();
                    var botName = request.GetString();
                    var furniItems = GetFurniItems(request, room);

                    wired.Items = furniItems;
                    wired.OtherString = botName;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionBotGiveHanditem:
                {
                    request.GetInteger();
                    var handitem = request.GetInteger();
                    var botName = request.GetString();

                    wired.OtherString = botName;
                    wired.Delay = handitem * 500;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionBotMove:
                {
                    request.GetInteger();
                    var botName = request.GetString();
                    var furniItems = GetFurniItems(request, room);

                    wired.Items = furniItems;
                    wired.OtherString = botName;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionCallStacks:
                {
                    request.GetInteger();
                    request.GetString();
                    var furniItems = GetFurniItems(request, room);

                    var num = request.GetInteger();
                    wired.Items = furniItems;
                    wired.Delay = num * 500;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ActionBotTalkToAvatar:
                {
                    request.GetInteger();
                    var type = request.GetIntegerAsBool();
                    var data = request.GetString().Split((char) 9);

                    wired.OtherBool = type;
                    wired.OtherString = data[0];
                    wired.OtherExtraString = data[1];
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ConditionTimeMoreThan:
                case Interaction.ConditionTimeLessThan:
                {
                    request.GetInteger();
                    var time = request.GetInteger();
                    wired.Delay = time * 500;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ConditionUserHasHanditem:
                {
                    request.GetInteger();
                    var handItem = request.GetInteger();
                    wired.Delay = handItem * 500;
                    wiredHandler.ReloadWired(wired);
                    break;
                }
                case Interaction.ConditionUserIsNotInTeam:
                case Interaction.ConditionUserIsInTeam:
                case Interaction.ActionJoinTeam:
                {
                    request.GetInteger();
                    var team = request.GetInteger();
                    wired.OtherString = team.ToString();
                    wiredHandler.ReloadWired(wired);
                    break;
                }
            }

            session.SendMessage(new ServerMessage(LibraryParser.OutgoingRequest("SaveWiredMessageComposer")));
        }

        private static ConcurrentList<RoomItem> GetFurniItems(ClientMessage request, Room room)
        {
            var list = new ConcurrentList<RoomItem>();
            var itemCount = request.GetInteger();

            for (var i = 0; i < itemCount; i++)
            {
                var item = room.GetRoomItemHandler()
                    .GetItem(Oblivion.GetGame().GetItemManager().GetRealId(request.GetUInteger()));

                if (item != null)
                    list.Add(item);
            }

            return list;
        }
    }
}