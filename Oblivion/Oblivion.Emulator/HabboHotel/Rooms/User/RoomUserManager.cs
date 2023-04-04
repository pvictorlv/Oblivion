using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.Configuration;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Navigators.Interfaces;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Pets.Enums;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Freeze;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Rooms.User
{
    /// <summary>
    ///     Class RoomUserManager.
    /// </summary>
    internal class RoomUserManager
    {
        /// <summary>
        ///     The _to remove
        /// </summary>
        private readonly ConcurrentList<RoomUser> _removeUsers;


        /// <summary>
        ///     The _bots
        /// </summary>
        internal Dictionary<uint, RoomUser> Bots;

        /// <summary>
        ///     The _pets
        /// </summary>
        private Dictionary<uint, RoomUser> _pets;

        /// <summary>
        ///     The _primary private user identifier
        /// </summary>
        private int _primaryPrivateUserId;

        /// <summary>
        ///     The _user count
        /// </summary>
        private uint _roomUserCount;

        /// <summary>
        ///     The _secondary private user identifier
        /// </summary>
        private int _secondaryPrivateUserId;

        /// <summary>
        ///     The _room
        /// </summary>
        private Room _room;

        /// <summary>
        ///     The users by user identifier
        /// </summary>
        internal ConcurrentDictionary<uint, RoomUser> UsersByUserId;

        /// <summary>
        ///     The users by user name
        /// </summary>
        internal ConcurrentDictionary<string, RoomUser> UsersByUserName;


        /// <summary>
        /// Set disposed class status
        /// </summary>
        public bool Disposed;


        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomUserManager" /> class.
        /// </summary>
        /// <param name="room">The room.</param>
        public RoomUserManager(Room room)
        {
            _room = room;
            UserList = new ConcurrentDictionary<int, RoomUser>();
            _pets = new Dictionary<uint, RoomUser>();
            Bots = new Dictionary<uint, RoomUser>();
            UsersByUserName = new ConcurrentDictionary<string, RoomUser>();
            UsersByUserId = new ConcurrentDictionary<uint, RoomUser>();
            _primaryPrivateUserId = 0;
            _secondaryPrivateUserId = 0;
            _removeUsers = new ConcurrentList<RoomUser>((int)room.RoomData.UsersMax);
            PetCount = 0;
            _roomUserCount = 0;
        }

        /// <summary>
        ///     Gets the pet count.
        /// </summary>
        /// <value>The pet count.</value>
        internal int PetCount { get; private set; }

        /// <summary>
        ///     Gets the user list.
        /// </summary>
        /// <value>The user list.</value>
        internal ConcurrentDictionary<int, RoomUser> UserList { get; private set; }

        /// <summary>
        ///     Gets the room user by habbo.
        /// </summary>
        /// <param name="pId">The p identifier.</param>
        /// <returns>RoomUser.</returns>
        public RoomUser GetRoomUserByHabbo(uint pId) => UsersByUserId.TryGetValue(pId, out var usr)
            ? usr
            : null;

        /// <summary>
        ///     Gets the room user count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetRoomUserCount()
        {
            if (UserList == null || Bots == null || _pets == null) return 0;

            var sum = UserList.Count - Bots.Count - _pets.Count;

            return sum >= 5 ? (sum) * Oblivion.Multipy : sum;
        }

        /// <summary>
        ///     Deploys the bot.
        /// </summary>
        /// <param name="bot">The bot.</param>
        /// <param name="petData">The pet data.</param>
        /// <returns>RoomUser.</returns>
        internal async Task<RoomUser> DeployBot(RoomBot bot, Pet petData)
        {
            var virtualId = _primaryPrivateUserId++;
            var roomUser = new RoomUser(0u, _room.RoomId, virtualId, _room, false);
            var num = _secondaryPrivateUserId++;
            roomUser.InternalRoomId = num;
            UserList.TryAdd(num, roomUser);
            await OnUserAdd(roomUser);

            var model = _room.GetGameMap().Model;
            var coord = new Point(bot.X, bot.Y);
            if ((bot.X > 0) && (bot.Y >= 0) && (bot.X < model.MapSizeX) && (bot.Y < model.MapSizeY))
            {
                _room.GetGameMap().AddUserToMap(roomUser, coord);
                roomUser.SetPos(bot.X, bot.Y, bot.Z);
                roomUser.SetRot(bot.Rot, false);
            }
            else
            {
                bot.X = model.DoorX;
                bot.Y = model.DoorY;
                roomUser.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                roomUser.SetRot(model.DoorOrientation, false);
            }

            bot.RoomUser = roomUser;
            roomUser.BotData = bot;

            roomUser.BotAi = bot.GenerateBotAi(roomUser.VirtualId, (int)bot.BotId);
            if (roomUser.IsPet)
            {
                roomUser.BotAi.Init(bot.BotId, roomUser.VirtualId, _room.RoomId, roomUser, _room);
                roomUser.PetData = petData;
                roomUser.PetData.VirtualId = roomUser.VirtualId;
            }
            else
            {
                roomUser.BotAi.Init(bot.BotId, roomUser.VirtualId, _room.RoomId, roomUser, _room);
            }

            await UpdateUserStatus(roomUser, false);
            roomUser.UpdateNeeded = true;
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer")))
            {
                await serverMessage.AppendIntegerAsync(1);
                roomUser.Serialize(serverMessage);
                await _room.SendMessage(serverMessage);
                roomUser.BotAi.OnSelfEnterRoom();
                if (roomUser.IsPet)
                {
                    _pets[roomUser.PetData.PetId] = roomUser;

                    PetCount++;
                }

                roomUser.BotAi.Modified();

                if (roomUser.BotData.AiType != AiType.Generic)
                    return roomUser;

                Bots[roomUser.BotData.BotId] = roomUser;

                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                await serverMessage.AppendIntegerAsync(roomUser.VirtualId);
                await serverMessage.AppendIntegerAsync(roomUser.BotData.DanceId);
                await _room.SendMessage(serverMessage);
               
                return roomUser;
            }
        }


        /// <summary>
        ///     Updates the bot.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="roomUser">The room user.</param>
        /// <param name="name">The name.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="look">The look.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="speech">The speech.</param>
        /// <param name="responses">The responses.</param>
        /// <param name="speak">if set to <c>true</c> [speak].</param>
        /// <param name="speechDelay">The speech delay.</param>
        /// <param name="mix">if set to <c>true</c> [mix].</param>
        internal void UpdateBot(int virtualId, RoomUser roomUser, string name, string motto, string look,
            string gender,
            List<string> speech, List<string> responses, bool speak, int speechDelay, bool mix)
        {
            var bot = GetRoomUserByVirtualId(virtualId);
            if (bot == null || !bot.IsBot) return;

            var rBot = bot.BotData;

            rBot.Name = name;
            rBot.Motto = motto;
            rBot.Look = look;
            rBot.Gender = gender;
            rBot.RandomSpeech = speech;
            rBot.Responses = responses;
            rBot.AutomaticChat = speak;
            rBot.SpeechInterval = speechDelay;
            rBot.RoomUser = roomUser;
            rBot.MixPhrases = mix;

            rBot.RoomUser?.BotAi?.Modified();
        }

        /// <summary>
        ///     Removes the bot.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal async Task RemoveBot(int virtualId, bool kicked)
        {
            var roomUserByVirtualId = GetRoomUserByVirtualId(virtualId);
            if (roomUserByVirtualId == null || !roomUserByVirtualId.IsBot) return;

            if (roomUserByVirtualId.IsPet)
            {
                _pets.Remove(roomUserByVirtualId.PetData.PetId);
                PetCount--;
            }
            else if (roomUserByVirtualId.IsBot)
            {
                Bots.Remove(roomUserByVirtualId.BotData.BotId);
            }

            roomUserByVirtualId.BotAi.OnSelfLeaveRoom(kicked);
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer")))
            {
                await serverMessage.AppendStringAsync(roomUserByVirtualId.VirtualId.ToString());
                await _room.SendMessageAsync(serverMessage);

                UserList.TryRemove(roomUserByVirtualId.InternalRoomId, out _);
            }
        }

        /// <summary>
        ///     Gets the user for square.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetUserForSquare(int x, int y)
        {
            var users = _room.GetGameMap().GetRoomUsers(new Point(x, y));
            return users.Count > 0 ? users[0] : null;
        }


        /// <summary>
        ///     Adds the user to room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="spectator">if set to <c>true</c> [spectator].</param>
        /// <param name="snow">if set to <c>true</c> [snow].</param>
        internal async Task AddUserToRoom(GameClient session, bool spectator, bool snow = false)
        {
            if (Disposed)
                return;
            var habbo = session?.GetHabbo();

            if (habbo == null || _room == null)
                return;
            var roomUser = new RoomUser(habbo.Id, _room.RoomId, _primaryPrivateUserId++, _room,
                    spectator)
                { UserId = habbo.Id };


            var userName = habbo.UserName;
            var userId = roomUser.UserId;
            UsersByUserName[userName.ToLower()] = roomUser;
            UsersByUserId[userId] = roomUser;

            var num = _secondaryPrivateUserId++;
            roomUser.InternalRoomId = num;
            session.CurrentRoomUserId = num;
            habbo.CurrentRoomId = _room.RoomId;
            habbo.CurrentRoom = _room;
            UserList[num] = roomUser;
            await OnUserAdd(roomUser);

            habbo.LoadingRoom = 0;

            if (Oblivion.GetGame().GetNavigator().PrivateCategories.Contains(_room.RoomData.Category))
                ((FlatCat)Oblivion.GetGame().GetNavigator().PrivateCategories[_room.RoomData.Category]).UsersNow++;
        }

        /// <summary>
        ///     Updates the user.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        internal void UpdateUser(string oldName, string newName)
        {
            if (oldName == newName)
                return;

            if (!UsersByUserName.TryRemove(oldName, out var user))
                return;

            UsersByUserName.TryAdd(newName, user);

            Oblivion.GetGame().GetClientManager().UpdateClient(oldName, newName);
        }

        /// <summary>
        ///     Removes the user from room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="notifyClient">if set to <c>true</c> [notify client].</param>
        /// <param name="notifyKick">if set to <c>true</c> [notify kick].</param>
        internal async Task RemoveUserFromRoom(GameClient session, bool notifyClient, bool notifyKick)
        {
            if (session?.GetHabbo()?.UserName != null)
            {
                var roomUser = GetRoomUserByHabbo(session.GetHabbo().UserName);
                if (roomUser == null) return;
                await RemoveUserFromRoom(roomUser, notifyClient, notifyKick);
            }
        }

        /// <summary>
        ///     Removes the user from room.
        /// </summary>
        /// <param name="user">The session.</param>
        /// <param name="notifyClient">if set to <c>true</c> [notify client].</param>
        /// <param name="notifyKick">if set to <c>true</c> [notify kick].</param>
        internal async Task RemoveUserFromRoom(RoomUser user, bool notifyClient, bool notifyKick)
        {
            try
            {
                var client = user?.GetClient();
                var habbo = user?.GetClient()?.GetHabbo();
                if (client == null || habbo == null)
                {
                    await RemoveRoomUser(user);

                    UsersByUserId.TryRemove(user.UserId, out _);
                    var rUser = UsersByUserName.FirstOrDefault(s => s.Value.UserId == user.UserId);
                    if (rUser.Value != null)
                        UsersByUserName.TryRemove(rUser.Key, out _);
                    
                    user.Dispose();
                    
                    return;
                }
                habbo.CurrentRoom = null;
                habbo.GetAvatarEffectsInventoryComponent()?.OnRoomExit();
                var room = _room;

                if (notifyKick)
                {
                    var model = room.GetGameMap().Model;
                    if (model == null) return;

                    await user.MoveTo(model.DoorX, model.DoorY);
                    user.CanWalk = false;
                    await client.GetMessageHandler()
                        .GetResponse()
                        .InitAsync(LibraryParser.OutgoingRequest("RoomErrorMessageComposer"));
                    await client.GetMessageHandler().GetResponse().AppendIntegerAsync(4008);
                    await client.GetMessageHandler().SendResponse();

                    await client.GetMessageHandler()
                        .GetResponse()
                        .InitAsync(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                    await client.GetMessageHandler().GetResponse().AppendShortAsync(2);
                    await client.GetMessageHandler().SendResponse();
                }
                else if (notifyClient)
                {
                    using (var serverMessage =
                           new ServerMessage(LibraryParser.OutgoingRequest("UserIsPlayingFreezeMessageComposer")))
                    {
                        serverMessage.AppendBool(user.Team != Team.None);
                        await client.SendMessage(serverMessage);
                        await client.GetMessageHandler()
                            .GetResponse()
                            .InitAsync(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                        await client.GetMessageHandler().GetResponse().AppendShortAsync(2);
                        await client.GetMessageHandler().SendResponse();
                    }
                }

                if (user.Team != Team.None)
                {
                    room.GetTeamManagerForBanzai().OnUserLeave(user);
                    room.GetTeamManagerForFreeze().OnUserLeave(user);
                }

                if (user.RidingHorse)
                {
                    user.RidingHorse = false;
                    var horse = GetRoomUserByVirtualId((int)user.HorseId);
                    if (horse != null)
                    {
                        horse.RidingHorse = false;
                        horse.HorseId = 0u;
                    }
                }

                if (user.IsLyingDown || user.IsSitting)
                {
                    user.IsSitting = false;
                    user.IsLyingDown = false;
                }

                await RemoveRoomUser(user);
                if (!user.IsSpectator)
                {
                    if (user.CurrentItemEffect != 0)
                        user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect =
                            -1;
                    if (room.HasActiveTrade(habbo.Id))
                        await room.TryStopTrade(habbo.Id);
                    habbo.CurrentRoomId = 0;
                    if (habbo.GetMessenger() != null)
                        await habbo.GetMessenger().OnStatusChanged(true);


                    UsersByUserName?.TryRemove(habbo.UserName.ToLower(), out _);
                }

                UsersByUserId.TryRemove(user.UserId, out _);
                user.Dispose();
            }
            catch (Exception ex)
            {
                await RemoveRoomUser(user);

                Logging.LogCriticalException($"Error during removing user from room:{ex}");
            }
        }

        /// <summary>
        ///     Removes the room user.
        /// </summary>
        /// <param name="user">The user.</param>
        internal async Task RemoveRoomUser(RoomUser user)
        {
            if (user == null) return;

            UserList.TryRemove(user.InternalRoomId, out _);

            user.InternalRoomId = -1;
            _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;
            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer")))
            {
                await serverMessage.AppendStringAsync(user.VirtualId.ToString());
                await _room.SendMessage(serverMessage);

                OnRemove(user);
            }
        }

        /// <summary>
        ///     Gets the pet.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetPet(uint petId) => _pets.TryGetValue(petId, out var pet) ? pet : null;

        /// <summary>
        ///     Gets the bot.
        /// </summary>
        /// <param name="botId">The bot identifier.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetBot(uint botId) => Bots.TryGetValue(botId, out var bot) ? bot : null;

        internal RoomUser GetBotByName(string name)
        {
            var roomUser = Bots.Values.FirstOrDefault(b => b.BotData != null && b.BotData.Name == name);
            return roomUser;
        }

        /// <summary>
        ///     Updates the user count.
        /// </summary>
        /// <param name="count">The count.</param>
        internal void UpdateUserCount(uint count)
        {
            _roomUserCount = count;
            if (_room?.RoomData == null)
                return;

            _room.RoomData.UsersNow = count;

            Oblivion.GetGame().GetRoomManager().QueueActiveRoomUpdate(_room.RoomData);
        }

        /// <summary>
        ///     Gets the room user by virtual identifier.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetRoomUserByVirtualId(int virtualId) => UserList.TryGetValue(virtualId, out var user)
            ? user
            : null;

        /// <summary>
        ///     Gets the room users.
        /// </summary>
        /// <returns>HashSet&lt;RoomUser&gt;.</returns>
        internal List<RoomUser> GetRoomUsers() => new List<RoomUser>(UserList.Values.Where(x => x.IsBot == false));

        /// <summary>
        ///     Gets the room user by rank.
        /// </summary>
        /// <param name="minRank">The minimum rank.</param>
        /// <returns>List&lt;RoomUser&gt;.</returns>
        internal List<RoomUser> GetRoomUserByRank(int minRank)
        {
            return
                UserList.Values.Where(
                    current =>
                        !current.IsBot && current.GetClient() != null && current.GetClient().GetHabbo() != null &&
                        current.GetClient().GetHabbo().Rank > (ulong)minRank).ToList();
        }

        /// <summary>
        ///     Gets the room user by habbo.
        /// </summary>
        /// <param name="pName">Name of the p.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetRoomUserByHabbo(string pName) =>
            UsersByUserName.TryGetValue(pName.ToLower(), out var user) ? user : null;

        /// <summary>
        ///     Saves the pets.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal async Task SavePets(IQueryAdapter dbClient)
        {
            try
            {
                if (GetPets().Count > 0)
                    await AppendPetsUpdateString(dbClient);
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(string.Concat("Error during saving furniture for room ", _room.RoomId,
                    ". Stack: ", ex.ToString()));
            }
        }

        /// <summary>
        ///     Appends the pets update string.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal async Task AppendPetsUpdateString(IQueryAdapter dbClient)
        {
            var queryChunk = new QueryChunk("INSERT INTO bots (id,user_id,room_id,name,x,y,z) VALUES ");
            var queryChunk2 =
                new QueryChunk(
                    "INSERT INTO pets_data (type,race,color,experience,energy,createstamp,nutrition,respect) VALUES ");
            var queryChunk3 = new QueryChunk();
            var list = new List<uint>();
            foreach (var current in GetPets().Where(current => !list.Contains(current.PetId)))
            {
                if (!list.Contains(current.PetId))
                    list.Add(current.PetId);
                switch (current.DbState)
                {
                    case DatabaseUpdateState.NeedsInsert:
                        await queryChunk.AddParameter($"{current.PetId}name", current.Name);
                        await queryChunk2.AddParameter($"{current.PetId}race", current.Race);
                        await queryChunk2.AddParameter($"{current.PetId}color", current.Color);
                        await queryChunk.AddQuery(string.Concat("(", current.PetId, ",", current.OwnerId, ",", current.RoomId,
                            ",@", current.PetId, "name,", current.X, ",", current.Y, ",", current.Z, ")"));
                        await queryChunk2.AddQuery(string.Concat("(", current.Type, ",@", current.PetId, "race,@",
                            current.PetId, "color,0,100,'", current.CreationStamp, "',0,0)"));
                        break;

                    case DatabaseUpdateState.NeedsUpdate:
                        await queryChunk3.AddParameter($"{current.PetId}name", current.Name);
                        await queryChunk3.AddParameter($"{current.PetId}race", current.Race);
                        await queryChunk3.AddParameter($"{current.PetId}color", current.Color);
                        var roomId = (current.RoomId <= 0) ? "NULL" : $"'{current.RoomId}'";

                        await queryChunk3.AddQuery(string.Concat("UPDATE bots SET room_id = ", roomId, ", name = @",
                            current.PetId, "name, x = ", current.X, ", Y = ", current.Y, ", Z = ", current.Z,
                            " WHERE id = ", current.PetId));
                        await queryChunk3.AddQuery(string.Concat("UPDATE pets_data SET race = @", current.PetId,
                            "race, color = @", current.PetId, "color, type = ", current.Type, ", experience = ",
                            current.Experience, ", energy = ", current.Energy, ", nutrition = ", current.Nutrition,
                            ", respect = ", current.Respect, ", createstamp = '", current.CreationStamp,
                            "' WHERE id = ",
                            current.PetId));
                        break;
                }

                current.DbState = DatabaseUpdateState.Updated;
            }

            await queryChunk.Execute(dbClient);
            await queryChunk3.Execute(dbClient);
            queryChunk.Dispose();
            queryChunk3.Dispose();
        }

        /// <summary>
        ///     Gets the pets.
        /// </summary>
        /// <returns>List&lt;Pet&gt;.</returns>
        internal List<Pet> GetPets() =>
            (from current in _pets select current.Value into value where value.IsPet select value.PetData).ToList();

        /// <summary>
        ///     Serializes the status updates.
        /// </summary>
        /// <param name="all">if set to <c>true</c> [all].</param>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializeStatusUpdates(bool all)
        {
            if (UserList.Count <= 0)
            {
                return null;
            }

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer"));
            var i = 0;
            serverMessage.StartArray();
            foreach (var current in UserList.Values)
            {
                if (!all)
                {
                    if (!current.UpdateNeeded)
                        continue;

                    if (current.UpdateNeededCounter > 0)
                    {
                        current.UpdateNeededCounter--;
                        continue;
                    }

                    current.UpdateNeeded = false;
                }

                await current.SerializeStatus(serverMessage);
                serverMessage.SaveArray();
                i++;
            }

            serverMessage.EndArray();
            if (i <= 0)
            {
                serverMessage.Clear();
                return null;
            }


            return serverMessage;
        }


        /// <summary>
        ///     Updates the user status.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cycleGameItems">if set to <c>true</c> [cyclegameitems].</param>
        internal async Task UpdateUserStatus(RoomUser user, bool cycleGameItems, bool removeStatusses = true)
        {
            if (user?.Statusses == null)
                return;

            if (removeStatusses)
            {
                if (user.Statusses.TryRemove("lay", out _) || user.Statusses.TryRemove("sit", out _))
                {
                    user.UpdateNeeded = true;
                }
            }

            var isBot = user.IsBot;
            if (isBot) cycleGameItems = false;

            try
            {
                var roomMap = _room.GetGameMap();
                var userPoint = new Point(user.X, user.Y);
                var itemsOnSquare = roomMap.GetCoordinatedItems(userPoint);

                var newZ = _room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare) +
                           ((user.RidingHorse && user.IsPet == false) ? 1 : 0);

                if (Math.Abs(newZ - user.Z) > 0)
                {
                    user.Z = newZ;
                    user.UpdateNeeded = true;
                }

                foreach (var item in itemsOnSquare)
                {
                    if (item == null) continue;

                    if (!user.IsPet)
                    {
                        UpdateUserEffect(user, item);
                    }

                    if (cycleGameItems)
                    {
                        await item.UserWalksOnFurni(user);
                    }

                    await item.Interactor.OnUserWalk(user.GetClient(), item, user);
                    user.LastItem = item.Id;
                }

                if (user.IsSitting && user.TeleportEnabled)
                {
                    user.Z -= 0.35;
                    user.UpdateNeeded = true;
                }

                if (cycleGameItems)
                {
                    if (_room.GotSoccer())
                        await _room.GetSoccer().OnUserWalk(user);
                    if (_room.GotBanzai())
                        await _room.GetBanzai().OnUserWalk(user);
                    if (_room.GotFreeze())
                        await _room.GetFreeze().OnUserWalk(user);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "RoomUserManager.cs:UpdateUserStatus");
            }
        }

        internal async void UserRoomTimeCycles(RoomUser roomUsers)
        {
            try
            {
                if (roomUsers == null) return;

                if (!roomUsers.IsPet && !roomUsers.IsPet) return;

                await Task.Yield();

                if (!roomUsers.IsOwner() && roomUsers.LastHostingDate + 60 < Oblivion.GetUnixTimeStamp())
                {
                    var roomOwner = (uint)roomUsers.GetRoom().RoomData.OwnerId;
                    var ownerClient = Oblivion.GetGame().GetClientManager().GetClientByUserId(roomOwner);
                    if (ownerClient != null)
                    {
                        await Oblivion.GetGame().GetAchievementManager()
                            .ProgressUserAchievement(ownerClient, "ACH_RoomDecoHosting", 1, true);
                    }

                    roomUsers.LastHostingDate = Oblivion.GetUnixTimeStamp();
                }

                if (_room == null) return;

                if ((!roomUsers.IsAsleep) && (roomUsers.IdleTime >= 600))
                {
                    roomUsers.IsAsleep = true;

                    var sleepEffectMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
                    await sleepEffectMessage.AppendIntegerAsync(roomUsers.VirtualId);
                    sleepEffectMessage.AppendBool(true);
                    await _room.SendMessage(sleepEffectMessage);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "UserRoomTimeCycles");
            }
        }

        internal async Task RoomUserBreedInteraction(RoomUser roomUsers)
        {
            if ((roomUsers.IsPet) && ((roomUsers.PetData.Type == 3) || (roomUsers.PetData.Type == 4)) &&
                (roomUsers.PetData.WaitingForBreading > 0) &&
                ((roomUsers.PetData.BreadingTile.X == roomUsers.X) &&
                 (roomUsers.PetData.BreadingTile.Y == roomUsers.Y)))
            {
                roomUsers.Freezed = true;
                _room.GetGameMap().RemoveUserFromMap(roomUsers, roomUsers.Coordinate);

                switch (roomUsers.PetData.Type)
                {
                    case 3:
                        if (
                            _room.GetRoomItemHandler().BreedingTerrier[roomUsers.PetData.WaitingForBreading]
                                .PetsList.Count == 2)
                        {
                            var petBreedOwner =
                                Oblivion.GetGame().GetClientManager().GetClientByUserId(roomUsers.PetData.OwnerId);

                            petBreedOwner?.SendMessage(PetBreeding.GetMessage(roomUsers.PetData.WaitingForBreading,
                                _room.GetRoomItemHandler().BreedingTerrier[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[0],
                                _room.GetRoomItemHandler().BreedingTerrier[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[1]));
                        }

                        break;

                    case 4:
                        if (
                            _room.GetRoomItemHandler().BreedingBear[roomUsers.PetData.WaitingForBreading].PetsList
                                .Count == 2)
                        {
                            var petBreedOwner =
                                Oblivion.GetGame().GetClientManager().GetClientByUserId(roomUsers.PetData.OwnerId);

                            petBreedOwner?.SendMessage(PetBreeding.GetMessage(roomUsers.PetData.WaitingForBreading,
                                _room.GetRoomItemHandler().BreedingBear[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[0],
                                _room.GetRoomItemHandler().BreedingBear[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[1]));
                        }

                        break;
                }

                await UpdateUserStatus(roomUsers, false);
            }
            else if ((roomUsers.IsPet) && ((roomUsers.PetData.Type == 3) || (roomUsers.PetData.Type == 4)) &&
                     (roomUsers.PetData.WaitingForBreading > 0) &&
                     ((roomUsers.PetData.BreadingTile.X != roomUsers.X) &&
                      (roomUsers.PetData.BreadingTile.Y != roomUsers.Y)))
            {
                roomUsers.Freezed = false;
                roomUsers.PetData.WaitingForBreading = 0;
                roomUsers.PetData.BreadingTile = new Point();
                await UpdateUserStatus(roomUsers, false);
            }
        }

        internal void UserSetPositionData(RoomUser roomUsers, Vector2D nextStep)
        {
            // Check if the User is in a Horse or Not..
            if ((roomUsers.RidingHorse) && (!roomUsers.IsPet))
            {
                var horseRidingPet = GetRoomUserByVirtualId(Convert.ToInt32(roomUsers.HorseId));

                // If exists a Horse and is Ridding.. Let's Create data for they..
                if (horseRidingPet != null)
                {
                    roomUsers.RotBody = horseRidingPet.RotBody;
                    roomUsers.RotHead = horseRidingPet.RotHead;
                    roomUsers.SetStep = true;

                    roomUsers.SetX = nextStep.X;
                    roomUsers.SetY = nextStep.Y;
                    roomUsers.SetZ = (_room.GetGameMap().SqAbsoluteHeight(nextStep.X, nextStep.Y) + 1);

                    horseRidingPet.SetX = roomUsers.SetX;
                    horseRidingPet.SetY = roomUsers.SetY;
                    horseRidingPet.SetZ = _room.GetGameMap().SqAbsoluteHeight(roomUsers.SetX, roomUsers.SetY);
                }
            }
            else
            {
                // Is a Normal User, Let's Create Data for They.
                roomUsers.RotBody = Rotation.Calculate(roomUsers.X, roomUsers.Y, nextStep.X, nextStep.Y,
                    roomUsers.IsMoonwalking);
                roomUsers.RotHead = Rotation.Calculate(roomUsers.X, roomUsers.Y, nextStep.X, nextStep.Y,
                    roomUsers.IsMoonwalking);
                roomUsers.SetStep = true;

                roomUsers.SetX = nextStep.X;
                roomUsers.SetY = nextStep.Y;
                roomUsers.SetZ = _room.GetGameMap().SqAbsoluteHeight(nextStep.X, nextStep.Y);
            }
        }

        internal void CheckUserSittableLayable(RoomUser roomUsers)
        {
            if (roomUsers == null) return;
            // Check if User Is ina  Special Action..

            // User is Laying Down..
            if (roomUsers.Statusses.TryRemove("lay", out _) || roomUsers.IsLyingDown)
            {
                roomUsers.IsLyingDown = false;
                roomUsers.UpdateNeeded = true;
            }

            // User is Sitting Down..
            if ((roomUsers.Statusses.TryRemove("sit", out _) || roomUsers.IsSitting) && (!roomUsers.RidingHorse))
            {
                roomUsers.IsSitting = false;
                roomUsers.UpdateNeeded = true;
            }
        }

        internal async Task<bool> UserGoToTile(RoomUser user, bool invalidStep)
        {
            var roomUser = user;

            if (roomUser?.Path == null) return false;
            if ((invalidStep || (roomUser.PathStep >= roomUser.Path.Count) ||
                 ((roomUser.GoalX == roomUser.X) && (roomUser.GoalY == roomUser.Y))))
            {
                // Erase all Movement Data..
                roomUser.IsWalking = false;
                roomUser.ClearMovement();
                roomUser.HandelingBallStatus = 0;
                await RoomUserBreedInteraction(roomUser);

                // Check if he is in a Horse, and if if Erase Horse and User Movement Data
                if ((roomUser.RidingHorse) && (!roomUser.IsPet))
                {
                    var horseStopWalkRidingPet = GetRoomUserByVirtualId(Convert.ToInt32(roomUser.HorseId));

                    if (horseStopWalkRidingPet != null)
                    {
                        using (var horseStopWalkRidingPetMessage =
                               new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer")))
                        {
                            await horseStopWalkRidingPetMessage.AppendIntegerAsync(1);
                            await horseStopWalkRidingPet.SerializeStatus(horseStopWalkRidingPetMessage, "");
                            await _room.SendMessage(horseStopWalkRidingPetMessage);

                            horseStopWalkRidingPet.IsWalking = false;
                            horseStopWalkRidingPet.ClearMovement();
                        }
                    }
                }

                // Finally Update User Status
                await UpdateUserStatus(roomUser, false);
            }
            else
            {
                // Region Set Variables
                var pathDataCount = ((roomUser.Path.Count - roomUser.PathStep) - 1);
                if (roomUser.Path.Count <= pathDataCount || pathDataCount < 0)
                    return false;
                var nextStep = roomUser.Path[pathDataCount];


                // Increase Step Data...
                roomUser.PathStep++;
                // Ins't a Invalid Step.. Continuing.

                if (roomUser.FastWalking)
                {
                    pathDataCount = (roomUser.Path.Count - roomUser.PathStep) - 1;
                    if (roomUser.Path.Count <= pathDataCount || pathDataCount < 0)
                        return false;
                    nextStep = roomUser.Path[pathDataCount];
                    roomUser.PathStep++;
                }

                roomUser.RemoveStatus("mv");


                // Check Against if is a Valid Step...
                if (_room?.GetGameMap() == null) return false;
                if (await _room.GetGameMap()
                        .IsValidStep(roomUser, new Vector2D(roomUser.X, roomUser.Y),
                            new Vector2D(nextStep.X, nextStep.Y),
                            ((roomUser.GoalX == nextStep.X) && (roomUser.GoalY == nextStep.Y)), roomUser.AllowOverride,
                            false, false))
                {
                    // If is a PET Must Give the Time Tick In Syncrony with User..
                    if ((roomUser.RidingHorse) && (!roomUser.IsPet))
                    {
                        var horsePetAi = GetRoomUserByVirtualId(Convert.ToInt32(roomUser.HorseId));
                        try
                        {
                            await horsePetAi.BotAi.OnTimerTick();
                        }
                        catch (Exception e)
                        {
                            Logging.HandleException(e, "RoomUserManager - horsePetAi.OnTimerTick");
                        }
                    }

                    // Horse Ridding need be Updated First
                    if (roomUser.RidingHorse)
                    {
                        // Set User Position Data
                        UserSetPositionData(roomUser, nextStep);
                        CheckUserSittableLayable(roomUser);

                        // Add Status of Walking
                        roomUser.AddStatus("mv",
                            +roomUser.SetX + "," + roomUser.SetY + "," + TextHandling.GetString(roomUser.SetZ));
                    }

                    // Check if User is Ridding in Horse, if if Let's Update Ride Data.
                    if ((roomUser.RidingHorse) && (!roomUser.IsPet))
                    {
                        var horseRidingPet = GetRoomUserByVirtualId(Convert.ToInt32(roomUser.HorseId));

                        if (horseRidingPet != null)
                        {
                            var theUser = "mv " + roomUser.SetX + "," + roomUser.SetY + "," +
                                          TextHandling.GetString(roomUser.SetZ);
                            var thePet = "mv " + roomUser.SetX + "," + roomUser.SetY + "," +
                                         TextHandling.GetString(horseRidingPet.SetZ);

                            var horseRidingPetMessage =
                                new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer"));
                            await horseRidingPetMessage.AppendIntegerAsync(2);
                            await roomUser.SerializeStatus(horseRidingPetMessage, theUser);
                            await horseRidingPet.SerializeStatus(horseRidingPetMessage, thePet);
                            await _room.SendMessageAsync(horseRidingPetMessage);

                            horseRidingPet.RotBody = roomUser.RotBody;
                            horseRidingPet.RotHead = roomUser.RotBody;
                            horseRidingPet.SetX = roomUser.SetX;
                            horseRidingPet.SetY = roomUser.SetY;
                            horseRidingPet.SetZ = (roomUser.SetZ - 1);
                            horseRidingPet.SetStep = true;

                            //                            UpdateUserEffect(horseRidingPet, horseRidingPet.SetX, horseRidingPet.SetY);
                            await UpdateUserStatus(horseRidingPet, false);
                        }
                    }

                    // If is not Ridding Horse doesn't Need Update Effect
                    if (!roomUser.RidingHorse)
                    {
                        // Set User Position Data
                        UserSetPositionData(roomUser, nextStep);
                        CheckUserSittableLayable(roomUser);

                        // Add Status of Walking
                        roomUser.AddStatus("mv",
                            +roomUser.SetX + "," + roomUser.SetY + "," + TextHandling.GetString(roomUser.SetZ));
                    }

                    // Region Update User Effect And Status
                    //                    UpdateUserEffect(roomUsers, roomUsers.SetX, roomUsers.SetY);

                    // Update Effect if is Ridding
                    if (roomUser.RidingHorse)
                        await UpdateUserStatus(roomUser, false);


                    // If user is in soccer proccess.
                    if (_room.GotSoccer())
                        await _room.GetSoccer().OnUserWalk(roomUser);


                    return true;
                }

                if (roomUser.PathRecalcNeeded && !roomUser.SetStep)
                    return false;
            }

            return true;
        }

        internal async Task<bool> UserCanWalkInTile(RoomUser roomUsers)
        {
            try
            {
                var hasGroup = false;
                if (!roomUsers.IsBot && !roomUsers.IsPet)
                    if (_room.GetGameMap().GuildGates
                        .TryGetValue(new Point(roomUsers.SetX, roomUsers.SetY), out var guild))
                    {
                        var guildId = guild.GroupId;
                        if (guildId > 0 &&
                            roomUsers.GetClient()
                                .GetHabbo()
                                .UserGroups.Any(member => member != null && member.GroupId == guildId))
                            hasGroup = true;
                    }

                // Check if User CanWalk...
                if ((_room.GetGameMap().SquareIsOpen(roomUsers.SetX, roomUsers.SetY, roomUsers.AllowOverride)) ||
                    (roomUsers.RidingHorse) || hasGroup)
                {
                    // Let's Update his Movement...
                    _room.GetGameMap()
                        .UpdateUserMovement(new Point(roomUsers.Coordinate.X, roomUsers.Coordinate.Y),
                            new Point(roomUsers.SetX, roomUsers.SetY), roomUsers);

                    var hasItemInPlace =
                        _room.GetGameMap().GetCoordinatedItems(new Point(roomUsers.X, roomUsers.Y));

                    // Set His Actual X,Y,Z Position...
                    roomUsers.X = roomUsers.SetX;
                    roomUsers.Y = roomUsers.SetY;
                    roomUsers.Z = roomUsers.SetZ;

                    // Check Sub Items Interactionables
                    foreach (var roomItem in hasItemInPlace)
                    {
                        await roomItem.UserWalksOffFurni(roomUsers);
                    }

                    // Let's Update user Status..
                    await UpdateUserStatus(roomUsers, true, false);
                    return false;
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "UserCanWalkInTile");
            }

            return !_room.RoomData.AllowWalkThrough;
        }


        /// <summary>
        ///     Turns the user thread
        /// </summary>
        /// <param name="roomUsers"></param>
        internal async Task UserCycleOnRoom(RoomUser roomUsers)
        {
            try
            {
                if (roomUsers?.Statusses == null) return;

                if (!IsValid(roomUsers))
                {
                    if (roomUsers.GetClient() != null)
                        await RemoveUserFromRoom(roomUsers, false, false);
                    else
                        await RemoveRoomUser(roomUsers);
                    return;
                }


                // Region Check User Remove Unlocking
                if (roomUsers.NeedsAutokick && !_removeUsers.Contains(roomUsers))
                {
                    _removeUsers.Add(roomUsers);
                    return;
                }

                // Region Idle and Room Tiem Check
                roomUsers.IdleTime++;

                // Region User Achievement of Room
                UserRoomTimeCycles(roomUsers);

                // Carry Item Hand Checking
                if (roomUsers.CarryItemId > 0)
                {
                    roomUsers.CarryTimer--;

                    // If The Carry Timer is 0.. Remove CarryItem.
                    if (roomUsers.CarryTimer <= 0)
                        await roomUsers.CarryItem(0);
                }


                if (roomUsers.Frozen)
                {
                    roomUsers.FrozenTick--;
                    if (roomUsers.FrozenTick <= 0)
                    {
                        roomUsers.Frozen = false;
                        await roomUsers.GetClient().SendWhisperAsync("Voc� foi descongelado!!");
                    }
                }


                // Region Check User Got Freezed
                if (_room.GotFreeze())
                {
                    await _room.GetFreeze().CycleUser(roomUsers);
                }

                // Region Variable Registering
                var invalidStep = false;
                // Region Check User Tile Selection
                if (roomUsers.SetStep)
                {
                    // Check if User is Going to the Door.

                    if ((roomUsers.SetX == _room.GetGameMap().Model.DoorX) &&
                        (roomUsers.SetY == _room.GetGameMap().Model.DoorY) &&
                        (!_removeUsers.Contains(roomUsers)) &&
                        (!roomUsers.IsBot) && (!roomUsers.IsPet))
                    {
                        _removeUsers.Add(roomUsers);
                        return;
                    }

                    _room.GetGameMap().GameMap[roomUsers.X, roomUsers.Y] = roomUsers.SqState;
                    roomUsers.SqState = _room.GetGameMap().GameMap[roomUsers.SetX, roomUsers.SetY];


                    // Check Elegibility of Walk In Tile
                    invalidStep = await UserCanWalkInTile(roomUsers);

                    // User isn't Anymore Set a Tile to Walk
                    roomUsers.SetStep = false;
                }

                // Pet Must Stop Too!
                if (((roomUsers.GoalX == roomUsers.X) && (roomUsers.GoalY == roomUsers.Y)) &&
                    (roomUsers.RidingHorse) &&
                    (!roomUsers.IsPet))
                {
                    var horseStopWalkRidingPet = GetRoomUserByVirtualId(Convert.ToInt32(roomUsers.HorseId));

                    if (horseStopWalkRidingPet != null)
                    {
                        var horseStopWalkRidingPetMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer"));
                        await horseStopWalkRidingPetMessage.AppendIntegerAsync(1);
                        await horseStopWalkRidingPet.SerializeStatus(horseStopWalkRidingPetMessage, "");
                        await _room.SendMessage(horseStopWalkRidingPetMessage);

                        horseStopWalkRidingPet.IsWalking = false;
                        horseStopWalkRidingPet.ClearMovement();
                    }
                }

                // User Reached Goal Need Stop.
                if (((roomUsers.GoalX == roomUsers.X) && (roomUsers.GoalY == roomUsers.Y)) || (roomUsers.Freezed))
                {
                    roomUsers.IsWalking = false;
                    roomUsers.ClearMovement();
                    roomUsers.SetStep = false;
                    await UpdateUserStatus(roomUsers, false);
                }
                else
                {
                    while (roomUsers.PathRecalcNeeded || roomUsers.IsWalking || roomUsers.SetStep)
                    {
                        if (roomUsers.Statusses == null) break;

                        await GenerateNewPath(roomUsers);

                        // If user Isn't Walking, Let's go Back..
                        if ((!roomUsers.IsWalking || roomUsers.Freezed))
                        {
                            if (roomUsers.Statusses.TryRemove("mv", out _))
                            {
                                roomUsers.UpdateNeeded = true;
                            }
                        }
                        else
                        {
                            // If he Want's to Walk.. Let's Continue!..


                            // Let's go to The Tile! And Walk :D
                            if (await UserGoToTile(roomUsers, invalidStep))
                            {
                                // If User isn't Riding, Must Update Statusses...
                                if (!roomUsers.RidingHorse)
                                    roomUsers.UpdateNeeded = true;
                            }
                            else
                            {
                                if (roomUsers.PathRecalcNeeded && !roomUsers.SetStep)
                                    continue;
                            }
                        }

                        break;
                    }
                }

                // If is a Bot.. Let's Tick the Time Count of Bot..
                if (roomUsers.IsBot)
                {
                    try
                    {
                        if (_room.GotWireds())
                        {
                            if (roomUsers.FollowingOwner != null)
                            {
                                await roomUsers.MoveTo(_room.GetGameMap()
                                    .SquareIsOpen(roomUsers.FollowingOwner.SquareInFront.X,
                                        roomUsers.FollowingOwner.SquareInFront.Y, false)
                                    ? roomUsers.FollowingOwner.SquareInFront
                                    : roomUsers.FollowingOwner.SquareBehind);
                            }

                            var users = _room.GetGameMap()
                                .GetRoomUsers(roomUsers.SquareInFront);

                            if (users != null && users.Count > 0)
                            {
                                var user = users[0];

                                await _room.GetWiredHandler().ExecuteWired(Interaction.TriggerBotReachedAvatar, user);
                            }
                        }

                        await roomUsers.BotAi.OnTimerTick();
                    }
                    catch (Exception e)
                    {
                        Logging.HandleException(e, "RoomUsers - BotAi - OnTimerTick");
                    }
                }

                //                UpdateUserEffect(roomUsers, roomUsers.X, roomUsers.Y);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "RoomMgr - Cycle");
            }
        }


        public async Task GenerateNewPath(RoomUser User)
        {
            if (User.PathRecalcNeeded || User.IsWalking)
            {
                User.Path.Clear();
                User.Path = await PathFinder.FindPath(User, _room.GetGameMap().DiagonalEnabled,
                    _room.GetGameMap(), new Vector2D(User.X, User.Y),
                    new Vector2D(User.GoalX, User.GoalY));

                if (User.Path.Count > 1)
                {
                    User.PathStep = 1;
                    User.IsWalking = true;
                    User.PathRecalcNeeded = false;
                }
                else
                {
                    User.PathRecalcNeeded = false;
                    User.Path.Clear();
                }
            }
        }


        /// <summary>
        ///     Called when [cycle].
        /// </summary>
        /// <param name="idleCount">The idle count.</param>
        internal async Task<int> OnCycle(int idleCount)
        {
            // User in Room Count for foreach
            uint userInRoomCount = 0;

            // Clear RemoveUser's List.
            _removeUsers.Clear();

            try
            {
                // Check Disco Procedure...
                if ((_room != null) && (_room.DiscoMode) && (_room.TonerData != null) &&
                    (_room.TonerData.Enabled == 1))
                {
                    var tonerItem = _room.GetRoomItemHandler().GetItem(_room.TonerData.ItemId);

                    if (tonerItem != null)
                    {
                        _room.TonerData.Data1 = Oblivion.GetRandomNumber(0, 255);
                        _room.TonerData.Data2 = Oblivion.GetRandomNumber(0, 255);
                        _room.TonerData.Data3 = Oblivion.GetRandomNumber(0, 255);
                        using (var tonerComposingMessage =
                               new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer")))
                        {
                            tonerItem.Serialize(tonerComposingMessage);
                            await _room.SendMessage(tonerComposingMessage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Writer.Writer.LogException("Disco mode: " + e);
            }

            // Region: Main User Procedure... Really Main..
            foreach (var roomUsers in UserList.Values)
            {
                // User Main OnCycle
                await UserCycleOnRoom(roomUsers);

                // If is a Valid user, We must increase the User Count..
                if ((!roomUsers.IsPet) && (!roomUsers.IsBot))
                {
                    userInRoomCount++;
                }
            }

            // Region: Check Removable Users and Users in Room Count

            // Check Users to be Removed from Room
            foreach (var userToRemove in _removeUsers)
            {
                var userRemovableClient = Oblivion.GetGame().GetClientManager()
                    .GetClientByUserId(userToRemove.HabboId);

                // Remove User from Room..
                if (userRemovableClient != null)
                    await RemoveUserFromRoom(userToRemove, true, false);
                else
                    await RemoveRoomUser(userToRemove);
            }

            if (userInRoomCount == 0)
            {
                idleCount++;
            }

            if (userInRoomCount >= 5 && Oblivion.Multipy > 1)
            {
                if (_roomUserCount != userInRoomCount * (uint)Oblivion.Multipy)
                {
                    UpdateUserCount(userInRoomCount * (uint)Oblivion.Multipy);
                }
            }
            else
            {
                if (_roomUserCount != userInRoomCount)
                {
                    UpdateUserCount(userInRoomCount);
                }
            }

            return idleCount;
        }

        /// <summary>
        ///     Updates the user effect.
        /// </summary>
        private static async void UpdateUserEffect(RoomUser user, RoomItem roomItem)
        {
            var baseItem = roomItem?.GetBaseItem();
            if (baseItem == null) return;
            var inv = user?.GetClient()?.GetHabbo()?.GetAvatarEffectsInventoryComponent();

            if (inv == null) return;

            var b = user.GetClient().GetHabbo().Gender == "m" ? baseItem.EffectM : baseItem.EffectF;
            if (b > 0)
            {
                if (inv.CurrentEffect == 0)
                {
                    user.CurrentItemEffect = 0;
                }

                if (b == user.CurrentItemEffect)
                    return;

                await inv.ActivateCustomEffect(b);
                user.CurrentItemEffect = b;
            }
            else
            {
                if (user.CurrentItemEffect == 0 || b != 0)
                    return;
                await inv.ActivateCustomEffect(-1);
                user.CurrentItemEffect = 0;
            }
        }

        /// <summary>
        ///     Handles the <see cref="E:UserAdd" /> event.
        /// </summary>
        /// <param name="user"></param>
        private async Task OnUserAdd(RoomUser user)
        {
            try
            {
                var client = user?.GetClient();

                if (client?.GetHabbo() == null || _room?.RoomData == null)
                    return;
                if (client.IsAir)
                {
                    var msg = new ServerMessage();
                    await _room.RoomData.SerializeRoomData(msg, client, true);
                    await client.SendMessageAsync(msg);
                }

                if (!user.IsSpectator)
                {
                    var model = _room.GetGameMap().Model;
                    if (model == null) return;
                    user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                    user.SetRot(model.DoorOrientation, false);

                    var session = user.GetClient();
                    if (_room.CheckRights(session, true))
                    {
                        var msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        await msg.AppendIntegerAsync(session.GetHabbo().Rank >= 5 ? 5 : 4);
                        await session.SendMessage(msg);
                        msg = new ServerMessage(LibraryParser.OutgoingRequest("HasOwnerRightsMessageComposer"));
                        await session.SendMessage(msg);
                        if (session.GetHabbo().Rank >= 5)
                        {
                            user.AddStatus("flatctrl 5", string.Empty);
                        }
                        else
                        {
                            user.AddStatus("flatctrl 4", string.Empty);
                        }
                    }
                    else if (_room.CheckRights(session, false, true))
                    {
                        var msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        await msg.AppendIntegerAsync(1);
                        await session.SendMessage(msg);
                        user.AddStatus("flatctrl 3", string.Empty);
                    }
                    else
                    {
                        var msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        await msg.AppendIntegerAsync(0);
                        await session.SendMessage(msg);
                        msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("YouAreNotControllerMessageComposer"));
                        await session.SendMessage(msg);
                    }

                    user.CurrentItemEffect = 0;

                    if (!user.IsBot && client.GetHabbo().IsTeleporting)
                    {
                        client.GetHabbo().IsTeleporting = false;
                        client.GetHabbo().TeleportingRoomId = 0;

                        var item = _room.GetRoomItemHandler().GetItem(client.GetHabbo().TeleporterId);

                        if (item != null)
                        {
                            item.ExtraData = "2";
                            await item.UpdateState(false, true);
                            user.SetPos(item.X, item.Y, item.Z);
                            user.SetRot(item.Rot, false);
                            item.InteractingUser2 = client.GetHabbo().Id;
                            item.ExtraData = "0";
                            await item.UpdateState(false, true);
                        }
                    }

                    if (!user.IsBot && client.GetHabbo().IsHopping)
                    {
                        client.GetHabbo().IsHopping = false;
                        client.GetHabbo().HopperId = "0u";

                        var item2 = _room.GetRoomItemHandler().GetItem(client.GetHabbo().HopperId);

                        if (item2 != null)
                        {
                            item2.ExtraData = "1";
                            await item2.UpdateState(false, true);
                            user.SetPos(item2.X, item2.Y, item2.Z);
                            user.SetRot(item2.Rot, false);
                            user.AllowOverride = false;
                            item2.InteractingUser2 = client.GetHabbo().Id;
                            item2.ExtraData = "2";
                            await item2.UpdateState(false, true);
                        }
                    }

                    if (!user.IsSpectator)
                    {
                        using (var serverMessage =
                               new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer")))
                        {
                            await serverMessage.AppendIntegerAsync(1);
                            if (!user.Serialize(serverMessage)) return;
                            await _room.SendMessage(serverMessage);
                        }
                    }

                    if (!user.IsBot)
                    {
                        using (var serverMessage2 = new ServerMessage())
                        {
                            await serverMessage2.InitAsync(
                                LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            await serverMessage2.AppendIntegerAsync(user.VirtualId);
                            await serverMessage2.AppendStringAsync(client.GetHabbo().Look);
                            await serverMessage2.AppendStringAsync(client.GetHabbo().Gender.ToLower());
                            await serverMessage2.AppendStringAsync(client.GetHabbo().Motto);
                            await serverMessage2.AppendIntegerAsync(client.GetHabbo().AchievementPoints);
                            await _room.SendMessage(serverMessage2);
                        }
                    }

                    if (_room.RoomData.Owner != client.GetHabbo().UserName)
                    {
                        await Oblivion.GetGame()
                            .GetQuestManager()
                            .ProgressUserQuest(client, QuestType.SocialVisit);
                        await Oblivion.GetGame()
                            .GetAchievementManager()
                            .ProgressUserAchievement(client, "ACH_RoomEntry", 1);
                    }
                }

                if (client.GetHabbo().GetMessenger() != null)
                    await client.GetHabbo().GetMessenger().OnStatusChanged(true);
                client.GetMessageHandler()?.OnRoomUserAdd();

                //                    if (client.GetHabbo().HasFuse("fuse_mod"))
                //                        client.GetHabbo().GetAvatarEffectsInventoryComponent()?.ActivateCustomEffect(102);
                if (client.GetHabbo().Rank == Convert.ToUInt32(Oblivion.GetDbConfig().DbData["ambassador.minrank"]))
                    client.GetHabbo().GetAvatarEffectsInventoryComponent()?.ActivateCustomEffect(178);

                if (_room.GotMusicController())
                    await _room.GetRoomMusicController().OnNewUserEnter(user);
                await _room.OnUserEnter(user);
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            try
            {
                foreach (var user in UserList.Values)
                {
                    if (user == null) continue;
                    var userClient = user.GetClient()?.GetHabbo();
                    if (userClient != null && userClient.CurrentRoomId == _room.RoomId)
                    {
                        userClient.CurrentRoomId = 0u;
                        userClient.CurrentRoom = null;
                        userClient.GetMessenger().DisposeRoom(_room.RoomId);
                    }

                    user.Dispose();
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "dipose roomusermanager");
            }

            _room = null;
            UsersByUserName.Clear();
            UsersByUserName = null;
            UsersByUserId.Clear();
            UsersByUserId = null;
            _pets.Clear();
            Bots.Clear();
            _pets = null;
            Bots = null;
            UserList = null;
        }


        /// <summary>
        ///     Handles the <see cref="E:Remove" /> event.
        /// </summary>
        /// <param name="user"></param>
        private async void OnRemove(RoomUser user)
        {
            try
            {
                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));

                if (user?.GetClient() == null) return;
                var client = user.GetClient();
                var list = Bots.Values;

                foreach (var bot in list)
                {
                    bot.BotAi.OnUserLeaveRoom(client);
                    if (bot.IsPet && bot.PetData.OwnerId == user.UserId &&
                        !_room.CheckRights(client, true))
                    {
                        if (user.GetClient()?.GetHabbo()?.GetInventoryComponent() != null)
                        {
                            user.GetClient().GetHabbo().GetInventoryComponent().AddPet(bot.PetData);
                            await RemoveBot(bot.VirtualId, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
        }

        /// <summary>
        ///     Called when [user update status].
        /// </summary>
        public async Task OnUserUpdateStatus()
        {
            foreach (var current in UserList.Values)
                await UpdateUserStatus(current, false);
        }

        /// <summary>
        ///     Called when [user update status].
        ///     <param name="x">x position</param>
        ///     <param name="y">y position</param>
        /// </summary>
        public async Task OnUserUpdateStatus(int x, int y)
        {
            foreach (var current in _room.GetGameMap().GetRoomUsers(new Point(x, y)))
                await UpdateUserStatus(current, false);
        }

        /// <summary>
        ///     Determines whether the specified user is valid.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the specified user is valid; otherwise, <c>false</c>.</returns>
        private bool IsValid(RoomUser user) => user != null && (user.IsBot ||
                                                                (user.GetClient() != null &&
                                                                 user.GetClient().GetHabbo() != null &&
                                                                 user.GetClient().GetHabbo().CurrentRoomId ==
                                                                 _room.RoomId));
    }
}