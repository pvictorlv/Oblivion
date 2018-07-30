using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Pets.Enums;
using Oblivion.HabboHotel.Polls.Enums;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages.Parsers;
using Oblivion.Security;
using Oblivion.Util;

namespace Oblivion.Messages.Handlers
{
    internal partial class GameClientMessageHandler
    {
        private int _floodCount;
        private DateTime _floodTime;

        public void GetPetBreeds()
        {
            var type = Request.GetString();
            var petId = PetRace.GetPetId(type, out var petType);
            var races = PetRace.GetRacesForRaceId(petId);
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SellablePetBreedsMessageComposer"));
            message.AppendString(petType);
            message.AppendInteger(races.Count);
            foreach (var current in races)
            {
                message.AppendInteger(petId);
                message.AppendInteger(current.Color1);
                message.AppendInteger(current.Color2);
                message.AppendBool(current.Has1Color);
                message.AppendBool(current.Has2Color);
            }

            Session.SendMessage(message);
        }

        internal void GoRoom()
        {
            if (Oblivion.ShutdownStarted || Session?.GetHabbo() == null)
                return;
            var num = Request.GetUInteger();
            var roomData = Oblivion.GetGame().GetRoomManager().GenerateRoomData(num);
            //            Session.GetHabbo().GetInventoryComponent().RunDbUpdate();
            if (roomData == null || roomData.Id == Session.GetHabbo().CurrentRoomId)
                return;
            roomData.SerializeRoomData(Response, Session, !Session.GetHabbo().InRoom);
            PrepareRoomForUser(num, roomData.PassWord);
        }

        internal void AddFavorite()
        {
            if (Session.GetHabbo() == null)
                return;

            var roomId = Request.GetUInteger();

            GetResponse().Init(LibraryParser.OutgoingRequest("FavouriteRoomsUpdateMessageComposer"));
            GetResponse().AppendInteger(roomId);
            GetResponse().AppendBool(true);
            SendResponse();

            Session.GetHabbo().Data.FavouritedRooms.Add(roomId);
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery("INSERT INTO users_favorites (user_id,room_id) VALUES (" +
                                          Session.GetHabbo().Id + "," + roomId + ")");
            }
        }

