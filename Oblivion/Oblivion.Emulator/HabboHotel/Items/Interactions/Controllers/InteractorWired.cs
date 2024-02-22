using System.Linq;
using System.Threading.Tasks;
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
        public override Task OnRemove(GameClient session, RoomItem item)
        {
            var room = item.GetRoom();
            room.GetWiredHandler().RemoveWired(item);
            return Task.CompletedTask;
        }

        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
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
                case Interaction.TriggerRoomEnter:
                    {
                        var serverMessage2 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage2.AppendBool(false);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(list.Count);
                        foreach (var current2 in list) await serverMessage2.AppendIntegerAsync(current2.VirtualId);
                        await serverMessage2.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage2.AppendIntegerAsync(item.VirtualId);
                        await serverMessage2.AppendStringAsync(extraInfo);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(7);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage2);
                        return;
                    }
                case Interaction.TriggerGameEnd:
                    {
                        var serverMessage3 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage3.AppendBool(false);
                        await serverMessage3.AppendIntegerAsync(0);
                        await serverMessage3.AppendIntegerAsync(list.Count);
                        foreach (var current3 in list) await serverMessage3.AppendIntegerAsync(current3.VirtualId);
                        await serverMessage3.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage3.AppendIntegerAsync(item.VirtualId);
                        await serverMessage3.AppendStringAsync(extraInfo);
                        await serverMessage3.AppendIntegerAsync(0);
                        await serverMessage3.AppendIntegerAsync(0);
                        await serverMessage3.AppendIntegerAsync(8);
                        await serverMessage3.AppendIntegerAsync(0);
                        await serverMessage3.AppendIntegerAsync(0);
                        await serverMessage3.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage3);
                        return;
                    }
                case Interaction.TriggerGameStart:
                    {
                        var serverMessage4 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage4.AppendBool(false);
                        await serverMessage4.AppendIntegerAsync(0);
                        await serverMessage4.AppendIntegerAsync(list.Count);
                        foreach (var current4 in list) await serverMessage4.AppendIntegerAsync(current4.VirtualId);
                        await serverMessage4.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage4.AppendIntegerAsync(item.VirtualId);
                        await serverMessage4.AppendStringAsync(extraInfo);
                        await serverMessage4.AppendIntegerAsync(0);
                        await serverMessage4.AppendIntegerAsync(0);
                        await serverMessage4.AppendIntegerAsync(8);
                        await serverMessage4.AppendIntegerAsync(0);
                        await serverMessage4.AppendIntegerAsync(0);
                        await serverMessage4.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage4);
                        return;
                    }
                case Interaction.TriggerLongRepeater:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage.AppendBool(false);
                        await serverMessage.AppendIntegerAsync(15);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync("");
                        await serverMessage.AppendIntegerAsync(1);
                        await serverMessage.AppendIntegerAsync(delay / 10); //fix
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(12);
                        await serverMessage.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage);
                        return;
            }
                case Interaction.ActionLeaveTeam:
                case Interaction.TriggerTimer:
                case Interaction.TriggerRepeater:
                    {
                        var serverMessage5 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage5.AppendBool(false);
                        await serverMessage5.AppendIntegerAsync(15);
                        await serverMessage5.AppendIntegerAsync(list.Count);
                        foreach (var current5 in list) await serverMessage5.AppendIntegerAsync(current5.VirtualId);
                        await serverMessage5.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage5.AppendIntegerAsync(item.VirtualId);
                        await serverMessage5.AppendStringAsync(extraInfo);
                        await serverMessage5.AppendIntegerAsync(1);
                        await serverMessage5.AppendIntegerAsync(delay);
                        await serverMessage5.AppendIntegerAsync(0);
                        await serverMessage5.AppendIntegerAsync(6);
                        await serverMessage5.AppendIntegerAsync(0);
                        await serverMessage5.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage5);
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
                        await session.SendMessage(serverMessage5);
                        return;*/
                        message.AppendBool(false);
                        await message.AppendIntegerAsync(5);
                        await message.AppendIntegerAsync(0);
                        await message.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await message.AppendIntegerAsync(item.VirtualId);
                        await message.AppendStringAsync("");
                        await message.AppendIntegerAsync(1);

                        await message.AppendIntegerAsync(team);

                        await message.AppendIntegerAsync(0);
                        await message.AppendIntegerAsync(9);
                        await message.AppendIntegerAsync(0);
                        await message.AppendIntegerAsync(0);
                        //                    foreach (var SpriteEx in WiredException)
                        //                        message.AppendInt32(SpriteEx);
                        await session.SendMessage(message);
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
                        await serverMessage5.AppendIntegerAsync(5);
                        await serverMessage5.AppendIntegerAsync(0);
                        await serverMessage5.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage5.AppendIntegerAsync(item.VirtualId);
                        await serverMessage5.AppendStringAsync("");
                        await serverMessage5.AppendIntegerAsync(1); //delay type
                        await serverMessage5.AppendIntegerAsync(team);
                        await serverMessage5.AppendIntegerAsync(0);
                        await serverMessage5.AppendIntegerAsync(6);
                        await session.SendMessage(serverMessage5);
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
                        await serverMessage5.AppendIntegerAsync(5);
                        await serverMessage5.AppendIntegerAsync(0);
                        await serverMessage5.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage5.AppendIntegerAsync(item.VirtualId);
                        await serverMessage5.AppendStringAsync("");
                        await serverMessage5.AppendIntegerAsync(1); //delay type
                        await serverMessage5.AppendIntegerAsync(team); //delay type
                        await serverMessage5.AppendIntegerAsync(0);
                        await serverMessage5.AppendIntegerAsync(17);
                        await session.SendMessage(serverMessage5);
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
                      await session.SendMessage(serverMessage5);
                      return;
                  }*/
                case Interaction.TriggerOnUserSay:
                case Interaction.TriggerOnUserSayCommand:
                    {
                        var serverMessage6 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage6.AppendBool(false);
                        await serverMessage6.AppendIntegerAsync(0);
                        await serverMessage6.AppendIntegerAsync(list.Count);
                        foreach (var current6 in list) await serverMessage6.AppendIntegerAsync(current6.VirtualId);
                        await serverMessage6.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage6.AppendIntegerAsync(item.VirtualId);
                        await serverMessage6.AppendStringAsync(extraInfo);
                        await serverMessage6.AppendIntegerAsync(0);
                        await serverMessage6.AppendIntegerAsync(0);
                        await serverMessage6.AppendIntegerAsync(0);
                        await serverMessage6.AppendIntegerAsync(0);
                        await serverMessage6.AppendIntegerAsync(0);
                        await serverMessage6.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage6);
                        return;
                    }
                case Interaction.TriggerScoreAchieved:
                    {
                        var serverMessage7 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage7.AppendBool(false);
                        await serverMessage7.AppendIntegerAsync(5);
                        await serverMessage7.AppendIntegerAsync(0);
                        await serverMessage7.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage7.AppendIntegerAsync(item.VirtualId);
                        await serverMessage7.AppendStringAsync("");
                        await serverMessage7.AppendIntegerAsync(1);
                        await serverMessage7.AppendIntegerAsync((string.IsNullOrWhiteSpace(extraInfo)) ? 100 : int.Parse(extraInfo));
                        await serverMessage7.AppendIntegerAsync(0);
                        await serverMessage7.AppendIntegerAsync(10);
                        await serverMessage7.AppendIntegerAsync(0);
                        await serverMessage7.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage7);
                        return;
                    }
                case Interaction.TriggerStateChanged:
                    {
                        var serverMessage8 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage8.AppendBool(false);
                        await serverMessage8.AppendIntegerAsync(15);
                        await serverMessage8.AppendIntegerAsync(list.Count);
                        foreach (var current8 in list) await serverMessage8.AppendIntegerAsync(current8.VirtualId);
                        await serverMessage8.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage8.AppendIntegerAsync(item.VirtualId);
                        await serverMessage8.AppendStringAsync(extraInfo);
                        await serverMessage8.AppendIntegerAsync(0);
                        await serverMessage8.AppendIntegerAsync(0);
                        await serverMessage8.AppendIntegerAsync(1);
                        await serverMessage8.AppendIntegerAsync(delay);
                        await serverMessage8.AppendIntegerAsync(0);
                        await serverMessage8.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage8);
                        return;
                    }
                case Interaction.TriggerWalkOnFurni:
                    {
                        var serverMessage9 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage9.AppendBool(false);
                        await serverMessage9.AppendIntegerAsync(15);
                        await serverMessage9.AppendIntegerAsync(list.Count);
                        foreach (var current9 in list) await serverMessage9.AppendIntegerAsync(current9.VirtualId);
                        await serverMessage9.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage9.AppendIntegerAsync(item.VirtualId);
                        await serverMessage9.AppendStringAsync(extraInfo);
                        await serverMessage9.AppendIntegerAsync(0);
                        await serverMessage9.AppendIntegerAsync(0);
                        await serverMessage9.AppendIntegerAsync(1);
                        await serverMessage9.AppendIntegerAsync(0);
                        await serverMessage9.AppendIntegerAsync(0);
                        await serverMessage9.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage9);
                        return;
                    }
                case Interaction.ActionMuteUser:
                    {
                        var serverMessage18 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage18.AppendBool(false);
                        await serverMessage18.AppendIntegerAsync(5);
                        await serverMessage18.AppendIntegerAsync(0);
                        await serverMessage18.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage18.AppendIntegerAsync(item.VirtualId);
                        await serverMessage18.AppendStringAsync(extraInfo);
                        await serverMessage18.AppendIntegerAsync(1);
                        await serverMessage18.AppendIntegerAsync(delay);
                        await serverMessage18.AppendIntegerAsync(0);
                        await serverMessage18.AppendIntegerAsync(20);
                        await serverMessage18.AppendIntegerAsync(0);
                        await serverMessage18.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage18);
                        return;
                    }
                case Interaction.TriggerWalkOffFurni:
                    {
                        var serverMessage10 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage10.AppendBool(false);
                        await serverMessage10.AppendIntegerAsync(15);
                        await serverMessage10.AppendIntegerAsync(list.Count);
                        foreach (var current10 in list) await serverMessage10.AppendIntegerAsync(current10.VirtualId);
                        await serverMessage10.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage10.AppendIntegerAsync(item.VirtualId);
                        await serverMessage10.AppendStringAsync(extraInfo);
                        await serverMessage10.AppendIntegerAsync(0);
                        await serverMessage10.AppendIntegerAsync(0);
                        await serverMessage10.AppendIntegerAsync(1);
                        await serverMessage10.AppendIntegerAsync(0);
                        await serverMessage10.AppendIntegerAsync(0);
                        await serverMessage10.AppendIntegerAsync(0);
                        await serverMessage10.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage10);
                        return;
                    }

                case Interaction.TriggerCollision:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage.AppendBool(false);
                        await serverMessage.AppendIntegerAsync(5);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync(string.Empty);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(11);
                        await serverMessage.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage);
                        return;
                    }

                case Interaction.ActionGiveScore:
                    {
                        // Por hacer.
                        var serverMessage11 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage11.AppendBool(false);
                        await serverMessage11.AppendIntegerAsync(5);
                        await serverMessage11.AppendIntegerAsync(0);
                        await serverMessage11.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage11.AppendIntegerAsync(item.VirtualId);
                        await serverMessage11.AppendStringAsync("");
                        await serverMessage11.AppendIntegerAsync(2);
                        if (string.IsNullOrWhiteSpace(extraInfo))
                        {
                            await serverMessage11.AppendIntegerAsync(10); // Puntos a dar
                            await serverMessage11.AppendIntegerAsync(1); // Numero de veces por equipo
                        }
                        else
                        {
                            var integers = extraInfo.Split(',');
                            await serverMessage11.AppendIntegerAsync(int.Parse(integers[0])); // Puntos a dar
                            await serverMessage11.AppendIntegerAsync(int.Parse(integers[1])); // Numero de veces por equipo
                        }
                        await serverMessage11.AppendIntegerAsync(0);
                        await serverMessage11.AppendIntegerAsync(6);
                        await serverMessage11.AppendIntegerAsync(0);
                        await serverMessage11.AppendIntegerAsync(0);
                        await serverMessage11.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage11);
                        return;
                    }

                case Interaction.ConditionGroupMember:
                case Interaction.ConditionNotGroupMember:
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        message.AppendBool(false);
                        await message.AppendIntegerAsync(5);
                        await message.AppendIntegerAsync(0);
                        await message.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await message.AppendIntegerAsync(item.VirtualId);
                        await message.AppendStringAsync("");
                        await message.AppendIntegerAsync(0);
                        await message.AppendIntegerAsync(0);
                        await message.AppendIntegerAsync(10);
                        await session.SendMessage(message);
                        return;
                    }

                case Interaction.ConditionItemsMatches:
                case Interaction.ConditionItemsDontMatch:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        await serverMessage21.AppendIntegerAsync(15);
                        await serverMessage21.AppendIntegerAsync(list.Count);
                        foreach (var current20 in list) await serverMessage21.AppendIntegerAsync(current20.VirtualId);
                        await serverMessage21.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage21.AppendIntegerAsync(item.VirtualId);
                        await serverMessage21.AppendStringAsync(extraString2);
                        await serverMessage21.AppendIntegerAsync(3);

                        if (string.IsNullOrWhiteSpace(extraInfo) || !extraInfo.Contains(","))
                        {
                            await serverMessage21.AppendIntegerAsync(0);
                            await serverMessage21.AppendIntegerAsync(0);
                            await serverMessage21.AppendIntegerAsync(0);
                        }
                        else
                        {
                            var boolz = extraInfo.Split(',');

                            foreach (var stringy in boolz)
                                await serverMessage21.AppendIntegerAsync(stringy.ToLower() == "true" ? 1 : 0);
                        }
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.ActionPosReset:
                    {
                        var serverMessage12 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage12.AppendBool(false);
                        await serverMessage12.AppendIntegerAsync(5);
                        await serverMessage12.AppendIntegerAsync(list.Count);
                        foreach (var current12 in list) await serverMessage12.AppendIntegerAsync(current12.VirtualId);
                        await serverMessage12.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage12.AppendIntegerAsync(item.VirtualId);
                        await serverMessage12.AppendStringAsync(extraString2);
                        await serverMessage12.AppendIntegerAsync(3);

                        if (string.IsNullOrWhiteSpace(extraInfo))
                        {
                            await serverMessage12.AppendIntegerAsync(0);
                            await serverMessage12.AppendIntegerAsync(0);
                            await serverMessage12.AppendIntegerAsync(0);
                        }
                        else
                        {
                            var boolz = extraInfo.Split(',');

                            foreach (var stringy in boolz)
                                await serverMessage12.AppendIntegerAsync(stringy.ToLower() == "true" ? 1 : 0);
                        }
                        await serverMessage12.AppendIntegerAsync(0);
                        await serverMessage12.AppendIntegerAsync(3);
                        await serverMessage12.AppendIntegerAsync(delay); // Delay
                        await serverMessage12.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage12);
                        return;
                    }
                case Interaction.ActionMoveRotate:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        await serverMessage.AppendIntegerAsync(15);

                        await serverMessage.AppendIntegerAsync(list.Count);
                        foreach (var roomItem in list.Where(roomItem => roomItem != null))
                            await serverMessage.AppendIntegerAsync(roomItem.VirtualId);

                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync(string.Empty);
                        await serverMessage.AppendIntegerAsync(2);
                        serverMessage.AppendIntegersArray(extraInfo, ';', 2);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(4);
                        await serverMessage.AppendIntegerAsync(delay);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage);
                    }
                    break;

                case Interaction.ActionMoveToDir:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        await serverMessage.AppendIntegerAsync(15);

                        await serverMessage.AppendIntegerAsync(list.Count);
                        foreach (var roomItem in list.Where(roomItem => roomItem != null))
                            await serverMessage.AppendIntegerAsync(roomItem.VirtualId);

                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync(string.Empty);
                        await serverMessage.AppendIntegerAsync(2);
                        serverMessage.AppendIntegersArray(extraInfo, ';', 2);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(13);
                        await serverMessage.AppendIntegerAsync(delay);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage);
                    }
                    break;

                case Interaction.ActionResetTimer:
                    {
                        var serverMessage14 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage14.AppendBool(false);
                        await serverMessage14.AppendIntegerAsync(0);
                        await serverMessage14.AppendIntegerAsync(0);
                        await serverMessage14.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage14.AppendIntegerAsync(item.VirtualId);
                        await serverMessage14.AppendStringAsync(extraInfo);
                        await serverMessage14.AppendIntegerAsync(0);
                        await serverMessage14.AppendIntegerAsync(0);
                        await serverMessage14.AppendIntegerAsync(1);
                        await serverMessage14.AppendIntegerAsync(delay);
                        await serverMessage14.AppendIntegerAsync(0);
                        await serverMessage14.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage14);
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
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(7);
                        await serverMessage15.AppendIntegerAsync(delay);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionTeleportTo:
                    {
                        var serverMessage16 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage16.AppendBool(false);
                        await serverMessage16.AppendIntegerAsync(15);

                        await serverMessage16.AppendIntegerAsync(list.Count);
                        foreach (var roomItem in list) await serverMessage16.AppendIntegerAsync(roomItem.VirtualId);

                        await serverMessage16.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage16.AppendIntegerAsync(item.VirtualId);
                        await serverMessage16.AppendStringAsync(extraInfo);
                        await serverMessage16.AppendIntegerAsync(0);
                        await serverMessage16.AppendIntegerAsync(8);
                        await serverMessage16.AppendIntegerAsync(0);
                        await serverMessage16.AppendIntegerAsync(delay);
                        await serverMessage16.AppendIntegerAsync(0);
                        serverMessage16.AppendByte(2);
                        await session.SendMessage(serverMessage16);
                        return;
                    }
                case Interaction.ActionToggleState:
                    {
                        var serverMessage17 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage17.AppendBool(false);
                        await serverMessage17.AppendIntegerAsync(15);
                        await serverMessage17.AppendIntegerAsync(list.Count);
                        foreach (var current17 in list) await serverMessage17.AppendIntegerAsync(current17.VirtualId);
                        await serverMessage17.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage17.AppendIntegerAsync(item.VirtualId);
                        await serverMessage17.AppendStringAsync(extraInfo);
                        await serverMessage17.AppendIntegerAsync(0);
                        await serverMessage17.AppendIntegerAsync(8);
                        await serverMessage17.AppendIntegerAsync(0);
                        await serverMessage17.AppendIntegerAsync(delay);
                        await serverMessage17.AppendIntegerAsync(0);
                        await serverMessage17.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage17);
                        return;
                    }
                case Interaction.ActionGiveReward:
                    {
                        if (!session.GetHabbo().HasFuse("fuse_use_superwired")) return;
                        var serverMessage18 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage18.AppendBool(false);
                        await serverMessage18.AppendIntegerAsync(5);
                        await serverMessage18.AppendIntegerAsync(0);
                        await serverMessage18.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage18.AppendIntegerAsync(item.VirtualId);
                        await serverMessage18.AppendStringAsync(extraInfo);
                        await serverMessage18.AppendIntegerAsync(3);
                        await serverMessage18.AppendIntegerAsync(extraString == "" ? 0 : int.Parse(extraString));
                        await serverMessage18.AppendIntegerAsync(flag ? 1 : 0);
                        await serverMessage18.AppendIntegerAsync(extraString2 == "" ? 0 : int.Parse(extraString2));
                        await serverMessage18.AppendIntegerAsync(0);
                        await serverMessage18.AppendIntegerAsync(17);
                        await serverMessage18.AppendIntegerAsync(0);
                        await serverMessage18.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage18);
                        return;
                    }

                case Interaction.ConditionHowManyUsersInRoom:
                case Interaction.ConditionNegativeHowManyUsers:
                    {
                        var serverMessage19 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage19.AppendBool(false);
                        await serverMessage19.AppendIntegerAsync(5);
                        await serverMessage19.AppendIntegerAsync(0);
                        await serverMessage19.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage19.AppendIntegerAsync(item.VirtualId);
                        await serverMessage19.AppendStringAsync("");
                        await serverMessage19.AppendIntegerAsync(2);
                        if (string.IsNullOrWhiteSpace(extraInfo))
                        {
                            await serverMessage19.AppendIntegerAsync(1);
                            await serverMessage19.AppendIntegerAsync(50);
                        }
                        else
                            foreach (var integers in extraInfo.Split(','))
                                await serverMessage19.AppendIntegerAsync(int.Parse(integers));
                        serverMessage19.AppendBool(false);
                        await serverMessage19.AppendIntegerAsync(0);
                        await serverMessage19.AppendIntegerAsync(1290);
                        await session.SendMessage(serverMessage19);
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
                        await serverMessage19.AppendIntegerAsync(15);
                        await serverMessage19.AppendIntegerAsync(list.Count);
                        foreach (var current18 in list) await serverMessage19.AppendIntegerAsync(current18.VirtualId);
                        await serverMessage19.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage19.AppendIntegerAsync(item.VirtualId);
                        await serverMessage19.AppendIntegerAsync(0);
                        await serverMessage19.AppendIntegerAsync(0);
                        await serverMessage19.AppendIntegerAsync(0);
                        serverMessage19.AppendBool(false);
                        serverMessage19.AppendBool(true);
                        await session.SendMessage(serverMessage19);
                        return;
                    }


                case Interaction.ConditionFurniHasNotFurni:
                case Interaction.ConditionFurniHasFurni:
                    {
                        var serverMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage.AppendBool(false);
                        await serverMessage.AppendIntegerAsync(15);
                        await serverMessage.AppendIntegerAsync(list.Count);
                        foreach (var current18 in list) await serverMessage.AppendIntegerAsync(current18.VirtualId);
                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync(string.Empty);
                        await serverMessage.AppendIntegerAsync(1);
                        await serverMessage.AppendIntegerAsync(flag); //bool
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().InteractionType == Interaction.ConditionFurniHasFurni
                            ? 7
                            : 18);
                        await session.SendMessage(serverMessage);
                        return;
                    }

                case Interaction.ConditionFurnisHaveUsers:

                case Interaction.ConditionFurnisHaveNotUsers:
                {
                    var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                    serverMessage.AppendBool(false);
                    await serverMessage.AppendIntegerAsync(15);
                    await serverMessage.AppendIntegerAsync(list.Count);
                    foreach (var current18 in list) await serverMessage.AppendIntegerAsync(current18.VirtualId);
                    await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                    await serverMessage.AppendIntegerAsync(item.VirtualId);
                    await serverMessage.AppendStringAsync(string.Empty);
                    await serverMessage.AppendIntegerAsync(0); //bool
                    await serverMessage.AppendIntegerAsync(0);
                    await serverMessage.AppendIntegerAsync(14);
                    await session.SendMessage(serverMessage);
                    return;
                }
                    

                case Interaction.ConditionTimeLessThan:
                case Interaction.ConditionTimeMoreThan:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage21.AppendIntegerAsync(item.VirtualId);
                        await serverMessage21.AppendStringAsync("");
                        await serverMessage21.AppendIntegerAsync(1);
                        await serverMessage21.AppendIntegerAsync(delay); //delay
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(item.GetBaseItem().InteractionType ==
                                                                 Interaction.ConditionTimeMoreThan
                            ? 3
                            : 4);
                        await session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.ConditionUserWearingEffect:
                case Interaction.ConditionUserNotWearingEffect:
                    {
                        int.TryParse(extraInfo, out var effect);
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        await serverMessage21.AppendIntegerAsync(5);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage21.AppendIntegerAsync(item.VirtualId);
                        await serverMessage21.AppendStringAsync("");
                        await serverMessage21.AppendIntegerAsync(1);
                        await serverMessage21.AppendIntegerAsync(effect);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(12);
                        await session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.ConditionUserWearingBadge:
                case Interaction.ConditionUserNotWearingBadge:
                case Interaction.ConditionUserHasFurni:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        await serverMessage21.AppendIntegerAsync(5);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage21.AppendIntegerAsync(item.VirtualId);
                        await serverMessage21.AppendStringAsync(extraInfo);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(11);
                        await session.SendMessage(serverMessage21);
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
                        await serverMessage21.AppendIntegerAsync(5);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage21.AppendIntegerAsync(item.VirtualId);
                        await serverMessage21.AppendStringAsync(extraInfo);
                        await serverMessage21.AppendIntegerAsync(2);
                        await serverMessage21.AppendIntegerAsync(date1);
                        await serverMessage21.AppendIntegerAsync(date2);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(24);
                        await session.SendMessage(serverMessage21);
                        return;
                    }

                case Interaction.TriggerBotReachedAvatar:
                    {
                        var serverMessage2 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage2.AppendBool(false);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(list.Count);
                        foreach (var current2 in list) await serverMessage2.AppendIntegerAsync(current2.VirtualId);
                        await serverMessage2.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage2.AppendIntegerAsync(item.VirtualId);
                        await serverMessage2.AppendStringAsync(extraInfo);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(14);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage2);
                        return;
                    }
                case Interaction.TriggerBotReachedStuff:
                    {
                        var serverMessage2 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredTriggerMessageComposer"));
                        serverMessage2.AppendBool(false);
                        await serverMessage2.AppendIntegerAsync(15);
                        await serverMessage2.AppendIntegerAsync(list.Count);
                        foreach (var current2 in list) await serverMessage2.AppendIntegerAsync(current2.VirtualId);
                        await serverMessage2.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage2.AppendIntegerAsync(item.VirtualId);
                        await serverMessage2.AppendStringAsync(extraInfo);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(13);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await serverMessage2.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage2);
                        return;
                    }
                case Interaction.ActionBotClothes:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo + (char)9 + extraString);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(26);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotFollowAvatar:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(25);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotGiveHanditem:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo);
                        await serverMessage15.AppendIntegerAsync(1);
                        await serverMessage15.AppendIntegerAsync(delay);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(24);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotMove:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        await serverMessage15.AppendIntegerAsync(15);
                        await serverMessage15.AppendIntegerAsync(list.Count);
                        foreach (var current2 in list) await serverMessage15.AppendIntegerAsync(current2.VirtualId);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(22);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotTalk:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo + (char)9 + extraString);
                        await serverMessage15.AppendIntegerAsync(1);
                        await serverMessage15.AppendIntegerAsync(Oblivion.BoolToInteger(flag));
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(23);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotTalkToAvatar:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo + (char)9 + extraString);
                        await serverMessage15.AppendIntegerAsync(1);
                        await serverMessage15.AppendIntegerAsync(Oblivion.BoolToInteger(flag));
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(27);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionBotTeleport:
                    {
                        var serverMessage15 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage15.AppendBool(false);
                        await serverMessage15.AppendIntegerAsync(15);
                        await serverMessage15.AppendIntegerAsync(list.Count);
                        foreach (var current2 in list) await serverMessage15.AppendIntegerAsync(current2.VirtualId);
                        await serverMessage15.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage15.AppendIntegerAsync(item.VirtualId);
                        await serverMessage15.AppendStringAsync(extraInfo);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(21);
                        await serverMessage15.AppendIntegerAsync(0);
                        await serverMessage15.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage15);
                        return;
                    }
                case Interaction.ActionChase:
                case Interaction.ActionInverseChase:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        await serverMessage.AppendIntegerAsync(15);

                        await serverMessage.AppendIntegerAsync(list.Count);
                        foreach (var roomItem in list) await serverMessage.AppendIntegerAsync(roomItem.VirtualId);

                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync(string.Empty);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(11);
                        await serverMessage.AppendIntegerAsync(0);

                        await serverMessage.AppendIntegerAsync(0);

                        await session.SendMessage(serverMessage);
                        return;
                    }
                case Interaction.ConditionUserHasHanditem:
                    {
                        var serverMessage21 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredConditionMessageComposer"));
                        serverMessage21.AppendBool(false);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage21.AppendIntegerAsync(item.VirtualId);
                        await serverMessage21.AppendStringAsync(extraInfo);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(0);
                        await serverMessage21.AppendIntegerAsync(25);
                        await session.SendMessage(serverMessage21);
                        return;
                    }
                case Interaction.ActionCallStacks:
                    {
                        var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage.AppendBool(false);
                        await serverMessage.AppendIntegerAsync(15);
                        await serverMessage.AppendIntegerAsync(list.Count);
                        foreach (var current15 in list) await serverMessage.AppendIntegerAsync(current15.VirtualId);
                        await serverMessage.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage.AppendIntegerAsync(item.VirtualId);
                        await serverMessage.AppendStringAsync(extraInfo);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(18);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage);
                        return;
                    }


                case Interaction.SpecialRandom:
                case Interaction.SpecialUnseen:
                    {
                        var serverMessage25 =
                            new ServerMessage(LibraryParser.OutgoingRequest("WiredEffectMessageComposer"));
                        serverMessage25.AppendBool(false);
                        await serverMessage25.AppendIntegerAsync(15);
                        await serverMessage25.AppendIntegerAsync(list.Count);
                        foreach (var current24 in list) await serverMessage25.AppendIntegerAsync(current24.VirtualId);
                        await serverMessage25.AppendIntegerAsync(item.GetBaseItem().SpriteId);
                        await serverMessage25.AppendIntegerAsync(item.VirtualId);
                        await serverMessage25.AppendStringAsync(extraInfo);
                        await serverMessage25.AppendIntegerAsync(0);
                        await serverMessage25.AppendIntegerAsync(8);
                        await serverMessage25.AppendIntegerAsync(0);
                        await serverMessage25.AppendIntegerAsync(0);
                        await serverMessage25.AppendIntegerAsync(0);
                        await serverMessage25.AppendIntegerAsync(0);
                        await session.SendMessage(serverMessage25);
                        return;
                    }
                default:
                    return;
            }
        }
    }
}