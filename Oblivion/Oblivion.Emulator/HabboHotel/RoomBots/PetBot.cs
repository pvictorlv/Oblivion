using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Pets.Enums;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.RoomBots
{
    /// <summary>
    ///     Class PetBot.
    /// </summary>
    internal class PetBot : BotAi
    {
        /// <summary>
        ///     The _action timer
        /// </summary>
        private int _actionTimer;

        /// <summary>
        ///     The _energy timer
        /// </summary>
        private int _energyTimer;

        /// <summary>
        ///     The _speech timer
        /// </summary>
        private int _speechTimer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PetBot" /> class.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        internal PetBot(int virtualId)
        {
            {
                _speechTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
                _actionTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + virtualId);
                _energyTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
            }
        }

        /// <summary>
        ///     Called when [self enter room].
        /// </summary>
        internal override async void OnSelfEnterRoom()
        {
            try
            {
                var randomWalkableSquare = GetRoom().GetGameMap().GetRandomWalkableSquare();
                if (GetRoomUser() != null && GetRoomUser().PetData.Type != 16u)
                {
                    await GetRoomUser().MoveTo(randomWalkableSquare.X, randomWalkableSquare.Y);
                }
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PetBot.OnSelfEnterRoom");
            }
        }

        /// <summary>
        ///     Called when [self leave room].
        /// </summary>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal override void OnSelfLeaveRoom(bool kicked)
        {
        }

        internal override Task OnChatTick()
        {
            return Task.CompletedTask;
        }

        internal override void StopTimerTick()
        {
        }

        /// <summary>
        ///     Modifieds this instance.
        /// </summary>
        internal override void Modified()
        {
        }

        /// <summary>
        ///     Called when [user enter room].
        /// </summary>
        /// <param name="user">The user.</param>
        internal override async Task OnUserEnterRoom(RoomUser user)
        {
            if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            var roomUser = GetRoomUser();
            if (roomUser == null || user.GetClient().GetHabbo().UserName != roomUser.PetData.OwnerName)
                return;

            var random = new Random();
            var value = PetLocale.GetValue("welcome.speech.pet");
            var message = value[random.Next(0, (value.Length - 1))];

            message += user.GetUserName();
            await roomUser.Chat(null, message, false, 0);
        }

        /// <summary>
        ///     Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal override void OnUserLeaveRoom(GameClient client)
        {
        }

        /// <summary>
        ///     Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="msg">The MSG.</param>
        internal override async Task OnUserSay(RoomUser user, string msg)
        {
            var roomUser = GetRoomUser();

            if (roomUser.PetData.OwnerId != user.GetClient().GetHabbo().Id)
            {
                return;
            }

            if (string.IsNullOrEmpty(msg))
            {
                msg = " ";
            }

            msg = msg.Substring(1);

            var lazy = false;
            var unknown = false;
            var sleeping = false;
            try
            {
                var command = PetCommandHandler.TryInvoke(msg);
                switch (command)
                {
                    case 1:
                        RemovePetStatus();
                        roomUser.FollowingOwner = null;
                        break;

                    case 2:
                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        RemovePetStatus();
                        await roomUser.PetData.AddExperience(10);
                        roomUser.Statusses.TryAdd("sit", "");
                        roomUser.Statusses.TryAdd("gst", "joy");
                        roomUser.UpdateNeeded = true;
                        _actionTimer = 25;
                        _energyTimer = 10;

                        SubtractAttributes();
                        break;

                    case 3:
                        if (!roomUser.PetData.HasCommand(2))
                        {
                            unknown = true;
                            break;
                        }

                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        RemovePetStatus();
                        await roomUser.PetData.AddExperience(10);
                        roomUser.Statusses.TryAdd("lay", "");
                        roomUser.Statusses.TryAdd("gst", "sml");
                        roomUser.UpdateNeeded = true;
                        _actionTimer = 25;
                        _energyTimer = 10;

                        SubtractAttributes();
                        break;

                    case 4:
                        if (!roomUser.PetData.HasCommand(3))
                        {
                            unknown = true;
                            break;
                        }

                        if (!roomUser.PetData.HasCommand(7))
                        {
                            unknown = true;
                            break;
                        }

                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        RemovePetStatus();
                        await roomUser.PetData.AddExperience(11);
                        await roomUser.MoveTo(user.SquareInFront);
                        roomUser.Statusses.TryAdd("gst", "sml");
                        roomUser.UpdateNeeded = true;

                        _actionTimer = 25;
                        _energyTimer = 10;
                        SubtractAttributes();
                        break;

                    case 5:
                        if (!roomUser.PetData.HasCommand(8))
                        {
                            unknown = true;
                            break;
                        }

                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        RemovePetStatus();
                        await roomUser.PetData.AddExperience(25);
                        roomUser.Statusses.TryAdd("std", "");
                        roomUser.UpdateNeeded = true;

                        _actionTimer = 25;
                        _energyTimer = 10;
                        SubtractAttributes();
                        break;

                    case 6:
                        if (!roomUser.PetData.HasCommand(19))
                        {
                            unknown = true;
                            break;
                        }

                        if (!roomUser.PetData.HasCommand(9))
                        {
                            unknown = true;
                            break;
                        }

                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        RemovePetStatus();
                        await roomUser.PetData.AddExperience(35);
                        roomUser.Statusses.TryAdd("jmp", "");
                        roomUser.Statusses.TryAdd("gst", "joy");
                        roomUser.UpdateNeeded = true;

                        _actionTimer = 45;
                        _energyTimer = 20;
                        SubtractAttributes();
                        break;

                    case 7:
                        if (!roomUser.PetData.HasCommand(15))
                        {
                            unknown = true;
                            break;
                        }

                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        roomUser.FollowingOwner = roomUser;

                        RemovePetStatus();
                        switch (roomUser.RotBody)
                        {
                            case 0:
                                await roomUser.MoveTo(roomUser.X + 2, roomUser.Y);
                                break;

                            case 1:
                                await roomUser.MoveTo(roomUser.X - 2, roomUser.Y - 2);
                                break;

                            case 2:
                                await roomUser.MoveTo(roomUser.X, roomUser.Y + 2);
                                break;

                            case 3:
                                await roomUser.MoveTo(roomUser.X + 2, roomUser.Y - 2);
                                break;

                            case 4:
                                await roomUser.MoveTo(roomUser.X - 2, roomUser.Y);
                                break;

                            case 5:
                                await roomUser.MoveTo(roomUser.X + 2, roomUser.Y + 2);
                                break;

                            case 6:
                                await roomUser.MoveTo(roomUser.X, roomUser.Y - 2);
                                break;

                            case 7:
                                await roomUser.MoveTo(roomUser.X - 2, roomUser.Y + 2);
                                break;
                        }

                        await roomUser.PetData.AddExperience(35);
                        roomUser.Statusses.TryAdd("gst", "sml");
                        roomUser.UpdateNeeded = true;
                        break;

                    case 8:
                        if (!roomUser.PetData.HasCommand(4))
                        {
                            unknown = true;
                            break;
                        }

                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        RemovePetStatus();
                        await roomUser.PetData.AddExperience(11);
                        roomUser.Statusses.TryAdd("beg", "");
                        roomUser.Statusses.TryAdd("gst", "sml");
                        roomUser.UpdateNeeded = true;

                        _actionTimer = 25;
                        _energyTimer = 10;
                        SubtractAttributes();
                        break;

                    case 9:
                        if (!roomUser.PetData.HasCommand(10))
                        {
                            unknown = true;
                            break;
                        }

                        if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                        {
                            lazy = true;
                            break;
                        }

                        _actionTimer = 1;
                        _energyTimer = 10;
                        roomUser.Statusses.TryAdd("gst", "sml");
                        roomUser.UpdateNeeded = true;
                        await roomUser.PetData.AddExperience(35);
                        SubtractAttributes();
                        break;

                    case 10:
                        RemovePetStatus();

                        RoomItem petNest = GetRoom()
                            .GetRoomItemHandler()
                            .FloorItems.Values.FirstOrDefault(x => x.GetBaseItem().InteractionType == Interaction.PetNest);

                        if (petNest == null)
                        {
                            lazy = true;
                            break;
                        }

                        await roomUser.MoveTo(petNest.X, petNest.Y);
                        await roomUser.PetData.AddExperience(40);
                        var rndmEnergy = new Random().Next(25, 51);
                        if (roomUser.PetData.Energy < (Pet.MaxEnergy - rndmEnergy))
                        {
                            roomUser.PetData.Energy += rndmEnergy;
                        }

                        roomUser.PetData.Nutrition += 15;
                        roomUser.AddStatus("lay", "");
                        roomUser.AddStatus("gst", "eyb");
                        roomUser.UpdateNeeded = true;
                        sleeping = true;
                        _actionTimer = 500;
                        _energyTimer = 500;
                        break;

                    case 46:
                        RemovePetStatus();

                        var coord = new Point();
                        switch (roomUser.PetData.Type)
                        {
                            case 3:
                                coord = GetRoom().GetRoomItemHandler().GetRandomBreedingTerrier(roomUser.PetData);
                                break;

                            case 4:
                                coord = GetRoom().GetRoomItemHandler().GetRandomBreedingBear(roomUser.PetData);
                                break;
                        }

                        if (coord == new Point())
                        {
                            var alert = new ServerMessage(
                                LibraryParser.OutgoingRequest("PetBreedErrorMessageComposer"));
                            await alert.AppendIntegerAsync(0);
                            await user.GetClient().SendMessageAsync(alert);

                            return;
                        }

                        await roomUser.MoveTo(coord);
                        await roomUser.PetData.AddExperience(0);
                        roomUser.PetData.PetEnergy(true);

                        _actionTimer = 25;
                        _energyTimer = 120;
                        break;

                    default:
                        lazy = true;
                        SubtractAttributes();
                        break;
                }
            }
            catch (Exception)
            {
                lazy = true;
                SubtractAttributes();
            }

            if (sleeping)
            {
                var value = PetLocale.GetValue("tired");
                var message = value[new Random().Next(0, (value.Length - 1))];

                await roomUser.Chat(null, message, false, 0);
            }
            else if (unknown)
            {
                var value = PetLocale.GetValue("pet.unknowncommand");
                var message = value[new Random().Next(0, (value.Length - 1))];

                await roomUser.Chat(null, message, false, 0);
            }
            else if (lazy)
            {
                var value = PetLocale.GetValue("pet.lazy");
                var message = value[new Random().Next(0, (value.Length - 1))];

                await roomUser.Chat(null, message, false, 0);
            }
            else
            {
                var value = PetLocale.GetValue("pet.done");
                var message = value[new Random().Next(0, (value.Length - 1))];

                await roomUser.Chat(null, message, false, 0);
            }
        }

        /// <summary>
        ///     Called when [user shout].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override Task OnUserShout(RoomUser user, string message)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called when [timer tick].
        /// </summary>
        internal override async Task OnTimerTick()
        {
            if (_speechTimer <= 0)
            {
                var roomUser = GetRoomUser();
                if (roomUser != null)
                {
                    if (roomUser.PetData.DbState != DatabaseUpdateState.NeedsInsert)
                        roomUser.PetData.DbState = DatabaseUpdateState.NeedsUpdate;
                    var random = new Random();
                    RemovePetStatus();
                    var value = PetLocale.GetValue($"speech.pet{roomUser.PetData.Type}");
                    var text = value[random.Next(0, value.Length - 1)];
                    if (GetRoom() != null && !GetRoom().MutedPets) await roomUser.Chat(null, text, false, 0);
                    else roomUser.Statusses.TryAdd(text, TextHandling.GetString(roomUser.Z));
                }

                _speechTimer = Oblivion.GetRandomNumber(20, 120);
            }
            else _speechTimer--;

            if (_actionTimer <= 0 && GetRoomUser() != null)
            {
                try
                {
                    _actionTimer = GetRoomUser().FollowingOwner != null
                        ? 2
                        : Oblivion.GetRandomNumber(15, 40 + GetRoomUser().PetData.VirtualId);
                    RemovePetStatus();
                    _actionTimer = Oblivion.GetRandomNumber(15, 40 + GetRoomUser().PetData.VirtualId);
                    if (GetRoomUser().RidingHorse != true)
                    {
                        RemovePetStatus();

                        if (GetRoomUser().FollowingOwner != null)
                        {
                            await GetRoomUser().MoveTo(GetRoomUser().FollowingOwner.SquareBehind);
                        }
                        else
                        {
                            if (GetRoomUser().PetData.Type == 16) return; //Monsterplants can't move
                            var nextCoord = GetRoom().GetGameMap().GetRandomValidWalkableSquare();
                            await GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                        }
                    }

                    if (new Random().Next(2, 15) % 2 == 0)
                    {
                        if (GetRoomUser().PetData.Type == 16)
                        {
                            var breed = GetRoomUser().PetData.MoplaBreed;
                            GetRoomUser().PetData.Energy--;
                            GetRoomUser().AddStatus("gst", (breed.LiveState == MoplaState.Dead) ? "sad" : "sml");
                            await GetRoomUser()
                                .PetData.MoplaBreed.OnTimerTick(GetRoomUser().PetData.LastHealth,
                                    GetRoomUser().PetData.UntilGrown);
                        }
                        else
                        {
                            if (GetRoomUser().PetData.Energy < 30) GetRoomUser().AddStatus("lay", "");
                            else
                            {
                                GetRoomUser().AddStatus("gst", "joy");
                                if (new Random().Next(1, 7) == 3) GetRoomUser().AddStatus("snf", "");
                            }
                        }

                        GetRoomUser().UpdateNeeded = true;
                    }

                    goto IL_1B5;
                }
                catch (Exception pException)
                {
                    Logging.HandleException(pException, "PetBot.OnTimerTick");
                    goto IL_1B5;
                }
            }

            _actionTimer--;
            IL_1B5:
            if (_energyTimer <= 0)
            {
                RemovePetStatus();
                var roomUser2 = GetRoomUser();
                roomUser2?.PetData?.PetEnergy(true);
                _energyTimer = Oblivion.GetRandomNumber(30, 120);
                return;
            }

            _energyTimer--;
        }

        /// <summary>
        ///     Removes the pet status.
        /// </summary>
        private void RemovePetStatus()
        {
            var roomUser = GetRoomUser();

            if (roomUser == null) return;
            roomUser.Statusses.Clear();
            roomUser.UpdateNeeded = true;
        }

        /// <summary>
        ///     Subtracts the attributes.
        /// </summary>
        private void SubtractAttributes()
        {
            var roomUser = GetRoomUser();
            if (roomUser == null) return;

            if (roomUser.PetData.Energy < 11)
                roomUser.PetData.Energy = 0;
            else
                roomUser.PetData.Energy -= 10;
            if (roomUser.PetData.Nutrition < 6)
                roomUser.PetData.Nutrition = 0;
            else
                roomUser.PetData.Nutrition -= 5;
        }
    }
}