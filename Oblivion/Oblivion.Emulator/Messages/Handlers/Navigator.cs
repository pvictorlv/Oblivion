using System.Threading.Tasks;
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
        internal async Task GetFlatCats() //GetEventCategoriesMessageEvent
        {
            if (Session?.GetHabbo() == null)
            {
                return;
            }
            await Session.SendMessageAsync(Oblivion.GetGame().GetNavigator().SerializePromotionCategories());
        }
        

        /// <summary>
        /// Enters the inquired room.
        /// </summary>
        internal void EnterInquiredRoom()
        {
            return ;
        }

        /// <summary>
        /// Gets the pub.
        /// </summary>
        internal void GetPub()
        {
            return ;
            /* uint roomId = Request.GetUInteger();
             RoomData roomData = Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
             if (roomData == null)
                 return;
             GetResponse().Init(LibraryParser.OutgoingRequest("453"));
             GetResponse().AppendInteger(roomData.Id);
             GetResponse().AppendString(roomData.CcTs);
             GetResponse().AppendInteger(roomData.Id);
             await SendResponse();*/
        }

        /// <summary>
        /// Opens the pub.
        /// </summary>
        internal async Task OpenPub()
        {
            Request.GetInteger();
            uint roomId = Request.GetUInteger();
            Request.GetInteger();
            RoomData roomData =await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
            if (roomData == null)
                return;
            await PrepareRoomForUser(roomData.Id, "");
        }

        /// <summary>
        /// News the navigator.
        /// </summary>
        internal async Task NewNavigator()
        {
            if (Session == null)
                return;
            
           await Oblivion.GetGame().GetNavigator().EnableNewNavigator(Session);
        }

        /// <summary>
        /// Searches the new navigator.
        /// </summary>
        internal async Task SearchNewNavigator()
        {
            if (Session == null)
                return;

            string name = Request.GetString();
            string junk = Request.GetString();
            var rooms = await Oblivion.GetGame().GetNavigator().SerializeNewNavigator(name, junk, Session);
            if (rooms == null) return;
            await Session.SendMessageAsync(rooms);
        }

        /// <summary>
        /// Saveds the search.
        /// </summary>
        internal async Task SavedSearch()
        {
            if (Session.GetHabbo().NavigatorLogs.Count > 50)
            {
                await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("navigator_max"));
                return;
            }
            string value1 = Request.GetString();
            string value2 = Request.GetString();
            var naviLogs = new NaviLogs(Session.GetHabbo().NavigatorLogs.Count, value1, value2);
            if (!Session.GetHabbo().NavigatorLogs.ContainsKey(naviLogs.Id))
                Session.GetHabbo().NavigatorLogs.Add(naviLogs.Id, naviLogs);
            var message = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorSavedSearchesComposer"));
            await message.AppendIntegerAsync(Session.GetHabbo().NavigatorLogs.Count);

            foreach (NaviLogs navi in Session.GetHabbo().NavigatorLogs.Values)
            {
                await message.AppendIntegerAsync(navi.Id);
                await message.AppendStringAsync(navi.Value1);
                await message.AppendStringAsync(navi.Value2);
                await message.AppendStringAsync("");
            }
            await Session.SendMessageAsync(message);
        }

        /// <summary>
        /// Serializes the saved search.
        /// </summary>
        /// <param name="textOne">The text one.</param>
        /// <param name="textTwo">The text two.</param>
        internal async Task SerializeSavedSearch(string textOne, string textTwo)
        {
            await GetResponse().AppendStringAsync(textOne);
            await GetResponse().AppendStringAsync(textTwo);
            await GetResponse().AppendStringAsync("fr");
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
            return ;
        }

        /// <summary>
        /// Hardcode to get users room via habbo air
        /// </summary>
        private int count;

        internal async Task HabboAirGetUserRooms()
        {
            if (count <= 4)
            {
                count++;
                return;
            }


            var message = new ServerMessage(LibraryParser.OutgoingRequest("HabboAirGetRoomUsersComposer"));

            await message.AppendIntegerAsync(5); //maybe category
            await message.AppendStringAsync("");
            await message.AppendIntegerAsync(Session.GetHabbo().Data.Rooms.Count);
            foreach (var current in Session.GetHabbo().Data.Rooms)
            {
                var data = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(current);

                await data.Serialize(message);
            }
            message.AppendBool(false);

            await Session.SendMessageAsync(message);
        }

        internal async Task HabboAirGetAllRooms()
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("HabboAirGetRoomUsersComposer"));

            await message.AppendIntegerAsync(1); //maybe category
            await message.AppendStringAsync("");
            var rooms = Oblivion.GetGame().GetRoomManager().GetActiveRooms();

            await message.AppendIntegerAsync(rooms.Length);
            foreach (var data in rooms)
            {
                await data.Key.Serialize(message);
            }
            message.AppendBool(false);

            await Session.SendMessageAsync(message);
        }

        /// <summary>
        /// News the navigator add saved search.
        /// </summary>
        internal async Task NewNavigatorAddSavedSearch()
        {
            await SavedSearch();
        }

        /// <summary>
        /// News the navigator delete saved search.
        /// </summary>
        internal async Task NewNavigatorDeleteSavedSearch()
        {
            int searchId = Request.GetInteger();
            if (!Session.GetHabbo().NavigatorLogs.ContainsKey(searchId))
                return;
            Session.GetHabbo().NavigatorLogs.Remove(searchId);
            var message = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorSavedSearchesComposer"));
            await message.AppendIntegerAsync(Session.GetHabbo().NavigatorLogs.Count);

            foreach (NaviLogs navi in Session.GetHabbo().NavigatorLogs.Values)
            {
                await message.AppendIntegerAsync(navi.Id);
                await message.AppendStringAsync(navi.Value1);
                await message.AppendStringAsync(navi.Value2);
                await message.AppendStringAsync("");
            }
            await Session.SendMessageAsync(message);
        }

        /// <summary>
        /// News the navigator collapse category.
        /// </summary>
        internal void NewNavigatorCollapseCategory()
        {
            Request.GetString();
            return ;
        }

        /// <summary>
        /// News the navigator uncollapse category.
        /// </summary>
        internal void NewNavigatorUncollapseCategory()
        {
            Request.GetString();
            return ;
        }

        /// <summary>
        /// Gets the pubs.
        /// </summary>
        internal async Task GetPubs()
        {
            if (Session.GetHabbo() == null)
                return;
            await Session.SendMessageAsync(await Oblivion.GetGame().GetNavigator().SerializePublicRooms());
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        internal async Task GetRoomInfo()
        {
            if (Session.GetHabbo() == null)
                return;
            uint roomId = Request.GetUInteger();
            Request.GetBool();
            Request.GetBool();
            RoomData roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
            if (roomData == null)
                return;
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("1491"));
            await GetResponse().AppendIntegerAsync(0);
            await roomData.Serialize(GetResponse());
            await SendResponse();
        }

       
        

        /// <summary>
        /// News the navigator flat cats.
        /// </summary>
        internal async Task NewNavigatorFlatCats()
        {
            if (Session.GetHabbo() == null)
                return;
            await Oblivion.GetGame().GetNavigator().SerializeFlatCategories(Session);
        }

        /// <summary>
        /// Gets the favorite rooms.
        /// </summary>
        internal async Task GetFavoriteRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            await Session.SendMessageAsync(await Oblivion.GetGame().GetNavigator().SerializeFavoriteRooms(Session));
        }

        /// <summary>
        /// Gets the recent rooms.
        /// </summary>
        internal async Task GetRecentRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            await Session.SendMessageAsync(await Oblivion.GetGame().GetNavigator().SerializeRecentRooms(Session));
        }

        /// <summary>
        /// Gets the popular tags.
        /// </summary>
        internal async Task GetPopularTags()
        {
            if (Session.GetHabbo() == null)
                return;
            await Session.SendMessageAsync(await Oblivion.GetGame().GetNavigator().SerializePopularRoomTags());
        }

        /// <summary>
        /// Gets the event rooms.
        /// </summary>
        internal async Task GetEventRooms()
        {
            if (Session.GetHabbo() == null)
                return;
            await Session.SendMessageAsync(await NavigatorManager.SerializePromoted(Session, Request.GetInteger()));
        }

        /// <summary>
        /// Performs the search.
        /// </summary>
        internal async Task PerformSearch()
        {
            if (Session.GetHabbo() == null)
                return;
            await Session.SendMessageAsync(
                await NavigatorManager.SerializeSearchResults(Request.GetString()));
        }

        /// <summary>
        /// Searches the by tag.
        /// </summary>
        internal async Task SearchByTag()
        {
            if (Session.GetHabbo() == null)
                return;
            await Session.SendMessageAsync(await NavigatorManager.SerializeSearchResults(
                $"tag:{Request.GetString()}"));
        }

        /// <summary>
        /// Performs the search2.
        /// </summary>
        internal async Task PerformSearch2()
        {
            if (Session.GetHabbo() == null)
                return;
            Request.GetInteger();
            await Session.SendMessageAsync(await NavigatorManager.SerializeSearchResults(Request.GetString()));
        }

        /// <summary>
        /// Opens the flat.
        /// </summary>
        internal async Task OpenFlat()
        {
            if (Session.GetHabbo() == null)
                return;

            var roomId = Request.GetUInteger();
            var pWd = Request.GetString();

            await PrepareRoomForUser(roomId, pWd);
        }

        internal async Task ToggleStaffPick()
        {
            var roomId = Request.GetUInteger();
            Request.GetBool();
            var room = Oblivion.GetGame().GetRoomManager().GetRoom(roomId);
            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Spr", 1, true);
            if (room == null) return;
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                var pubItem = Oblivion.GetGame().GetNavigator().GetPublicItem(roomId);
                if (pubItem == null) // not picked
                {
                    queryReactor.SetQuery(
                        "INSERT INTO navigator_publics (bannertype, room_id, category_parent_id) VALUES ('0', @roomId, '-2')");
                    queryReactor.AddParameter("roomId", room.RoomId);
                    await queryReactor.RunQueryAsync();
                    await queryReactor.RunFastQueryAsync("SELECT last_insert_id()");
                    var publicItemId = (uint) queryReactor.GetInteger();
                    var publicItem = new PublicItem(publicItemId, 0, string.Empty, string.Empty, string.Empty,
                        PublicImageType.Internal, room.RoomId, 0, -2, false, 1);
                    Oblivion.GetGame().GetNavigator().AddPublicItem(publicItem);
                }
                else // picked
                {
                    queryReactor.SetQuery("DELETE FROM navigator_publics WHERE id = @pubId");
                    queryReactor.AddParameter("pubId", pubItem.Id);
                    await queryReactor.RunQueryAsync();
                    Oblivion.GetGame().GetNavigator().RemovePublicItem(pubItem.Id);
                }
                await room.RoomData.SerializeRoomData(Response, Session, false, true);
                await Oblivion.GetGame().GetNavigator().LoadNewPublicRooms();
            }
        }
    }
}