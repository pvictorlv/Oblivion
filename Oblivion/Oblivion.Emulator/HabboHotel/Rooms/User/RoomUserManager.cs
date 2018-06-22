using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.Configuration;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
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
        private Room _userRoom;

        /// <summary>
        ///     The users by user identifier
        /// </summary>
        internal Dictionary<uint,RoomUser> UsersByUserId;

        /// <summary>
        ///     The users by user name
        /// </summary>
        internal Dictionary<string, RoomUser> UsersByUserName;


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
            _userRoom = room;
            UserList = new ConcurrentDictionary<int, RoomUser>();
            _pets = new Dictionary<uint, RoomUser>();
            Bots = new Dictionary<uint, RoomUser>();
            UsersByUserName = new Dictionary<string, RoomUser>();
            UsersByUserId = new Dictionary<uint, RoomUser>();
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
        internal RoomUser DeployBot(RoomBot bot, Pet petData)
        {
            var virtualId = _primaryPrivateUserId++;
            var roomUser = new RoomUser(0u, _userRoom.RoomId, virtualId, _userRoom, false);
            var num = _secondaryPrivateUserId++;
            roomUser.InternalRoomId = num;
            UserList.TryAdd(num, roomUser);
            OnUserAdd(roomUser);

            var model = _userRoom.GetGameMap().Model;
            var coord = new Point(bot.X, bot.Y);
            if ((bot.X > 0) && (bot.Y >= 0) && (bot.X < model.MapSizeX) && (bot.Y < model.MapSizeY))
            {
                _userRoom.GetGameMap().AddUserToMap(roomUser, coord);
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
                roomUser.BotAi.Init(bot.BotId, roomUser.VirtualId, _userRoom.RoomId, roomUser, _userRoom);
                roomUser.PetData = petData;
                roomUser.PetData.VirtualId = roomUser.VirtualId;
            }
            else
            {
                roomUser.BotAi.Init(bot.BotId, roomUser.VirtualId, _userRoom.RoomId, roomUser, _userRoom);
            }

            UpdateUserStatus(roomUser, false);
            roomUser.UpdateNeeded = true;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
            serverMessage.AppendInteger(1);
            roomUser.Serialize(serverMessage);
            _userRoom.SendMessage(serverMessage);
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

            serverMessage.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
            serverMessage.AppendInteger(roomUser.VirtualId);
            serverMessage.AppendInteger(roomUser.BotData.DanceId);
            _userRoom.SendMessage(serverMessage);
            PetCount++;

            return roomUser;
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
        internal void UpdateBot(int virtualId, RoomUser roomUser, string name, string motto, string look, string gender,
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
        internal void RemoveBot(int virtualId, bool kicked)
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
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer"));
            serverMessage.AppendString(roomUserByVirtualId.VirtualId.ToString());
            _userRoom.SendMessage(serverMessage);

            UserList.TryRemove(roomUserByVirtualId.InternalRoomId, out _);
        }

        /// <summary>
        ///     Gets the user for square.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetUserForSquare(int x, int y)
        {
            var users = _userRoom.GetGameMap().GetRoomUsers(new Point(x, y));
            if (users.Count > 0)
            {
                return users[0];
            }

            return null;
        }


        /// <summary>
        ///     Adds the user to room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="spectator">if set to <c>true</c> [spectator].</param>
        /// <param name="snow">if set to <c>true</c> [snow].</param>
        internal void AddUserToRoom(GameClient session, bool spectator, bool snow = false)
        {
            if (Disposed) return;
            var habbo = session?.GetHabbo();

            if (habbo == null || _userRoom == null)
                return;
            var roomUser = new RoomUser(habbo.Id, _userRoom.RoomId, _primaryPrivateUserId++, _userRoom,
                spectator)
            { UserId = habbo.Id };


            var userName = habbo.UserName;
            var userId = roomUser.UserId;
            UsersByUserName[userName.ToLower()] = roomUser;
            UsersByUserId[userId] = roomUser;

            var num = _secondaryPrivateUserId++;
            roomUser.InternalRoomId = num;
            session.CurrentRoomUserId = num;
            habbo.CurrentRoomId = _userRoom.RoomId;
            habbo.CurrentRoom = _userRoom;
            UserList[num] = roomUser;
            OnUserAdd(roomUser);

            habbo.LoadingRoom = 0;

            if (Oblivion.GetGame().GetNavigator().PrivateCategories.Contains(_userRoom.RoomData.Category))
                ((FlatCat)Oblivion.GetGame().GetNavigator().PrivateCategories[_userRoom.RoomData.Category]).UsersNow++;
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

            if (!UsersByUserName.TryGetValue(oldName, out var user))
                return;
            UsersByUserName.Add(newName, user);
            UsersByUserName.Remove(oldName);
            //
            Oblivion.GetGame().GetClientManager().UpdateClient(oldName, newName);
        }

        /// <summary>
        ///     Removes the user from room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="notifyClient">if set to <c>true</c> [notify client].</param>
        /// <param name="notifyKick">if set to <c>true</c> [notify kick].</param>
        internal void RemoveUserFromRoom(GameClient session, bool notifyClient, bool notifyKick)
        {
            if (session?.GetHabbo()?.UserName != null)
            {
                var roomUser = GetRoomUserByHabbo(session.GetHabbo().UserName);
                if (roomUser == null) return;
                RemoveUserFromRoom(roomUser, notifyClient, notifyKick);
            }
        }

        /// <summary>
        ///     Removes the user from room.
        /// </summary>
        /// <param name="user">The session.</param>
        /// <param name="notifyClient">if set to <c>true</c> [notify client].</param>
        /// <param name="notifyKick">if set to <c>true</c> [notify kick].</param>
        internal void RemoveUserFromRoom(RoomUser user, bool notifyClient, bool notifyKick)
        {
            try
            {
                var client = user.GetClient();
                var habbo = user.GetClient().GetHabbo();
                if (client == null || habbo == null) return;
                var userId = habbo.Id;
                habbo.CurrentRoom = null;
                habbo.GetAvatarEffectsInventoryComponent()?.OnRoomExit();
                var room = _userRoom;

                if (notifyKick)
                {
                    var model = room.GetGameMap().Model;
                    if (model == null) return;

                    user.MoveTo(model.DoorX, model.DoorY);
                    user.CanWalk = false;
                    client.GetMessageHandler()
                        .GetResponse()
                        .Init(LibraryParser.OutgoingRequest("RoomErrorMessageComposer"));
                    client.GetMessageHandler().GetResponse().AppendInteger(4008);
                    client.GetMessageHandler().SendResponse();

                    client.GetMessageHandler()
                        .GetResponse()
                        .Init(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                    client.GetMessageHandler().GetResponse().AppendShort(2);
                    client.GetMessageHandler().SendResponse();
                }
                else if (notifyClient)
                {
                    var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("UserIsPlayingFreezeMessageComposer"));
                    serverMessage.AppendBool(user.Team != Team.None);
                    client.SendMessage(serverMessage);
                    client.GetMessageHandler()
                        .GetResponse()
                        .Init(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                    client.GetMessageHandler().GetResponse().AppendShort(2);
                    client.GetMessageHandler().SendResponse();
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

                RemoveRoomUser(user);
                if (!user.IsSpectator)
                {
                    if (user.CurrentItemEffect != 0)
                        user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect =
                            -1;
                    if (room.HasActiveTrade(habbo.Id))
                        room.TryStopTrade(habbo.Id);
                    habbo.CurrentRoomId = 0;
                    if (habbo.GetMessenger() != null)
                        habbo.GetMessenger().OnStatusChanged(true);


                    using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryreactor2.RunFastQuery(string.Concat(
                            "UPDATE users_rooms_visits SET exit_timestamp = '", Oblivion.GetUnixTimeStamp(),
                            "' WHERE room_id = '", room.RoomId, "' AND user_id = '", userId,
                            "' ORDER BY exit_timestamp DESC LIMIT 1"));
                    UsersByUserName?.Remove(habbo.UserName.ToLower());
                }

                UsersByUserId.Remove(user.UserId);
                user.Dispose();
            }
            catch (Exception ex)
            {
                RemoveRoomUser(user);

                Logging.LogCriticalException($"Error during removing user from room:{ex}");
            }
        }

        /// <summary>
        ///     Removes the room user.
        /// </summary>
        /// <param name="user">The user.</param>
        internal void RemoveRoomUser(RoomUser user)
        {
            if (user == null) return;

            if (!UserList.TryRemove(user.InternalRoomId, out _)) return;

            user.InternalRoomId = -1;
            _userRoom.GetGameMap().GameMap[user.X, user.Y] = user.SqState;
            _userRoom.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer"));
            serverMessage.AppendString(user.VirtualId.ToString());
            _userRoom.SendMessage(serverMessage);

            OnRemove(user);
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
            if (_userRoom?.RoomData == null)
                return;

            _userRoom.RoomData.UsersNow = count;

            Oblivion.GetGame().GetRoomManager().QueueActiveRoomUpdate(_userRoom.RoomData);
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
        internal void SavePets(IQueryAdapter dbClient)
        {
            try
            {
                if (GetPets().Any())
                    AppendPetsUpdateString(dbClient);
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(string.Concat("Error during saving furniture for room ", _userRoom.RoomId,
                    ". Stack: ", ex.ToString()));
            }
        }

        /// <summary>
        ///     Appends the pets update string.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void AppendPetsUpdateString(IQueryAdapter dbClient)
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
                        queryChunk.AddParameter($"{current.PetId}name", current.Name);
                        queryChunk2.AddParameter($"{current.PetId}race", current.Race);
                        queryChunk2.AddParameter($"{current.PetId}color", current.Color);
                        queryChunk.AddQuery(string.Concat("(", current.PetId, ",", current.OwnerId, ",", current.RoomId,
                            ",@", current.PetId, "name,", current.X, ",", current.Y, ",", current.Z, ")"));
                        queryChunk2.AddQuery(string.Concat("(", current.Type, ",@", current.PetId, "race,@",
                            current.PetId, "color,0,100,'", current.CreationStamp, "',0,0)"));
                        break;

                    case DatabaseUpdateState.NeedsUpdate:
                        queryChunk3.AddParameter($"{current.PetId}name", current.Name);
                        queryChunk3.AddParameter($"{current.PetId}race", current.Race);
                        queryChunk3.AddParameter($"{current.PetId}color", current.Color);
                        queryChunk3.AddQuery(string.Concat("UPDATE bots SET room_id = ", current.RoomId, ", name = @",
                            current.PetId, "name, x = ", current.X, ", Y = ", current.Y, ", Z = ", current.Z,
                            " WHERE id = ", current.PetId));
                        queryChunk3.AddQuery(string.Concat("UPDATE pets_data SET race = @", current.PetId,
                            "race, color = @", current.PetId, "color, type = ", current.Type, ", experience = ",
                            current.Experience, ", energy = ", current.Energy, ", nutrition = ", current.Nutrition,
                            ", respect = ", current.Respect, ", createstamp = '", current.CreationStamp,
                            "' WHERE id = ",
                            current.PetId));
                        break;
                }

                current.DbState = DatabaseUpdateState.Updated;
            }

            queryChunk.Execute(dbClient);
            queryChunk3.Execute(dbClient);
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
        internal ServerMessage SerializeStatusUpdates(bool all)
        {
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

                current.SerializeStatus(serverMessage);
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
        internal void UpdateUserStatus(RoomUser user, bool cycleGameItems, bool removeStatusses = true)
        {
            if (user?.Statusses == null) return;

            if (removeStatusses)
            {
                if (user.Statusses.TryRemove("lay", out _))
                {
                    user.UpdateNeeded = true;
                }

                if (user.Statusses.TryRemove("sit", out _))
                {
                    user.UpdateNeeded = true;
                }
            }

            var isBot = user.IsBot;
            if (isBot) cycleGameItems = false;

            try
            {
                var roomMap = _userRoom.GetGameMap();
                var userPoint = new Point(user.X, user.Y);
                var allRoomItemForSquare = roomMap.GetCoordinatedHeighestItems(userPoint);
                var itemsOnSquare = roomMap.GetCoordinatedItems(userPoint);

                var newZ = _userRoom.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare) +
                           ((user.RidingHorse && user.IsPet == false) ? 1 : 0);

                if (Math.Abs(newZ - user.Z) > 0)
                {
                    user.Z = newZ;
                    user.UpdateNeeded = true;
                }

                /* TODO CHECK */
                foreach (var item in allRoomItemForSquare)
                {

                    if (cycleGameItems)
                    {
                        item.UserWalksOnFurni(user);
                    }

                    if (item.GetBaseItem().IsSeat)
                    {
                        if (!user.Statusses.ContainsKey("sit"))
                        {
                            if (item.GetBaseItem().StackMultipler && !string.IsNullOrWhiteSpace(item.ExtraData))
                                if (item.ExtraData != "0")
                                {
                                    var num2 = Convert.ToInt32(item.ExtraData);
                                    user.Statusses.TryAdd("sit",
                                        item.GetBaseItem().ToggleHeight[num2].ToString(CultureInfo.InvariantCulture)
                                            .Replace(',', '.'));
                                }
                                else
                                {
                                    user.Statusses.TryAdd("sit",
                                        Convert.ToString(item.GetBaseItem().Height, CultureInfo.InvariantCulture));
                                }
                            else
                            {
                                user.Statusses.TryAdd("sit",
                                    Convert.ToString(item.GetBaseItem().Height, CultureInfo.InvariantCulture));
                            }
                        }

                        if (Math.Abs(user.Z - item.Z) > 0 || user.RotBody != item.Rot)
                        {
                            user.Z = item.Z;
                            user.RotHead = item.Rot;
                            user.RotBody = item.Rot;
                            user.UpdateNeeded = true;
                        }
                    }

                    var interactionType = item.GetBaseItem().InteractionType;

                    switch (interactionType)
                    {
                        case Interaction.QuickTeleport:
                        case Interaction.GuildGate:
                        case Interaction.WalkInternalLink:
                        case Interaction.FloorSwitch:
                            {
                                item.Interactor.OnUserWalk(user.GetClient(), item, user);
                                break;
                            }
                        case Interaction.None:
                            break;

                        case Interaction.PressurePadBed:
                        case Interaction.Bed:
                            {
                                user.Statusses["lay"] = TextHandling.GetString(item.GetBaseItem().Height);


                                user.Z = item.Z;
                                user.RotHead = item.Rot;
                                user.RotBody = item.Rot;
                                user.UpdateNeeded = true;

                                if (item.GetBaseItem().InteractionType == Interaction.PressurePadBed)
                                {
                                    item.ExtraData = "1";
                                    item.UpdateState();
                                }

                                break;
                            }

                        case Interaction.Guillotine:
                            {
                                user.Statusses["lay"] = TextHandling.GetString(item.GetBaseItem().Height);

                                user.Z = item.Z;
                                user.RotBody = item.Rot;

                                item.ExtraData = "1";
                                item.UpdateState();
                                var avatarEffectsInventoryComponent =
                                    user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();

                                avatarEffectsInventoryComponent.ActivateCustomEffect(133);
                                break;
                            }

                        case Interaction.FootballGate:
                            break;

                        case Interaction.BanzaiGateBlue:
                        case Interaction.BanzaiGateRed:
                        case Interaction.BanzaiGateYellow:
                        case Interaction.BanzaiGateGreen:
                            {
                                var effect = (int)item.Team + 32;
                                var teamManagerForBanzai =
                                    user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();
                                var avatarEffectsInventoryComponent =
                                    user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();
                                if (user.Team == Team.None)
                                {
                                    if (!teamManagerForBanzai.CanEnterOnTeam(item.Team)) break;
                                    user.Team = item.Team;
                                    teamManagerForBanzai.AddUser(user);
                                    if (avatarEffectsInventoryComponent.CurrentEffect != effect)
                                        avatarEffectsInventoryComponent.ActivateCustomEffect(effect);
                                    break;
                                }

                                if (user.Team != Team.None && user.Team != item.Team)
                                {
                                    teamManagerForBanzai.OnUserLeave(user);
                                    user.Team = Team.None;
                                    avatarEffectsInventoryComponent.ActivateCustomEffect(0);
                                    break;
                                }

                                teamManagerForBanzai.OnUserLeave(user);

                                if (avatarEffectsInventoryComponent.CurrentEffect == effect)
                                    avatarEffectsInventoryComponent.ActivateCustomEffect(0);
                                user.Team = Team.None;
                                break;
                            }

                        case Interaction.Jump:
                            break;

                        case Interaction.Pinata:
                            {
                                if (!user.IsWalking || item.ExtraData.Length <= 0) break;
                                var num5 = int.Parse(item.ExtraData);
                                if (num5 >= 100 || user.CurrentEffect != 158) break;
                                var num6 = num5 + 1;
                                item.ExtraData = num6.ToString();
                                item.UpdateState();
                                Oblivion.GetGame()
                                    .GetAchievementManager()
                                    .ProgressUserAchievement(user.GetClient(), "ACH_PinataWhacker", 1);
                                if (num6 == 100)
                                {
                                    Oblivion.GetGame().GetPinataHandler().DeliverRandomPinataItem(user, _userRoom, item);
                                    Oblivion.GetGame()
                                        .GetAchievementManager()
                                        .ProgressUserAchievement(user.GetClient(), "ACH_PinataBreaker", 1);
                                }

                                break;
                            }
                        case Interaction.TileStackMagic:
                        case Interaction.Poster:
                            break;

                        case Interaction.Tent:
                        case Interaction.BedTent:
                            if (user.LastItem == item.Id)
                            {
                                user.OnCampingTent = true;
                                break;
                            }
                            if (!user.IsBot && !user.OnCampingTent)
                            {
                                var serverMessage22 = new ServerMessage();
                                serverMessage22.Init(
                                    LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                                serverMessage22.AppendString(item.Id.ToString());
                                serverMessage22.AppendInteger(0);
                                serverMessage22.AppendString("1");
                                user.GetClient().SendMessage(serverMessage22);
                                user.OnCampingTent = true;
                                user.LastItem = item.Id;
                            }
                            user.OnCampingTent = true;
                            break;

                        case Interaction.RunWaySage:
                            {
                                var num7 = new Random().Next(1, 4);
                                item.ExtraData = num7.ToString();
                                item.UpdateState();
                                break;
                            }
                        case Interaction.Shower:
                        case Interaction.ChairState:
                        case Interaction.PressurePad:
                            {
                                item.ExtraData = "1";
                                item.UpdateState();
                                break;
                            }
                        case Interaction.BanzaiTele:
                            {
                                if (user.IsWalking)
                                    _userRoom.GetGameItemHandler().OnTeleportRoomUserEnter(user, item);
                                break;
                            }
                        case Interaction.FreezeYellowGate:
                        case Interaction.FreezeRedGate:
                        case Interaction.FreezeGreenGate:
                        case Interaction.FreezeBlueGate:
                            {
                                if (cycleGameItems)
                                {
                                    var num4 = (int)(item.Team + 39);
                                    var teamManagerForFreeze =
                                        user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();
                                    var avatarEffectsInventoryComponent2 =
                                        user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();
                                    if (user.Team != item.Team)
                                    {
                                        if (teamManagerForFreeze.CanEnterOnTeam(item.Team))
                                        {
                                            if (user.Team != Team.None) teamManagerForFreeze.OnUserLeave(user);
                                            user.Team = item.Team;
                                            teamManagerForFreeze.AddUser(user);
                                            if (avatarEffectsInventoryComponent2.CurrentEffect != num4)
                                                avatarEffectsInventoryComponent2.ActivateCustomEffect(num4);
                                        }
                                    }
                                    else
                                    {
                                        teamManagerForFreeze.OnUserLeave(user);
                                        if (avatarEffectsInventoryComponent2.CurrentEffect == num4)
                                            avatarEffectsInventoryComponent2.ActivateCustomEffect(0);
                                        user.Team = Team.None;
                                    }

                                    var serverMessage33 =
                                        new ServerMessage(
                                            LibraryParser.OutgoingRequest("UserIsPlayingFreezeMessageComposer"));
                                    serverMessage33.AppendBool(user.Team != Team.None);
                                    user.GetClient().SendMessage(serverMessage33);
                                }

                                break;
                            }
                    }
                    
                    user.LastItem = item.Id;
                }

                if (user.IsSitting && user.TeleportEnabled)
                {
                    user.Z -= 0.35;
                    user.UpdateNeeded = true;
                }

                if (cycleGameItems)
                {
                    if (_userRoom.GotSoccer())
                        _userRoom.GetSoccer().OnUserWalk(user);
                    if (_userRoom.GotBanzai())
                        _userRoom.GetBanzai().OnUserWalk(user);
                    if (_userRoom.GotFreeze())
                        _userRoom.GetFreeze().OnUserWalk(user);
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
                await Task.Yield();

                if (!roomUsers.IsOwner() && roomUsers.LastHostingDate + 60 < Oblivion.GetUnixTimeStamp())
                {
                    var roomOwner = (uint)roomUsers.GetRoom().RoomData.OwnerId;
                    var ownerClient = Oblivion.GetGame().GetClientManager().GetClientByUserId(roomOwner);
                    if (ownerClient != null)
                    {
                        Oblivion.GetGame().GetAchievementManager()
                            .ProgressUserAchievement(ownerClient, "ACH_RoomDecoHosting", 1, true);
                    }

                    roomUsers.LastHostingDate = Oblivion.GetUnixTimeStamp();
                }

                if ((!roomUsers.IsAsleep) && (roomUsers.IdleTime >= 600) && (!roomUsers.IsBot) && (!roomUsers.IsPet))
                {
                    roomUsers.IsAsleep = true;

                    var sleepEffectMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
                    sleepEffectMessage.AppendInteger(roomUsers.VirtualId);
                    sleepEffectMessage.AppendBool(true);
                    _userRoom.SendMessage(sleepEffectMessage);
                    roomUsers.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateEffect(517);
                }

                if ((!roomUsers.IsOwner()) && (roomUsers.IdleTime >= 300) && (!roomUsers.IsBot) && (!roomUsers.IsPet))
                {
                    try
                    {
                        var ownerAchievementMessage =
                            Oblivion.GetGame().GetClientManager().GetClientByUserId((uint)_userRoom.RoomData.OwnerId);

                        if (ownerAchievementMessage != null)
                            Oblivion.GetGame()
                                .GetAchievementManager()
                                .ProgressUserAchievement(ownerAchievementMessage, "ACH_RoomDecoHosting", 1, true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "UserRoomTimeCycles");
            }
        }

        internal void RoomUserBreedInteraction(RoomUser roomUsers)
        {
            if ((roomUsers.IsPet) && ((roomUsers.PetData.Type == 3) || (roomUsers.PetData.Type == 4)) &&
                (roomUsers.PetData.WaitingForBreading > 0) &&
                ((roomUsers.PetData.BreadingTile.X == roomUsers.X) &&
                 (roomUsers.PetData.BreadingTile.Y == roomUsers.Y)))
            {
                roomUsers.Freezed = true;
                _userRoom.GetGameMap().RemoveUserFromMap(roomUsers, roomUsers.Coordinate);

                switch (roomUsers.PetData.Type)
                {
                    case 3:
                        if (
                            _userRoom.GetRoomItemHandler().BreedingTerrier[roomUsers.PetData.WaitingForBreading]
                                .PetsList.Count == 2)
                        {
                            var petBreedOwner =
                                Oblivion.GetGame().GetClientManager().GetClientByUserId(roomUsers.PetData.OwnerId);

                            petBreedOwner?.SendMessage(PetBreeding.GetMessage(roomUsers.PetData.WaitingForBreading,
                                _userRoom.GetRoomItemHandler().BreedingTerrier[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[0],
                                _userRoom.GetRoomItemHandler().BreedingTerrier[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[1]));
                        }

                        break;

                    case 4:
                        if (
                            _userRoom.GetRoomItemHandler().BreedingBear[roomUsers.PetData.WaitingForBreading].PetsList
                                .Count == 2)
                        {
                            var petBreedOwner =
                                Oblivion.GetGame().GetClientManager().GetClientByUserId(roomUsers.PetData.OwnerId);

                            petBreedOwner?.SendMessage(PetBreeding.GetMessage(roomUsers.PetData.WaitingForBreading,
                                _userRoom.GetRoomItemHandler().BreedingBear[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[0],
                                _userRoom.GetRoomItemHandler().BreedingBear[roomUsers.PetData.WaitingForBreading]
                                    .PetsList[1]));
                        }

                        break;
                }

                UpdateUserStatus(roomUsers, false);
            }
            else if ((roomUsers.IsPet) && ((roomUsers.PetData.Type == 3) || (roomUsers.PetData.Type == 4)) &&
                     (roomUsers.PetData.WaitingForBreading > 0) &&
                     ((roomUsers.PetData.BreadingTile.X != roomUsers.X) &&
                      (roomUsers.PetData.BreadingTile.Y != roomUsers.Y)))
            {
                roomUsers.Freezed = false;
                roomUsers.PetData.WaitingForBreading = 0;
                roomUsers.PetData.BreadingTile = new Point();
                UpdateUserStatus(roomUsers, false);
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
                    roomUsers.SetZ = (_userRoom.GetGameMap().SqAbsoluteHeight(nextStep.X, nextStep.Y) + 1);

                    horseRidingPet.SetX = roomUsers.SetX;
                    horseRidingPet.SetY = roomUsers.SetY;
                    horseRidingPet.SetZ = _userRoom.GetGameMap().SqAbsoluteHeight(roomUsers.SetX, roomUsers.SetY);
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
                roomUsers.SetZ = _userRoom.GetGameMap().SqAbsoluteHeight(nextStep.X, nextStep.Y);
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

        internal bool UserGoToTile(RoomUser roomUsers, bool invalidStep)
        {
            if (roomUsers?.Path == null) return false;
            if ((invalidStep || (roomUsers.PathStep >= roomUsers.Path.Count) ||
                 ((roomUsers.GoalX == roomUsers.X) && (roomUsers.GoalY == roomUsers.Y))))
            {
                // Erase all Movement Data..
                roomUsers.IsWalking = false;
                roomUsers.ClearMovement();
                roomUsers.HandelingBallStatus = 0;
                RoomUserBreedInteraction(roomUsers);

                // Check if he is in a Horse, and if if Erase Horse and User Movement Data
                if ((roomUsers.RidingHorse) && (!roomUsers.IsPet))
                {
                    var horseStopWalkRidingPet = GetRoomUserByVirtualId(Convert.ToInt32(roomUsers.HorseId));

                    if (horseStopWalkRidingPet != null)
                    {
                        var horseStopWalkRidingPetMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer"));
                        horseStopWalkRidingPetMessage.AppendInteger(1);
                        horseStopWalkRidingPet.SerializeStatus(horseStopWalkRidingPetMessage, "");
                        _userRoom.SendMessage(horseStopWalkRidingPetMessage);

                        horseStopWalkRidingPet.IsWalking = false;
                        horseStopWalkRidingPet.ClearMovement();
                    }
                }

                // Finally Update User Status
                UpdateUserStatus(roomUsers, false);
            }
            else
            {


                // Region Set Variables
                var pathDataCount = ((roomUsers.Path.Count - roomUsers.PathStep) - 1);
                if (roomUsers.Path.Count <= pathDataCount || pathDataCount < 0)
                    return false;
                var nextStep = roomUsers.Path[pathDataCount];

                
                // Increase Step Data...
                roomUsers.PathStep++;
                // Ins't a Invalid Step.. Continuing.

                if (roomUsers.FastWalking)
                {
                    pathDataCount = (roomUsers.Path.Count - roomUsers.PathStep) - 1;
                    if (roomUsers.Path.Count <= pathDataCount || pathDataCount < 0)
                        return false;
                    nextStep = roomUsers.Path[pathDataCount];
                    roomUsers.PathStep++;
                }

                roomUsers.RemoveStatus("mv");


                // Check Against if is a Valid Step...

                if (_userRoom.GetGameMap()
                    .IsValidStep(roomUsers, new Vector2D(roomUsers.X, roomUsers.Y),
                        new Vector2D(nextStep.X, nextStep.Y),
                        ((roomUsers.GoalX == nextStep.X) && (roomUsers.GoalY == nextStep.Y)), roomUsers.AllowOverride,
                        false, false))
                {
                    // If is a PET Must Give the Time Tick In Syncrony with User..
                    if ((roomUsers.RidingHorse) && (!roomUsers.IsPet))
                    {
                        var horsePetAi = GetRoomUserByVirtualId(Convert.ToInt32(roomUsers.HorseId));
                        try
                        {
                            horsePetAi?.BotAi.OnTimerTick();
                        }
                        catch (Exception e)
                        {
                            Logging.HandleException(e, "RoomUserManager - horsePetAi.OnTimerTick");
                        }
                    }

                    // Horse Ridding need be Updated First
                    if (roomUsers.RidingHorse)
                    {
                        // Set User Position Data
                        UserSetPositionData(roomUsers, nextStep);
                        CheckUserSittableLayable(roomUsers);

                        // Add Status of Walking
                        roomUsers.AddStatus("mv",
                            +roomUsers.SetX + "," + roomUsers.SetY + "," + TextHandling.GetString(roomUsers.SetZ));
                    }

                    // Check if User is Ridding in Horse, if if Let's Update Ride Data.
                    if ((roomUsers.RidingHorse) && (!roomUsers.IsPet))
                    {
                        var horseRidingPet = GetRoomUserByVirtualId(Convert.ToInt32(roomUsers.HorseId));

                        if (horseRidingPet != null)
                        {
                            var theUser = "mv " + roomUsers.SetX + "," + roomUsers.SetY + "," +
                                          TextHandling.GetString(roomUsers.SetZ);
                            var thePet = "mv " + roomUsers.SetX + "," + roomUsers.SetY + "," +
                                         TextHandling.GetString(horseRidingPet.SetZ);

                            var horseRidingPetMessage =
                                new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer"));
                            horseRidingPetMessage.AppendInteger(2);
                            roomUsers.SerializeStatus(horseRidingPetMessage, theUser);
                            horseRidingPet.SerializeStatus(horseRidingPetMessage, thePet);
                            _userRoom.SendMessage(horseRidingPetMessage);

                            horseRidingPet.RotBody = roomUsers.RotBody;
                            horseRidingPet.RotHead = roomUsers.RotBody;
                            horseRidingPet.SetX = roomUsers.SetX;
                            horseRidingPet.SetY = roomUsers.SetY;
                            horseRidingPet.SetZ = (roomUsers.SetZ - 1);
                            horseRidingPet.SetStep = true;

                            UpdateUserEffect(horseRidingPet, horseRidingPet.SetX, horseRidingPet.SetY);
                            UpdateUserStatus(horseRidingPet, false);
                        }
                    }

                    // If is not Ridding Horse doesn't Need Update Effect
                    if (!roomUsers.RidingHorse)
                    {

                        // Set User Position Data
                        UserSetPositionData(roomUsers, nextStep);
                        CheckUserSittableLayable(roomUsers);

                        // Add Status of Walking
                        roomUsers.AddStatus("mv",
                            +roomUsers.SetX + "," + roomUsers.SetY + "," + TextHandling.GetString(roomUsers.SetZ));
                    }

                    // Region Update User Effect And Status
                    UpdateUserEffect(roomUsers, roomUsers.SetX, roomUsers.SetY);

                    // Update Effect if is Ridding
                    if (roomUsers.RidingHorse)
                        UpdateUserStatus(roomUsers, false);


                    // If user is in soccer proccess.
                    if (_userRoom.GotSoccer())
                        _userRoom.GetSoccer().OnUserWalk(roomUsers);

                    //                    if (!roomUsers.RidingHorse)
                    //                        roomUsers.UpdateNeeded = true;

                    return true;
                }

                if (roomUsers.PathRecalcNeeded && !roomUsers.SetStep)
                    return false;

            }

//            if (!roomUsers.RidingHorse)
//                    roomUsers.UpdateNeeded = true;
/*

                // Isn't a Valid Step! And he Can Go? Erase Imediatile Effect
                roomUsers.ClearMovement();

                // If user isn't pet and Bot, we have serious Problems. Let Recalculate Path!
                if ((!roomUsers.IsPet) && (!roomUsers.IsBot))
                    roomUsers.PathRecalcNeeded = true;
*/

                return true;
            
        }

        internal bool UserCanWalkInTile(RoomUser roomUsers)
        {
            try
            {
                var hasGroup = false;
                if (!roomUsers.IsBot && !roomUsers.IsPet)
                if (_userRoom.GetGameMap().GuildGates.TryGetValue(new Point(roomUsers.SetX, roomUsers.SetY), out var guild))
                {
                    var guildId = guild.GroupId;
                    if (guildId > 0 &&
                        roomUsers.GetClient()
                            .GetHabbo()
                            .UserGroups.Any(member => member != null && member.GroupId == guildId))
                        hasGroup = true;
                }

                // Check if User CanWalk...
                if ((_userRoom.GetGameMap().SquareIsOpen(roomUsers.SetX, roomUsers.SetY, roomUsers.AllowOverride)) ||
                    (roomUsers.RidingHorse) || hasGroup)
                {
                    // Let's Update his Movement...
                    _userRoom.GetGameMap()
                        .UpdateUserMovement(new Point(roomUsers.Coordinate.X, roomUsers.Coordinate.Y),
                            new Point(roomUsers.SetX, roomUsers.SetY), roomUsers);
                    var hasItemInPlace = _userRoom.GetGameMap().GetCoordinatedItems(new Point(roomUsers.X, roomUsers.Y))
                        .ToList();

                    // Set His Actual X,Y,Z Position...
                    roomUsers.X = roomUsers.SetX;
                    roomUsers.Y = roomUsers.SetY;
                    roomUsers.Z = roomUsers.SetZ;

                    // Check Sub Items Interactionables
                    /* TODO CHECK */
                    foreach (var roomItem in hasItemInPlace)
                    {
                        roomItem.UserWalksOffFurni(roomUsers);
                        switch (roomItem.GetBaseItem().InteractionType)
                        {
                            case Interaction.Normslaskates:
                                if (roomUsers.LastRollerDate + 60 < Oblivion.GetUnixTimeStamp())
                                {
                                    Oblivion.GetGame().GetAchievementManager()
                                        .ProgressUserAchievement(roomUsers.GetClient(), "ACH_RbTagC", 1);
                                    roomUsers.LastRollerDate = Oblivion.GetUnixTimeStamp();
                                }

                                Oblivion.GetGame().GetAchievementManager()
                                    .ProgressUserAchievement(roomUsers.GetClient(), "ACH_TagB", 1);

                                break;
                            case Interaction.IceSkates:
                                if (roomUsers.LastRollerDate + 60 < Oblivion.GetUnixTimeStamp())
                                {
                                    Oblivion.GetGame().GetAchievementManager()
                                        .ProgressUserAchievement(roomUsers.GetClient(), "ACH_TagC", 1);
                                    roomUsers.LastRollerDate = Oblivion.GetUnixTimeStamp();
                                }

                                Oblivion.GetGame().GetAchievementManager()
                                    .ProgressUserAchievement(roomUsers.GetClient(), "ACH_TagB", 1);
                                break;

                            case Interaction.QuickTeleport:
                            case Interaction.GuildGate:
                            case Interaction.WalkInternalLink:
                            case Interaction.FloorSwitch:
                                roomItem.Interactor.OnUserWalk(roomUsers.GetClient(), roomItem, roomUsers);
                                break;
                            case Interaction.RunWaySage:
                            case Interaction.ChairState:
                            case Interaction.Shower:
                            case Interaction.PressurePad:
                            case Interaction.PressurePadBed:
                            case Interaction.Guillotine:
                                roomItem.ExtraData = "0";
                                roomItem.UpdateState();
                                break;

                            case Interaction.Tent:
                            case Interaction.BedTent:
                                if (!roomUsers.IsBot && roomUsers.OnCampingTent)
                                {
                                    var serverMessage = new ServerMessage();
                                    serverMessage.Init(
                                        LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                                    serverMessage.AppendString(roomItem.Id.ToString());
                                    serverMessage.AppendInteger(0);
                                    serverMessage.AppendString("0");
                                    roomUsers.GetClient().SendMessage(serverMessage);
                                    roomUsers.OnCampingTent = false;
                                }

                                break;

                            case Interaction.None:
                                break;
                        }
                    }

                    hasItemInPlace.Clear();
                    // Let's Update user Status..
                    UpdateUserStatus(roomUsers, true, false);
                    return false;
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "UserCanWalkInTile");
            }

            return !_userRoom.RoomData.AllowWalkThrough;
        }


        /// <summary>
        ///     Turns the user thread
        /// </summary>
        /// <param name="roomUsers"></param>
        internal void UserCycleOnRoom(RoomUser roomUsers)
        {
            try
            {
                if (roomUsers?.Statusses == null) return;

                if (!IsValid(roomUsers))
                {
                    if (roomUsers.GetClient() != null)
                        RemoveUserFromRoom(roomUsers, false, false);
                    else
                        RemoveRoomUser(roomUsers);
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
                        roomUsers.CarryItem(0);
                }


                if (roomUsers.Frozen)
                {
                    roomUsers.FrozenTick--;
                    if (roomUsers.FrozenTick <= 0)
                    {
                        roomUsers.Frozen = false;
                        roomUsers.GetClient().SendWhisper("Voc� foi descongelado!!");
                    }
                }


                // Region Check User Got Freezed
                if (_userRoom.GotFreeze())
                {
                    Freeze.CycleUser(roomUsers);
                }

                // Region Variable Registering
                var invalidStep = false;
                // Region Check User Tile Selection
                if (roomUsers.SetStep)
                {
                    // Check if User is Going to the Door.

                    if ((roomUsers.SetX == _userRoom.GetGameMap().Model.DoorX) &&
                        (roomUsers.SetY == _userRoom.GetGameMap().Model.DoorY) &&
                        (!_removeUsers.Contains(roomUsers)) &&
                        (!roomUsers.IsBot) && (!roomUsers.IsPet))
                    {
                        _removeUsers.Add(roomUsers);
                        return;
                    }

                    _userRoom.GetGameMap().GameMap[roomUsers.X, roomUsers.Y] = roomUsers.SqState;
                    roomUsers.SqState = _userRoom.GetGameMap().GameMap[roomUsers.SetX, roomUsers.SetY];


                    // Check Elegibility of Walk In Tile
                    invalidStep = UserCanWalkInTile(roomUsers);

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
                        horseStopWalkRidingPetMessage.AppendInteger(1);
                        horseStopWalkRidingPet.SerializeStatus(horseStopWalkRidingPetMessage, "");
                        _userRoom.SendMessage(horseStopWalkRidingPetMessage);

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
                    UpdateUserStatus(roomUsers, false);
                }

                /*
                                // Check if Proably the Pathfinder is with Some Errors..
                                if (roomUsers.PathRecalcNeeded)
                                {
                                    roomUsers.Path.Clear();
                                    roomUsers.Path = PathFinder.FindPath(roomUsers, _userRoom.GetGameMap().DiagonalEnabled,
                                        _userRoom.GetGameMap(), new Vector2D(roomUsers.X, roomUsers.Y),
                                        new Vector2D(roomUsers.GoalX, roomUsers.GoalY));

                                    if (roomUsers.Path.Count > 1)
                                    {
                                        roomUsers.PathStep = 1;
                                        roomUsers.IsWalking = true;
                                        roomUsers.PathRecalcNeeded = false;
                                    }
                                    else
                                    {
                                        roomUsers.PathRecalcNeeded = false;
                                        roomUsers.Path.Clear();
                                    }
                                }*/

                while (true)
                {
                    GenerateNewPath(roomUsers);
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
                        if (UserGoToTile(roomUsers, invalidStep))
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

                // If is a Bot.. Let's Tick the Time Count of Bot..
                if (roomUsers.IsBot)
                {
                    try
                    {
                        if (_userRoom.GotWireds())
                        {
                            if (roomUsers.FollowingOwner != null)
                            {
                                roomUsers.MoveTo(_userRoom.GetGameMap()
                                    .SquareIsOpen(roomUsers.FollowingOwner.SquareInFront.X,
                                        roomUsers.FollowingOwner.SquareInFront.Y, false)
                                    ? roomUsers.FollowingOwner.SquareInFront
                                    : roomUsers.FollowingOwner.SquareBehind);
                            }

                            var users = _userRoom.GetGameMap()
                                .GetRoomUsers(roomUsers.SquareInFront);

                            if (users != null && users.Count > 0)
                            {
                                var user = users[0];

                                _userRoom.GetWiredHandler().ExecuteWired(Interaction.TriggerBotReachedAvatar, user);
                            }
                        }

                        roomUsers.BotAi?.OnTimerTick();
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


        public void GenerateNewPath(RoomUser User)
        {
            if (User.PathRecalcNeeded || User.IsWalking)
            {
                User.Path.Clear();
                User.Path = PathFinder.FindPath(User, _userRoom.GetGameMap().DiagonalEnabled,
                    _userRoom.GetGameMap(), new Vector2D(User.X, User.Y),
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
        internal void OnCycle(ref int idleCount)
        {
            // User in Room Count for foreach
            uint userInRoomCount = 0;

            // Clear RemoveUser's List.
            _removeUsers.Clear();

            try
            {
                // Check Disco Procedure...
                if ((_userRoom != null) && (_userRoom.DiscoMode) && (_userRoom.TonerData != null) &&
                    (_userRoom.TonerData.Enabled == 1))
                {
                    var tonerItem = _userRoom.GetRoomItemHandler().GetItem(_userRoom.TonerData.ItemId);

                    if (tonerItem != null)
                    {
                        _userRoom.TonerData.Data1 = Oblivion.GetRandomNumber(0, 255);
                        _userRoom.TonerData.Data2 = Oblivion.GetRandomNumber(0, 255);
                        _userRoom.TonerData.Data3 = Oblivion.GetRandomNumber(0, 255);

                        var tonerComposingMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
                        tonerItem.Serialize(tonerComposingMessage);
                        _userRoom.SendMessage(tonerComposingMessage);
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
                UserCycleOnRoom(roomUsers);

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
                    RemoveUserFromRoom(userToRemove, true, false);
                else
                    RemoveRoomUser(userToRemove);
            }

            if (userInRoomCount == 0)
            {
                idleCount++;
            }

            if (userInRoomCount >= 5 && Oblivion.Multipy > 1)
            {
                if (_roomUserCount != userInRoomCount * (uint) Oblivion.Multipy)
                {
                    UpdateUserCount(userInRoomCount * (uint) Oblivion.Multipy);
                }
            }
            else
            {
                if (_roomUserCount != userInRoomCount)
                {
                    UpdateUserCount(userInRoomCount);
                }
            }
        }

        /// <summary>
        ///     Updates the user effect.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        private void UpdateUserEffect(RoomUser user, int x, int y)
        {
            if (user.IsBot)
                return;
            try
            {
                var inv = user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();
                if (inv == null) return;
                var items = _userRoom.GetGameMap().GetAllRoomItemForSquare(x, y);
                if (items.Count <= 0)
                {
                    if (user.CurrentItemEffect != 0)
                    {
                        inv.ActivateCustomEffect(0);
                        user.CurrentItemEffect = 0;
                    }

                    return;
                }

                var item = items[0]?.GetBaseItem();
                if (item == null)
                {
                    return;
                }

                var b = user.GetClient().GetHabbo().Gender == "m" ? item.EffectM : item.EffectF;
                if (b > 0)
                {
                    if (inv.CurrentEffect == 0)
                    {
                        user.CurrentItemEffect = 0;
                    }

                    if (b == user.CurrentItemEffect)
                        return;

                    inv.ActivateCustomEffect(b);
                    user.CurrentItemEffect = b;
                }
                else
                {
                    if (user.CurrentItemEffect == 0 || b != 0)
                        return;
                    inv.ActivateCustomEffect(-1);
                    user.CurrentItemEffect = 0;
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     Handles the <see cref="E:UserAdd" /> event.
        /// </summary>
        /// <param name="user"></param>
        private void OnUserAdd(RoomUser user)
        {
            try
            {
                var client = user?.GetClient();

                if (client?.GetHabbo() == null || _userRoom?.RoomData == null)
                    return;
                if (client.IsAir)
                {
                    var msg = new ServerMessage();
                    _userRoom.RoomData.SerializeRoomData(msg, client, true);
                    client.SendMessage(msg);
                }

                if (!user.IsSpectator)
                {
                    var model = _userRoom.GetGameMap().Model;
                    if (model == null) return;
                    user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                    user.SetRot(model.DoorOrientation, false);

                    var session = user.GetClient();
                    if (_userRoom.CheckRights(session, true))
                    {
                        var msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        msg.AppendInteger(4);
                        session.SendMessage(msg);
                        msg = new ServerMessage(LibraryParser.OutgoingRequest("HasOwnerRightsMessageComposer"));
                        session.SendMessage(msg);
                        user.AddStatus("flatctrl 4", string.Empty);
                    }
                    else if (_userRoom.CheckRights(session, false, true))
                    {
                        var msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        msg.AppendInteger(1);
                        session.SendMessage(msg);
                        user.AddStatus("flatctrl 3", string.Empty);
                    }
                    else
                    {
                        var msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        msg.AppendInteger(0);
                        session.SendMessage(msg);
                        msg = new ServerMessage(
                            LibraryParser.OutgoingRequest("YouAreNotControllerMessageComposer"));
                        session.SendMessage(msg);
                    }

                    user.CurrentItemEffect = 0;

                    if (!user.IsBot && client.GetHabbo().IsTeleporting)
                    {
                        client.GetHabbo().IsTeleporting = false;
                        client.GetHabbo().TeleportingRoomId = 0;

                        var item = _userRoom.GetRoomItemHandler().GetItem(client.GetHabbo().TeleporterId);

                        if (item != null)
                        {
                            item.ExtraData = "2";
                            item.UpdateState(false, true);
                            user.SetPos(item.X, item.Y, item.Z);
                            user.SetRot(item.Rot, false);
                            item.InteractingUser2 = client.GetHabbo().Id;
                            item.ExtraData = "0";
                            item.UpdateState(false, true);
                        }
                    }

                    if (!user.IsBot && client.GetHabbo().IsHopping)
                    {
                        client.GetHabbo().IsHopping = false;
                        client.GetHabbo().HopperId = 0;

                        var item2 = _userRoom.GetRoomItemHandler().GetItem(client.GetHabbo().HopperId);

                        if (item2 != null)
                        {
                            item2.ExtraData = "1";
                            item2.UpdateState(false, true);
                            user.SetPos(item2.X, item2.Y, item2.Z);
                            user.SetRot(item2.Rot, false);
                            user.AllowOverride = false;
                            item2.InteractingUser2 = client.GetHabbo().Id;
                            item2.ExtraData = "2";
                            item2.UpdateState(false, true);
                        }
                    }

                    if (!user.IsSpectator)
                    {
                        var serverMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
                        serverMessage.AppendInteger(1);
                        if (!user.Serialize(serverMessage)) return;
                        _userRoom.SendMessage(serverMessage);
                    }

                    if (!user.IsBot)
                    {
                        var serverMessage2 = new ServerMessage();
                        serverMessage2.Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                        serverMessage2.AppendInteger(user.VirtualId);
                        serverMessage2.AppendString(client.GetHabbo().Look);
                        serverMessage2.AppendString(client.GetHabbo().Gender.ToLower());
                        serverMessage2.AppendString(client.GetHabbo().Motto);
                        serverMessage2.AppendInteger(client.GetHabbo().AchievementPoints);
                        _userRoom.SendMessage(serverMessage2);
                    }

                    if (_userRoom.RoomData.Owner != client.GetHabbo().UserName)
                    {
                        Oblivion.GetGame()
                            .GetQuestManager()
                            .ProgressUserQuest(client, QuestType.SocialVisit);
                        Oblivion.GetGame()
                            .GetAchievementManager()
                            .ProgressUserAchievement(client, "ACH_RoomEntry", 1);
                    }
                }

                if (client.GetHabbo().GetMessenger() != null)
                    client.GetHabbo().GetMessenger().OnStatusChanged(true);
                client.GetMessageHandler()?.OnRoomUserAdd();

                //                    if (client.GetHabbo().HasFuse("fuse_mod"))
                //                        client.GetHabbo().GetAvatarEffectsInventoryComponent()?.ActivateCustomEffect(102);
                if (client.GetHabbo().Rank == Convert.ToUInt32(Oblivion.GetDbConfig().DbData["ambassador.minrank"]))
                    client.GetHabbo().GetAvatarEffectsInventoryComponent()?.ActivateCustomEffect(178);

                if (_userRoom.GotMusicController())
                    _userRoom.GetRoomMusicController().OnNewUserEnter(user);
                _userRoom.OnUserEnter(user);
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
                    if (userClient != null && userClient.CurrentRoomId == _userRoom.RoomId)
                    {
                        userClient.CurrentRoomId = 0u;
                        userClient.CurrentRoom = null;
                        userClient.GetMessenger().DisposeRoom(_userRoom.RoomId);
                    }

                    user.Dispose();
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "dipose roomusermanager");
            }

            _userRoom = null;
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
        private void OnRemove(RoomUser user)
        {
            try
            {
                if (user?.GetClient() == null) return;
                var client = user.GetClient();
                var list = Bots.Values;

                var userOnCurrentItem = _userRoom.GetGameMap().GetCoordinatedItems(new Point(user.X, user.Y));
                /* TODO CHECK */
                foreach (var roomItem in userOnCurrentItem.ToList())
                {
                    switch (roomItem.GetBaseItem().InteractionType)
                    {
                        case Interaction.RunWaySage:
                        case Interaction.ChairState:
                        case Interaction.Shower:
                        case Interaction.PressurePad:
                        case Interaction.PressurePadBed:
                        case Interaction.Guillotine:
                            roomItem.ExtraData = "0";
                            roomItem.UpdateState();
                            break;
                    }
                }

                foreach (var bot in list)
                {
                    bot.BotAi.OnUserLeaveRoom(client);
                    if (bot.IsPet && bot.PetData.OwnerId == user.UserId &&
                        !_userRoom.CheckRights(client, true))
                    {
                        if (user.GetClient()?.GetHabbo()?.GetInventoryComponent() != null)
                        {
                            user.GetClient().GetHabbo().GetInventoryComponent().AddPet(bot.PetData);
                            RemoveBot(bot.VirtualId, false);
                        }
                    }
                }

                _userRoom.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
        }

        /// <summary>
        ///     Called when [user update status].
        /// </summary>
        public void OnUserUpdateStatus()
        {
            foreach (var current in UserList.Values)
                UpdateUserStatus(current, false);
        }

        /// <summary>
        ///     Called when [user update status].
        ///     <param name="x">x position</param>
        ///     <param name="y">y position</param>
        /// </summary>
        public void OnUserUpdateStatus(int x, int y)
        {
            foreach (var current in _userRoom.GetGameMap().GetRoomUsers(new Point(x, y)))
                UpdateUserStatus(current, false);
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
                                                                 _userRoom.RoomId));
    }
}