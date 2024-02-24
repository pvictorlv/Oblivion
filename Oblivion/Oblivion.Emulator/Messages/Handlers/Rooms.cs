using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Pets.Enums;
using Oblivion.HabboHotel.Polls.Enums;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;
using Oblivion.Messages.Parsers;
using Oblivion.Security;
using Oblivion.Util;

namespace Oblivion.Messages.Handlers
{
    internal partial class GameClientMessageHandler
    {
        private int _floodCount;
        private DateTime _floodTime;

        public async Task GetPetBreeds()
        {
            var type = Request.GetString();
            var petId = PetRace.GetPetId(type, out var petType);
            var races = PetRace.GetRacesForRaceId(petId);
            var message = Response;
            await message.InitAsync(LibraryParser.OutgoingRequest("SellablePetBreedsMessageComposer"));
            await message.AppendStringAsync(petType);
            await message.AppendIntegerAsync(races.Count);
            foreach (var current in races)
            {
                await message.AppendIntegerAsync(petId);
                await message.AppendIntegerAsync(current.Color1);
                await message.AppendIntegerAsync(current.Color2);
                message.AppendBool(current.Has1Color);
                message.AppendBool(current.Has2Color);
            }

            await SendResponse();
        }

        internal async Task GoRoom()
        {
            if (Oblivion.ShutdownStarted || Session?.GetHabbo() == null)
                return;
            var num = Request.GetUInteger();
            var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(num);
            //            Session.GetHabbo().GetInventoryComponent().RunDbUpdate();
            if (roomData == null || roomData.Id == Session.GetHabbo().CurrentRoomId)
                return;
            await roomData.SerializeRoomData(Response, Session, !Session.GetHabbo().InRoom);
            await PrepareRoomForUser(num, roomData.PassWord);
        }

        internal async Task AddFavorite()
        {
            if (Session.GetHabbo() == null)
                return;

            var roomId = Request.GetUInteger();


            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("FavouriteRoomsUpdateMessageComposer"));
            await GetResponse().AppendIntegerAsync(roomId);
            GetResponse().AppendBool(true);
            await SendResponse();


