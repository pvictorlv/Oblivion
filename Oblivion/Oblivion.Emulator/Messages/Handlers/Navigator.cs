using Oblivion.HabboHotel.Navigators;
using Oblivion.HabboHotel.Navigators.Enums;
using Oblivion.HabboHotel.Navigators.Interfaces;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.Messages.Parsers;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Gets the flat cats.
        /// </summary>
        internal void GetFlatCats() //GetEventCategoriesMessageEvent
        {
            if (Session?.GetHabbo() == null)
            {
                return;
            }
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializePromotionCategories());
        }

        /// <summary>
        /// Enters the inquired room.
        /// </summary>
        internal void EnterInquiredRoom()
        {
        }

        /// <summary>
        /// Gets the pub.
        /// </summary>
        internal void GetPub()
        {
            /* uint roomId = Request.GetUInteger();
             RoomData roomData = Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
             if (roomData == null)
                 return;
             GetResponse().Init(LibraryParser.OutgoingRequest("453"));
             GetResponse().AppendInteger(roomData.Id);
             GetResponse().AppendString(roomData.CcTs);
             GetResponse().AppendInteger(roomData.Id);
             SendResponse();*/
        }

        /// <summary>
        /// Opens the pub.
        /// </summary>
        internal void OpenPub()
        {
            Request.GetInteger();
            uint roomId = Request.GetUInteger();
            Request.GetInteger();
            RoomData roomData = Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
            if (roomData == null)
                return;
            PrepareRoomForUser(roomData.Id, "");
        }

        /// <summary>
        /// News the navigator.
        /// </summary>
        internal void NewNavigator()
        {
            if (Session == null)
                return;
            Oblivion.GetGame().GetNavigator().EnableNewNavigator(Session);
        }

        /// <summary>
        /// Searches the new navigator.
        /// </summary>
        internal void SearchNewNavigator()
        {
            if (Session == null)
                return;
            lock (SessionLock)
            {
                string name = Request.GetString();
                string junk = Request.GetString();
                var roomsMessage = Oblivion.GetGame().GetNavigator().SerializeNewNavigator(name, junk, Session);
                if (roomsMessage == null) return;
                Session.SendMessage(roomsMessage);
            }
        }

        /// <summary>
        /// Saveds the search.
        /// </summary>
        internal void SavedSearch()
        {
            if (Session.GetHabbo().NavigatorLogs.Count > 50)
            {
                Session.SendNotif(Oblivion.GetLanguage().GetVar("navigator_max"));
                return;
            }
            string value1 = Request.GetString();
            string value2 = Request.GetString();
            var naviLogs = new NaviLogs(Session.GetHabbo().NavigatorLogs.Count, value1, value2);
            if (!Session.GetHabbo().NavigatorLogs.ContainsKey(naviLogs.Id))
                Session.GetHabbo().NavigatorLogs.Add(naviLogs.Id, naviLogs);
            var message = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorSavedSearchesComposer"));
            message.AppendInteger(Session.GetHabbo().NavigatorLogs.Count);

            foreach (NaviLogs navi in Session.GetHabbo().NavigatorLogs.Values)
            {
                message.AppendInteger(navi.Id);
                message.AppendString(navi.Value1);
                message.AppendString(navi.Value2);
                message.AppendString("");
            }
            Session.SendMessage(message);
        }

        /// <summary>
        /// Serializes the saved search.
        /// </summary>
        /// <param name="textOne">The text one.</param>
        /// <param name="textTwo">The text two.</param>
        internal void SerializeSavedSearch(string textOne, string textTwo)
        {
            GetResponse().AppendString(textOne);
            GetResponse().AppendString(textTwo);
            GetResponse().AppendString("fr");
        }

        /// <summary>
        /// News the navigator resize.
        /// </summary>
        internal void NewNavigatorResize()
        {
            int x = Request.GetInteger();
            int y = Request.GetInteger();
            int width = Request.GetInteger();
            int height = Request.GetInteger();
            Session.GetHabbo().Preferences.NewnaviX = x;
            Session.GetHabbo().Preferences.NewnaviY = y;
            Session.GetHabbo().Preferences.NewnaviWidth = width;
            Session.GetHabbo().Preferences.NewnaviHeight = height;
            Session.GetHabbo().Preferences.Save();
        }

        /// <summary>
        /// Hardcode to get users room via habbo air
        /// </summary>
        private int _count;

        internal void HabboAirGetUserRooms()
        {
            if (_count <= 4)
            {
                _count++;
                return;
            }


            var message = new ServerMessage(LibraryParser.OutgoingRequest("HabboAirGetRoomUsersComposer"));

            message.AppendInteger(5); //maybe category
            message.AppendString("");
            message.AppendInteger(Session.GetHabbo().Data.Rooms.Count);
            foreach (var room in Session.GetHabbo().Data.Rooms)
            {
                var data = Oblivion.GetGame().GetRoomManager().GenerateRoomData(room);
                data.Serialize(message);
            }
            message.AppendBool(false);

            Session.SendMessage(message);
        }

        internal void HabboAirGetAllRooms()
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("HabboAirGetRoomUsersComposer"));

            message.AppendInteger(1); //maybe category
            message.AppendString("");
            var rooms = Oblivion.GetGame().GetRoomManager().LoadedRooms;

            message.AppendInteger(rooms.Count);
            foreach (var data in rooms)
            {
                data.Value.RoomData.Serialize(message);
            }
            message.AppendBool(false);

            Session.SendMessage(message);
        }

        /// <summary>
        /// News the navigator add saved search.
        /// </summary>
        internal void NewNavigatorAddSavedSearch()
        {
            SavedSearch();
        }

        /// <summary>
        /// News the navigator delete saved search.
        /// </summary>
        internal void NewNavigatorDeleteSavedSearch()
        {
            int searchId = Request.GetInteger();
            if (!Session.GetHabbo().NavigatorLogs.Remove(searchId))
                return;

            var message = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorSavedSearchesComposer"));
            message.AppendInteger(Session.GetHabbo().NavigatorLogs.Count);

            foreach (NaviLogs navi in Session.GetHabbo().NavigatorLogs.Values)
            {
                message.AppendInteger(navi.Id);
                message.AppendString(navi.Value1);
                message.AppendString(navi.Value2);
                message.AppendString("");
            }
            Session.SendMessage(message);
        }

        /// <summary>
        /// News the navigator collapse category.
        /// </summary>
        internal void NewNavigatorCollapseCategory()
        {
            Request.GetString();
        }

        /// <summary>
        /// News the navigator uncollapse category.
        /// </summary>
        internal void NewNavigatorUncollapseCategory()
        {
            Request.GetString();
        }

        /// <summary>
        /// Gets the pubs.
        /// </summary>
        internal void GetPubs()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializePublicRooms());
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        internal void GetRoomInfo()
        {
            if (Session.GetHabbo() == null)
                return;
            uint roomId = Request.GetUInteger();
            Request.GetBool();
            Request.GetBool();
            RoomData roomData = Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
            if (roomData == null)
                return;
            GetResponse().Init(LibraryParser.OutgoingRequest("1491"));
            GetResponse().AppendInteger(0);
            roomData.Serialize(GetResponse());
            SendResponse();
        }

        /// <summary>
        /// Gets the popular rooms.
        /// </summary>
        internal void GetPopularRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator()
                .SerializeNavigator(Session, int.Parse(Request.GetString())));
        }

        /// <summary>
        /// Gets the recommended rooms.
        /// </summary>
        internal void GetRecommendedRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeNavigator(Session, -1));
        }

        /// <summary>
        /// Gets the popular groups.
        /// </summary>
        internal void GetPopularGroups()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeNavigator(Session, -2));
        }

        /// <summary>
        /// Gets the high rated rooms.
        /// </summary>
        internal void GetHighRatedRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeNavigator(Session, -2));
        }

        /// <summary>
        /// Gets the friends rooms.
        /// </summary>
        internal void GetFriendsRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeNavigator(Session, -4));
        }

        /// <summary>
        /// Gets the rooms with friends.
        /// </summary>
        internal void GetRoomsWithFriends()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeNavigator(Session, -5));
        }

        /// <summary>
        /// Gets the own rooms.
        /// </summary>
        internal void GetOwnRooms()
        {
            if (Session?.GetHabbo() == null)
                return;

            if (Session.GetHabbo().OwnRoomsSerialized == false)
            {
                Session.GetHabbo().UpdateRooms();
                Session.GetHabbo().OwnRoomsSerialized = true;
            }

            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeNavigator(Session, -3));
        }

        /// <summary>
        /// News the navigator flat cats.
        /// </summary>
        internal void NewNavigatorFlatCats()
        {
            if (Session.GetHabbo() == null)
                return;
            Oblivion.GetGame().GetNavigator().SerializeFlatCategories(Session);
        }

        /// <summary>
        /// Gets the favorite rooms.
        /// </summary>
        internal void GetFavoriteRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeFavoriteRooms(Session));
        }

        /// <summary>
        /// Gets the recent rooms.
        /// </summary>
        internal void GetRecentRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializeRecentRooms(Session));
        }

        /// <summary>
        /// Gets the popular tags.
        /// </summary>
        internal void GetPopularTags()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(Oblivion.GetGame().GetNavigator().SerializePopularRoomTags());
        }

        /// <summary>
        /// Gets the event rooms.
        /// </summary>
        internal void GetEventRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(NavigatorManager.SerializePromoted(Session, Request.GetInteger()));
        }

        /// <summary>
        /// Performs the search.
        /// </summary>
        internal void PerformSearch()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(
                NavigatorManager.SerializeSearchResults(Request.GetString()));
        }

        /// <summary>
        /// Searches the by tag.
        /// </summary>
        internal void SearchByTag()
        {
            if (Session.GetHabbo() == null)
                return;
            Session.SendMessage(NavigatorManager.SerializeSearchResults(
                $"tag:{Request.GetString()}"));
        }

        /// <summary>
        /// Performs the search2.
        /// </summary>
        internal void PerformSearch2()
        {
            if (Session.GetHabbo() == null)
                return;
            Request.GetInteger();
            Session.SendMessage(NavigatorManager.SerializeSearchResults(Request.GetString()));
        }

        /// <summary>
        /// Opens the flat.
        /// </summary>
        internal void OpenFlat()
        {
            if (Session.GetHabbo() == null)
                return;

            var roomId = Request.GetUInteger();
            var pWd = Request.GetString();

            PrepareRoomForUser(roomId, pWd);
        }

        internal void ToggleStaffPick()
        {
            var roomId = Request.GetUInteger();
            Request.GetBool();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);
            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Spr", 1, true);
            if (room == null) return;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var pubItem = Oblivion.GetGame().GetNavigator().GetPublicItem(roomId);
                if (pubItem == null) // not picked
                {
                    queryReactor.SetQuery(
                        "INSERT INTO navigator_publics (bannertype, room_id, category_parent_id) VALUES ('0', @roomId, '-2')");
                    queryReactor.AddParameter("roomId", room.RoomId);
                    queryReactor.RunQuery();
                    queryReactor.RunFastQuery("SELECT last_insert_id()");
                    var publicItemId = (uint) queryReactor.GetInteger();
                    var publicItem = new PublicItem(publicItemId, 0, string.Empty, string.Empty, string.Empty,
                        PublicImageType.Internal, room.RoomId, 0, -2, false, 1);
                    Oblivion.GetGame().GetNavigator().AddPublicItem(publicItem);
                }
                else // picked
                {
                    queryReactor.SetQuery("DELETE FROM navigator_publics WHERE id = @pubId");
                    queryReactor.AddParameter("pubId", pubItem.Id);
                    queryReactor.RunQuery();
                    Oblivion.GetGame().GetNavigator().RemovePublicItem(pubItem.Id);
                }
                room.RoomData.SerializeRoomData(Response, Session, false, true);
                Oblivion.GetGame().GetNavigator().LoadNewPublicRooms();
            }
        }
    }
}