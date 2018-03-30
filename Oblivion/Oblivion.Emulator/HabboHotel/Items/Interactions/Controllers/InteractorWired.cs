using System;
using System.Linq;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorWired : FurniInteractorModel
    {
        public override void OnRemove(GameClient session, RoomItem item)
        {
            var room = item.GetRoom();
            room.GetWiredHandler().RemoveWired(item);
        }

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null || item?.GetRoom() == null || !hasRights)
                return;

            var wired = item.GetRoom().GetWiredHandler().GetWired(item);

            if (wired == null)
                return;

            var extraInfo = wired.OtherString;
            var flag = wired.OtherBool;
            var delay = wired.Delay / 500;
            var list = wired.Items.Where(roomItem => roomItem != null).ToList();
            var extraString = wired.OtherExtraString;
            var extraString2 = wired.OtherExtraString2;

            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.ActionLeaveTeam:
                case Interaction.TriggerTimer:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(5);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(extraInfo);
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(delay * 2);
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(3);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        session.SendMessage(serverMessage);
                        return;
                    }
                case Interaction.TriggerRoomEnter:
                    {
                        var serverMessage2 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage2.AppendBool(false);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(list.Count);
                        foreach (var current2 in list) serverMessage2.AppendInteger(current2.VirtualId);
                        serverMessage2.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage2.AppendInteger(item.VirtualId);
                        serverMessage2.AppendString(extraInfo);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(7);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        session.SendMessage(serverMessage2);
                        return;
                    }
                case Interaction.TriggerGameEnd:
                    {
                        var serverMessage3 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage3.AppendBool(false);
                        serverMessage3.AppendInteger(0);
                        serverMessage3.AppendInteger(list.Count);
                        foreach (var current3 in list) serverMessage3.AppendInteger(current3.VirtualId);
                        serverMessage3.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage3.AppendInteger(item.VirtualId);
                        serverMessage3.AppendString(extraInfo);
                        serverMessage3.AppendInteger(0);
                        serverMessage3.AppendInteger(0);
                        serverMessage3.AppendInteger(8);
                        serverMessage3.AppendInteger(0);
                        serverMessage3.AppendInteger(0);
                        serverMessage3.AppendInteger(0);
                        session.SendMessage(serverMessage3);
                        return;
                    }
                case Interaction.TriggerGameStart:
                    {
                        var serverMessage4 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage4.AppendBool(false);
                        serverMessage4.AppendInteger(0);
                        serverMessage4.AppendInteger(list.Count);
                        foreach (var current4 in list) serverMessage4.AppendInteger(current4.VirtualId);
                        serverMessage4.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage4.AppendInteger(item.VirtualId);
                        serverMessage4.AppendString(extraInfo);
                        serverMessage4.AppendInteger(0);
                        serverMessage4.AppendInteger(0);
                        serverMessage4.AppendInteger(8);
                        serverMessage4.AppendInteger(0);
                        serverMessage4.AppendInteger(0);
                        serverMessage4.AppendInteger(0);
                        session.SendMessage(serverMessage4);
                        return;
                    }
                case Interaction.TriggerLongRepeater:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(15);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString("");
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(delay / 10); //fix
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(12);
                        serverMessage.AppendInteger(0);
                        session.SendMessage(serverMessage);
                        return;
                    }

                case Interaction.TriggerRepeater:
                    {
                        var serverMessage5 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage5.AppendBool(false);
                        serverMessage5.AppendInteger(15);
                        serverMessage5.AppendInteger(list.Count);
                        foreach (var current5 in list) serverMessage5.AppendInteger(current5.VirtualId);
                        serverMessage5.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage5.AppendInteger(item.VirtualId);
                        serverMessage5.AppendString(extraInfo);
                        serverMessage5.AppendInteger(1);
                        serverMessage5.AppendInteger(delay);
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(6);
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(0);
                        session.SendMessage(serverMessage5);
                        return;
                    }
                //backmen
                case Interaction.ActionJoinTeam:
                    {
                        if (!int.TryParse(extraInfo, out var team))
                        {
                            team = 0;
                        }
                        if (team >= 5)
                        {
                            team = 4;
                        }

                        var message =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        /*serverMessage5.AppendBool(false);
                        serverMessage5.AppendInteger(5);
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage5.AppendInteger(item.VirtualId);
                        serverMessage5.AppendString(extraInfo);
                        serverMessage5.AppendInteger(0); //delay type
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(6);
                        session.SendMessage(serverMessage5);
                        return;*/
                        message.AppendBool(false);
                        message.AppendInteger(5);
                        message.AppendInteger(0);
                        message.AppendInteger(item.GetBaseItem().SpriteId);
                        message.AppendInteger(item.VirtualId);
                        message.AppendString("");
                        message.AppendInteger(1);

                        message.AppendInteger(team);

                        message.AppendInteger(0);
                        message.AppendInteger(9);
                        message.AppendInteger(0);
                        message.AppendInteger(0);
                        //                    foreach (var SpriteEx in WiredException)
                        //                        message.AppendInt32(SpriteEx);
                        session.SendMessage(message);
                        return;
                    }

                case Interaction.ConditionUserIsInTeam:
                    {
                        if (!int.TryParse(extraInfo, out var team))
                        {
                            team = 0;
                        }
                        var serverMessage5 =
                        new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage5.AppendBool(false);
                        serverMessage5.AppendInteger(5);
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage5.AppendInteger(item.VirtualId);
                        serverMessage5.AppendString("");
                        serverMessage5.AppendInteger(1); //delay type
                        serverMessage5.AppendInteger(team);
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(6);
                        session.SendMessage(serverMessage5);
                        return;
                    }
                case Interaction.ConditionUserIsNotInTeam:
                    {
                        if (!int.TryParse(extraInfo, out var team))
                        {
                            team = 0;
                        }
                        var serverMessage5 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage5.AppendBool(false);
                        serverMessage5.AppendInteger(5);
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage5.AppendInteger(item.VirtualId);
                        serverMessage5.AppendString("");
                        serverMessage5.AppendInteger(1); //delay type
                        serverMessage5.AppendInteger(team); //delay type
                        serverMessage5.AppendInteger(0);
                        serverMessage5.AppendInteger(17);
                        session.SendMessage(serverMessage5);
                        return;
                    }
                /*  case Interaction.ActionJoinTeam:
                  {
                          var serverMessage5 =
                          new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                      serverMessage5.AppendBool(false);
                      serverMessage5.AppendInteger(5);
                      serverMessage5.AppendInteger(0);
                      serverMessage5.AppendInteger(item.GetBaseItem().SpriteId);
                      serverMessage5.AppendInteger(item.VirtualId);
                      serverMessage5.AppendInteger(Convert.ToInt32(extraInfo));
                      serverMessage5.AppendInteger(0); //delay type
                      serverMessage5.AppendInteger(0);
                      serverMessage5.AppendInteger(6);
                      session.SendMessage(serverMessage5);
                      return;
                  }*/
                case Interaction.TriggerOnUserSay:
                case Interaction.TriggerOnUserSayCommand:
                    {
                        var serverMessage6 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage6.AppendBool(false);
                        serverMessage6.AppendInteger(0);
                        serverMessage6.AppendInteger(list.Count);
                        foreach (var current6 in list) serverMessage6.AppendInteger(current6.VirtualId);
                        serverMessage6.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage6.AppendInteger(item.VirtualId);
                        serverMessage6.AppendString(extraInfo);
                        serverMessage6.AppendInteger(0);
                        serverMessage6.AppendInteger(0);
                        serverMessage6.AppendInteger(0);
                        serverMessage6.AppendInteger(0);
                        serverMessage6.AppendInteger(0);
                        serverMessage6.AppendInteger(0);
                        session.SendMessage(serverMessage6);
                        return;
                    }
                case Interaction.TriggerScoreAchieved:
                    {
                        var serverMessage7 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage7.AppendBool(false);
                        serverMessage7.AppendInteger(5);
                        serverMessage7.AppendInteger(0);
                        serverMessage7.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage7.AppendInteger(item.VirtualId);
                        serverMessage7.AppendString("");
                        serverMessage7.AppendInteger(1);
                        serverMessage7.AppendInteger((string.IsNullOrWhiteSpace(extraInfo)) ? 100 : int.Parse(extraInfo));
                        serverMessage7.AppendInteger(0);
                        serverMessage7.AppendInteger(10);
                        serverMessage7.AppendInteger(0);
                        serverMessage7.AppendInteger(0);
                        session.SendMessage(serverMessage7);
                        return;
                    }
                case Interaction.TriggerStateChanged:
                    {
                        var serverMessage8 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage8.AppendBool(false);
                        serverMessage8.AppendInteger(15);
                        serverMessage8.AppendInteger(list.Count);
                        foreach (var current8 in list) serverMessage8.AppendInteger(current8.VirtualId);
                        serverMessage8.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage8.AppendInteger(item.VirtualId);
                        serverMessage8.AppendString(extraInfo);
                        serverMessage8.AppendInteger(0);
                        serverMessage8.AppendInteger(0);
                        serverMessage8.AppendInteger(1);
                        serverMessage8.AppendInteger(delay);
                        serverMessage8.AppendInteger(0);
                        serverMessage8.AppendInteger(0);
                        session.SendMessage(serverMessage8);
                        return;
                    }
                case Interaction.TriggerWalkOnFurni:
                    {
                        var serverMessage9 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage9.AppendBool(false);
                        serverMessage9.AppendInteger(15);
                        serverMessage9.AppendInteger(list.Count);
                        foreach (var current9 in list) serverMessage9.AppendInteger(current9.VirtualId);
                        serverMessage9.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage9.AppendInteger(item.VirtualId);
                        serverMessage9.AppendString(extraInfo);
                        serverMessage9.AppendInteger(0);
                        serverMessage9.AppendInteger(0);
                        serverMessage9.AppendInteger(1);
                        serverMessage9.AppendInteger(0);
                        serverMessage9.AppendInteger(0);
                        serverMessage9.AppendInteger(0);
                        session.SendMessage(serverMessage9);
                        return;
                    }
                case Interaction.ActionMuteUser:
                    {
                        var serverMessage18 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage18.AppendBool(false);
                        serverMessage18.AppendInteger(5);
                        serverMessage18.AppendInteger(0);
                        serverMessage18.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage18.AppendInteger(item.VirtualId);
                        serverMessage18.AppendString(extraInfo);
                        serverMessage18.AppendInteger(1);
                        serverMessage18.AppendInteger(delay);
                        serverMessage18.AppendInteger(0);
                        serverMessage18.AppendInteger(20);
                        serverMessage18.AppendInteger(0);
                        serverMessage18.AppendInteger(0);
                        session.SendMessage(serverMessage18);
                        return;
                    }
                case Interaction.TriggerWalkOffFurni:
                    {
                        var serverMessage10 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage10.AppendBool(false);
                        serverMessage10.AppendInteger(15);
                        serverMessage10.AppendInteger(list.Count);
                        foreach (var current10 in list) serverMessage10.AppendInteger(current10.VirtualId);
                        serverMessage10.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage10.AppendInteger(item.VirtualId);
                        serverMessage10.AppendString(extraInfo);
                        serverMessage10.AppendInteger(0);
                        serverMessage10.AppendInteger(0);
                        serverMessage10.AppendInteger(1);
                        serverMessage10.AppendInteger(0);
                        serverMessage10.AppendInteger(0);
                        serverMessage10.AppendInteger(0);
                        serverMessage10.AppendInteger(0);
                        session.SendMessage(serverMessage10);
                        return;
                    }

                case Interaction.TriggerCollision:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(5);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(string.Empty);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(11);
                        serverMessage.AppendInteger(0);
                        session.SendMessage(serverMessage);
                        return;
                    }

                case Interaction.ActionGiveScore:
                    {
                        // Por hacer.
                        var serverMessage11 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage11.AppendBool(false);
                        serverMessage11.AppendInteger(5);
                        serverMessage11.AppendInteger(0);
                        serverMessage11.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage11.AppendInteger(item.VirtualId);
                        serverMessage11.AppendString("");
                        serverMessage11.AppendInteger(2);
                        if (string.IsNullOrWhiteSpace(extraInfo))
                        {
                            serverMessage11.AppendInteger(10); // Puntos a dar
                            serverMessage11.AppendInteger(1); // Numero de veces por equipo
                        }
                        else
                        {
                            var integers = extraInfo.Split(',');
                            serverMessage11.AppendInteger(int.Parse(integers[0])); // Puntos a dar
                            serverMessage11.AppendInteger(int.Parse(integers[1])); // Numero de veces por equipo
                        }
                        serverMessage11.AppendInteger(0);
                        serverMessage11.AppendInteger(6);
                        serverMessage11.AppendInteger(0);
                        serverMessage11.AppendInteger(0);
                        serverMessage11.AppendInteger(0);
                        session.SendMessage(serverMessage11);
                        return;
                    }

                case Interaction.ConditionGroupMember:
                case Interaction.ConditionNotGroupMember:
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        message.AppendBool(false);
                        message.AppendInteger(5);
                        message.AppendInteger(0);
                        message.AppendInteger(item.GetBaseItem().SpriteId);
                        message.AppendInteger(item.VirtualId);
                        message.AppendString("");
                        message.AppendInteger(0);
                        message.AppendInteger(0);
                        message.AppendInteger(10);
                        session.SendMessage(message);
                        return;
                    }

                case Interaction.ConditionItemsMatches:
                case Interaction.ConditionItemsDontMatch:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        serverMessage21.AppendInteger(15);
                        serverMessage21.AppendInteger(list.Count);
                        foreach (var current20 in list) serverMessage21.AppendInteger(current20.VirtualId);
                        serverMessage21.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage21.AppendInteger(item.VirtualId);
                        serverMessage21.AppendString(extraString2);
                        serverMessage21.AppendInteger(3);

                        if (string.IsNullOrWhiteSpace(extraInfo) || !extraInfo.Contains(","))
                        {
                            serverMessage21.AppendInteger(0);
                            serverMessage21.AppendInteger(0);
                            serverMessage21.AppendInteger(0);
                        }
                        else
                        {
                            var boolz = extraInfo.Split(',');

                            foreach (var stringy in boolz)
                                serverMessage21.AppendInteger(stringy.ToLower() == "true" ? 1 : 0);
                        }
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(0);
                        session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.ActionPosReset:
                    {
                        var serverMessage12 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage12.AppendBool(false);
                        serverMessage12.AppendInteger(15);
                        serverMessage12.AppendInteger(list.Count);
                        foreach (var current12 in list) serverMessage12.AppendInteger(current12.VirtualId);
                        serverMessage12.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage12.AppendInteger(item.VirtualId);
                        serverMessage12.AppendString(extraString2);
                        serverMessage12.AppendInteger(3);

                        if (string.IsNullOrWhiteSpace(extraInfo))
                        {
                            serverMessage12.AppendInteger(0);
                            serverMessage12.AppendInteger(0);
                            serverMessage12.AppendInteger(0);
                        }
                        else
                        {
                            var boolz = extraInfo.Split(',');

                            foreach (var stringy in boolz)
                                serverMessage12.AppendInteger(stringy.ToLower() == "true" ? 1 : 0);
                        }
                        serverMessage12.AppendInteger(0);
                        serverMessage12.AppendInteger(3);
                        serverMessage12.AppendInteger(delay); // Delay
                        serverMessage12.AppendInteger(0);
                        session.SendMessage(serverMessage12);
                        return;
                    }
                case Interaction.ActionMoveRotate:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(15);

                        serverMessage.AppendInteger(list.Count);
                        foreach (var roomItem in list.Where(roomItem => roomItem != null))
                            serverMessage.AppendInteger(roomItem.VirtualId);

                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(string.Empty);
                        serverMessage.AppendInteger(2);
                        serverMessage.AppendIntegersArray(extraInfo, ';', 2);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(4);
                        serverMessage.AppendInteger(delay);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        session.SendMessage(serverMessage);
                    }
                    break;

                case Interaction.ActionMoveToDir:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(15);

                        serverMessage.AppendInteger(list.Count);
                        foreach (var roomItem in list.Where(roomItem => roomItem != null))
                            serverMessage.AppendInteger(roomItem.VirtualId);

                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(string.Empty);
                        serverMessage.AppendInteger(2);
                        serverMessage.AppendIntegersArray(extraInfo, ';', 2);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(13);
                        serverMessage.AppendInteger(delay);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        session.SendMessage(serverMessage);
                    }
                    break;

                case Interaction.ActionResetTimer:
                    {
                        var serverMessage14 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage14.AppendBool(false);
                        serverMessage14.AppendInteger(0);
                        serverMessage14.AppendInteger(0);
                        serverMessage14.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage14.AppendInteger(item.VirtualId);
                        serverMessage14.AppendString(extraInfo);
                        serverMessage14.AppendInteger(0);
                        serverMessage14.AppendInteger(0);
                        serverMessage14.AppendInteger(1);
                        serverMessage14.AppendInteger(delay);
                        serverMessage14.AppendInteger(0);
                        serverMessage14.AppendInteger(0);
                        session.SendMessage(serverMessage14);
                        return;
                    }
                case Interaction.ActionShowMessage:
                case Interaction.ActionRollerSpeed:
                case Interaction.ActionKickUser:
                case Interaction.ActionEffectUser:
                case Interaction.ActionEnableDance:
                case Interaction.ActionHandItem:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(7);
                        serverMessage15.AppendInteger(delay);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionTeleportTo:
                    {
                        var serverMessage16 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage16.AppendBool(false);
                        serverMessage16.AppendInteger(15);

                        serverMessage16.AppendInteger(list.Count);
                        foreach (var roomItem in list) serverMessage16.AppendInteger(roomItem.VirtualId);

                        serverMessage16.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage16.AppendInteger(item.VirtualId);
                        serverMessage16.AppendString(extraInfo);
                        serverMessage16.AppendInteger(0);
                        serverMessage16.AppendInteger(8);
                        serverMessage16.AppendInteger(0);
                        serverMessage16.AppendInteger(delay);
                        serverMessage16.AppendInteger(0);
                        serverMessage16.AppendByte(2);
                        session.SendMessage(serverMessage16);
                        return;
                    }
                case Interaction.ActionToggleState:
                    {
                        var serverMessage17 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage17.AppendBool(false);
                        serverMessage17.AppendInteger(15);
                        serverMessage17.AppendInteger(list.Count);
                        foreach (var current17 in list) serverMessage17.AppendInteger(current17.VirtualId);
                        serverMessage17.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage17.AppendInteger(item.VirtualId);
                        serverMessage17.AppendString(extraInfo);
                        serverMessage17.AppendInteger(0);
                        serverMessage17.AppendInteger(8);
                        serverMessage17.AppendInteger(0);
                        serverMessage17.AppendInteger(delay);
                        serverMessage17.AppendInteger(0);
                        serverMessage17.AppendInteger(0);
                        session.SendMessage(serverMessage17);
                        return;
                    }
                case Interaction.ActionGiveReward:
                    {
                        if (!session.GetHabbo().HasFuse("fuse_use_superwired")) return;
                        var serverMessage18 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage18.AppendBool(false);
                        serverMessage18.AppendInteger(5);
                        serverMessage18.AppendInteger(0);
                        serverMessage18.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage18.AppendInteger(item.VirtualId);
                        serverMessage18.AppendString(extraInfo);
                        serverMessage18.AppendInteger(3);
                        serverMessage18.AppendInteger(extraString == "" ? 0 : int.Parse(extraString));
                        serverMessage18.AppendInteger(flag ? 1 : 0);
                        serverMessage18.AppendInteger(extraString2 == "" ? 0 : int.Parse(extraString2));
                        serverMessage18.AppendInteger(0);
                        serverMessage18.AppendInteger(17);
                        serverMessage18.AppendInteger(0);
                        serverMessage18.AppendInteger(0);
                        session.SendMessage(serverMessage18);
                        return;
                    }

                case Interaction.ConditionHowManyUsersInRoom:
                case Interaction.ConditionNegativeHowManyUsers:
                    {
                        var serverMessage19 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage19.AppendBool(false);
                        serverMessage19.AppendInteger(5);
                        serverMessage19.AppendInteger(0);
                        serverMessage19.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage19.AppendInteger(item.VirtualId);
                        serverMessage19.AppendString("");
                        serverMessage19.AppendInteger(2);
                        if (string.IsNullOrWhiteSpace(extraInfo))
                        {
                            serverMessage19.AppendInteger(1);
                            serverMessage19.AppendInteger(50);
                        }
                        else
                            foreach (var integers in extraInfo.Split(','))
                                serverMessage19.AppendInteger(int.Parse(integers));
                        serverMessage19.AppendBool(false);
                        serverMessage19.AppendInteger(0);
                        serverMessage19.AppendInteger(1290);
                        session.SendMessage(serverMessage19);
                        return;
                    }

                case Interaction.ConditionStatePos:
                case Interaction.ConditionTriggerOnFurni:
                case Interaction.ConditionFurniTypeMatches:
                case Interaction.ConditionFurniTypeDontMatch:
                case Interaction.ConditionTriggererNotOnFurni:
                    {
                        var serverMessage19 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage19.AppendBool(false);
                        serverMessage19.AppendInteger(15);
                        serverMessage19.AppendInteger(list.Count);
                        foreach (var current18 in list) serverMessage19.AppendInteger(current18.VirtualId);
                        serverMessage19.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage19.AppendInteger(item.VirtualId);
                        serverMessage19.AppendInteger(0);
                        serverMessage19.AppendInteger(0);
                        serverMessage19.AppendInteger(0);
                        serverMessage19.AppendBool(false);
                        serverMessage19.AppendBool(true);
                        session.SendMessage(serverMessage19);
                        return;
                    }

                case Interaction.ConditionFurnisHaveNotUsers:
                case Interaction.ConditionFurnisHaveUsers:

                case Interaction.ConditionFurniHasNotFurni:
                case Interaction.ConditionFurniHasFurni:
                    {
                        var serverMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(15);
                        serverMessage.AppendInteger(list.Count);
                        foreach (var current18 in list) serverMessage.AppendInteger(current18.VirtualId);
                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(string.Empty);
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(flag); //bool
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(item.GetBaseItem().InteractionType == Interaction.ConditionFurniHasFurni
                            ? 7
                            : 18);
                        session.SendMessage(serverMessage);
                        return;
                    }
                case Interaction.ConditionTimeLessThan:
                case Interaction.ConditionTimeMoreThan:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage21.AppendInteger(item.VirtualId);
                        serverMessage21.AppendString("");
                        serverMessage21.AppendInteger(1);
                        serverMessage21.AppendInteger(delay); //delay
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(item.GetBaseItem().InteractionType ==
                                                      Interaction.ConditionTimeMoreThan
                            ? 3
                            : 4);
                        session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.ConditionUserWearingEffect:
                case Interaction.ConditionUserNotWearingEffect:
                    {
                        int.TryParse(extraInfo, out var effect);
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        serverMessage21.AppendInteger(5);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage21.AppendInteger(item.VirtualId);
                        serverMessage21.AppendString("");
                        serverMessage21.AppendInteger(1);
                        serverMessage21.AppendInteger(effect);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(12);
                        session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.ConditionUserWearingBadge:
                case Interaction.ConditionUserNotWearingBadge:
                case Interaction.ConditionUserHasFurni:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        serverMessage21.AppendInteger(5);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage21.AppendInteger(item.VirtualId);
                        serverMessage21.AppendString(extraInfo);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(11);
                        session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.ConditionDateRangeActive:
                    {
                        var date1 = 0;
                        var date2 = 0;

                        try
                        {
                            var strArray = extraInfo.Split(',');
                            date1 = int.Parse(strArray[0]);
                            date2 = int.Parse(strArray[1]);
                        }
                        catch
                        {
                        }

                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        serverMessage21.AppendInteger(5);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage21.AppendInteger(item.VirtualId);
                        serverMessage21.AppendString(extraInfo);
                        serverMessage21.AppendInteger(2);
                        serverMessage21.AppendInteger(date1);
                        serverMessage21.AppendInteger(date2);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(24);
                        session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.TriggerBotReachedAvatar:
                    {
                        var serverMessage2 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage2.AppendBool(false);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(list.Count);
                        foreach (var current2 in list) serverMessage2.AppendInteger(current2.VirtualId);
                        serverMessage2.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage2.AppendInteger(item.VirtualId);
                        serverMessage2.AppendString(extraInfo);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(14);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        session.SendMessage(serverMessage2);
                        return;
                    }
                case Interaction.TriggerBotReachedStuff:
                    {
                        var serverMessage2 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage2.AppendBool(false);
                        serverMessage2.AppendInteger(15);
                        serverMessage2.AppendInteger(list.Count);
                        foreach (var current2 in list) serverMessage2.AppendInteger(current2.VirtualId);
                        serverMessage2.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage2.AppendInteger(item.VirtualId);
                        serverMessage2.AppendString(extraInfo);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(13);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        serverMessage2.AppendInteger(0);
                        session.SendMessage(serverMessage2);
                        return;
                    }
                case Interaction.ActionBotClothes:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo + (char)9 + extraString);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(26);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotFollowAvatar:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(25);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotGiveHanditem:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo);
                        serverMessage15.AppendInteger(1);
                        serverMessage15.AppendInteger(delay);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(24);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotMove:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(15);
                        serverMessage15.AppendInteger(list.Count);
                        foreach (var current2 in list) serverMessage15.AppendInteger(current2.VirtualId);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(22);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotTalk:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo + (char)9 + extraString);
                        serverMessage15.AppendInteger(1);
                        serverMessage15.AppendInteger(Oblivion.BoolToInteger(flag));
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(23);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotTalkToAvatar:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo + (char)9 + extraString);
                        serverMessage15.AppendInteger(1);
                        serverMessage15.AppendInteger(Oblivion.BoolToInteger(flag));
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(27);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotTeleport:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        serverMessage15.AppendInteger(15);
                        serverMessage15.AppendInteger(list.Count);
                        foreach (var current2 in list) serverMessage15.AppendInteger(current2.VirtualId);
                        serverMessage15.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage15.AppendInteger(item.VirtualId);
                        serverMessage15.AppendString(extraInfo);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(21);
                        serverMessage15.AppendInteger(0);
                        serverMessage15.AppendInteger(0);
                        session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionChase:
                case Interaction.ActionInverseChase:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(15);

                        serverMessage.AppendInteger(list.Count);
                        foreach (var roomItem in list) serverMessage.AppendInteger(roomItem.VirtualId);

                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(string.Empty);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(11);
                        serverMessage.AppendInteger(0);

                        serverMessage.AppendInteger(0);

                        session.SendMessage(serverMessage);
                        return;
                    }
                case Interaction.ConditionUserHasHanditem:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage21.AppendInteger(item.VirtualId);
                        serverMessage21.AppendString(extraInfo);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(0);
                        serverMessage21.AppendInteger(25);
                        session.SendMessage(serverMessage21);
                        return;
                    }
                case Interaction.ActionCallStacks:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        serverMessage.AppendInteger(15);
                        serverMessage.AppendInteger(list.Count);
                        foreach (var current15 in list) serverMessage.AppendInteger(current15.VirtualId);
                        serverMessage.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage.AppendInteger(item.VirtualId);
                        serverMessage.AppendString(extraInfo);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(18);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        session.SendMessage(serverMessage);
                        return;
                    }


                case Interaction.SpecialRandom:
                case Interaction.SpecialUnseen:
                    {
                        var serverMessage25 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage25.AppendBool(false);
                        serverMessage25.AppendInteger(15);
                        serverMessage25.AppendInteger(list.Count);
                        foreach (var current24 in list) serverMessage25.AppendInteger(current24.VirtualId);
                        serverMessage25.AppendInteger(item.GetBaseItem().SpriteId);
                        serverMessage25.AppendInteger(item.VirtualId);
                        serverMessage25.AppendString(extraInfo);
                        serverMessage25.AppendInteger(0);
                        serverMessage25.AppendInteger(8);
                        serverMessage25.AppendInteger(0);
                        serverMessage25.AppendInteger(0);
                        serverMessage25.AppendInteger(0);
                        serverMessage25.AppendInteger(0);
                        session.SendMessage(serverMessage25);
                        return;
                    }
                default:
                    return;
            }
        }
    }
}