        internal void RemoveFavorite()
        {
            if (Session.GetHabbo() == null)
                return;
            var roomId = Request.GetUInteger();
            Session.GetHabbo().Data.FavouritedRooms.Remove(roomId);

            GetResponse().Init(LibraryParser.OutgoingRequest("FavouriteRoomsUpdateMessageComposer"));
            GetResponse().AppendInteger(roomId);
            GetResponse().AppendBool(false);
            SendResponse();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery("DELETE FROM users_favorites WHERE user_id = " + Session.GetHabbo().Id +
                                          " AND room_id = " + roomId);
            }
        }

        internal void OnlineConfirmationEvent()
        {
//            Out.WriteLine(
//                "Is connected now with user: " + Request.GetString() + " and ip: " + Session.GetConnection().GetIp(),
//                "Oblivion.Users",
//                ConsoleColor.DarkGreen);
        }

        internal void GoToHotelView()
        {
            if (Session?.GetHabbo() == null)
                return;
            if (!Session.GetHabbo().InRoom)
                return;
            var room = Session.GetHabbo().CurrentRoom;
            room?.GetRoomUserManager().RemoveUserFromRoom(Session, true, false);

            var rankings = Oblivion.GetGame().GetHallOfFame().Rankings;

            GetResponse().Init(LibraryParser.OutgoingRequest("HotelViewHallOfFameMessageComposer"));
            GetResponse().AppendString("");
            GetResponse().StartArray();
            foreach (var element in rankings)
            {
                GetResponse().AppendInteger(element.UserId);
                GetResponse().AppendString(element.Username);
                GetResponse().AppendString(element.Look);
                GetResponse().AppendInteger(2);
                GetResponse().AppendInteger(element.Score);
                GetResponse().SaveArray();
            }

            GetResponse().EndArray();
            SendResponse();

            var hotelView = Oblivion.GetGame().GetHotelView();
            if (hotelView.FurniRewardName != null)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LandingRewardMessageComposer"));
                serverMessage.AppendString(hotelView.FurniRewardName);
                serverMessage.AppendInteger(hotelView.FurniRewardId);
                serverMessage.AppendInteger(120);
                serverMessage.AppendInteger(120 - Session.GetHabbo().Respect);
                Session.SendMessage(serverMessage);
            }

            Session.CurrentRoomUserId = -1;
        }

        internal void LandingCommunityGoal()
        {
            var onlineFriends = Session.GetHabbo().GetMessenger().Friends.Count(x => x.Value.IsOnline);
            var goalMeter =
                new ServerMessage(LibraryParser.OutgoingRequest("LandingCommunityChallengeMessageComposer"));
            goalMeter.AppendBool(true); //
            goalMeter.AppendInteger(0); //points
            goalMeter.AppendInteger(0); //my rank
            goalMeter.AppendInteger(onlineFriends); //totalAmount
            goalMeter.AppendInteger(onlineFriends >= 20 ? 1 : onlineFriends >= 50 ? 2 : onlineFriends >= 80 ? 3 : 0);
            //communityHighestAchievedLevel
            goalMeter.AppendInteger(0); //scoreRemainingUntilNextLevel
            goalMeter.AppendInteger(0); //percentCompletionTowardsNextLevel
            goalMeter.AppendString("friendshipChallenge"); //Type
            goalMeter.AppendInteger(0); //unknown
            goalMeter.AppendInteger(0); //ranks and loop
            Session.SendMessage(goalMeter);
        }

        internal void RequestFloorItems()
        {
        }

        internal void RequestWallItems()
        {
        }

        internal void SaveBranding()
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
            room.GetRoomItemHandler()
                .SetFloorItem(Session, item, item.X, item.Y, item.Rot, false, false, true);
        }

        internal void OnRoomUserAdd()
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
            Response.Init(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
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
            SendResponse();
            Session.SendMessage(RoomFloorAndWallComposer(CurrentLoadingRoom));
            SendResponse();

            Response.Init(LibraryParser.OutgoingRequest("RoomOwnershipMessageComposer"));
            Response.AppendInteger(CurrentLoadingRoom.RoomId);
            Response.AppendBool(CurrentLoadingRoom.CheckRights(Session, true));
            SendResponse();


            /* TODO CHECK */
            foreach (var habbo in CurrentLoadingRoom.UsersWithRights.Select(Oblivion.GetHabboById))
            {
                if (habbo == null) continue;

                Response.Init(LibraryParser.OutgoingRequest("GiveRoomRightsMessageComposer"));
                Response.AppendInteger(CurrentLoadingRoom.RoomId);
                Response.AppendInteger(habbo.Id);
                Response.AppendString(habbo.UserName);
                SendResponse();
            }

            var serverMessage = CurrentLoadingRoom.GetRoomUserManager().SerializeStatusUpdates(true);
            if (serverMessage != null)
                Session.SendMessage(serverMessage);

            if (CurrentLoadingRoom.RoomData.Event != null)
                Oblivion.GetGame().GetRoomEvents().SerializeEventInfo(CurrentLoadingRoom.RoomId);

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
                            Response.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                            Response.AppendInteger(current4.VirtualId);
                            Response.AppendInteger(current4.BotData.DanceId);
                            SendResponse();
                        }
                    }
                    else if (current4.IsDancing)
                    {
                        Response.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                        Response.AppendInteger(current4.VirtualId);
                        Response.AppendInteger(current4.DanceId);
                        SendResponse();
                    }

                    if (current4.IsAsleep)
                    {
                        var sleepMsg = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
                        sleepMsg.AppendInteger(current4.VirtualId);
                        sleepMsg.AppendBool(true);
                        Session.SendMessage(sleepMsg);
                    }

                    if (current4.CarryItemId > 0 && current4.CarryTimer > 0)
                    {
                        Response.Init(LibraryParser.OutgoingRequest("ApplyHanditemMessageComposer"));
                        Response.AppendInteger(current4.VirtualId);
                        Response.AppendInteger(current4.CarryTimer);
                        SendResponse();
                    }

                    if (current4.IsBot) continue;
                    try
                    {
                        if (current4.GetClient() != null && current4.GetClient().GetHabbo() != null)
                        {
                            if (current4.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() != null &&
                                current4.CurrentEffect >= 1)
                            {
                                Response.Init(LibraryParser.OutgoingRequest("ApplyEffectMessageComposer"));
                                Response.AppendInteger(current4.VirtualId);
                                Response.AppendInteger(current4.CurrentEffect);
                                Response.AppendInteger(0);
                                SendResponse();
                            }

                            var serverMessage2 =
                                new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            serverMessage2.AppendInteger(current4.VirtualId);
                            serverMessage2.AppendString(current4.GetClient().GetHabbo().Look);
                            serverMessage2.AppendString(current4.GetClient().GetHabbo().Gender.ToLower());
                            serverMessage2.AppendString(current4.GetClient().GetHabbo().Motto);
                            serverMessage2.AppendInteger(current4.GetClient().GetHabbo().AchievementPoints);
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

        internal void EnterOnRoom()
        {
            if (Oblivion.ShutdownStarted) return;

            var id = Request.GetUInteger();
            var password = Request.GetString();
            PrepareRoomForUser(id, password);
        }

        internal void PrepareRoomForUser(uint id, string pWd, bool isReload = false)
        {
            try
            {
                if (Session?.GetHabbo() == null || Session.GetHabbo().LoadingRoom == id)
                    return;

                if (Oblivion.ShutdownStarted)
                {
                    Session.SendNotif(Oblivion.GetLanguage().GetVar("server_shutdown"));
                    return;
                }

                Session.GetHabbo().LoadingRoom = id;

                Room room;
                if (Session.GetHabbo().InRoom)
                {
                    room = Session.GetHabbo().CurrentRoom;
                    if (room?.GetRoomUserManager() != null)
                        room.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
                }

                room = Oblivion.GetGame().GetRoomManager().LoadRoom(id);
                if (room == null)
                    return;

                if (room.UserCount >= room.RoomData.UsersMax && !Session.GetHabbo().HasFuse("fuse_enter_full_rooms") &&
                    Session.GetHabbo().Id != (ulong) room.RoomData.OwnerId)
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

                    var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("RoomEnterErrorMessageComposer"));
                    serverMessage.AppendInteger(1);
                    Session.SendMessage(serverMessage);
                    var message = new ServerMessage(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                    Session.SendMessage(message);

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
                        serverMessage2.AppendInteger(4);
                        Session.SendMessage(serverMessage2);
                        Response.Init(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                        SendResponse();
                        return;
                    }

                    room.RemoveBan(Session.GetHabbo().Id);
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
                            Session.SendMessage(
                                new ServerMessage(LibraryParser.OutgoingRequest("DoorbellNoOneMessageComposer")));
                            return;
                        }

                        Session.GetHabbo().LastBellId = room.RoomId;
                        var msg = new ServerMessage(LibraryParser.OutgoingRequest("DoorbellMessageComposer"));
                        msg.AppendString("");
                        Session.SendMessage(msg);
                        var serverMessage3 =
                            new ServerMessage(LibraryParser.OutgoingRequest("DoorbellMessageComposer"));
                        serverMessage3.AppendString(Session.GetHabbo().UserName);
                        room.SendMessageToUsersWithRights(serverMessage3);
                        return;
                    }

                    if (room.RoomData.State == 2 &&
                        !string.Equals(pWd, room.RoomData.PassWord, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ClearRoomLoading();

                        Session.GetMessageHandler()
                            .GetResponse()
                            .Init(LibraryParser.OutgoingRequest("RoomErrorMessageComposer"));
                        Session.GetMessageHandler().GetResponse().AppendInteger(-100002);
                        Session.GetMessageHandler().SendResponse();

                        Session.GetMessageHandler()
                            .GetResponse()
                            .Init(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                        Session.GetMessageHandler().GetResponse();
                        Session.GetMessageHandler().SendResponse();
                        return;
                    }
                }

                Response.Init(LibraryParser.OutgoingRequest("PrepareRoomMessageComposer"));
                SendResponse();
                Session.GetHabbo().LoadingChecksPassed = true;
                LoadRoomForUser();

                if (!string.IsNullOrEmpty(room.RoomVideo))
                {
                    Oblivion.GetWebSocket().SendMessage(Session.GetHabbo().WebSocketConnId, $"2|{room.RoomVideo}");
                }

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

        internal void ReqLoadRoomForUser()
        {
            LoadRoomForUser();
        }

        internal void LoadRoomForUser()
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

            Response.Init(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
            Response.AppendInteger(CurrentLoadingRoom.LoadedGroups.Count);
            /* TODO CHECK */
            foreach (var guild1 in CurrentLoadingRoom.LoadedGroups.ToList())
            {
                Response.AppendInteger(guild1.Key);
                Response.AppendString(guild1.Value);
            }

            SendResponse();

            Response.Init(LibraryParser.OutgoingRequest("InitialRoomInfoMessageComposer"));
            Response.AppendString(currentLoadingRoom.RoomData.ModelName);
            Response.AppendInteger(currentLoadingRoom.RoomId);
            SendResponse();
            if (Session.GetHabbo().SpectatorMode)
            {
                Response.Init(LibraryParser.OutgoingRequest("SpectatorModeMessageComposer"));
                SendResponse();
            }

            if (currentLoadingRoom.RoomData.WallPaper != "0.0")
            {
                Response.Init(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
                Response.AppendString("wallpaper");
                Response.AppendString(currentLoadingRoom.RoomData.WallPaper);
                SendResponse();
            }

            if (currentLoadingRoom.RoomData.Floor != "0.0")
            {
                Response.Init(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
                Response.AppendString("floor");
                Response.AppendString(currentLoadingRoom.RoomData.Floor);
                SendResponse();
            }

            Response.Init(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
            Response.AppendString("landscape");
            Response.AppendString(currentLoadingRoom.RoomData.LandScape);
            SendResponse();


            if (Session?.GetHabbo()?.RatedRooms != null)
            {
                Response.Init(LibraryParser.OutgoingRequest("RoomRatingMessageComposer"));
                Response.AppendInteger(currentLoadingRoom.RoomData.Score);
                Response.AppendBool(!Session.GetHabbo().RatedRooms.Contains(currentLoadingRoom.RoomId) &&
                                    !currentLoadingRoom.CheckRights(Session, true));
                SendResponse();
            }

            Response.Init(LibraryParser.OutgoingRequest("RoomUpdateMessageComposer"));
            Response.AppendInteger(currentLoadingRoom.RoomId);

            SendResponse();
        }

        internal void ClearRoomLoading()
        {
            if (Session?.GetHabbo() == null)
                return;
            Session.GetHabbo().LoadingRoom = 0u;
            Session.GetHabbo().LoadingChecksPassed = false;
        }

        internal void Move()
        {
            if (Session?.GetHabbo() == null)
                return;
            var currentRoom = Session.GetHabbo().CurrentRoom;

            var roomUserByHabbo = currentRoom?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null || !roomUserByHabbo.CanWalk)
                return;


            var targetX = Request.GetInteger();
            var targetY = Request.GetInteger();

            if (targetX == roomUserByHabbo.X && targetY == roomUserByHabbo.Y)
                return;

            roomUserByHabbo.MoveTo(targetX, targetY);

            if (!roomUserByHabbo.RidingHorse)
                return;

            var roomUserByVirtualId = currentRoom.GetRoomUserManager()
                .GetRoomUserByVirtualId((int) roomUserByHabbo.HorseId);


            roomUserByVirtualId.MoveTo(targetX, targetY);
        }

        internal void CanCreateRoom()
        {
            if (Session?.GetHabbo() == null)
                return;
            Response.Init(LibraryParser.OutgoingRequest("CanCreateRoomMessageComposer"));
            Response.AppendInteger(Session.GetHabbo().Data.Rooms.Count >= 75 ? 1 : 0);
            Response.AppendInteger(75);
            SendResponse();
        }

        internal void CreateRoom()
        {
            if (Session?.GetHabbo()?.Data?.Rooms == null)
                return;
            if (Session.GetHabbo().Data.Rooms.Count >= 75)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("user_has_more_then_75_rooms"));
                return;
            }

            if (Oblivion.GetUnixTimeStamp() - Session.GetHabbo().LastSqlQuery < 20)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("user_create_room_flood_error"));
                return;
            }

            var name = Request.GetString();
            var description = Request.GetString();
            var roomModel = Request.GetString();
            var category = Request.GetInteger();
            var maxVisitors = Request.GetInteger();
            var tradeState = Request.GetInteger();

            var data = Oblivion.GetGame()
                .GetRoomManager()
                .CreateRoom(Session, name, description, roomModel, category, maxVisitors, tradeState);
            if (data == null)
                return;

            Session.GetHabbo().LastSqlQuery = Oblivion.GetUnixTimeStamp();
            Response.Init(LibraryParser.OutgoingRequest("OnCreateRoomInfoMessageComposer"));
            Response.AppendInteger(data.Id);
            Response.AppendString(data.Name);
            SendResponse();
        }

        internal void GetRoomEditData()
        {
            if (Session?.GetHabbo() == null)
                return;
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(Convert.ToUInt32(Request.GetInteger()));
            if (room == null)
                return;

            GetResponse().Init(LibraryParser.OutgoingRequest("RoomSettingsDataMessageComposer"));
            GetResponse().AppendInteger(room.RoomId);
            GetResponse().AppendString(room.RoomData.Name);
            GetResponse().AppendString(room.RoomData.Description);
            GetResponse().AppendInteger(room.RoomData.State);
            GetResponse().AppendInteger(room.RoomData.Category);
            GetResponse().AppendInteger(room.RoomData.UsersMax);
            GetResponse()
                .AppendInteger(room.RoomData.Model.MapSizeX * room.RoomData.Model.MapSizeY > 200 ? 50 : 25);

            GetResponse().AppendInteger(room.TagCount);
            foreach (var s in room.RoomData.Tags)
                GetResponse().AppendString(s);
            GetResponse().AppendInteger(room.RoomData.TradeState);
            GetResponse().AppendInteger(room.RoomData.AllowPets);
            GetResponse().AppendInteger(room.RoomData.AllowPetsEating);
            GetResponse().AppendInteger(room.RoomData.AllowWalkThrough);
            GetResponse().AppendInteger(room.RoomData.HideWall);
            GetResponse().AppendInteger(room.RoomData.WallThickness);
            GetResponse().AppendInteger(room.RoomData.FloorThickness);
            GetResponse().AppendInteger(room.RoomData.ChatType);
            GetResponse().AppendInteger(room.RoomData.ChatBalloon);
            GetResponse().AppendInteger(room.RoomData.ChatSpeed);
            GetResponse().AppendInteger(room.RoomData.ChatMaxDistance);
            GetResponse().AppendInteger(room.RoomData.ChatFloodProtection > 2 ? 2 : room.RoomData.ChatFloodProtection);
            GetResponse().AppendBool(false); //allow_dyncats_checkbox
            GetResponse().AppendInteger(room.RoomData.WhoCanMute);
            GetResponse().AppendInteger(room.RoomData.WhoCanKick);
            GetResponse().AppendInteger(room.RoomData.WhoCanBan);
            SendResponse();
        }

        internal void RoomSettingsOkComposer(uint roomId)
        {
            if (Session?.GetHabbo() == null)
                return;
            GetResponse().Init(LibraryParser.OutgoingRequest("RoomSettingsSavedMessageComposer"));
            GetResponse().AppendInteger(roomId);
            SendResponse();
        }

        internal void RoomUpdatedOkComposer(uint roomId)
        {
            if (Session?.GetHabbo() == null)
                return;
            GetResponse().Init(LibraryParser.OutgoingRequest("RoomUpdateMessageComposer"));
            GetResponse().AppendInteger(roomId);
            SendResponse();
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

        internal void ParseRoomDataInformation()
        {
            if (Session?.GetHabbo() == null)
                return;
            var id = Request.GetUInteger();
            var num = Request.GetInteger();
            var num2 = Request.GetInteger();
            var room = Oblivion.GetGame().GetRoomManager().LoadRoom(id);
            if (room == null) return;
            if (num == 0 && num2 == 1)
            {
                SerializeRoomInformation(room, false);
                return;
            }

            SerializeRoomInformation(room, true);
        }

        internal void SerializeRoomInformation(Room room, bool show)
        {
            if (room?.RoomData == null)
                return;

            if (Session?.GetHabbo() == null)
                return;

            room.RoomData.SerializeRoomData(Response, Session, true, false, show);
            SendResponse();

            if (room.UsersWithRights == null) return;

            Response.Init(LibraryParser.OutgoingRequest("LoadRoomRightsListMessageComposer"));
            Response.AppendInteger(room.RoomData.Id);

            Response.StartArray();
            foreach (var id in room.UsersWithRights)
            {
                var habboForId = Oblivion.GetHabboById(id);

                if (habboForId == null) continue;

                Response.AppendInteger(habboForId.Id);
                Response.AppendString(habboForId.UserName);
                Response.SaveArray();
            }

            Response.EndArray();

            SendResponse();
        }

        internal void SaveRoomData()
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

            Request.GetBool(); //allow_dyncats_checkbox
            var flatCat = Oblivion.GetGame().GetNavigator().GetFlatCat(room.RoomData.Category);
            if (flatCat == null || flatCat.MinRank > Session.GetHabbo().Rank) room.RoomData.Category = 0;

            room.ClearTags();
            room.AddTagRange(tags);

            RoomSettingsOkComposer(room.RoomId);
            RoomUpdatedOkComposer(room.RoomId);
            Session.GetHabbo().CurrentRoom.SendMessage(RoomFloorAndWallComposer(room));
            Session.GetHabbo().CurrentRoom.SendMessage(SerializeRoomChatOption(room));
            room.RoomData.SerializeRoomData(Response, Session, false, true);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModWalkthroughSeen", 1);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModChatScrollSpeedSeen", 1);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModChatFloodFilterSeen", 1);
            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModChatHearRangeSeen", 1);
        }


        internal void GetBannedUsers()
        {
            if (Session?.GetHabbo() == null)
                return;

            var num = Request.GetUInteger();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(num);
            if (room == null)
                return;
            var list = room.BannedUsers();
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("RoomBannedListMessageComposer"));
            msg.AppendInteger(num);
            var count = list.Count;
            if (count <= 0)
            {
                count = 1;
            }

            msg.AppendInteger(count);
            if (list.Count <= 0)
            {
                msg.AppendInteger(-1);
                msg.AppendString("");
            }
            else
                foreach (var current in list)
                {
                    msg.AppendInteger(current);
                    msg.AppendString(Oblivion.GetHabboById(current) != null
                        ? Oblivion.GetHabboById(current).UserName
                        : "Undefined");
                }

            Session.SendMessage(msg);
        }

        internal void UsersWithRights()
        {
            if (Session?.GetHabbo()?.CurrentRoom == null)
                return;

            Response.Init(LibraryParser.OutgoingRequest("LoadRoomRightsListMessageComposer"));
            Response.AppendInteger(Session.GetHabbo().CurrentRoom.RoomId);
            Response.AppendInteger(Session.GetHabbo().CurrentRoom.UsersWithRights.Count);
            /* TODO CHECK */
            foreach (var current in Session.GetHabbo().CurrentRoom.UsersWithRights)
            {
                var habboForId = Oblivion.GetHabboById(current);
                if (habboForId == null) continue;
                Response.AppendInteger(current);
                Response.AppendString(habboForId.Look);
            }

            SendResponse();
        }

        internal void UnbanUser()
        {
            if (Session?.GetHabbo() == null)
                return;
            var num = Request.GetUInteger();
            var num2 = Request.GetUInteger();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(num2);
            if (room == null)
                return;
            if (num <= 0) return;
            room.Unban(num);
            Response.Init(LibraryParser.OutgoingRequest("RoomUnbanUserMessageComposer"));
            Response.AppendInteger(num2);
            Response.AppendInteger(num);
            SendResponse();
        }

        internal void GiveRights()
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
                Session.SendNotif(Oblivion.GetLanguage().GetVar("no_room_rights_error"));
                return;
            }

            if (num == 0)
                return;
            room.UsersWithRights.Add(num);
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("INSERT INTO rooms_rights (room_id,user_id) VALUES (",
                    room.RoomId, ",", num, ")"));
            }

            if (roomUserByHabbo != null && !roomUserByHabbo.IsBot)
            {
                if (!roomUserByHabbo.IsBot)
                {
                    roomUserByHabbo.AddStatus("flatctrl 1", "");
                    Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                    Response.AppendInteger(1);
                    roomUserByHabbo.GetClient().SendMessage(GetResponse());
                }

                Response.Init(LibraryParser.OutgoingRequest("GiveRoomRightsMessageComposer"));
                Response.AppendInteger(room.RoomId);
                Response.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Id);
                Response.AppendString(roomUserByHabbo.GetClient().GetHabbo().UserName);
                SendResponse();
                roomUserByHabbo.UpdateNeeded = true;
            }

            UsersWithRights();
        }

        internal void TakeRights()
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
                        Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        Response.AppendInteger(0);
                        roomUserByHabbo.GetClient().SendMessage(GetResponse());
                        roomUserByHabbo.RemoveStatus("flatctrl 1");
                        roomUserByHabbo.UpdateNeeded = true;
                    }

                    Response.Init(LibraryParser.OutgoingRequest("RemoveRightsMessageComposer"));
                    Response.AppendInteger(room.RoomId);
                    Response.AppendInteger(num2);
                    SendResponse();
                }

                UsersWithRights();
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery($"DELETE FROM rooms_rights WHERE {stringBuilder}");
                }
            }
        }

        internal void TakeAllRights()
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
                Response.Init(LibraryParser.OutgoingRequest("RemoveRightsMessageComposer"));
                Response.AppendInteger(room.RoomId);
                Response.AppendInteger(num);
                SendResponse();
                if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
                    continue;
                Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                Response.AppendInteger(0);
                roomUserByHabbo.GetClient().SendMessage(GetResponse());
                roomUserByHabbo.RemoveStatus("flatctrl 1");
                roomUserByHabbo.UpdateNeeded = true;
            }

            using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.RunFastQuery($"DELETE FROM rooms_rights WHERE room_id = {room.RoomId}");
            }

            room.UsersWithRights.Clear();
            UsersWithRights();
        }

        internal void KickUser()
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
            room.GetRoomUserManager().RemoveUserFromRoom(roomUserByHabbo, true, true);
            roomUserByHabbo.GetClient().CurrentRoomUserId = -1;
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModKickSeen", 1);
        }

        internal void BanUser()
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
            room.AddBan(num, time);
            room.GetRoomUserManager().RemoveUserFromRoom(roomUserByHabbo, true, true);
            Session.CurrentRoomUserId = -1;
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModBanSeen", 1);
        }

        internal void SetHomeRoom()
        {
            var roomId = Request.GetUInteger();
            var data = Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);

            if (roomId != 0 && data == null)
            {
                Session.GetHabbo().HomeRoom = roomId
                    ;
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Concat("UPDATE users SET home_room = ", roomId, " WHERE id = ",
                        Session.GetHabbo().Id));
                }

                Response.Init(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));
                Response.AppendInteger(roomId);
                Response.AppendInteger(0);
                SendResponse();
            }
        }

        internal void DeleteRoom()
        {
            var roomId = Request.GetUInteger();
            if (Session?.GetHabbo() == null || Session.GetHabbo().Data.Rooms == null)
                return;

            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);

            if (room?.RoomData == null)
                return;

            if (room.RoomData.Owner != Session.GetHabbo().UserName && Session.GetHabbo().Rank <= 6u)
                return;

            room.GetRoomItemHandler().RemoveAllFurniture(Session);

            var roomData = room.RoomData;
            Oblivion.GetGame().GetRoomManager().UnloadRoom(room, "Delete room");

            if (roomData == null || Session == null)
                return;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery($"DELETE FROM rooms_data WHERE id = {roomId}");
                queryReactor.RunFastQuery($"DELETE FROM users_favorites WHERE room_id = {roomId}");
                queryReactor.RunFastQuery($"DELETE FROM items_rooms WHERE room_id = {roomId}");
                queryReactor.RunFastQuery($"DELETE FROM rooms_rights WHERE room_id = {roomId}");
                queryReactor.RunFastQuery($"UPDATE users SET home_room = '0' WHERE home_room = {roomId}");
            }

            if (Session.GetHabbo().Rank > 5u && Session.GetHabbo().UserName != roomData.Owner)
                Oblivion.GetGame()
                    .GetModerationTool()
                    .LogStaffEntry(Session.GetHabbo().UserName, roomData.Name, "Room deletion",
                        $"Deleted room ID {roomData.Id}");

            Session.GetHabbo().Data.Rooms.Remove(roomId);
        }

        internal void AirClickUser()
        {
            var userId = Request.GetUInteger();
            var habbo = Oblivion.GetHabboById(userId);
            if (habbo == null) return;
            var createTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(habbo.CreateDate);

            var msg = new ServerMessage(LibraryParser.OutgoingRequest("UserProfileMessageComposer"));
            msg.AppendInteger(habbo.Id);
            msg.AppendString(habbo.UserName);
            msg.AppendString(habbo.Look);
            msg.AppendString(habbo.Motto);
            msg.AppendString(createTime.ToString("dd/MM/yyyy"));
            msg.AppendInteger(habbo.AchievementPoints);
            msg.AppendInteger(GetFriendsCount(userId));
            msg.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                           Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id));
            msg.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                           !Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id) &&
                           Session.GetHabbo().GetMessenger().RequestExists(habbo.Id));
            msg.AppendBool(Oblivion.GetGame().GetClientManager().GetClientByUserId(habbo.Id) != null);
            var groups = habbo.UserGroups;
            msg.AppendInteger(groups.Count);
            /* TODO CHECK */
            foreach (var group in groups.Select(groupUs => Oblivion.GetGame().GetGroupManager()
                .GetGroup(groupUs.GroupId))
            )
                if (group != null)
                {
                    msg.AppendInteger(group.Id);
                    msg.AppendString(group.Name);
                    msg.AppendString(group.Badge);
                    msg.AppendString(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour1, true));
                    msg.AppendString(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour2, false));
                    msg.AppendBool(group.Id == habbo.FavouriteGroup);
                    msg.AppendInteger(-1);
                    msg.AppendBool(group.HasForum);
                }
                else
                {
                    msg.AppendInteger(1);
                    msg.AppendString("THIS GROUP IS INVALID");
                    msg.AppendString("");
                    msg.AppendString("");
                    msg.AppendString("");
                    msg.AppendBool(false);
                    msg.AppendInteger(-1);
                    msg.AppendBool(false);
                }

            if (habbo.PreviousOnline == 0)
                msg.AppendInteger(-1);
            else if (Oblivion.GetGame().GetClientManager().GetClientByUserId(habbo.Id) == null)
                msg.AppendInteger((Oblivion.GetUnixTimeStamp() - habbo.PreviousOnline));
            else
                msg.AppendInteger((Oblivion.GetUnixTimeStamp() - habbo.LastOnline));

            msg.AppendBool(true);
            Session.SendMessage(msg);

            if (habbo.GetBadgeComponent()?.BadgeList == null) return;

            var msg2 = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            msg2.AppendInteger(habbo.Id);

            var badges = habbo.GetBadgeComponent().BadgeList.Values.Where(badge => badge.Slot > 0)
                .ToList();
            msg2.AppendInteger(badges.Count);
            foreach (
                var badge in badges)
            {
                msg2.AppendInteger(badge.Slot);
                msg2.AppendString(badge.Code);
            }

            Session.SendMessage(msg2);
        }

        internal void LookAt()
        {
            if (Session?.GetHabbo() == null) return;

            var room = Session.GetHabbo().CurrentRoom;

            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            roomUserByHabbo.UnIdle();
            var x = Request.GetInteger();
            var y = Request.GetInteger();

            var rotation = PathFinder.CalculateRotation(roomUserByHabbo.X, roomUserByHabbo.Y, x, y);
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

        internal void StartTyping()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TypingStatusMessageComposer"));
            serverMessage.AppendInteger(roomUserByHabbo.VirtualId);
            serverMessage.AppendInteger(1);
            room.SendMessage(serverMessage);
        }

        internal void StopTyping()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TypingStatusMessageComposer"));
            serverMessage.AppendInteger(roomUserByHabbo.VirtualId);
            serverMessage.AppendInteger(0);
            room.SendMessage(serverMessage);
        }

        internal void IgnoreUser()
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
            Response.Init(LibraryParser.OutgoingRequest("UpdateIgnoreStatusMessageComposer"));
            Response.AppendInteger(1);
            Response.AppendString(text);
            SendResponse();
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModIgnoreSeen", 1);
        }

        internal void UnignoreUser()
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
            Response.Init(LibraryParser.OutgoingRequest("UpdateIgnoreStatusMessageComposer"));
            Response.AppendInteger(3);
            Response.AppendString(text);
            SendResponse();
        }

        internal void CanCreateRoomEvent()
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
            Response.AppendInteger(i);
        }

        internal void Sign()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            roomUserByHabbo.UnIdle();
            var value = Request.GetInteger();
            roomUserByHabbo.AddStatus("sign", Convert.ToString(value));
            roomUserByHabbo.UpdateNeeded = true;
            roomUserByHabbo.SignTime = Oblivion.GetUnixTimeStamp() + 5;
        }

        internal void InitRoomGroupBadges()
        {
            Oblivion.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().LoadingRoom);
        }

        internal void RateRoom()
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

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Concat("UPDATE rooms_data SET score = ", room.RoomData.Score,
                        " WHERE id = ", room.RoomId));
                }

                Session.GetHabbo().RatedRooms.Add(room.RoomId);
                Response.Init(LibraryParser.OutgoingRequest("RoomRatingMessageComposer"));
                Response.AppendInteger(room.RoomData.Score);
                Response.AppendBool(room.CheckRights(Session, true));
                SendResponse();
            }
        }

        internal void Dance()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            roomUserByHabbo.UnIdle();
            var num = Request.GetInteger();
            if (num < 0 || num > 4)
                num = 0;
            if (num > 0 && roomUserByHabbo.CarryItemId > 0)
                roomUserByHabbo.CarryItem(0);
            roomUserByHabbo.DanceId = num;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
            serverMessage.AppendInteger(roomUserByHabbo.VirtualId);
            serverMessage.AppendInteger(num);
            room.SendMessage(serverMessage);
            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialDance);
            if (room.GetRoomUserManager().GetRoomUsers().Count > 19)
                Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.MassDance);
        }

        internal void AnswerDoorbell()
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
                clientByUserName.GetMessageHandler()
                    .Response.Init(LibraryParser.OutgoingRequest("DoorbellOpenedMessageComposer"));
                clientByUserName.GetMessageHandler().Response.AppendString("");
                clientByUserName.GetMessageHandler().SendResponse();
                return;
            }

            if (clientByUserName.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
            {
                clientByUserName.GetMessageHandler()
                    .Response.Init(LibraryParser.OutgoingRequest("DoorbellNoOneMessageComposer"));
                clientByUserName.GetMessageHandler().Response.AppendString("");
                clientByUserName.GetMessageHandler().SendResponse();
            }
        }

        internal void AlterRoomFilter()
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
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("DELETE FROM rooms_wordfilter WHERE room_id = @id AND word = @word");
                    queryReactor.AddParameter("id", num);
                    queryReactor.AddParameter("word", text);
                    queryReactor.RunQuery();
                    return;
                }
            }

            if (room.RoomData.WordFilter.Contains(text))
                return;
            if (text.Contains("+"))
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("character_error_plus"));
                return;
            }

            room.RoomData.WordFilter.Add(text);
            using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.SetQuery("INSERT INTO rooms_wordfilter (room_id, word) VALUES (@id, @word);");
                queryreactor2.AddParameter("id", num);
                queryreactor2.AddParameter("word", text);
                queryreactor2.RunQuery();
            }
        }

        internal void GetRoomFilter()
        {
            var roomId = Request.GetUInteger();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);
            if (room == null || !room.CheckRights(Session, true))
                return;
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("RoomLoadFilterMessageComposer"));
            serverMessage.AppendInteger(room.RoomData.WordFilter.Count);
            /* TODO CHECK */
            foreach (var current in room.RoomData.WordFilter)
                serverMessage.AppendString(current);
            Response = serverMessage;
            SendResponse();

            Oblivion.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_SelfModRoomFilterSeen", 1);
        }

        internal void ApplyRoomEffect()
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

                    Oblivion.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoFloor", 1);
                    Oblivion.GetGame()
                        .GetQuestManager()
                        .ProgressUserQuest(Session, QuestType.FurniDecorationFloor);
                    break;

                case "wallpaper":

                    room.RoomData.WallPaper = item.ExtraData;

                    Oblivion.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoWallpaper", 1);
                    Oblivion.GetGame()
                        .GetQuestManager()
                        .ProgressUserQuest(Session, QuestType.FurniDecorationWall);
                    break;

                case "landscape":

                    room.RoomData.LandScape = item.ExtraData;

                    Oblivion.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Concat("UPDATE rooms_data SET ", type, " = @extradata WHERE id = ",
                    room.RoomId));
                queryReactor.AddParameter("extradata", item.ExtraData);
                queryReactor.RunQuery();
                queryReactor.RunFastQuery($"DELETE FROM items_rooms WHERE id={item.Id} LIMIT 1");
            }

            Session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false, 0);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
            serverMessage.AppendString(type);
            serverMessage.AppendString(item.ExtraData);
            room.SendMessage(serverMessage);
        }

        internal void PromoteRoom()
        {
            var pageId = Request.GetInteger();
            var item = Request.GetUInteger();

            var page2 = Oblivion.GetGame().GetCatalog().GetPage(pageId);
            var catalogItem = page2?.GetItem(item);

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
            room.Start(Oblivion.GetGame().GetRoomManager().GenerateNullableRoomData(num), true);

            if (!room.CheckRights(Session, true)) return;
            if (catalogItem.CreditsCost > 0)
            {
                if (catalogItem.CreditsCost > Session.GetHabbo().Credits) return;
                Session.GetHabbo().Credits -= (int) catalogItem.CreditsCost;
                Session.GetHabbo().UpdateCreditsBalance(true);
            }

            if (catalogItem.DucketsCost > 0)
            {
                if (catalogItem.DucketsCost > Session.GetHabbo().ActivityPoints) return;
                Session.GetHabbo().ActivityPoints -= (int) catalogItem.DucketsCost;
                Session.GetHabbo().UpdateActivityPointsBalance(true);
            }

            if (catalogItem.DiamondsCost > 0)
            {
                if (catalogItem.DiamondsCost > Session.GetHabbo().Diamonds) return;
                Session.GetHabbo().Diamonds -= (int) catalogItem.DiamondsCost;
                Session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
            }

            Session.SendMessage(CatalogPageComposer.PurchaseOk(catalogItem, catalogItem.Items));

            if (room.RoomData.Event != null && !room.RoomData.Event.HasExpired)
            {
                room.RoomData.Event.Time = Oblivion.GetUnixTimeStamp();
                Oblivion.GetGame().GetRoomEvents().SerializeEventInfo(room.RoomId);
            }
            else
            {
                Oblivion.GetGame().GetRoomEvents().AddNewEvent(room.RoomId, text, text2, Session, 7200, category);
                Oblivion.GetGame().GetRoomEvents().SerializeEventInfo(room.RoomId);
            }

            Session.GetHabbo().GetBadgeComponent().GiveBadge("RADZZ", true, Session);
        }

        internal void GetPromotionableRooms()
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("CatalogPromotionGetRoomsMessageComposer"));
            serverMessage.AppendBool(true);
            serverMessage.AppendInteger(Session.GetHabbo().Data.Rooms.Count);
            /* TODO CHECK */
            foreach (var data in Session.GetHabbo().Data.Rooms)
            {
                var current = Oblivion.GetGame().GetRoomManager().GenerateRoomData(data);

                serverMessage.AppendInteger(current.Id);
                serverMessage.AppendString(current.Name);
                serverMessage.AppendBool(false);
            }

            Response = serverMessage;
            SendResponse();
        }

        internal void SaveHeightmap()
        {
            if (Session?.GetHabbo() != null)
            {
                var room = Session.GetHabbo().CurrentRoom;

                if (room == null)
                {
                    Session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_not_in_room"));
                    return;
                }

                if (!room.CheckRights(Session, true))
                {
                    Session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_not_his_room"));
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
                    Session.SendNotif(Oblivion.GetLanguage().GetVar("invalid_room_length"));
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
                    Session.SendNotif(Oblivion.GetLanguage().GetVar("user_floor_editor_error"));

                    return;
                }

                if (heightMap.Last() == Convert.ToChar(13))
                    heightMap = heightMap.Remove(heightMap.Length - 1);

                if (heightMap.Length > 74200)
                {
                    var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    message.AppendString("floorplan_editor.error");
                    message.AppendInteger(1);
                    message.AppendString("errors");
                    message.AppendString(
                        "(general): too large height (max 128 tiles)\r(general): too large area (max 1800 tiles)");
                    Session.SendMessage(message);

                    return;
                }

                if (heightMap.Split((char) 13).Length - 1 < doorY)
                {
                    var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    message.AppendString("floorplan_editor.error");
                    message.AppendInteger(1);
                    message.AppendString("errors");
                    message.AppendString("Y: Door is in invalid place.");
                    Session.SendMessage(message);

                    return;
                }

                var lines = heightMap.Split((char) 13);
                var lineWidth = lines[0].Length;
                for (var i = 1; i < lines.Length; i++)
                    if (lines[i].Length != lineWidth)
                    {
                        var message =
                            new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                        message.AppendString("floorplan_editor.error");
                        message.AppendInteger(1);
                        message.AppendString("errors");
                        message.AppendString("(general): Line " + (i + 1) + " is of different length than line 1");
                        Session.SendMessage(message);

                        return;
                    }

                double doorZ;
                var charDoor = lines[doorY][doorX];
                if (charDoor >= (char) 97 && charDoor <= 119) // a-w
                    doorZ = charDoor - 87;
                else
                    double.TryParse(charDoor.ToString(), out doorZ);
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(
                        "REPLACE INTO rooms_models_customs (roomid,door_x,door_y,door_z,door_dir,heightmap) VALUES ('" +
                        room.RoomId + "', '" + doorX + "','" +
                        doorY + "','" + doorZ.ToString(CultureInfo.InvariantCulture).Replace(',', '.') + "','" +
                        doorOrientation + "',@newmodel)");
                    queryReactor.AddParameter("newmodel", heightMap);
                    queryReactor.RunQuery();

                    room.RoomData.WallHeight = wallHeight;
                    room.RoomData.WallThickness = wallThickness;
                    room.RoomData.FloorThickness = floorThickness;
                    room.RoomData.Model.DoorZ = doorZ;

                    Oblivion.GetGame().GetAchievementManager()
                        .ProgressUserAchievement(Session, "ACH_RoomDecoHoleFurniCount", 1);

                    queryReactor.RunFastQuery(
                        $"UPDATE rooms_data SET model_name = 'custom', wallthick = '{wallThickness}', floorthick = '{floorThickness}', walls_height = '{wallHeight}' WHERE id = {room.RoomId};");
                }

                room.ResetGameMap("custom", wallHeight, wallThickness, floorThickness);
                Oblivion.GetGame().GetRoomManager().UnloadRoom(room, "Reload floor");

                var forwardToRoom = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                forwardToRoom.AppendInteger(room.RoomId);
                Session?.SendMessage(forwardToRoom);
            }
        }

        internal void PlantMonsterplant(RoomItem mopla, Room room)
        {
            int rarity = 0, internalRarity = 0;
            if (room == null || mopla == null)
                return;

            if (mopla.GetBaseItem().InteractionType != Interaction.Moplaseed &&
                mopla.GetBaseItem().InteractionType != Interaction.RareMoplaSeed)
                return;
            if (string.IsNullOrEmpty(mopla.ExtraData) || mopla.ExtraData == "0")
                rarity = 1;
            if (!string.IsNullOrEmpty(mopla.ExtraData) && mopla.ExtraData != "0")
                rarity = int.TryParse(mopla.ExtraData, out internalRarity) ? internalRarity : 1;

            var getX = mopla.X;
            var getY = mopla.Y;
            room.GetRoomItemHandler().RemoveFurniture(Session, mopla.Id, false);
            var pet = CatalogManager.CreatePet(Session.GetHabbo().Id, "Monsterplant", 16, "0", "0", rarity);
            Response.Init(LibraryParser.OutgoingRequest("SendMonsterplantIdMessageComposer"));
            Response.AppendInteger(pet.PetId);
            SendResponse();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE bots SET room_id = '", room.RoomId, "', x = '", getX,
                    "', y = '", getY, "' WHERE id = '", pet.PetId, "'"));
            }

            pet.PlacedInRoom = true;
            pet.RoomId = room.RoomId;
            var bot = new RoomBot(pet.PetId, pet.OwnerId, pet.RoomId, AiType.Pet, "freeroam", pet.Name, "", pet.Look,
                getX, getY, 0.0, 4, 0, 0, 0, 0, null, null, "", 0, false);
            room.GetRoomUserManager().DeployBot(bot, pet);

            if (pet.DbState != DatabaseUpdateState.NeedsInsert)
                pet.DbState = DatabaseUpdateState.NeedsUpdate;

            using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.RunFastQuery($"DELETE FROM items_rooms WHERE id = {mopla.Id}");
                room.GetRoomUserManager().SavePets(queryreactor2);
            }
        }

        internal void KickBot()
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true))
                return;
            var roomUserByVirtualId = room.GetRoomUserManager().GetRoomUserByVirtualId(Request.GetInteger());
            if (roomUserByVirtualId == null || !roomUserByVirtualId.IsBot)
                return;

            room.GetRoomUserManager().RemoveBot(roomUserByVirtualId.VirtualId, true);
        }

        internal void PlacePet()
        {
            var room = Session.GetHabbo().CurrentRoom;

            if (room == null || !room.RoomData.AllowPets && !room.CheckRights(Session, true) ||
                !room.CheckRights(Session, true))
                return;
            if (room.GetRoomUserManager().GetPets().Count >= 10)
            {
                Session.SendWhisper("Apenas 10 pets por sala!");
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

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery("UPDATE bots SET room_id = '" + room.RoomId + "', x = '" + x + "', y = '" +
                                          y + "' WHERE id = '" + petId + "'");
            }

            pet.PlacedInRoom = true;
            pet.RoomId = room.RoomId;

            room.GetRoomUserManager()
                .DeployBot(
                    new RoomBot(pet.PetId, Convert.ToUInt32(pet.OwnerId), pet.RoomId, AiType.Pet, "freeroam", pet.Name,
                        "", pet.Look, x, y, 0.0, 4, 0, 0, 0, 0, null, null, "", 0, false), pet);
            Session.GetHabbo().GetInventoryComponent().MovePetToRoom(pet.PetId);
            if (pet.DbState != DatabaseUpdateState.NeedsInsert)
                pet.DbState = DatabaseUpdateState.NeedsUpdate;
            using (var queryreactor2 = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                room.GetRoomUserManager().SavePets(queryreactor2);
            }

            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        internal void UpdateEventInfo()
        {
            Request.GetInteger();
            var original = Request.GetString();
            var original2 = Request.GetString();
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(Session, true) || room.RoomData.Event == null)
                return;
            room.RoomData.Event.Name = original;
            room.RoomData.Event.Description = original2;
            Oblivion.GetGame().GetRoomEvents().UpdateEvent(room.RoomData.Event);
        }

        internal void HandleBotSpeechList()
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
                serverMessage.AppendInteger(botId);
                serverMessage.AppendInteger(num2);
                serverMessage.AppendString(text);
                Response = serverMessage;
                SendResponse();
                return;
            }

            if (num3 != 5)
                return;

            var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("BotSpeechListMessageComposer"));
            serverMessage2.AppendInteger(botId);
            serverMessage2.AppendInteger(num2);
            serverMessage2.AppendString(bot.BotData.Name);

            Response = serverMessage2;
            SendResponse();
        }

        internal void ManageBotActions()
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
                        var array = data.Split(new[] {";#;"}, StringSplitOptions.None);

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
                        using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        {
                            queryReactor.SetQuery(
                                "UPDATE bots SET automatic_chat = @autochat, speaking_interval = @interval, mix_phrases = @mix_phrases, speech = @speech WHERE id = @botid");

                            queryReactor.AddParameter("autochat", speak ? "1" : "0");
                            queryReactor.AddParameter("interval", speechDelay);
                            queryReactor.AddParameter("mix_phrases", mix ? "1" : "0");
                            queryReactor.AddParameter("speech", speechs);
                            queryReactor.AddParameter("botid", botId);
                            queryReactor.RunQuery();
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
                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery("UPDATE bots SET walk_mode = @walkmode WHERE id = @botid");
                        queryReactor.AddParameter("walkmode", bot.BotData.WalkingMode);
                        queryReactor.AddParameter("botid", botId);
                        queryReactor.RunQuery();
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
                        BotErrorComposer(4);
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
            serverMessage.AppendInteger(bot.VirtualId);
            serverMessage.AppendInteger(bot.BotData.DanceId);
            Session.GetHabbo().CurrentRoom.SendMessage(serverMessage);
            IL_439:
            if (!flag)
            {
                var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
                serverMessage2.AppendInteger(1);
                bot.Serialize(serverMessage2);
                room.SendMessage(serverMessage2);
            }
        }

        internal void BotErrorComposer(int errorid)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("GeneralErrorHabboMessageComposer"));
            serverMessage.AppendInteger(errorid);
            Session.SendMessage(serverMessage);
        }

        internal void RoomOnLoad()
        {
            // TODO!
            Response.Init(LibraryParser.OutgoingRequest("SendRoomCampaignFurnitureMessageComposer"));
            Response.AppendInteger(0);
            SendResponse();
        }

        internal void MuteAll()
        {
            var currentRoom = Session.GetHabbo().CurrentRoom;
            if (currentRoom == null || !currentRoom.CheckRights(Session, true))
                return;
            currentRoom.RoomMuted = !currentRoom.RoomMuted;

            Response.Init(LibraryParser.OutgoingRequest("RoomMuteStatusMessageComposer"));
            Response.AppendBool(currentRoom.RoomMuted);
            Session.SendMessage(Response);
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SelfModMuteSeen", 1);
        }

        internal void HomeRoom()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));
            GetResponse().AppendInteger(Session.GetHabbo().HomeRoom);
            GetResponse().AppendInteger(0);
            SendResponse();
        }

        internal void RemoveFavouriteRoom()
        {
            if (Session.GetHabbo() == null)
                return;
            var num = Request.GetUInteger();
            Session.GetHabbo().Data.FavouritedRooms.Remove(num);
            Response.Init(LibraryParser.OutgoingRequest("FavouriteRoomsUpdateMessageComposer"));
            Response.AppendInteger(num);
            Response.AppendBool(false);
            SendResponse();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("DELETE FROM users_favorites WHERE user_id = ",
                    Session.GetHabbo().Id, " AND room_id = ", num));
            }
        }

        internal void RoomUserAction()
        {
            var room = Session.GetHabbo().CurrentRoom;
            var roomUserByHabbo = room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            roomUserByHabbo.UnIdle();
            var num = Request.GetInteger();
            roomUserByHabbo.DanceId = 0;

            var action = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
            action.AppendInteger(roomUserByHabbo.VirtualId);
            action.AppendInteger(num);
            room.SendMessage(action);

            if (num == 5)
            {
                roomUserByHabbo.IsAsleep = true;
                var sleep = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
                sleep.AppendInteger(roomUserByHabbo.VirtualId);
                sleep.AppendBool(roomUserByHabbo.IsAsleep);
                room.SendMessage(sleep);
            }

            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialWave);
        }

        internal void GetRoomData1()
        {
            /*this.Response.Init(StaticClientMessageHandler.OutgoingRequest("297"));//Not in release
            this.Response.AppendInt32(0);
            this.SendResponse();*/
        }

        internal void GetRoomData2()
        {
            try
            {
                if (Session?.GetConnection() != null)
                {
                    if (Session?.GetHabbo()?.LoadingRoom <= 0u || CurrentLoadingRoom == null)
                        return;
                    var roomData = CurrentLoadingRoom.RoomData;
                    if (roomData == null)
                        return;
                    if (roomData.Model == null || CurrentLoadingRoom.GetGameMap() == null)
                    {
                        Session.SendMessage(
                            new ServerMessage(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer")));
                        ClearRoomLoading();
                    }
                    else
                    {
                        Session.SendMessage(CurrentLoadingRoom.GetGameMap().GetNewHeightmap());
                        Session.SendMessage(CurrentLoadingRoom.GetGameMap().Model.GetHeightmap());
                        GetRoomData3();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException("Unable to load room ID [" + Session?.GetHabbo().LoadingRoom + "]" + ex);
                Logging.HandleException(ex, "Oblivion.Messages.Handlers.Rooms");
            }
        }

        internal void GetRoomData3()
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().LoadingRoom <= 0u ||
                !Session.GetHabbo().LoadingChecksPassed ||
                CurrentLoadingRoom == null)
                return;
            if (CurrentLoadingRoom.RoomData.UsersNow + 1 > CurrentLoadingRoom.RoomData.UsersMax &&
                !Session.GetHabbo().HasFuse("fuse_enter_full_rooms"))
            {
                var roomFull = new ServerMessage(LibraryParser.OutgoingRequest("RoomEnterErrorMessageComposer"));
                roomFull.AppendInteger(1);
                return;
            }

            var array = CurrentLoadingRoom.GetRoomItemHandler().FloorItems.Values;
            var array2 = CurrentLoadingRoom.GetRoomItemHandler().WallItems.Values;
            Response.Init(LibraryParser.OutgoingRequest("RoomFloorItemsMessageComposer"));


            Response.AppendInteger(1);
            Response.AppendInteger(CurrentLoadingRoom.RoomData.OwnerId);
            Response.AppendString(CurrentLoadingRoom.RoomData.Owner);


            Response.AppendInteger(array.Count);
            /* TODO CHECK */
            foreach (var roomItem in array)
                roomItem.Serialize(Response);

            SendResponse();
            Response.Init(LibraryParser.OutgoingRequest("RoomWallItemsMessageComposer"));

            Response.AppendInteger(1);
            Response.AppendInteger(CurrentLoadingRoom.RoomData.OwnerId);
            Response.AppendString(CurrentLoadingRoom.RoomData.Owner);

            Response.AppendInteger(array2.Count);
            foreach (var roomItem2 in array2)
                roomItem2.Serialize(Response);

            SendResponse();

            CurrentLoadingRoom.GetRoomUserManager().AddUserToRoom(Session, Session.GetHabbo().SpectatorMode);
            Session.GetHabbo().SpectatorMode = false;

            if (Oblivion.GetUnixTimeStamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                serverMessage.AppendInteger(Session.GetHabbo().FloodTime - Oblivion.GetUnixTimeStamp());

                Session.SendMessage(serverMessage);
            }

            ClearRoomLoading();

            if (!Oblivion.GetGame().GetPollManager().TryGetPoll(CurrentLoadingRoom.RoomId, out var poll) ||
                Session.GetHabbo().GotPollData(poll.Id))
                return;
            if (poll.Type == PollType.Matching)
            {
                Response.Init(LibraryParser.OutgoingRequest("MatchingPollMessageComposer"));
                Response.AppendString("MATCHING_POLL");
                Response.AppendInteger(poll.RoomId);
                Response.AppendInteger(poll.RoomId);
                Response.AppendInteger(1);
                Response.AppendInteger(poll.RoomId);
                Response.AppendInteger(120);
                Response.AppendInteger(3);
                Response.AppendString(poll.PollName);
            }
            else
            {
                Response.Init(LibraryParser.OutgoingRequest("SuggestPollMessageComposer"));
                poll.Serialize(Response);
            }

            SendResponse();
        }

        internal void WidgetContainers()
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

            serverMessage.AppendString(text);
            serverMessage.AppendString(campaingName);


            Session.SendMessage(serverMessage);
        }

        internal void RefreshPromoEvent()
        {
            var hotelView = Oblivion.GetGame().GetHotelView();

            if (Session?.GetHabbo() == null)
                return;

            var rankings = Oblivion.GetGame().GetHallOfFame().Rankings;

            GetResponse().Init(LibraryParser.OutgoingRequest("HotelViewHallOfFameMessageComposer"));
            GetResponse().AppendString("");
            GetResponse().StartArray();
            foreach (var element in rankings)
            {
                GetResponse().AppendInteger(element.UserId);
                GetResponse().AppendString(element.Username);
                GetResponse().AppendString(element.Look);
                GetResponse().AppendInteger(2);
                GetResponse().AppendInteger(element.Score);
                GetResponse().SaveArray();
            }

            GetResponse().EndArray();
            SendResponse();

            if (hotelView.HotelViewPromosIndexers.Count <= 0)
                return;

            var message =
                hotelView.SmallPromoComposer(
                    new ServerMessage(LibraryParser.OutgoingRequest("LandingPromosMessageComposer")));
            Session.SendMessage(message);
        }

        internal void AcceptPoll()
        {
            var key = Request.GetUInteger();
            if (!Oblivion.GetGame().GetPollManager().Polls.TryGetValue(key, out var poll))
                return;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PollQuestionsMessageComposer"));

            serverMessage.AppendInteger(poll.Id);
            serverMessage.AppendString(poll.PollName);
            serverMessage.AppendString(poll.Thanks);
            serverMessage.AppendInteger(poll.Questions.Count);

            /* TODO CHECK */
            foreach (var current in poll.Questions)
            {
                var questionNumber = poll.Questions.IndexOf(current) + 1;

                current.Serialize(serverMessage, questionNumber);
            }

            Response = serverMessage;
            SendResponse();
        }

        internal void RefusePoll()
        {
            var num = Request.GetUInteger();

            Session.GetHabbo().Data.SuggestedPolls.Add(num);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("INSERT INTO users_polls VALUES (@userid , @pollid , 0 , '0' , '')");
                queryReactor.AddParameter("userid", Session.GetHabbo().Id);
                queryReactor.AddParameter("pollid", num);
                queryReactor.RunQuery();
            }
        }

        internal void AnswerPoll()
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

                answered.AppendInteger(Session.GetHabbo().Id);
                answered.AppendString(text);
                answered.AppendInteger(2);
                answered.AppendString("0");
                answered.AppendInteger(poll.AnswersNegative);
                answered.AppendString("1");
                answered.AppendInteger(poll.AnswersPositive);

                Session.GetHabbo().CurrentRoom.SendMessage(answered);
                Session.GetHabbo().AnsweredPool = true;

                return;
            }

            Session.GetHabbo().Data.SuggestedPolls.Add(pollId);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "INSERT INTO users_polls VALUES (@userid , @pollid , @questionid , '1' , @answer)");

                queryReactor.AddParameter("userid", Session.GetHabbo().Id);
                queryReactor.AddParameter("pollid", pollId);
                queryReactor.AddParameter("questionid", questionId);
                queryReactor.AddParameter("answer", text);
                queryReactor.RunQuery();
            }
        }

        public string WallPositionCheck(string wallPosition)
        {
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13)) || wallPosition.Contains(Convert.ToChar(9)))
                    return null;

                var array = wallPosition.Split(' ');

                if (array[2] != "l" && array[2] != "r")
                    return null;

                var array2 = array[0].Substring(3).Split(',');
                var num = int.Parse(array2[0]);
                var num2 = int.Parse(array2[1]);

                if (num >= 0 && num2 >= 0 && num <= 200 && num2 <= 200)
                {
                    var array3 = array[1].Substring(2).Split(',');
                    var num3 = int.Parse(array3[0]);
                    var num4 = int.Parse(array3[1]);

                    return num3 < 0 || num4 < 0 || num3 > 200 || num4 > 200
                        ? null
                        : string.Concat(":w=", num, ",", num2, " l=", num3, ",", num4, " ", array[2]);
                }
            }
            catch
            {
                // ignored
            }

            return null;
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
        }

        public void Whisper()
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
            if (!BobbaFilter.CanTalk(Session, msg))
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
                Session.SendWhisper(msg);
                return;
            }

            if (Session.GetHabbo().Rank < 4 && currentRoom.CheckMute(Session))
                return;

            currentRoom.AddChatlog(Session.GetHabbo().Id, $"<Sussurro para {text2}>: {msg}", false);

            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialChat);

            var colour2 = colour;

            if (!roomUserByHabbo.IsBot)
                if (colour2 == 2 || colour2 == 23 && !Session.GetHabbo().HasFuse("fuse_mod") || colour2 < 0 ||
                    colour2 > 29)
                    colour2 = roomUserByHabbo.LastBubble; // or can also be just 0

            roomUserByHabbo.UnIdle();

            var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
            whisp.AppendInteger(roomUserByHabbo.VirtualId);
            whisp.AppendString(msg);
            whisp.AppendInteger(0);
            whisp.AppendInteger(colour2);
            whisp.AppendInteger(0);
            whisp.AppendInteger(-1);

            roomUserByHabbo.GetClient().SendMessage(whisp);

            if (!roomUserByHabbo2.IsBot && roomUserByHabbo2.UserId != roomUserByHabbo.UserId && !roomUserByHabbo2
                    .GetClient().GetHabbo().Data.Ignores.Contains(Session.GetHabbo().Id))
                roomUserByHabbo2.GetClient().SendMessage(whisp);

            var roomUserByRank = currentRoom.GetRoomUserManager().GetRoomUserByRank(4);

            if (!roomUserByRank.Any())
                return;

            /* TODO CHECK */
            foreach (var current2 in roomUserByRank)
                if (current2 != null && current2.HabboId != roomUserByHabbo2.HabboId &&
                    current2.HabboId != roomUserByHabbo.HabboId && current2.GetClient() != null)
                {
                    var whispStaff = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));

                    whispStaff.AppendInteger(roomUserByHabbo.VirtualId);
                    whispStaff.AppendString($"Whisper to {text2}: {msg}");
                    whispStaff.AppendInteger(0);
                    whispStaff.AppendInteger(colour2);
                    whispStaff.AppendInteger(0);
                    whispStaff.AppendInteger(-1);

                    current2.GetClient().SendMessage(whispStaff);
                }
        }

        public void Chat()
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

            roomUser.Chat(Session, message, false, count, bubble);
        }

        public void Shout()
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

            roomUserByHabbo.Chat(Session, msg, true, -1, bubble);
        }

        public void GetFloorPlanUsedCoords()
        {
            Response.Init(LibraryParser.OutgoingRequest("GetFloorPlanUsedCoordsMessageComposer"));

            var room = Session.GetHabbo().CurrentRoom;

            if (room == null)
            {
                Response.AppendInteger(0);
            }
            else
            {
                var coords = room.GetGameMap().CoordinatedItems.Keys;
                Response.AppendInteger(coords.Count);

                /* TODO CHECK */
                foreach (var point in coords)
                {
                    Response.AppendInteger(point.X);
                    Response.AppendInteger(point.Y);
                }
            }

            SendResponse();
        }

        public void GetFloorPlanDoor()
        {
            var room = Session.GetHabbo().CurrentRoom;

            if (room == null)
                return;

            Response.Init(LibraryParser.OutgoingRequest("SetFloorPlanDoorMessageComposer"));
            Response.AppendInteger(room.GetGameMap().Model.DoorX);
            Response.AppendInteger(room.GetGameMap().Model.DoorY);
            Response.AppendInteger(room.GetGameMap().Model.DoorOrientation);

            SendResponse();
        }

        public Image Base64ToImage(string base64String)
        {
            var imageBytes = Convert.FromBase64String(base64String);

            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                var image = Image.FromStream(ms, true);
                return image;
            }
        }

        public void EnterRoomQueue()
        {
            Session.SendNotif("Currently working on Watch live TV");

            Session.GetHabbo().SpectatorMode = true;

            var forwardToRoom = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            forwardToRoom.AppendInteger(1);

            Session.SendMessage(forwardToRoom);
        }

        public void GetCameraRequest()
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            var str = Request.GetString();
            if (!int.TryParse(str, out var photoId) || photoId < 0)
                return;

            var preview = Oblivion.GetGame().GetCameraManager().GetPreview(photoId);

            if (preview == null || preview.CreatorId != Session.GetHabbo().Id)
                return;

            User.LastPhotoPreview = preview;
            var messageBuffer = new ServerMessage(LibraryParser.OutgoingRequest("CameraStorageUrlMessageComposer"));

            messageBuffer.AppendString(Oblivion.GetGame()
                .GetCameraManager()
                .GetPath(CameraPhotoType.PREVIEW, preview.Id, preview.CreatorId));

            Session.SendMessage(messageBuffer);
        }

        public void SubmitRoomToCompetition()
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

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
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
                    queryReactor.RunQuery();
                    competition.Entries.Add(room.RoomId, roomData);

                    var message = new ServerMessage();

                    roomData.CompetitionStatus = 2;
                    competition.AppendEntrySubmitMessage(message, 3, room);

                    Session.SendMessage(message);
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
                    queryReactor.RunQuery();
                    roomData.CompetitionStatus = 3;

                    var message = new ServerMessage();
                    competition.AppendEntrySubmitMessage(message, 0);

                    Session.SendMessage(message);
                }
            }
        }

        public void VoteForRoom()
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

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "UPDATE rooms_competitions_entries SET votes = @votes WHERE competition_id = @competition_id AND room_id = @roomid");

                queryReactor.AddParameter("votes", entry.CompetitionVotes);
                queryReactor.AddParameter("competition_id", competition.Id);
                queryReactor.AddParameter("roomid", room.RoomId);
                queryReactor.RunQuery();
                queryReactor.RunFastQuery("UPDATE users_stats SET daily_competition_votes = " +
                                          Session.GetHabbo().DailyCompetitionVotes + " WHERE id = " +
                                          Session.GetHabbo().Id);
            }

            var message = new ServerMessage();
            competition.AppendVoteMessage(message, Session.GetHabbo());

            Session.SendMessage(message);
        }
    }
}