            if (!Session.GetHabbo().Data.FavouritedRooms.Contains(roomId))
                Session.GetHabbo().Data.FavouritedRooms.Add(roomId);
            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await queryReactor.RunFastQueryAsync("REPLACE INTO users_favorites (user_id,room_id) VALUES (" +
                                                     Session.GetHabbo().Id + "," + roomId + ")");
            }

            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("FavouriteRoomsMessageComposer"));
            await GetResponse().AppendIntegerAsync(30);

            if (Session.GetHabbo().Data.FavouritedRooms == null || Session.GetHabbo().Data.FavouritedRooms.Count <= 0)
                await GetResponse().AppendIntegerAsync(0);
            else
            {
                await GetResponse().AppendIntegerAsync(Session.GetHabbo().Data.FavouritedRooms.Count);

                /* TODO CHECK */
                foreach (var i in Session.GetHabbo().Data.FavouritedRooms)
                    await GetResponse().AppendIntegerAsync(i);
            }

            await SendResponse();

        }

        internal async Task RemoveFavorite()
        {
            if (Session.GetHabbo() == null)
                return;
            var roomId = Request.GetUInteger();
            Session.GetHabbo().Data.FavouritedRooms.Remove(roomId);

            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("FavouriteRoomsUpdateMessageComposer"));
            await GetResponse().AppendIntegerAsync(roomId);
            GetResponse().AppendBool(false);
            await SendResponse();

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await queryReactor.RunFastQueryAsync("DELETE FROM users_favorites WHERE user_id = " +
                                                     Session.GetHabbo().Id +
                                                     " AND room_id = " + roomId);
            }

            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("FavouriteRoomsMessageComposer"));
            await GetResponse().AppendIntegerAsync(30);

            if (Session.GetHabbo().Data.FavouritedRooms == null || Session.GetHabbo().Data.FavouritedRooms.Count <= 0)
                await GetResponse().AppendIntegerAsync(0);
            else
            {
                await GetResponse().AppendIntegerAsync(Session.GetHabbo().Data.FavouritedRooms.Count);

                /* TODO CHECK */
                foreach (var i in Session.GetHabbo().Data.FavouritedRooms)
                    await GetResponse().AppendIntegerAsync(i);
            }

            await SendResponse();
        }

        internal void OnlineConfirmationEvent()
        {
            return;
            //            Out.WriteLine(
//                "Is connected now with user: " + Request.GetString() + " and ip: " + Session.GetConnection().GetIp(),
//                "Oblivion.Users",
//                ConsoleColor.DarkGreen);
        }

        internal async Task GoToHotelView()
        {
            if (Session?.GetHabbo() == null)
                return;
            if (!Session.GetHabbo().InRoom)
                return;
            var room = Session.GetHabbo().CurrentRoom;
            room?.GetRoomUserManager().RemoveUserFromRoom(Session, true, false);

            var rankings = Oblivion.GetGame().GetHallOfFame().Rankings;

            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("HotelViewHallOfFameMessageComposer"));
            await GetResponse().AppendStringAsync("");
            GetResponse().StartArray();
            foreach (var element in rankings)
            {
                await GetResponse().AppendIntegerAsync(element.UserId);
                await GetResponse().AppendStringAsync(element.Username);
                await GetResponse().AppendStringAsync(element.Look);
                await GetResponse().AppendIntegerAsync(2);
                await GetResponse().AppendIntegerAsync(element.Score);
                GetResponse().SaveArray();
            }

            GetResponse().EndArray();
            await SendResponse();

            var hotelView = Oblivion.GetGame().GetHotelView();
            if (hotelView.FurniRewardName != null)
            {
                using var serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("LandingRewardMessageComposer"));
                await serverMessage.AppendStringAsync(hotelView.FurniRewardName);
                await serverMessage.AppendIntegerAsync(hotelView.FurniRewardId);
                await serverMessage.AppendIntegerAsync(120);
                await serverMessage.AppendIntegerAsync(120 - Session.GetHabbo().Respect);
                await Session.SendMessage(serverMessage);
            }

            Session.CurrentRoomUserId = -1;
        }

        internal async Task LandingCommunityGoal()
        {
            var onlineFriends = Session.GetHabbo().GetMessenger().Friends.Count(x => x.Value.IsOnline);
            using var goalMeter =
                new ServerMessage(LibraryParser.OutgoingRequest("LandingCommunityChallengeMessageComposer"));
            goalMeter.AppendBool(true); //
            await goalMeter.AppendIntegerAsync(0); //points
            await goalMeter.AppendIntegerAsync(0); //my rank
            await goalMeter.AppendIntegerAsync(onlineFriends); //totalAmount
            await goalMeter.AppendIntegerAsync(onlineFriends >= 20 ? 1 :
                onlineFriends >= 50 ? 2 :
                onlineFriends >= 80 ? 3 : 0);
            //communityHighestAchievedLevel
            await goalMeter.AppendIntegerAsync(0); //scoreRemainingUntilNextLevel
            await goalMeter.AppendIntegerAsync(0); //percentCompletionTowardsNextLevel
            await goalMeter.AppendStringAsync("friendshipChallenge"); //Type
            await goalMeter.AppendIntegerAsync(0); //unknown
            await goalMeter.AppendIntegerAsync(0); //ranks and loop
            await Session.SendMessage(goalMeter);
        }

        internal void RequestFloorItems()
        {
            return;
        }

        internal void RequestWallItems()
        {
            return;
        }

        internal async Task SaveBranding()
        {
            var itemId = Request.GetUInteger();
            var countBrand = Request.GetUInteger();

            if (Session?.GetHabbo() == null) return;
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true)) return;

            var item = room.GetRoomItemHandler().GetItem(Oblivion.GetGame().GetItemManager().GetRealId(itemId));
            if (item == null)
                return;

            var extraData = $"state{Convert.ToChar(9)}0";
            for (uint i = 1; i <= countBrand; i++)
            {
                var data = Request.GetString();

                extraData = $"{extraData}{Convert.ToChar(9)}{data}";
            }

            var strings = extraData.Split('\t');
            bool found = false;
            foreach (var str in strings)
            {
                if (str == "offsetZ")
                {
                    found = true;
                    continue;
                }

                if (found)
                {
                    if (!ushort.TryParse(str, out _))
                        return;
                    break;
                }
            }

            item.ExtraData = extraData;
            await room.GetRoomItemHandler()
                .SetFloorItem(Session, item, item.X, item.Y, item.Rot, false, false, true);
        }

        internal async Task OnRoomUserAdd()
        {
            if (Session == null || Response == null)
                return;
//            var queuedServerMessage = new QueuedServerMessage(Session.GetConnection());
            if (CurrentLoadingRoom?.GetRoomUserManager() == null ||
                CurrentLoadingRoom.GetRoomUserManager().UserList == null)
                return;
            var list =
                CurrentLoadingRoom.GetRoomUserManager()
                    .UserList.Values.Where(current => current != null && !current.IsSpectator);
            await Response.InitAsync(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
            Response.StartArray();
            foreach (var current2 in list)
                try
                {
                    if (!current2.Serialize(Response)) continue;
                    Response.SaveArray();
                }
                catch (Exception e)
                {
                    Writer.Writer.LogException(e.ToString());
                }

            Response.EndArray();
            await SendResponse();
            using (var msg = RoomFloorAndWallComposer(CurrentLoadingRoom))
            {
                await Session.SendMessage(msg);
            }

            await SendResponse();

            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomOwnershipMessageComposer"));
            await Response.AppendIntegerAsync(CurrentLoadingRoom.RoomId);
            Response.AppendBool(CurrentLoadingRoom.CheckRights(Session, true));
            await SendResponse();


            /* TODO CHECK */
            foreach (var habbo in CurrentLoadingRoom.UsersWithRights.Select(Oblivion.GetHabboById))
            {
                if (habbo == null) continue;

                await Response.InitAsync(LibraryParser.OutgoingRequest("GiveRoomRightsMessageComposer"));
                await Response.AppendIntegerAsync(CurrentLoadingRoom.RoomId);
                await Response.AppendIntegerAsync(habbo.Id);
                await Response.AppendStringAsync(habbo.UserName);
                await SendResponse();
            }

            var serverMessage = await CurrentLoadingRoom.GetRoomUserManager().SerializeStatusUpdates(true);
            if (serverMessage != null)
                await Session.SendMessage(serverMessage);

            if (CurrentLoadingRoom.RoomData.Event != null)
                await Oblivion.GetGame().GetRoomEvents().SerializeEventInfo(CurrentLoadingRoom.RoomId);

            CurrentLoadingRoom.JustLoaded = false;
            /* TODO CHECK */
            foreach (var current4 in CurrentLoadingRoom.GetRoomUserManager().UserList.Values)
            {
                if (current4 != null)
                {
                    if (current4.IsBot)
                    {
                        if (current4.BotData.DanceId > 0)
                        {
                            await Response.InitAsync(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                            await Response.AppendIntegerAsync(current4.VirtualId);
                            await Response.AppendIntegerAsync(current4.BotData.DanceId);
                            await SendResponse();
                        }
                    }
                    else if (current4.IsDancing)
                    {
                        await Response.InitAsync(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                        await Response.AppendIntegerAsync(current4.VirtualId);
                        await Response.AppendIntegerAsync(current4.DanceId);
                        await SendResponse();
                    }

                    if (current4.IsAsleep)
                    {
                        using var sleepMsg =
                            new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
                        await sleepMsg.AppendIntegerAsync(current4.VirtualId);
                        sleepMsg.AppendBool(true);
                        await Session.SendMessage(sleepMsg);
                    }

                    if (current4.CarryItemId > 0 && current4.CarryTimer > 0)
                    {
                        await Response.InitAsync(LibraryParser.OutgoingRequest("ApplyHanditemMessageComposer"));
                        await Response.AppendIntegerAsync(current4.VirtualId);
                        await Response.AppendIntegerAsync(current4.CarryTimer);
                        await SendResponse();
                    }

                    if (current4.IsBot) continue;
                    try
                    {
                        if (current4.GetClient() != null && current4.GetClient().GetHabbo() != null)
                        {
                            if (current4.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() != null &&
                                current4.CurrentEffect >= 1)
                            {
                                await Response.InitAsync(LibraryParser.OutgoingRequest("ApplyEffectMessageComposer"));
                                await Response.AppendIntegerAsync(current4.VirtualId);
                                await Response.AppendIntegerAsync(current4.CurrentEffect);
                                await Response.AppendIntegerAsync(0);
                                await SendResponse();
                            }

                            var serverMessage2 =
                                new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            await serverMessage2.AppendIntegerAsync(current4.VirtualId);
                            await serverMessage2.AppendStringAsync(current4.GetClient().GetHabbo().Look);
                            await serverMessage2.AppendStringAsync(current4.GetClient().GetHabbo().Gender.ToLower());
                            await serverMessage2.AppendStringAsync(current4.GetClient().GetHabbo().Motto);
                            await serverMessage2.AppendIntegerAsync(current4.GetClient().GetHabbo().AchievementPoints);
                            CurrentLoadingRoom?.SendMessage(serverMessage2);
                        }
                    }
                    catch (Exception pException)
                    {
                        Logging.HandleException(pException, "Rooms.SendRoomData3");
                    }
                }
            }
        }

        internal async Task EnterOnRoom()
        {
            if (Oblivion.ShutdownStarted) return;

            var id = Request.GetUInteger();
            var password = Request.GetString();
            await PrepareRoomForUser(id, password);
        }

        internal async Task PrepareRoomForUser(uint id, string pWd, bool isReload = false)
        {
            try
            {
                if (Session?.GetHabbo() == null || (Session.GetHabbo().LoadingRoom == id && !Session.IsAir))
                    return;

                if (Oblivion.ShutdownStarted)
                {
                    await Session.SendNotif(Oblivion.GetLanguage().GetVar("server_shutdown"));
                    return;
                }

                Session.GetHabbo().LoadingRoom = id;

                Room room;
                if (Session.GetHabbo().InRoom)
                {
                    room = Session.GetHabbo().CurrentRoom;
                    if (room?.GetRoomUserManager() != null)
                        await room.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
                }

                room = await Oblivion.GetGame().GetRoomManager().LoadRoom(id);
                if (room == null)
                    return;

                if (room.UserCount >= room.RoomData.UsersMax && !Session.GetHabbo().HasFuse("fuse_enter_full_rooms") &&
                    Session.GetHabbo().Id != (ulong)room.RoomData.OwnerId)
                {
                    /* var roomQueue = new ServerMessage(LibraryParser.OutgoingRequest("RoomsQueue"));
                    //todo: room queue
                     roomQueue.AppendInteger(2);
                     roomQueue.AppendString("visitors");
                     roomQueue.AppendInteger(2);
                     roomQueue.AppendInteger(1);
                     roomQueue.AppendString("visitors");
                     roomQueue.AppendInteger(room.UserCount -
                                             (int) room.RoomData.UsersNow); // Currently people are in the queue -1 ()
                     roomQueue.AppendString("spectators");
                     roomQueue.AppendInteger(1);
                     roomQueue.AppendInteger(1);
                     roomQueue.AppendString("spectators");
                     roomQueue.AppendInteger(0);

                     Session.SendMessage(roomQueue);
 */
//                    ClearRoomLoading();
//                    return;


                    await Response.InitAsync(LibraryParser.OutgoingRequest("RoomEnterErrorMessageComposer"));

                    await Response.AppendIntegerAsync(1);
                    await SendResponse();

                    await Response.InitAsync(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                    await SendResponse();

                    ClearRoomLoading();
                    return;
                }

                CurrentLoadingRoom = room;

/*
                if (Session.GetHabbo().Id != room.RoomData.OwnerId &&
                    !Session.GetHabbo().HasFuse("fuse_enter_any_room") &&
                    !(Session.GetHabbo().IsTeleporting && Session.GetHabbo().TeleportingRoomId == id))
                    if (Session.GetHabbo().LastBellId == room.RoomId && room.RoomData.State == 1)
                    {
                        return;
                    }*/

                if (!Session.GetHabbo().HasFuse("fuse_enter_any_room") && room.UserIsBanned(Session.GetHabbo().Id))
                {
                    if (!room.HasBanExpired(Session.GetHabbo().Id))
                    {
                        ClearRoomLoading();

                        var serverMessage2 =
                            new ServerMessage(LibraryParser.OutgoingRequest("RoomEnterErrorMessageComposer"));
                        await serverMessage2.AppendIntegerAsync(4);
                        await Session.SendMessage(serverMessage2);
                        await Response.InitAsync(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                        await SendResponse();
                        return;
                    }

                    await room.RemoveBan(Session.GetHabbo().Id);
                }

                if (!isReload && !Session.GetHabbo().HasFuse("fuse_enter_any_room") &&
                    !room.CheckRightsDoorBell(Session, true, true,
                        room.RoomData.Group != null &&
                        room.RoomData.Group.Members.ContainsKey(Session.GetHabbo().Id)) &&
                    !(Session.GetHabbo().IsTeleporting && Session.GetHabbo().TeleportingRoomId == id) &&
                    !Session.GetHabbo().IsHopping)
                {
                    if (room.RoomData.State == 1)
                    {
                        if (room.UserCount <= 0)
                        {
                            await Session.SendMessage(
                                new ServerMessage(LibraryParser.OutgoingRequest("DoorbellNoOneMessageComposer")));
                            return;
                        }

                        Session.GetHabbo().LastBellId = room.RoomId;
                        var msg = new ServerMessage(LibraryParser.OutgoingRequest("DoorbellMessageComposer"));
                        await msg.AppendStringAsync("");
                        await Session.SendMessage(msg);
                        var serverMessage3 =
                            new ServerMessage(LibraryParser.OutgoingRequest("DoorbellMessageComposer"));
                        await serverMessage3.AppendStringAsync(Session.GetHabbo().UserName);
                        await room.SendMessageToUsersWithRights(serverMessage3);
                        return;
                    }

                    if (room.RoomData.State == 2 &&
                        !string.Equals(pWd, room.RoomData.PassWord, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ClearRoomLoading();

                        await Session.GetMessageHandler()
                            .GetResponse()
                            .InitAsync(LibraryParser.OutgoingRequest("RoomErrorMessageComposer"));
                        await Session.GetMessageHandler().GetResponse().AppendIntegerAsync(-100002);
                        await Session.GetMessageHandler().SendResponse();

                        await Session.GetMessageHandler()
                            .GetResponse()
                            .InitAsync(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                        Session.GetMessageHandler().GetResponse();
                        await Session.GetMessageHandler().SendResponse();
                        return;
                    }
                }

                await Response.InitAsync(LibraryParser.OutgoingRequest("PrepareRoomMessageComposer"));
                await SendResponse();
                Session.GetHabbo().LoadingChecksPassed = true;
                await LoadRoomForUser();


                if (Session.GetHabbo().RecentlyVisitedRooms.Contains(room.RoomId))
                    Session.GetHabbo().RecentlyVisitedRooms.Remove(room.RoomId);
                Session.GetHabbo().RecentlyVisitedRooms.AddFirst(room.RoomId);
            }
            catch (Exception e)
            {
                Writer.Writer.LogException("PrepareRoomForUser. RoomId: " + id + "; UserId: " +
                                           (Session?.GetHabbo().Id.ToString(CultureInfo.InvariantCulture) ?? "null") +
                                           Environment.NewLine + e);
            }
        }

        internal async Task ReqLoadRoomForUser()
        {
            await LoadRoomForUser();
        }

        internal async Task LoadRoomForUser()
        {
            var currentLoadingRoom = CurrentLoadingRoom;

//            var queuedServerMessage = new QueuedServerMessage(Session.GetConnection());
            if (currentLoadingRoom == null || !Session.GetHabbo().LoadingChecksPassed) return;
            if (Session.GetHabbo().FavouriteGroup > 0u)
            {
                if (CurrentLoadingRoom.RoomData.Group != null &&
                    !CurrentLoadingRoom.LoadedGroups.ContainsKey(CurrentLoadingRoom.RoomData.Group.Id))
                    CurrentLoadingRoom.LoadedGroups.Add(CurrentLoadingRoom.RoomData.Group.Id,
                        CurrentLoadingRoom.RoomData.Group.Badge);
                if (!CurrentLoadingRoom.LoadedGroups.ContainsKey(Session.GetHabbo().FavouriteGroup) &&
                    Oblivion.GetGame().GetGroupManager().GetGroup(Session.GetHabbo().FavouriteGroup) != null)
                    CurrentLoadingRoom.LoadedGroups.Add(Session.GetHabbo().FavouriteGroup,
                        Oblivion.GetGame().GetGroupManager().GetGroup(Session.GetHabbo().FavouriteGroup).Badge);
            }

            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
            await Response.AppendIntegerAsync(CurrentLoadingRoom.LoadedGroups.Count);
            /* TODO CHECK */
            foreach (var guild1 in CurrentLoadingRoom.LoadedGroups.ToList())
            {
                await Response.AppendIntegerAsync(guild1.Key);
                await Response.AppendStringAsync(guild1.Value);
            }

            await SendResponse();

            await Response.InitAsync(LibraryParser.OutgoingRequest("InitialRoomInfoMessageComposer"));
            await Response.AppendStringAsync(currentLoadingRoom.RoomData.ModelName);
            await Response.AppendIntegerAsync(currentLoadingRoom.RoomId);
            await SendResponse();
            if (Session.GetHabbo().SpectatorMode)
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("SpectatorModeMessageComposer"));
                await SendResponse();
            }

            if (currentLoadingRoom.RoomData.WallPaper != "0.0")
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
                await Response.AppendStringAsync("wallpaper");
                await Response.AppendStringAsync(currentLoadingRoom.RoomData.WallPaper);
                await SendResponse();
            }

            if (currentLoadingRoom.RoomData.Floor != "0.0")
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
                await Response.AppendStringAsync("floor");
                await Response.AppendStringAsync(currentLoadingRoom.RoomData.Floor);
                await SendResponse();
            }

            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
            await Response.AppendStringAsync("landscape");
            await Response.AppendStringAsync(currentLoadingRoom.RoomData.LandScape);
            await SendResponse();


            if (Session?.GetHabbo()?.RatedRooms != null)
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRatingMessageComposer"));
                await Response.AppendIntegerAsync(currentLoadingRoom.RoomData.Score);
                Response.AppendBool(!Session.GetHabbo().RatedRooms.Contains(currentLoadingRoom.RoomId) &&
                                    !currentLoadingRoom.CheckRights(Session, true));
                await SendResponse();
            }

            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomUpdateMessageComposer"));
            await Response.AppendIntegerAsync(currentLoadingRoom.RoomId);

            await SendResponse();
        }

        internal void ClearRoomLoading()
        {
            if (Session?.GetHabbo() == null)
                return;
            Session.GetHabbo().LoadingRoom = 0u;
            Session.GetHabbo().LoadingChecksPassed = false;
        }

        internal async Task Move()
        {
            if (Session?.GetHabbo() == null)
                return;
            var currentRoom = Session.GetHabbo().CurrentRoom;

            var roomUserByHabbo = currentRoom?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null || !roomUserByHabbo.CanWalk)
                return;


            var targetX = Request.GetInteger();
            var targetY = Request.GetInteger();

            await roomUserByHabbo.MoveTo(targetX, targetY);

            if (!roomUserByHabbo.RidingHorse)
                return;

            var roomUserByVirtualId = currentRoom.GetRoomUserManager()
                .GetRoomUserByVirtualId((int)roomUserByHabbo.HorseId);


            await roomUserByVirtualId.MoveTo(targetX, targetY);
        }

        internal async Task CanCreateRoom()
        {
            if (Session?.GetHabbo() == null)
                return;
            await Response.InitAsync(LibraryParser.OutgoingRequest("CanCreateRoomMessageComposer"));
            await Response.AppendIntegerAsync(Session.GetHabbo().Data.Rooms.Count >= 75 ? 1 : 0);
            await Response.AppendIntegerAsync(75);
            await SendResponse();
        }

        internal async Task CreateRoom()
        {
            if (Session?.GetHabbo()?.Data?.Rooms == null)
                return;
            if (Session.GetHabbo().Data.Rooms.Count >= 75)
            {
                await Session.SendNotif(Oblivion.GetLanguage().GetVar("user_has_more_then_75_rooms"));
                return;
            }

            if (Oblivion.GetUnixTimeStamp() - Session.GetHabbo().LastSqlQuery < 20)
            {
                await Session.SendNotif(Oblivion.GetLanguage().GetVar("user_create_room_flood_error"));
                return;
            }

            var name = Request.GetString();
            var description = Request.GetString();
            var roomModel = Request.GetString();
            var category = Request.GetInteger();
            var maxVisitors = Request.GetInteger();
            var tradeState = Request.GetInteger();

            var data = await Oblivion.GetGame()
                .GetRoomManager()
                .CreateRoom(Session, name, description, roomModel, category, maxVisitors, tradeState);
            if (data == null)
                return;

            Session.GetHabbo().LastSqlQuery = Oblivion.GetUnixTimeStamp();
            await Response.InitAsync(LibraryParser.OutgoingRequest("OnCreateRoomInfoMessageComposer"));
            await Response.AppendIntegerAsync(data.Id);
            await Response.AppendStringAsync(data.Name);
            await SendResponse();
        }

        internal async Task GetRoomEditData()
        {
            if (Session?.GetHabbo() == null)
                return;
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(Convert.ToUInt32(Request.GetInteger()));
            if (room?.RoomData == null)
                return;

            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("RoomSettingsDataMessageComposer"));
            await GetResponse().AppendIntegerAsync(room.RoomId);
            await GetResponse().AppendStringAsync(room.RoomData.Name);
            await GetResponse().AppendStringAsync(room.RoomData.Description);
            await GetResponse().AppendIntegerAsync(room.RoomData.State);
            await GetResponse().AppendIntegerAsync(room.RoomData.Category);
            await GetResponse().AppendIntegerAsync(room.RoomData.UsersMax);
            await GetResponse()
                .AppendIntegerAsync(room.RoomData.Model.MapSizeX * room.RoomData.Model.MapSizeY > 200 ? 50 : 25);

            await GetResponse().AppendIntegerAsync(room.TagCount);
            foreach (var s in room.RoomData.Tags)
                await GetResponse().AppendStringAsync(s);
            await GetResponse().AppendIntegerAsync(room.RoomData.TradeState);
            await GetResponse().AppendIntegerAsync(room.RoomData.AllowPets);
            await GetResponse().AppendIntegerAsync(room.RoomData.AllowPetsEating);
            await GetResponse().AppendIntegerAsync(room.RoomData.AllowWalkThrough);
            await GetResponse().AppendIntegerAsync(room.RoomData.HideWall);
            await GetResponse().AppendIntegerAsync(room.RoomData.WallThickness);
            await GetResponse().AppendIntegerAsync(room.RoomData.FloorThickness);
            await GetResponse().AppendIntegerAsync(room.RoomData.ChatType);
            await GetResponse().AppendIntegerAsync(room.RoomData.ChatBalloon);
            await GetResponse().AppendIntegerAsync(room.RoomData.ChatSpeed);
            await GetResponse().AppendIntegerAsync(room.RoomData.ChatMaxDistance);
            await GetResponse()
                .AppendIntegerAsync(room.RoomData.ChatFloodProtection > 2 ? 2 : room.RoomData.ChatFloodProtection);
            GetResponse().AppendBool(false); //allow_dyncats_checkbox
            await GetResponse().AppendIntegerAsync(room.RoomData.WhoCanMute);
            await GetResponse().AppendIntegerAsync(room.RoomData.WhoCanKick);
            await GetResponse().AppendIntegerAsync(room.RoomData.WhoCanBan);
            await SendResponse();
        }

        internal async Task RoomSettingsOkComposer(uint roomId)
        {
            if (Session?.GetHabbo() == null)
                return;
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("RoomSettingsSavedMessageComposer"));
            await GetResponse().AppendIntegerAsync(roomId);
            await SendResponse();
        }

        internal async Task RoomUpdatedOkComposer(uint roomId)
        {
            if (Session?.GetHabbo() == null)
                return;
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("RoomUpdateMessageComposer"));
            await GetResponse().AppendIntegerAsync(roomId);
            await SendResponse();
        }

        internal static ServerMessage RoomFloorAndWallComposer(Room room)
        {
            if (room?.RoomData == null) return null;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RoomFloorWallLevelsMessageComposer"));
            serverMessage.AppendBool(room.RoomData.HideWall);
            serverMessage.AppendInteger(room.RoomData.WallThickness);
            serverMessage.AppendInteger(room.RoomData.FloorThickness);
            return serverMessage;
        }

        internal static ServerMessage SerializeRoomChatOption(Room room)
        {
            if (room?.RoomData == null) return null;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RoomChatOptionsMessageComposer"));
            serverMessage.AppendInteger(room.RoomData.ChatType);
            serverMessage.AppendInteger(room.RoomData.ChatBalloon);
            serverMessage.AppendInteger(room.RoomData.ChatSpeed);
            serverMessage.AppendInteger(room.RoomData.ChatMaxDistance);
            serverMessage.AppendInteger(room.RoomData.ChatFloodProtection);
            return serverMessage;
        }

        internal async Task ParseRoomDataInformation()
        {
            if (Session?.GetHabbo() == null)
                return;
            var id = Request.GetUInteger();
            var num = Request.GetInteger();
            var num2 = Request.GetInteger();
            var room = await Oblivion.GetGame().GetRoomManager().LoadRoom(id);
            if (room == null) return;
            if (num == 0 && num2 == 1)
            {
                await SerializeRoomInformation(room, false);
                return;
            }

            await SerializeRoomInformation(room, true);
        }

        internal async Task SerializeRoomInformation(Room room, bool show)
        {
            if (room?.RoomData == null)
                return;

            if (Session?.GetHabbo() == null)
                return;

            await room.RoomData.SerializeRoomData(Response, Session, true, false, show);
            await SendResponse();

            if (room.UsersWithRights == null) return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("LoadRoomRightsListMessageComposer"));
            await Response.AppendIntegerAsync(room.RoomData.Id);

            Response.StartArray();
            foreach (var id in room.UsersWithRights)
            {
                var habboForId = Oblivion.GetHabboById(id);

                if (habboForId == null) continue;

                await Response.AppendIntegerAsync(habboForId.Id);
                await Response.AppendStringAsync(habboForId.UserName);
                Response.SaveArray();
            }

            Response.EndArray();

            await SendResponse();
        }

        internal async Task SaveRoomData()
        {
            if (Session?.GetHabbo() == null)
                return;
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;
            Request.GetInteger();

            var oldName = room.RoomData.Name;
            room.RoomData.Name = Request.GetString();
            if (room.RoomData.Name.Length < 3)
            {
                room.RoomData.Name = oldName;
                return;
            }

            room.RoomData.Description = Request.GetString();
            room.RoomData.State = Request.GetInteger();
            if (room.RoomData.State < 0 || room.RoomData.State > 2)
            {
                room.RoomData.State = 0;
                return;
            }

            room.RoomData.PassWord = Request.GetString();
            room.RoomData.UsersMax = Request.GetUInteger();
            room.RoomData.Category = Request.GetInteger();
            var tagCount = Request.GetUInteger();

            if (tagCount > 2) return;
            var tags = new List<string>();

            for (var i = 0; i < tagCount; i++)
                tags.Add(Request.GetString().ToLower());

            room.RoomData.TradeState = Request.GetInteger();
            room.RoomData.AllowPets = Request.GetBool();
            room.RoomData.AllowPetsEating = Request.GetBool();
            room.RoomData.AllowWalkThrough = Request.GetBool();
            room.RoomData.HideWall = Request.GetBool();
            room.RoomData.WallThickness = Request.GetInteger();
            if (room.RoomData.WallThickness < -2 || room.RoomData.WallThickness > 1) room.RoomData.WallThickness = 0;

            room.RoomData.FloorThickness = Request.GetInteger();
            if (room.RoomData.FloorThickness < -2 || room.RoomData.FloorThickness > 1) room.RoomData.FloorThickness = 0;

            room.RoomData.WhoCanMute = Request.GetInteger();
            room.RoomData.WhoCanKick = Request.GetInteger();
            room.RoomData.WhoCanBan = Request.GetInteger();
            room.RoomData.ChatType = Request.GetInteger();
            room.RoomData.ChatBalloon = Request.GetUInteger();
            room.RoomData.ChatSpeed = Request.GetUInteger();
            room.RoomData.ChatMaxDistance = Request.GetUInteger();
            if (room.RoomData.ChatMaxDistance > 90) room.RoomData.ChatMaxDistance = 90;

            room.RoomData.ChatFloodProtection = Request.GetUInteger(); //chat_flood_sensitivity
            if (room.RoomData.ChatFloodProtection > 2) room.RoomData.ChatFloodProtection = 2;

            var flatCat = Oblivion.GetGame().GetNavigator().GetFlatCat(room.RoomData.Category);
            if (flatCat == null || flatCat.MinRank > Session.GetHabbo().Rank) room.RoomData.Category = 0;

            room.ClearTags();
            room.AddTagRange(tags);

            await RoomSettingsOkComposer(room.RoomId);
            await RoomUpdatedOkComposer(room.RoomId);
            await Session.GetHabbo().CurrentRoom.SendMessage(RoomFloorAndWallComposer(room));
            await Session.GetHabbo().CurrentRoom.SendMessage(SerializeRoomChatOption(room));
            await room.RoomData.SerializeRoomData(Response, Session, false, true);
            await Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModWalkthroughSeen", 1);
            await Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModChatScrollSpeedSeen", 1);
            await Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModChatFloodFilterSeen", 1);
            await Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModChatHearRangeSeen", 1);
        }


        internal async Task GetBannedUsers()
        {
            if (Session?.GetHabbo() == null)
                return;

            var num = Request.GetUInteger();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(num);
            if (room == null)
                return;
            var list = room.BannedUsers();
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("RoomBannedListMessageComposer"));
            await msg.AppendIntegerAsync(num);
            var count = list.Count;
            if (count <= 0)
            {
                count = 1;
            }

            await msg.AppendIntegerAsync(count);
            if (list.Count <= 0)
            {
                await msg.AppendIntegerAsync(-1);
                await msg.AppendStringAsync("");
            }
            else
                foreach (var current in list)
                {
                    await msg.AppendIntegerAsync(current);
                    await msg.AppendStringAsync(Oblivion.GetHabboById(current) != null
                        ? Oblivion.GetHabboById(current).UserName
                        : "Undefined");
                }

            await Session.SendMessage(msg);
        }

        internal async Task UsersWithRights()
        {
            if (Session?.GetHabbo()?.CurrentRoom == null)
                return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("LoadRoomRightsListMessageComposer"));
            await Response.AppendIntegerAsync(Session.GetHabbo().CurrentRoom.RoomId);
            await Response.AppendIntegerAsync(Session.GetHabbo().CurrentRoom.UsersWithRights.Count);
            /* TODO CHECK */
            foreach (var current in Session.GetHabbo().CurrentRoom.UsersWithRights)
            {
                var habboForId = Oblivion.GetHabboById(current);
                if (habboForId == null) continue;
                await Response.AppendIntegerAsync(current);
                await Response.AppendStringAsync(habboForId.Look);
            }

            await SendResponse();
        }

        internal async Task UnbanUser()
        {
            if (Session?.GetHabbo() == null)
                return;
            var num = Request.GetUInteger();
            var num2 = Request.GetUInteger();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(num2);
            if (room == null)
                return;
            if (num <= 0) return;
            await room.Unban(num);
            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomUnbanUserMessageComposer"));
            await Response.AppendIntegerAsync(num2);
            await Response.AppendIntegerAsync(num);
            await SendResponse();
        }

        internal async Task GiveRights()
        {
            if (Session?.GetHabbo() == null)
                return;
            var num = Request.GetUInteger();
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null)
                return;
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(num);
            if (!room.CheckRights(Session, true))
                return;
            if (room.UsersWithRights.Contains(num))
            {
                await Session.SendNotif(Oblivion.GetLanguage().GetVar("no_room_rights_error"));
                return;
            }

            if (num == 0)
                return;
            room.UsersWithRights.Add(num);
            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await queryReactor.RunFastQueryAsync(string.Concat(
                    "INSERT INTO rooms_rights (room_id,user_id) VALUES (",
                    room.RoomId, ",", num, ")"));
            }

            if (roomUserByHabbo != null && !roomUserByHabbo.IsBot)
            {
                if (!roomUserByHabbo.IsBot)
                {
                    roomUserByHabbo.AddStatus("flatctrl 1", "");
                    await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                    await Response.AppendIntegerAsync(1);
                    await roomUserByHabbo.GetClient().SendMessage(GetResponse());
                }

                await Response.InitAsync(LibraryParser.OutgoingRequest("GiveRoomRightsMessageComposer"));
                await Response.AppendIntegerAsync(room.RoomId);
                await Response.AppendIntegerAsync(roomUserByHabbo.GetClient().GetHabbo().Id);
                await Response.AppendStringAsync(roomUserByHabbo.GetClient().GetHabbo().UserName);
                await SendResponse();
                roomUserByHabbo.UpdateNeeded = true;
            }

            await UsersWithRights();
        }

        internal async Task TakeRights()
        {
            if (Session?.GetHabbo() == null)
                return;
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;
            var stringBuilder = new StringBuilder();
            var num = Request.GetInteger();

            {
                for (var i = 0; i < num; i++)
                {
                    if (i > 0)
                        stringBuilder.Append(" OR ");
                    var num2 = Request.GetUInteger();
                    if (room.UsersWithRights.Contains(num2))
                        room.UsersWithRights.Remove(num2);
                    stringBuilder.Append(string.Concat("room_id = '", room.RoomId, "' AND user_id = '", num2, "'"));
                    var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(num2);
                    if (roomUserByHabbo != null && !roomUserByHabbo.IsBot)
                    {
                        await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        await Response.AppendIntegerAsync(0);
                        await roomUserByHabbo.GetClient().SendMessage(GetResponse());
                        roomUserByHabbo.RemoveStatus("flatctrl 1");
                        roomUserByHabbo.UpdateNeeded = true;
                    }

                    await Response.InitAsync(LibraryParser.OutgoingRequest("RemoveRightsMessageComposer"));
                    await Response.AppendIntegerAsync(room.RoomId);
                    await Response.AppendIntegerAsync(num2);
                    await SendResponse();
                }

                await UsersWithRights();
                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    await queryReactor.RunFastQueryAsync($"DELETE FROM rooms_rights WHERE {stringBuilder}");
                }
            }
        }

        internal async Task TakeAllRights()
        {
            if (Session?.GetHabbo() == null)
                return;
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;

            /* TODO CHECK */
            foreach (var num in room.UsersWithRights)
            {
                var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(num);
                await Response.InitAsync(LibraryParser.OutgoingRequest("RemoveRightsMessageComposer"));
                await Response.AppendIntegerAsync(room.RoomId);
                await Response.AppendIntegerAsync(num);
                await SendResponse();
                if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
                    continue;
                await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                await Response.AppendIntegerAsync(0);
                await roomUserByHabbo.GetClient().SendMessage(Response);
                roomUserByHabbo.RemoveStatus("flatctrl 1");
                roomUserByHabbo.UpdateNeeded = true;
            }

            using (var queryreactor2 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await queryreactor2.RunFastQueryAsync($"DELETE FROM rooms_rights WHERE room_id = {room.RoomId}");
            }

            room.UsersWithRights.Clear();
            await UsersWithRights();
        }

        internal async Task KickUser()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null)
                return;
            if (!room.CheckRights(Session) && room.RoomData.WhoCanKick != 2
                                           && Session.GetHabbo().Rank <
                                           Convert.ToUInt32(Oblivion.GetDbConfig().DbData["ambassador.minrank"]))
                return;
            var pId = Request.GetUInteger();
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(pId);
            if (roomUserByHabbo?.GetClient()?.GetHabbo() == null || roomUserByHabbo.IsBot)
                return;
            if (room.CheckRights(roomUserByHabbo.GetClient(), true) ||
                roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_mod") ||
                roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_no_kick"))
                return;
            await room.GetRoomUserManager().RemoveUserFromRoom(roomUserByHabbo, true, true);
            roomUserByHabbo.GetClient().CurrentRoomUserId = -1;
            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModKickSeen", 1);
        }

        internal async Task BanUser()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || room.RoomData.WhoCanBan == 0 && !room.CheckRights(Session, true) ||
                room.RoomData.WhoCanBan == 1 && !room.CheckRights(Session))
                return;
            var num = Request.GetInteger();
            Request.GetUInteger();
            var text = Request.GetString();
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToUInt32(num));
            if (roomUserByHabbo?.GetClient()?.GetHabbo() == null || roomUserByHabbo.IsBot)
                return;
            if (roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_mod") ||
                roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_no_kick"))
                return;
            var time = 0L;
            if (text.ToLower().Contains("hour"))
                time = 3600L;
            else if (text.ToLower().Contains("day"))
                time = 86400L;
            else if (text.ToLower().Contains("perm"))
                time = 788922000L;
            await room.AddBan(num, time);
            await room.GetRoomUserManager().RemoveUserFromRoom(roomUserByHabbo, true, true);
            Session.CurrentRoomUserId = -1;
            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModBanSeen", 1);
        }

        internal async Task SetHomeRoom()
        {
            var roomId = Request.GetUInteger();
            var data = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);

            if (roomId != 0 && data != null)
            {
                Session.GetHabbo().HomeRoom = roomId
                    ;
                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    await queryReactor.RunFastQueryAsync(string.Concat("UPDATE users SET home_room = ", roomId,
                        " WHERE id = ",
                        Session.GetHabbo().Id));
                }

                await Response.InitAsync(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));
                await Response.AppendIntegerAsync(roomId);
                await Response.AppendIntegerAsync(0);
                await SendResponse();
            }
        }

        internal async Task DeleteRoom()
        {
            var roomId = Request.GetUInteger();
            if (Session?.GetHabbo() == null || Session.GetHabbo().Data.Rooms == null)
                return;

            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);

            if (room?.RoomData == null)
                return;

            if (room.RoomData.Owner != Session.GetHabbo().UserName && Session.GetHabbo().Rank <= 6u)
                return;

            await room.GetRoomItemHandler().RemoveAllFurniture(Session);

            var roomData = room.RoomData;
            await Oblivion.GetGame().GetRoomManager().UnloadRoom(room, "Delete room");

            if (roomData == null || Session == null)
                return;
            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await queryReactor.RunFastQueryAsync($"DELETE FROM rooms_data WHERE id = {roomId}");
                await queryReactor.RunFastQueryAsync($"DELETE FROM users_favorites WHERE room_id = {roomId}");
                await queryReactor.RunNoLockFastQueryAsync($"DELETE FROM items_rooms WHERE room_id = '{roomId}';");
                await queryReactor.RunFastQueryAsync($"DELETE FROM rooms_rights WHERE room_id = {roomId}");
                await queryReactor.RunFastQueryAsync($"UPDATE users SET home_room = '0' WHERE home_room = {roomId}");
            }

            if (Session.GetHabbo().Rank > 5u && Session.GetHabbo().UserName != roomData.Owner)
                await Oblivion.GetGame()
                    .GetModerationTool()
                    .LogStaffEntry(Session.GetHabbo().UserName, roomData.Name, "Room deletion",
                        $"Deleted room ID {roomData.Id}");

            Session.GetHabbo().Data.Rooms.Remove(roomId);
        }

        internal async Task AirClickUser()
        {
            var userId = Request.GetUInteger();
            var habbo = Oblivion.GetHabboById(userId);
            if (habbo == null) return;
            var createTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(habbo.CreateDate);

            var msg = new ServerMessage(LibraryParser.OutgoingRequest("UserProfileMessageComposer"));
            await msg.AppendIntegerAsync(habbo.Id);
            await msg.AppendStringAsync(habbo.UserName);
            await msg.AppendStringAsync(habbo.Look);
            await msg.AppendStringAsync(habbo.Motto);
            await msg.AppendStringAsync(createTime.ToString("dd/MM/yyyy"));
            await msg.AppendIntegerAsync(habbo.AchievementPoints);
            await msg.AppendIntegerAsync(GetFriendsCount(userId));
            msg.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                           Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id));
            msg.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                           !Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id) &&
                           Session.GetHabbo().GetMessenger().RequestExists(habbo.Id));
            msg.AppendBool(Oblivion.GetGame().GetClientManager().GetClientByUserId(habbo.Id) != null);
            var groups = habbo.UserGroups;
            await msg.AppendIntegerAsync(groups.Count);
            /* TODO CHECK */
            foreach (var groupUs in groups)
            {
                var group = Oblivion.GetGame()
                    .GetGroupManager()
                    .GetGroup(groupUs.GroupId);
                if (group != null)
                {
                    await msg.AppendIntegerAsync(group.Id);
                    await msg.AppendStringAsync(group.Name);
                    await msg.AppendStringAsync(group.Badge);
                    await msg.AppendStringAsync(
                        Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour1, true));
                    await msg.AppendStringAsync(Oblivion.GetGame().GetGroupManager()
                        .GetGroupColour(group.Colour2, false));
                    msg.AppendBool(group.Id == habbo.FavouriteGroup);
                    await msg.AppendIntegerAsync(-1);
                    msg.AppendBool(group.HasForum);
                }
                else
                {
                    await msg.AppendIntegerAsync(1);
                    await msg.AppendStringAsync("THIS GROUP IS INVALID");
                    await msg.AppendStringAsync("");
                    await msg.AppendStringAsync("");
                    await msg.AppendStringAsync("");
                    msg.AppendBool(false);
                    await msg.AppendIntegerAsync(-1);
                    msg.AppendBool(false);
                }
            }

            if (habbo.PreviousOnline == 0)
                await msg.AppendIntegerAsync(-1);
            else if (Oblivion.GetGame().GetClientManager().GetClientByUserId(habbo.Id) == null)
                await msg.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - habbo.PreviousOnline));
            else
                await msg.AppendIntegerAsync((Oblivion.GetUnixTimeStamp() - habbo.LastOnline));

            msg.AppendBool(true);
            await Session.SendMessage(msg);

            if (habbo.GetBadgeComponent()?.BadgeList == null) return;

            var msg2 = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            await msg2.AppendIntegerAsync(habbo.Id);


            msg2.StartArray();
            foreach (var badge in habbo.GetBadgeComponent().BadgeList.Values)
            {
                if (badge.Slot <= 0) continue;

                await msg2.AppendIntegerAsync(badge.Slot);
                await msg2.AppendStringAsync(badge.Code);

                msg2.SaveArray();
            }

            msg2.EndArray();

            await Session.SendMessage(msg2);
        }

        internal async Task LookAt()
        {
            if (Session?.GetHabbo() == null) return;

            var room = Session.GetHabbo().CurrentRoom;

            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            await roomUserByHabbo.UnIdle();
            var x = Request.GetInteger();
            var y = Request.GetInteger();

            var rotation = Rotation.Calculate(roomUserByHabbo.X, roomUserByHabbo.Y, x, y);
            roomUserByHabbo.SetRot(rotation, false);
            roomUserByHabbo.UpdateNeeded = true;

            if (!roomUserByHabbo.RidingHorse)
                return;
            var roomUserByVirtualId =
                Session.GetHabbo()
                    .CurrentRoom.GetRoomUserManager()
                    .GetRoomUserByVirtualId(Convert.ToInt32(roomUserByHabbo.HorseId));
            roomUserByVirtualId.SetRot(rotation, false);
            roomUserByVirtualId.UpdateNeeded = true;
        }

        internal async Task StartTyping()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TypingStatusMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await serverMessage.AppendIntegerAsync(1);
            await room.SendMessage(serverMessage);
        }

        internal async Task StopTyping()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TypingStatusMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await serverMessage.AppendIntegerAsync(0);
            await room.SendMessage(serverMessage);
        }

        internal async Task IgnoreUser()
        {
            if (Session?.GetHabbo()?.CurrentRoom == null || Session?.GetHabbo()?.Data?.Ignores == null)
                return;

            var text = Request.GetString();
            var habbo = Oblivion.GetGame().GetClientManager().GetClientByUserName(text)?.GetHabbo();
            if (habbo == null)
                return;
            if (Session.GetHabbo().Data.Ignores.Contains(habbo.Id) || habbo.Rank > 4u)
                return;
            Session.GetHabbo().Data.Ignores.Add(habbo.Id);
            await Response.InitAsync(LibraryParser.OutgoingRequest("UpdateIgnoreStatusMessageComposer"));
            await Response.AppendIntegerAsync(1);
            await Response.AppendStringAsync(text);
            await SendResponse();
            await Oblivion.GetGame().GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModIgnoreSeen", 1);
        }

        internal async Task UnignoreUser()
        {
            if (Session.GetHabbo().CurrentRoom == null)
                return;
            var text = Request.GetString();
            var habbo = Oblivion.GetGame().GetClientManager().GetClientByUserName(text).GetHabbo();
            if (habbo == null)
                return;
            if (!Session.GetHabbo().Data.Ignores.Contains(habbo.Id))
                return;
            Session.GetHabbo().Data.Ignores.Remove(habbo.Id);
            await Response.InitAsync(LibraryParser.OutgoingRequest("UpdateIgnoreStatusMessageComposer"));
            await Response.AppendIntegerAsync(3);
            await Response.AppendStringAsync(text);
            await SendResponse();
        }

        internal async Task CanCreateRoomEvent()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;
            var b = true;
            var i = 0;
            if (room.RoomData.State != 0)
            {
                b = false;
                i = 3;
            }

            Response.AppendBool(b);
            await Response.AppendIntegerAsync(i);
        }

        internal async Task Sign()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            await roomUserByHabbo.UnIdle();
            var value = Request.GetInteger();
            roomUserByHabbo.AddStatus("sign", Convert.ToString(value));
            roomUserByHabbo.UpdateNeeded = true;
            roomUserByHabbo.SignTime = Oblivion.GetUnixTimeStamp() + 5;
        }

        internal void InitRoomGroupBadges()
        {
            //todo
            Oblivion.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().LoadingRoom);
            return;
        }

        internal async Task RateRoom()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || Session.GetHabbo().RatedRooms.Contains(room.RoomId) ||
                room.CheckRights(Session, true))
                return;

            {
                switch (Request.GetInteger())
                {
                    case -1:
                        room.RoomData.Score--;
                        room.RoomData.Score--;
                        break;

                    case 0:
                        return;

                    case 1:
                        room.RoomData.Score++;
                        room.RoomData.Score++;
                        break;

                    default:
                        return;
                }

                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    await queryReactor.RunFastQueryAsync(string.Concat("UPDATE rooms_data SET score = ",
                        room.RoomData.Score,
                        " WHERE id = ", room.RoomId));
                }

                Session.GetHabbo().RatedRooms.Add(room.RoomId);
                await Response.InitAsync(LibraryParser.OutgoingRequest("RoomRatingMessageComposer"));
                await Response.AppendIntegerAsync(room.RoomData.Score);
                Response.AppendBool(room.CheckRights(Session, true));
                await SendResponse();
            }
        }

        internal async Task Dance()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            await roomUserByHabbo.UnIdle();
            var num = Request.GetInteger();
            if (num < 0 || num > 4)
                num = 0;
            if (num > 0 && roomUserByHabbo.CarryItemId > 0)
                await roomUserByHabbo.CarryItem(0);
            roomUserByHabbo.DanceId = num;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await serverMessage.AppendIntegerAsync(num);
            await room.SendMessage(serverMessage);
            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialDance);
            if (room.GetRoomUserManager().GetRoomUsers().Count > 19)
                await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.MassDance);
        }

        internal async Task AnswerDoorbell()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session))
                return;
            var userName = Request.GetString();
            var flag = Request.GetBool();
            var clientByUserName = Oblivion.GetGame().GetClientManager().GetClientByUserName(userName);
            if (clientByUserName?.GetHabbo() == null)
                return;
            if (clientByUserName.GetHabbo().LastBellId != room.RoomId)
                return;

            clientByUserName.GetHabbo().LastBellId = 0;

            if (flag)
            {
                clientByUserName.GetHabbo().LoadingChecksPassed = true;
                clientByUserName.GetHabbo().LoadingRoom = room.RoomId;
                await clientByUserName.GetMessageHandler()
                    .Response.InitAsync(LibraryParser.OutgoingRequest("DoorbellOpenedMessageComposer"));
                await clientByUserName.GetMessageHandler().Response.AppendStringAsync("");
                await clientByUserName.GetMessageHandler().SendResponse();
                return;
            }

            if (clientByUserName.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
            {
                await clientByUserName.GetMessageHandler()
                    .Response.InitAsync(LibraryParser.OutgoingRequest("DoorbellNoOneMessageComposer"));
                await clientByUserName.GetMessageHandler().Response.AppendStringAsync("");
                await clientByUserName.GetMessageHandler().SendResponse();
            }
        }

        internal async Task AlterRoomFilter()
        {
            var num = Request.GetUInteger();
            var flag = Request.GetBool();
            var text = Request.GetString();
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;
            if (!flag)
            {
                if (!room.RoomData.WordFilter.Contains(text))
                    return;
                room.RoomData.WordFilter.Remove(text);
                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    queryReactor.SetQuery("DELETE FROM rooms_wordfilter WHERE room_id = @id AND word = @word");
                    queryReactor.AddParameter("id", num);
                    queryReactor.AddParameter("word", text);
                    await queryReactor.RunQueryAsync();
                    return;
                }
            }

            if (room.RoomData.WordFilter.Contains(text))
                return;
            if (text.Contains("+"))
            {
                await Session.SendNotif(Oblivion.GetLanguage().GetVar("character_error_plus"));
                return;
            }

            room.RoomData.WordFilter.Add(text);
            using (var queryreactor2 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryreactor2.SetQuery("INSERT INTO rooms_wordfilter (room_id, word) VALUES (@id, @word);");
                queryreactor2.AddParameter("id", num);
                queryreactor2.AddParameter("word", text);
                await queryreactor2.RunQueryAsync();
            }
        }

        internal async Task GetRoomFilter()
        {
            var roomId = Request.GetUInteger();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);
            if (room == null || !room.CheckRights(Session, true))
                return;

            var serverMessage = Response;

            await serverMessage.InitAsync(LibraryParser.OutgoingRequest("RoomLoadFilterMessageComposer"));
            await serverMessage.AppendIntegerAsync(room.RoomData.WordFilter.Count);
            /* TODO CHECK */
            foreach (var current in room.RoomData.WordFilter)
                await serverMessage.AppendStringAsync(current);
            Response = serverMessage;
            await SendResponse();

            await Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModRoomFilterSeen", 1);
        }

        internal async Task ApplyRoomEffect()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;
            var item = Session.GetHabbo().GetInventoryComponent()
                .GetItem(Oblivion.GetGame().GetItemManager().GetRealId(Request.GetUInteger()));
            if (item == null)
                return;
            var type = "floor";

            if (item.BaseItem.Name.ToLower().Contains("wallpaper"))
                type = "wallpaper";
            else if (item.BaseItem.Name.ToLower().Contains("landscape"))
                type = "landscape";

            switch (type)
            {
                case "floor":

                    room.RoomData.Floor = item.ExtraData;

                    await Oblivion.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoFloor", 1);
                    await Oblivion.GetGame()
                        .GetQuestManager()
                        .ProgressUserQuest(Session, QuestType.FurniDecorationFloor);
                    break;

                case "wallpaper":

                    room.RoomData.WallPaper = item.ExtraData;

                    await Oblivion.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoWallpaper", 1);
                    await Oblivion.GetGame()
                        .GetQuestManager()
                        .ProgressUserQuest(Session, QuestType.FurniDecorationWall);
                    break;

                case "landscape":

                    room.RoomData.LandScape = item.ExtraData;

                    await Oblivion.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery(string.Concat("UPDATE rooms_data SET ", type, " = @extradata WHERE id = ",
                    room.RoomId));
                queryReactor.AddParameter("extradata", item.ExtraData);
                await queryReactor.RunQueryAsync();
                await queryReactor.RunNoLockFastQueryAsync($"DELETE FROM items_rooms WHERE id='{item.Id}' LIMIT 1;");
            }

            await Session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false, 0);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
            await serverMessage.AppendStringAsync(type);
            await serverMessage.AppendStringAsync(item.ExtraData);
            await room.SendMessage(serverMessage);
        }

        internal async Task PromoteRoom()
        {
            var pageId = Request.GetInteger();
            var item = Request.GetUInteger();

            var page2 = Oblivion.GetGame().GetCatalog().GetPage(pageId);
            var catalogItem = page2?.Items.Values.FirstOrDefault();

            if (catalogItem == null) return;

            var num = Request.GetUInteger();
            var text = Request.GetString();
            Request.GetBool();

            var text2 = string.Empty;
            try
            {
                text2 = Request.GetString();
            }
            catch (Exception)
            {
            }

            var category = Request.GetInteger();

            var room = Oblivion.GetGame().GetRoomManager().GetRoom(num) ?? new Room();
            await room.Start(await Oblivion.GetGame().GetRoomManager().GenerateNullableRoomData(num), true);

            if (!room.CheckRights(Session, true)) return;
            if (catalogItem.CreditsCost > 0)
            {
                if (catalogItem.CreditsCost > Session.GetHabbo().Credits) return;
                Session.GetHabbo().Credits -= (int)catalogItem.CreditsCost;
                await Session.GetHabbo().UpdateCreditsBalance(true);
            }

            if (catalogItem.DucketsCost > 0)
            {
                if (catalogItem.DucketsCost > Session.GetHabbo().ActivityPoints) return;
                Session.GetHabbo().ActivityPoints -= (int)catalogItem.DucketsCost;
                await Session.GetHabbo().UpdateActivityPointsBalance(true);
            }

            if (catalogItem.DiamondsCost > 0)
            {
                if (catalogItem.DiamondsCost > Session.GetHabbo().Diamonds) return;
                Session.GetHabbo().Diamonds -= (int)catalogItem.DiamondsCost;
                await Session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
            }

            await Session.SendMessage(CatalogPageComposer.PurchaseOk(catalogItem, catalogItem.Items));

            if (room.RoomData.Event != null && !room.RoomData.Event.HasExpired)
            {
                room.RoomData.Event.Time = Oblivion.GetUnixTimeStamp();
                await Oblivion.GetGame().GetRoomEvents().SerializeEventInfo(room.RoomId);
            }
            else
            {
                await Oblivion.GetGame().GetRoomEvents().AddNewEvent(room.RoomId, text, text2, Session, 7200, category);
                await Oblivion.GetGame().GetRoomEvents().SerializeEventInfo(room.RoomId);
            }

            await Session.GetHabbo().GetBadgeComponent().GiveBadge("RADZZ", true, Session);
        }

        internal async Task GetPromotionableRooms()
        {
            var serverMessage = new ServerMessage();
            await serverMessage.InitAsync(LibraryParser.OutgoingRequest("CatalogPromotionGetRoomsMessageComposer"));
            serverMessage.AppendBool(true);
            await serverMessage.AppendIntegerAsync(Session.GetHabbo().Data.Rooms.Count);
            /* TODO CHECK */
            foreach (var data in Session.GetHabbo().Data.Rooms)
            {
                var current = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(data);

                await serverMessage.AppendIntegerAsync(current.Id);
                await serverMessage.AppendStringAsync(current.Name);
                serverMessage.AppendBool(false);
            }

            Response = serverMessage;
            await SendResponse();
        }

        internal async Task SaveHeightmap()
        {
            if (Session?.GetHabbo() != null)
            {
                var room = Session.GetHabbo().CurrentRoom;

                if (room == null)
                {
                    await Session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_not_in_room"));
                    return;
                }

                if (!room.CheckRights(Session, true))
                {
                    await Session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_not_his_room"));
                    return;
                }

                var heightMap = Request.GetString().ToLower().TrimEnd();
                var doorX = Request.GetInteger();
                var doorY = Request.GetInteger();
                var doorOrientation = Request.GetInteger();
                var wallThickness = Request.GetInteger();
                var floorThickness = Request.GetInteger();
                var wallHeight = Request.GetInteger();

                if (heightMap.Length < 2)
                {
                    await Session.SendNotif(Oblivion.GetLanguage().GetVar("invalid_room_length"));
                    return;
                }

                if (wallThickness < -2 || wallThickness > 1)
                    wallThickness = 0;

                if (floorThickness < -2 || floorThickness > 1)
                    floorThickness = 0;

                if (doorOrientation < 0 || doorOrientation > 8)
                    doorOrientation = 2;

                if (wallHeight < -1 || wallHeight > 16)
                    wallHeight = -1;

                char[] validLetters =
                {
                    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                    'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', '\r'
                };
                if (heightMap.Any(letter => !validLetters.Contains(letter)))
                {
                    await Session.SendNotif(Oblivion.GetLanguage().GetVar("user_floor_editor_error"));

                    return;
                }

                if (heightMap.Last() == Convert.ToChar(13))
                    heightMap = heightMap.Remove(heightMap.Length - 1);

                if (heightMap.Length > 74200)
                {
                    var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    await message.AppendStringAsync("floorplan_editor.error");
                    await message.AppendIntegerAsync(1);
                    await message.AppendStringAsync("errors");
                    await message.AppendStringAsync(
                        "(general): too large height (max 128 tiles)\r(general): too large area (max 1800 tiles)");
                    await Session.SendMessage(message);

                    return;
                }

                if (heightMap.Split((char)13).Length - 1 < doorY)
                {
                    var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    await message.AppendStringAsync("floorplan_editor.error");
                    await message.AppendIntegerAsync(1);
                    await message.AppendStringAsync("errors");
                    await message.AppendStringAsync("Y: Door is in invalid place.");
                    await Session.SendMessage(message);

                    return;
                }

                var lines = heightMap.Split((char)13);
                var lineWidth = lines[0].Length;
                for (var i = 1; i < lines.Length; i++)
                    if (lines[i].Length != lineWidth)
                    {
                        using var message =
                            new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                        await message.AppendStringAsync("floorplan_editor.error");
                        await message.AppendIntegerAsync(1);
                        await message.AppendStringAsync("errors");
                        await message.AppendStringAsync("(general): Line " + (i + 1) +
                                                        " is of different length than line 1");
                        await Session.SendMessage(message);

                        return;
                    }

                double doorZ;
                var charDoor = lines[doorY][doorX];
                if (charDoor >= (char)97 && charDoor <= 119) // a-w
                    doorZ = charDoor - 87;
                else
                    double.TryParse(charDoor.ToString(), out doorZ);
                using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    queryReactor.SetQuery(
                        "REPLACE INTO rooms_models_customs (roomid,door_x,door_y,door_z,door_dir,heightmap) VALUES ('" +
                        room.RoomId + "', '" + doorX + "','" +
                        doorY + "','" + doorZ.ToString(CultureInfo.InvariantCulture).Replace(',', '.') + "','" +
                        doorOrientation + "',@newmodel)");
                    queryReactor.AddParameter("newmodel", heightMap);
                    await queryReactor.RunQueryAsync();

                    room.RoomData.WallHeight = wallHeight;
                    room.RoomData.WallThickness = wallThickness;
                    room.RoomData.FloorThickness = floorThickness;
                    room.RoomData.Model.DoorZ = doorZ;

                    await Oblivion.GetGame().GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoHoleFurniCount", 1);

                    await queryReactor.RunFastQueryAsync(
                        $"UPDATE rooms_data SET model_name = 'custom', wallthick = '{wallThickness}', floorthick = '{floorThickness}', walls_height = '{wallHeight}' WHERE id = {room.RoomId};");
                }

                await room.ResetGameMap("custom", wallHeight, wallThickness, floorThickness);
                room.GetGameMap().GenerateMaps();

                //                Oblivion.GetGame().GetRoomManager().UnloadRoom(room, "Reload floor");

                var UsersToReturn = room.GetRoomUserManager().GetRoomUsers();

                foreach (var User in UsersToReturn.Where(User => User?.GetClient() != null))
                {
                    var forwardToRoom = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                    await forwardToRoom.AppendIntegerAsync(room.RoomId);
                    await User.GetClient().SendMessage(forwardToRoom);
                }
            }
        }

        internal async Task PlantMonsterplant(RoomItem mopla, Room room)
        {
            int rarity = 0;
            if (room == null || mopla == null)
                return;

            if (mopla.GetBaseItem().InteractionType != Interaction.Moplaseed &&
                mopla.GetBaseItem().InteractionType != Interaction.RareMoplaSeed)
                return;
            if (string.IsNullOrEmpty(mopla.ExtraData) || mopla.ExtraData == "0")
                rarity = 1;
            if (!string.IsNullOrEmpty(mopla.ExtraData) && mopla.ExtraData != "0")
                rarity = int.TryParse(mopla.ExtraData, out var internalRarity) ? internalRarity : 1;

            var getX = mopla.X;
            var getY = mopla.Y;
            await room.GetRoomItemHandler().RemoveFurniture(Session, mopla.Id, false);
            var pet = await CatalogManager.CreatePet(Session.GetHabbo().Id, "Monsterplant", 16, "0", "0", rarity);
            await Response.InitAsync(LibraryParser.OutgoingRequest("SendMonsterplantIdMessageComposer"));
            await Response.AppendIntegerAsync(pet.PetId);
            await SendResponse();
            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                var roomId = (room.RoomId <= 0) ? "NULL" : $"'{room.RoomId}'";

                await queryReactor.RunFastQueryAsync(string.Concat("UPDATE bots SET room_id = ", roomId, ", x = '",
                    getX,
                    "', y = '", getY, "' WHERE id = '", pet.PetId, "';"));
            }

            pet.PlacedInRoom = true;
            pet.RoomId = room.RoomId;
            var bot = new RoomBot(pet.PetId, pet.OwnerId, pet.RoomId, AiType.Pet, "freeroam", pet.Name, "", pet.Look,
                getX, getY, 0.0, 4, 0, 0, 0, 0, null, null, "", 0, false);
            await room.GetRoomUserManager().DeployBot(bot, pet);

            if (pet.DbState != DatabaseUpdateState.NeedsInsert)
                pet.DbState = DatabaseUpdateState.NeedsUpdate;

            using (var queryreactor2 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await queryreactor2.RunNoLockFastQueryAsync($"DELETE FROM items_rooms WHERE id = '{mopla.Id}';");
                await room.GetRoomUserManager().SavePets(queryreactor2);
            }
        }

        internal async Task KickBot()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;
            var roomUserByVirtualId = room.GetRoomUserManager().GetRoomUserByVirtualId(Request.GetInteger());
            if (roomUserByVirtualId == null || !roomUserByVirtualId.IsBot)
                return;

            await room.GetRoomUserManager().RemoveBot(roomUserByVirtualId.VirtualId, true);
        }

        internal async Task PlacePet()
        {
            var room = Session.GetHabbo().CurrentRoom;

            if (room == null || !room.RoomData.AllowPets && !room.CheckRights(Session, true) ||
                !room.CheckRights(Session, true))
                return;
            if (room.GetRoomUserManager().GetPets().Count >= 12)
            {
                await Session.SendWhisperAsync("Apenas 12 pets por sala!");
                return;
            }

            var petId = Request.GetUInteger();
            var pet = Session.GetHabbo().GetInventoryComponent().GetPet(petId);

            if (pet == null || pet.PlacedInRoom)
                return;

            var x = Request.GetInteger();
            var y = Request.GetInteger();

            if (!room.GetGameMap().CanWalk(x, y, false))
                return;

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                var roomId = (room.RoomId <= 0) ? "NULL" : $"'{room.RoomId}'";

                await queryReactor.RunFastQueryAsync("UPDATE bots SET room_id = " + roomId + ", x = '" + x +
                                                     "', y = '" +
                                                     y + "' WHERE id = '" + petId + "'");
            }

            pet.PlacedInRoom = true;
            pet.RoomId = room.RoomId;

            await room.GetRoomUserManager()
                .DeployBot(
                    new RoomBot(pet.PetId, Convert.ToUInt32(pet.OwnerId), pet.RoomId, AiType.Pet, "freeroam", pet.Name,
                        "", pet.Look, x, y, 0.0, 4, 0, 0, 0, 0, null, null, "", 0, false), pet);
            await Session.GetHabbo().GetInventoryComponent().MovePetToRoom(pet.PetId);
            if (pet.DbState != DatabaseUpdateState.NeedsInsert)
                pet.DbState = DatabaseUpdateState.NeedsUpdate;
            using (var queryreactor2 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                await room.GetRoomUserManager().SavePets(queryreactor2);
            }

            await Session.SendMessage(await Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        internal async Task UpdateEventInfo()
        {
            Request.GetInteger();
            var original = Request.GetString();
            var original2 = Request.GetString();
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true) || room.RoomData.Event == null)
                return;
            room.RoomData.Event.Name = original;
            room.RoomData.Event.Description = original2;
            await Oblivion.GetGame().GetRoomEvents().UpdateEvent(room.RoomData.Event);
        }

        internal async Task HandleBotSpeechList()
        {
            var botId = Request.GetUInteger();
            var num2 = Request.GetInteger();
            var num3 = num2;

            var room = Session.GetHabbo().CurrentRoom;
            var bot = room?.GetRoomUserManager().GetBot(botId);
            if (bot == null || !bot.IsBot)
                return;

            if (num3 == 2)
            {
                var text = bot.BotData.RandomSpeech == null
                    ? string.Empty
                    : string.Join("\n", bot.BotData.RandomSpeech);
                text += ";#;";
                text += bot.BotData.AutomaticChat ? "true" : "false";
                text += ";#;";
                text += bot.BotData.SpeechInterval;
                text += ";#;";
                text += bot.BotData.MixPhrases ? "true" : "false";

                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("BotSpeechListMessageComposer"));
                await serverMessage.AppendIntegerAsync(botId);
                await serverMessage.AppendIntegerAsync(num2);
                await serverMessage.AppendStringAsync(text);
                Response = serverMessage;
                await SendResponse();
                return;
            }

            if (num3 != 5)
                return;

            var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("BotSpeechListMessageComposer"));
            await serverMessage2.AppendIntegerAsync(botId);
            await serverMessage2.AppendIntegerAsync(num2);
            await serverMessage2.AppendStringAsync(bot.BotData.Name);

            Response = serverMessage2;
            await SendResponse();
        }

        internal async Task ManageBotActions()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var botId = Request.GetUInteger();
            var action = Request.GetInteger();
            var data = Oblivion.FilterInjectionChars(Request.GetString());
            var bot = room.GetRoomUserManager().GetBot(botId);
            if (bot?.BotData == null) return;

            var flag = false;
            switch (action)
            {
                case 1:
                    bot.BotData.Look = Session.GetHabbo().Look;
                    goto IL_439;
                case 2:
                    try
                    {
                        var array = data.Split(new[] { ";#;" }, StringSplitOptions.None);

                        var speechsJunk =
                            array[0].Substring(0, array[0].Length > 1024 ? 1024 : array[0].Length)
                                .Split(Convert.ToChar(13));
                        var speak = array[1] == "true";
                        var speechDelay = int.Parse(array[2]);
                        var mix = array[3] == "true";
                        if (speechDelay < 7) speechDelay = 7;

                        var speechs =
                            speechsJunk.Where(
                                    speech =>
                                        !string.IsNullOrEmpty(speech) &&
                                        (!speech.ToLower().Contains("update") || !speech.ToLower().Contains("set")))
                                .Aggregate(string.Empty,
                                    (current, speech) =>
                                        current + TextHandling.FilterHtml(speech, Session.GetHabbo().GotCommand("ha")) +
                                        ";");
                        using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            queryReactor.SetQuery(
                                "UPDATE bots SET automatic_chat = @autochat, speaking_interval = @interval, mix_phrases = @mix_phrases, speech = @speech WHERE id = @botid");

                            queryReactor.AddParameter("autochat", speak ? "1" : "0");
                            queryReactor.AddParameter("interval", speechDelay);
                            queryReactor.AddParameter("mix_phrases", mix ? "1" : "0");
                            queryReactor.AddParameter("speech", speechs);
                            queryReactor.AddParameter("botid", botId);
                            await queryReactor.RunQueryAsync();
                        }

                        var randomSpeech = speechs.Split(';').ToList();
                        room.GetRoomUserManager()
                            .UpdateBot(bot.VirtualId, bot, bot.BotData.Name, bot.BotData.Motto, bot.BotData.Look,
                                bot.BotData.Gender, randomSpeech, null, speak, speechDelay, mix);
                        flag = true;
                        goto IL_439;
                    }
                    catch (Exception e)
                    {
                        Writer.Writer.LogException(e.ToString());
                        return;
                    }
                case 3:
                    bot.BotData.WalkingMode = bot.BotData.WalkingMode == "freeroam" ? "stand" : "freeroam";
                    using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        queryReactor.SetQuery("UPDATE bots SET walk_mode = @walkmode WHERE id = @botid");
                        queryReactor.AddParameter("walkmode", bot.BotData.WalkingMode);
                        queryReactor.AddParameter("botid", botId);
                        await queryReactor.RunQueryAsync();
                    }

                    goto IL_439;
                case 4:
                    break;

                case 5:
                    var name = TextHandling.FilterHtml(data, Session.GetHabbo().GotCommand("ha"));
                    if (name.Length < 15)
                    {
                        bot.BotData.Name = name;
                    }
                    else
                    {
                        await BotErrorComposer(4);
                        break;
                    }

                    goto IL_439;
                default:
                    goto IL_439;
            }

            if (bot.BotData.DanceId > 0)
            {
                bot.BotData.DanceId = 0;
            }
            else
            {
                var random = new Random();
                bot.DanceId = random.Next(1, 4);
                bot.BotData.DanceId = bot.DanceId;
            }

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
            await serverMessage.AppendIntegerAsync(bot.VirtualId);
            await serverMessage.AppendIntegerAsync(bot.BotData.DanceId);
            await Session.GetHabbo().CurrentRoom.SendMessage(serverMessage);
            IL_439:
            if (!flag)
            {
                var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
                await serverMessage2.AppendIntegerAsync(1);
                bot.Serialize(serverMessage2);
                await room.SendMessage(serverMessage2);
            }
        }

        internal async Task BotErrorComposer(int errorid)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("GeneralErrorHabboMessageComposer"));
            await serverMessage.AppendIntegerAsync(errorid);
            await Session.SendMessage(serverMessage);
        }

        internal async Task RoomOnLoad()
        {
            // TODO!
            await Response.InitAsync(LibraryParser.OutgoingRequest("SendRoomCampaignFurnitureMessageComposer"));
            await Response.AppendIntegerAsync(0);
            await SendResponse();
        }

        internal async Task MuteAll()
        {
            var currentRoom = Session.GetHabbo().CurrentRoom;
            if (currentRoom == null || !currentRoom.CheckRights(Session, true))
                return;
            currentRoom.RoomMuted = !currentRoom.RoomMuted;

            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomMuteStatusMessageComposer"));
            Response.AppendBool(currentRoom.RoomMuted);
            await Session.SendMessage(Response);
            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModMuteSeen", 1);
        }

        internal async Task HomeRoom()
        {
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));
            await GetResponse().AppendIntegerAsync(Session.GetHabbo().HomeRoom);
            await GetResponse().AppendIntegerAsync(0);
            await SendResponse();
        }



        internal async Task RoomUserAction()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            await roomUserByHabbo.UnIdle();
            var num = Request.GetInteger();
            roomUserByHabbo.DanceId = 0;

            var action = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
            await action.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await action.AppendIntegerAsync(num);
            await room.SendMessage(action);

            if (num == 5)
            {
                roomUserByHabbo.IsAsleep = true;
                var sleep = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
                await sleep.AppendIntegerAsync(roomUserByHabbo.VirtualId);
                sleep.AppendBool(roomUserByHabbo.IsAsleep);
                await room.SendMessage(sleep);
            }

            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialWave);
        }

        internal void GetRoomData1()
        {
            return;
            /*this.Response.Init(StaticClientMessageHandler.OutgoingRequest("297"));//Not in release
            this.Response.AppendInt32(0);
            thisSendResponse();*/
        }

        internal async Task GetRoomData2()
        {
            try
            {
                var session = Session;
                if (session != null && session.GetConnection() != null)
                {
                    var CurrentLoadingRoom = this.CurrentLoadingRoom;


                    if (session?.GetHabbo()?.LoadingRoom <= 0u || CurrentLoadingRoom == null)
                        return;
                    ;

                    var roomData = CurrentLoadingRoom.RoomData;
                    if (roomData == null)
                        return;

                    var gameMap = CurrentLoadingRoom.GetGameMap();


                    if (roomData.Model == null || gameMap == null)
                    {
                        await session.SendMessage(
                            new ServerMessage(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer")));
                        ClearRoomLoading();
                    }
                    else
                    {
                        await session.SendMessage(gameMap.GetNewHeightmap());
                        await session.SendMessage(gameMap.Model?.GetHeightmap());
                        await GetRoomData3();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException("Unable to load room ID [" + Session?.GetHabbo().LoadingRoom + "]" + ex);
                Logging.HandleException(ex, "Oblivion.Messages.Handlers.Rooms");
            }
        }

        internal async Task GetRoomData3()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().LoadingRoom <= 0u ||
                !Session.GetHabbo().LoadingChecksPassed ||
                CurrentLoadingRoom == null)
                return;
            if (CurrentLoadingRoom.RoomData.UsersNow + 1 > CurrentLoadingRoom.RoomData.UsersMax &&
                !Session.GetHabbo().HasFuse("fuse_enter_full_rooms"))
            {
                var roomFull = new ServerMessage(LibraryParser.OutgoingRequest("RoomEnterErrorMessageComposer"));
                await roomFull.AppendIntegerAsync(1);
                return;
            }

            var array = CurrentLoadingRoom.GetRoomItemHandler().FloorItems.Values;
            var array2 = CurrentLoadingRoom.GetRoomItemHandler().WallItems.Values;
            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomFloorItemsMessageComposer"));


            await Response.AppendIntegerAsync(1);
            await Response.AppendIntegerAsync(CurrentLoadingRoom.RoomData.OwnerId);
            await Response.AppendStringAsync(CurrentLoadingRoom.RoomData.Owner);


            await Response.AppendIntegerAsync(array.Count);
            /* TODO CHECK */
            foreach (var roomItem in array)
                roomItem.Serialize(Response);

            await SendResponse();
            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomWallItemsMessageComposer"));

            await Response.AppendIntegerAsync(1);
            await Response.AppendIntegerAsync(CurrentLoadingRoom.RoomData.OwnerId);
            await Response.AppendStringAsync(CurrentLoadingRoom.RoomData.Owner);

            await Response.AppendIntegerAsync(array2.Count);
            foreach (var roomItem2 in array2)
                roomItem2.Serialize(Response);

            await SendResponse();

            await CurrentLoadingRoom.GetRoomUserManager().AddUserToRoom(Session, Session.GetHabbo().SpectatorMode);
            Session.GetHabbo().SpectatorMode = false;

            if (Oblivion.GetUnixTimeStamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                await serverMessage.AppendIntegerAsync(Session.GetHabbo().FloodTime - Oblivion.GetUnixTimeStamp());

                await Session.SendMessage(serverMessage);
            }

            ClearRoomLoading();

            if (!Oblivion.GetGame().GetPollManager().TryGetPoll(CurrentLoadingRoom.RoomId, out var poll) ||
                Session.GetHabbo().GotPollData(poll.Id))
                return;
            if (poll.Type == PollType.Matching)
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("MatchingPollMessageComposer"));
                await Response.AppendStringAsync("MATCHING_POLL");
                await Response.AppendIntegerAsync(poll.RoomId);
                await Response.AppendIntegerAsync(poll.RoomId);
                await Response.AppendIntegerAsync(1);
                await Response.AppendIntegerAsync(poll.RoomId);
                await Response.AppendIntegerAsync(120);
                await Response.AppendIntegerAsync(3);
                await Response.AppendStringAsync(poll.PollName);
            }
            else
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("SuggestPollMessageComposer"));
                await poll.Serialize(Response);
            }

            await SendResponse();
        }

        internal async Task WidgetContainers()
        {
            var text = Request.GetString();

            if (Session == null)
                return;

            if (text.Contains("gamesmaker"))
                return;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LandingWidgetMessageComposer"));

            var campaingName = "";
            var parser = text.Split(';');

            /* TODO CHECK */
            foreach (
                var data in from t in parser where !string.IsNullOrEmpty(t) && !t.EndsWith(",") select t.Split(','))
                campaingName = data[1];

            await serverMessage.AppendStringAsync(text);
            await serverMessage.AppendStringAsync(campaingName);


            await Session.SendMessage(serverMessage);
        }

        internal async Task RefreshPromoEvent()
        {
            var hotelView = Oblivion.GetGame().GetHotelView();

            if (Session?.GetHabbo() == null)
                return;

            var rankings = Oblivion.GetGame().GetHallOfFame().Rankings;

            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("HotelViewHallOfFameMessageComposer"));
            await GetResponse().AppendStringAsync("");
            GetResponse().StartArray();
            foreach (var element in rankings)
            {
                await GetResponse().AppendIntegerAsync(element.UserId);
                await GetResponse().AppendStringAsync(element.Username);
                await GetResponse().AppendStringAsync(element.Look);
                await GetResponse().AppendIntegerAsync(2);
                await GetResponse().AppendIntegerAsync(element.Score);
                GetResponse().SaveArray();
            }

            GetResponse().EndArray();
            await SendResponse();

            if (hotelView.HotelViewPromosIndexers.Count <= 0)
                return;

            using var message =
                hotelView.SmallPromoComposer(
                    new ServerMessage(LibraryParser.OutgoingRequest("LandingPromosMessageComposer")));
            await Session.SendMessage(message);
        }

        internal void RefreshCompetition()
        {
            return;
            //LandingRefreshCompetitionMessageComposer -> type, amount, goal
        }

        internal async Task AcceptPoll()
        {
            var key = Request.GetUInteger();
            if (!Oblivion.GetGame().GetPollManager().Polls.TryGetValue(key, out var poll))
                return;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PollQuestionsMessageComposer"));

            await serverMessage.AppendIntegerAsync(poll.Id);
            await serverMessage.AppendStringAsync(poll.PollName);
            await serverMessage.AppendStringAsync(poll.Thanks);
            await serverMessage.AppendIntegerAsync(poll.Questions.Count);

            /* TODO CHECK */
            foreach (var current in poll.Questions)
            {
                var questionNumber = poll.Questions.IndexOf(current) + 1;

                current.Serialize(serverMessage, questionNumber);
            }

            Response = serverMessage;
            await SendResponse();
        }

        internal async Task RefusePoll()
        {
            var num = Request.GetUInteger();

            Session.GetHabbo().Data.SuggestedPolls.Add(num);

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery("INSERT INTO users_polls VALUES (@userid , @pollid , 0 , '0' , '')");
                queryReactor.AddParameter("userid", Session.GetHabbo().Id);
                queryReactor.AddParameter("pollid", num);
                await queryReactor.RunQueryAsync();
            }
        }

        internal async Task AnswerPoll()
        {
            var pollId = Request.GetUInteger();
            var questionId = Request.GetUInteger();
            var num3 = Request.GetInteger();

            var list = new List<string>();

            for (var i = 0; i < num3; i++)
                list.Add(Request.GetString());

            var text = string.Join("\r\n", list);

            var poll = Oblivion.GetGame().GetPollManager().TryGetPollById(pollId);

            if (poll != null && poll.Type == PollType.Matching)
            {
                if (text == "1")
                    poll.AnswersPositive++;
                else
                    poll.AnswersNegative++;

                var answered = new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollAnsweredMessageComposer"));

                await answered.AppendIntegerAsync(Session.GetHabbo().Id);
                await answered.AppendStringAsync(text);
                await answered.AppendIntegerAsync(2);
                await answered.AppendStringAsync("0");
                await answered.AppendIntegerAsync(poll.AnswersNegative);
                await answered.AppendStringAsync("1");
                await answered.AppendIntegerAsync(poll.AnswersPositive);

                await Session.GetHabbo().CurrentRoom.SendMessage(answered);
                Session.GetHabbo().AnsweredPool = true;

                return;
            }

            Session.GetHabbo().Data.SuggestedPolls.Add(pollId);

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery(
                    "INSERT INTO users_polls VALUES (@userid , @pollid , @questionid , '1' , @answer)");

                queryReactor.AddParameter("userid", Session.GetHabbo().Id);
                queryReactor.AddParameter("pollid", pollId);
                queryReactor.AddParameter("questionid", questionId);
                queryReactor.AddParameter("answer", text);
                await queryReactor.RunQueryAsync();
            }
        }


        internal void Sit()
        {
            if (Session?.GetHabbo()?.CurrentRoom == null) return;

            var user = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (user == null)
                return;

            if (user.Statusses.ContainsKey("lay") || user.IsLyingDown || user.RidingHorse || user.IsWalking)
                return;

            if (user.RotBody % 2 != 0)
                user.RotBody--;

            user.Z = Session.GetHabbo().CurrentRoom.GetGameMap().SqAbsoluteHeight(user.X, user.Y);

            if (user.Statusses.TryAdd("sit", "0.55"))
            {
                user.UpdateNeeded = true;
            }

            user.IsSitting = true;
            return;
        }

        public async Task Whisper()
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var currentRoom = Session.GetHabbo().CurrentRoom;
            var text = Request.GetString();
            var text2 = text.Split(' ')[0];
            var msg = text.Substring(text2.Length + 1);
            var colour = Request.GetInteger();

            var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            var roomUserByHabbo2 = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(text2);

            msg = currentRoom.RoomData.WordFilter.Aggregate(msg,
                (current, s) => Regex.Replace(current, Regex.Escape(s), "bobba", RegexOptions.IgnoreCase));
            if (!await BobbaFilter.CanTalk(Session, msg))
            {
                return;
            }

            var span = DateTime.Now - _floodTime;

            if (span.Seconds > 4)
                _floodCount = 0;

            if (span.Seconds < 4 && _floodCount > 5 && Session.GetHabbo().Rank < 5)
                return;

            _floodTime = DateTime.Now;
            _floodCount++;

            if (roomUserByHabbo?.GetClient()?.GetHabbo() == null ||
                roomUserByHabbo2?.GetClient()?.GetHabbo()?.Data?.Ignores == null)
            {
                await Session.SendWhisperAsync(msg);
                return;
            }

            if (Session.GetHabbo().Rank < 4 && currentRoom.CheckMute(Session))
                return;

            currentRoom.AddChatlog(Session.GetHabbo().Id, $"<Sussurro para {text2}>: {msg}", false);

            await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialChat);

            var colour2 = colour;

            if (!roomUserByHabbo.IsBot)
                if (colour2 == 2 || colour2 == 23 && !Session.GetHabbo().HasFuse("fuse_mod") || colour2 < 0 ||
                    colour2 > 29)
                    colour2 = roomUserByHabbo.LastBubble; // or can also be just 0

            await roomUserByHabbo.UnIdle();

            var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
            await whisp.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await whisp.AppendStringAsync(msg);
            await whisp.AppendIntegerAsync(0);
            await whisp.AppendIntegerAsync(colour2);
            await whisp.AppendIntegerAsync(0);
            await whisp.AppendIntegerAsync(-1);

            await roomUserByHabbo.GetClient().SendMessage(whisp);

            if (!roomUserByHabbo2.IsBot && roomUserByHabbo2.UserId != roomUserByHabbo.UserId && !roomUserByHabbo2
                    .GetClient().GetHabbo().Data.Ignores.Contains(Session.GetHabbo().Id))
                await roomUserByHabbo2.GetClient().SendMessage(whisp);

            var roomUserByRank = currentRoom.GetRoomUserManager().GetRoomUserByRank(4);

            if (roomUserByRank.Count <= 0)
                return;

            /* TODO CHECK */
            foreach (var current2 in roomUserByRank)
                if (current2 != null && current2.HabboId != roomUserByHabbo2.HabboId &&
                    current2.HabboId != roomUserByHabbo.HabboId && current2.GetClient() != null)
                {
                    using (var whispStaff = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer")))
                    {
                        await whispStaff.AppendIntegerAsync(roomUserByHabbo.VirtualId);
                        await whispStaff.AppendStringAsync($"Whisper to {text2}: {msg}");
                        await whispStaff.AppendIntegerAsync(0);
                        await whispStaff.AppendIntegerAsync(colour2);
                        await whispStaff.AppendIntegerAsync(0);
                        await whispStaff.AppendIntegerAsync(-1);

                        await current2.GetClient().SendMessage(whispStaff);
                    }
                }
        }

        public async Task Chat()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (roomUser == null)
                return;

            var message = Request.GetString();
            var bubble = Request.GetInteger();
            var count = Request.GetInteger();

            if (!roomUser.IsBot)
                if (bubble == 2 || bubble >= 23 && !Session.GetHabbo().HasFuse("fuse_mod") || bubble < 0)
                    bubble = roomUser.LastBubble;

            await roomUser.Chat(Session, message, false, count, bubble);
        }

        public async Task Shout()
        {
            var room = Session.GetHabbo().CurrentRoom;

            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (roomUserByHabbo == null)
                return;

            var msg = Request.GetString();
            var bubble = Request.GetInteger();

            if (!roomUserByHabbo.IsBot)
                if (bubble == 2 || bubble >= 23 && !Session.GetHabbo().HasFuse("fuse_mod") || bubble < 0 || bubble > 29)
                    bubble = roomUserByHabbo.LastBubble;

            await roomUserByHabbo.Chat(Session, msg, true, -1, bubble);
        }

        public async Task GetFloorPlanUsedCoords()
        {
            var room = Session.GetHabbo().CurrentRoom;

            if (room == null)
                return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("SetFloorPlanDoorMessageComposer"));
            await Response.AppendIntegerAsync(room.GetGameMap().Model.DoorX);
            await Response.AppendIntegerAsync(room.GetGameMap().Model.DoorY);
            await Response.AppendIntegerAsync(room.GetGameMap().Model.DoorOrientation);

            await SendResponse();


            await Response.InitAsync(LibraryParser.OutgoingRequest("GetFloorPlanUsedCoordsMessageComposer"));

            var coords = room.GetGameMap().CoordinatedItems.Keys;
            await Response.AppendIntegerAsync(coords.Count);

            /* TODO CHECK */
            foreach (var point in coords)
            {
                await Response.AppendIntegerAsync(point.X);
                await Response.AppendIntegerAsync(point.Y);
            }


            await SendResponse();
        }

        public async Task GetFloorPlanDoor()
        {
            var room = Session.GetHabbo().CurrentRoom;

            if (room == null)
                return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("SetFloorPlanDoorMessageComposer"));
            await Response.AppendIntegerAsync(room.GetGameMap().Model.DoorX);
            await Response.AppendIntegerAsync(room.GetGameMap().Model.DoorY);
            await Response.AppendIntegerAsync(room.GetGameMap().Model.DoorOrientation);

            await SendResponse();
        }


        public async Task EnterRoomQueue()
        {
            await Session.SendNotif("Currently working on Watch live TV");

            Session.GetHabbo().SpectatorMode = true;

            using var forwardToRoom = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            await forwardToRoom.AppendIntegerAsync(1);

            await Session.SendMessage(forwardToRoom);
        }

        public async Task GetCameraRequest()
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var room = Session.GetHabbo().CurrentRoom;

            var user = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (user == null)
                return;

            var str = Request.GetString();
            if (!int.TryParse(str, out var photoId) || photoId < 0)
                return;

            var preview = Oblivion.GetGame().GetCameraManager().GetPreview(photoId);

            if (preview == null || preview.CreatorId != Session.GetHabbo().Id)
                return;

            user.LastPhotoPreview = preview;

            using var messageBuffer =
                new ServerMessage(LibraryParser.OutgoingRequest("CameraStorageUrlMessageComposer"));

            await messageBuffer.AppendStringAsync(Oblivion.GetGame()
                .GetCameraManager()
                .GetPath(CameraPhotoType.PREVIEW, preview.Id, preview.CreatorId));

            await Session.SendMessage(messageBuffer);
        }

        public async Task SubmitRoomToCompetition()
        {
            Request.GetString();

            var code = Request.GetInteger();
            var room = Session.GetHabbo().CurrentRoom;
            var roomData = room?.RoomData;

            if (roomData == null)
                return;

            var competition = Oblivion.GetGame().GetRoomManager().GetCompetitionManager().Competition;

            if (competition == null)
                return;

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                if (code == 2)
                {
                    if (competition.Entries.ContainsKey(room.RoomId))
                        return;

                    queryReactor.SetQuery(
                        "INSERT INTO rooms_competitions_entries (competition_id, room_id, status) VALUES (@competition_id, @room_id, @status)");

                    queryReactor.AddParameter("competition_id", competition.Id);
                    queryReactor.AddParameter("room_id", room.RoomId);
                    queryReactor.AddParameter("status", 2);
                    await queryReactor.RunQueryAsync();
                    competition.Entries.Add(room.RoomId, roomData);

                    using var message = new ServerMessage();

                    roomData.CompetitionStatus = 2;
                    competition.AppendEntrySubmitMessage(message, 3, room);

                    await Session.SendMessage(message);
                }
                else if (code == 3)
                {
                    if (!competition.Entries.TryGetValue(room.RoomId, out var entry))
                        return;


                    if (entry == null)
                        return;

                    queryReactor.SetQuery(
                        "UPDATE rooms_competitions_entries SET status = @status WHERE competition_id = @competition_id AND room_id = @roomid");

                    queryReactor.AddParameter("status", 3);
                    queryReactor.AddParameter("competition_id", competition.Id);
                    queryReactor.AddParameter("roomid", room.RoomId);
                    await queryReactor.RunQueryAsync();
                    roomData.CompetitionStatus = 3;


                    using var message = new ServerMessage();
                    competition.AppendEntrySubmitMessage(message, 0);

                    await Session.SendMessage(message);
                }
            }
        }

        public async Task VoteForRoom()
        {
            Request.GetString();

            if (Session.GetHabbo().DailyCompetitionVotes <= 0)
                return;

            var room = Session.GetHabbo().CurrentRoom;

            var roomData = room?.RoomData;

            if (roomData == null)
                return;

            var competition = Oblivion.GetGame().GetRoomManager().GetCompetitionManager().Competition;

            if (competition == null)
                return;

            if (!competition.Entries.TryGetValue(room.RoomId, out var entry))
                return;


            entry.CompetitionVotes++;
            Session.GetHabbo().DailyCompetitionVotes--;

            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                queryReactor.SetQuery(
                    "UPDATE rooms_competitions_entries SET votes = @votes WHERE competition_id = @competition_id AND room_id = @roomid");

                queryReactor.AddParameter("votes", entry.CompetitionVotes);
                queryReactor.AddParameter("competition_id", competition.Id);
                queryReactor.AddParameter("roomid", room.RoomId);
                await queryReactor.RunQueryAsync();
                await queryReactor.RunFastQueryAsync("UPDATE users_stats SET daily_competition_votes = " +
                                                     Session.GetHabbo().DailyCompetitionVotes + " WHERE id = " +
                                                     Session.GetHabbo().Id);
            }

            using var message = new ServerMessage();
            competition.AppendVoteMessage(message, Session.GetHabbo());

            await Session.SendMessage(message);
        }
    }
}