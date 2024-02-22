using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Navigators.Enums;
using Oblivion.HabboHotel.Navigators.Interfaces;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Navigators
{
    /// <summary>
    ///     Class NavigatorManager.
    /// </summary>
    internal class NavigatorManager
    {
        /// <summary>
        ///     The _public items
        /// </summary>
        private readonly Dictionary<uint, PublicItem> _publicItems;

        /// <summary>
        ///     The _navigator headers
        /// </summary>
        public readonly List<NavigatorHeader> NavigatorHeaders;

        /// <summary>
        ///     The in categories
        /// </summary>
        internal Dictionary<int, string> InCategories;

        /// <summary>
        ///     The new public rooms
        /// </summary>
        internal ServerMessage NewStaffPicks;

        /// <summary>
        ///     The private categories
        /// </summary>
        internal HybridDictionary PrivateCategories;

        /// <summary>
        ///     The promo categories
        /// </summary>
        internal Dictionary<int, PromoCat> PromoCategories;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigatorManager" /> class.
        /// </summary>
        internal NavigatorManager()
        {
            PrivateCategories = new HybridDictionary();
            InCategories = new Dictionary<int, string>();
            _publicItems = new Dictionary<uint, PublicItem>();
            NavigatorHeaders = new List<NavigatorHeader>();
            PromoCategories = new Dictionary<int, PromoCat>();
        }

        /// <summary>
        ///     Gets the flat cats count.
        /// </summary>
        /// <value>The flat cats count.</value>
        internal int FlatCatsCount => PrivateCategories.Count;

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="navLoaded">The nav loaded.</param>
        public void Initialize(IQueryAdapter dbClient, out uint navLoaded)
        {
            Initialize(dbClient);
            navLoaded = (uint)NavigatorHeaders.Count;
        }

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        public void Initialize(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM navigator_flatcats WHERE enabled = '2'");
            var table = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM navigator_publics");
            var table2 = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM navigator_pubcats");
            var table3 = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM navigator_promocats");
            var table4 = dbClient.GetTable();

            if (table4 != null)
            {
                PromoCategories.Clear();

                /* TODO CHECK */
                foreach (DataRow dataRow in table4.Rows)
                    PromoCategories.Add((int)dataRow["id"],
                        new PromoCat((int)dataRow["id"], (string)dataRow["caption"], (int)dataRow["min_rank"],
                            Oblivion.EnumToBool((string)dataRow["visible"])));
            }

            if (table != null)
            {
                PrivateCategories.Clear();

                /* TODO CHECK */
                foreach (DataRow dataRow in table.Rows)
                    PrivateCategories.Add((int)dataRow["id"],
                        new FlatCat((int)dataRow["id"], (string)dataRow["caption"], (int)dataRow["min_rank"]));
            }

            if (table2 != null)
            {
                _publicItems.Clear();

                /* TODO CHECK */
                foreach (DataRow row in table2.Rows)
                {
                    _publicItems.Add(Convert.ToUInt32(row["id"]),
                        new PublicItem(Convert.ToUInt32(row["id"]), int.Parse(row["bannertype"].ToString()),
                            (string)row["caption"],
                            (string)row["description"], (string)row["image"],
                            row["image_type"].ToString().ToLower() == "internal"
                                ? PublicImageType.Internal
                                : PublicImageType.External, (uint)row["room_id"], 0, (int)row["category_parent_id"],
                            row["recommended"].ToString() == "1", (int)row["typeofdata"]));
                }
            }

            if (table3 != null)
            {
                InCategories.Clear();

                /* TODO CHECK */
                foreach (DataRow dataRow in table3.Rows)
                    InCategories.Add((int)dataRow["id"], (string)dataRow["caption"]);
            }
        }

        public void AddPublicItem(PublicItem item)
        {
            if (item == null)
                return;

            _publicItems.Add(Convert.ToUInt32(item.Id), item);
        }

        public void RemovePublicItem(uint id)
        {
            if (!_publicItems.ContainsKey(id))
                return;

            _publicItems.Remove(id);
        }

        /// <summary>
        ///     Loads the new public rooms.
        /// </summary>
        public async Task LoadNewPublicRooms()
        {
            NewStaffPicks = await SerializeNewStaffPicks();
        }

        /// <summary>
        ///     Serializes the navigator popular rooms news.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="rooms">The rooms.</param>
        /// <param name="category">The category.</param>
        /// <param name="direct">if set to <c>true</c> [direct].</param>
        public async Task<ServerMessage> SerializeNavigatorPopularRoomsNews(ServerMessage reply, KeyValuePair<RoomData, uint>[] rooms,
            int category, bool direct)
        {
            if (rooms == null || rooms.Length <= 0)
            {
                await reply.AppendIntegerAsync(0);
                return reply;
            }


            /* TODO CHECK */
            var i = 0;
            reply.StartArray();
            foreach (var pair in rooms.Where(pair => pair.Key.Category.Equals(category)))
            {
                await pair.Key.Serialize(reply);
                reply.SaveArray();
                i++;
                if (i >= (direct ? 40 : 10)) break;
            }

            reply.EndArray();

            return reply;
        }

        /// <summary>
        ///     Serializes the promotion categories.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializePromotionCategories()
        {
            var categories =
                new ServerMessage(LibraryParser.OutgoingRequest("CatalogPromotionGetCategoriesMessageComposer"));
            categories.AppendInteger(PromoCategories.Count); //count

            /* TODO CHECK */
            foreach (var cat in PromoCategories.Values)
            {
                categories.AppendInteger(cat.Id);
                categories.AppendString(cat.Caption);
                categories.AppendBool(cat.Visible);
            }

            return categories;
        }

        /// <summary>
        ///     Serializes the new public rooms.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializeNewPublicRooms()
        {
            var message = new ServerMessage();
            message.StartArray();

            foreach (var item in _publicItems.Values)
            {
                if (item.ParentId == -1)
                {
                    message.Clear();

                    if (await item.RoomInfo() == null)
                        continue;

                    await (await item.RoomInfo()).Serialize(message);
                    message.SaveArray();
                }
            }

            message.EndArray();

            return message;
        }

        internal async Task<ServerMessage> SerializeNewStaffPicks()
        {
            var message = new ServerMessage();
            message.StartArray();

            /* TODO CHECK */
            foreach (var item in _publicItems.Values.Where(t => t.ParentId == -2))
            {
                message.Clear();

                if (await item.RoomInfo() == null)
                    continue;

                await (await item.RoomInfo()).Serialize(message);
                message.SaveArray();
            }

            message.EndArray();

            return message;
        }

        /// <summary>
        ///     Gets the flat cat.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>FlatCat.</returns>
        internal FlatCat GetFlatCat(int id) => PrivateCategories.Contains(id) ? (FlatCat)PrivateCategories[id] : null;

        /// <summary>
        ///     Serializes the nv recommend rooms.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeNvRecommendRooms()
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorLiftedRoomsComposer"));

            message.AppendInteger(_publicItems.Count); //count

            /* TODO CHECK */
         //   foreach (var item in _publicItems.Values)
//                item.SerializeNew(message);

            return message;
        }

        /// <summary>
        ///     Serializes the nv flat categories.
        /// </summary>
        /// <param name="myWorld">if set to <c>true</c> [my world].</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeNvFlatCategories(bool myWorld)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorMetaDataComposer"));
            message.AppendInteger(InCategories.Count);
            message.AppendString("categories");
            message.AppendInteger(1);

            if (myWorld)
            {
                message.AppendInteger(1);
                message.AppendString("myworld_view");
                message.AppendString("");
                message.AppendString("br");

                /* TODO CHECK */
                foreach (var item in InCategories.Values)
                {
                    message.AppendString(item);
                    message.AppendInteger(1);
                }
            }
            else
            {
                /* TODO CHECK */
                foreach (var item in InCategories.Values)
                {
                    message.AppendString(item);
                    message.AppendInteger(0);
                }
            }

            return message;
        }

        /// <summary>
        ///     Serlializes the new navigator.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <param name="session">The session.</param>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializeNewNavigator(string value1, string value2, GameClient session)
        {
            var newNavigator = new ServerMessage(LibraryParser.OutgoingRequest("SearchResultSetComposer"));

            await newNavigator.AppendStringAsync(value1);
            await newNavigator.AppendStringAsync(value2);
            await newNavigator.AppendIntegerAsync(value2.Length > 0 ? 1 : GetNewNavigatorLength(value1));

            if (value2.Length > 0)
                await SearchResultList.SerializeSearches(value2, newNavigator, session);
            else if (!await SearchResultList.SerializeSearchResultListStatics(value1, true, newNavigator, session))
                return null;

            return newNavigator;
        }

        /// <summary>
        ///     Enables the new navigator.
        /// </summary>
        /// <param name="session">The session.</param>
        internal async Task EnableNewNavigator(GameClient session)
        {
            var navigatorMetaDataParser = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorMetaDataComposer"));

            await navigatorMetaDataParser.AppendIntegerAsync(4);
            await navigatorMetaDataParser.AppendStringAsync("official_view");
            await navigatorMetaDataParser.AppendIntegerAsync(0);
            await navigatorMetaDataParser.AppendStringAsync("hotel_view");
            await navigatorMetaDataParser.AppendIntegerAsync(0);
            await navigatorMetaDataParser.AppendStringAsync("roomads_view");
            await navigatorMetaDataParser.AppendIntegerAsync(0);
            await navigatorMetaDataParser.AppendStringAsync("myworld_view");
            await navigatorMetaDataParser.AppendIntegerAsync(0);
            await session.SendMessage(navigatorMetaDataParser);

            var navigatorLiftedRoomsParser =
                new ServerMessage(LibraryParser.OutgoingRequest("NavigatorLiftedRoomsComposer"));
            await navigatorLiftedRoomsParser.AppendIntegerAsync(NavigatorHeaders.Count);

            /* TODO CHECK */
            foreach (var navHeader in NavigatorHeaders)
            {
                await navigatorLiftedRoomsParser.AppendIntegerAsync(navHeader.RoomId);
                await navigatorLiftedRoomsParser.AppendIntegerAsync(0);
                await navigatorLiftedRoomsParser.AppendStringAsync(navHeader.Image);
                await navigatorLiftedRoomsParser.AppendStringAsync(navHeader.Caption);
            }

            await session.SendMessage(navigatorLiftedRoomsParser);

            var collapsedCategoriesMessageParser =
                new ServerMessage(LibraryParser.OutgoingRequest("NavigatorCategorys"));
            await collapsedCategoriesMessageParser.AppendIntegerAsync(FlatCatsCount + 4);

            /* TODO CHECK */
            foreach (FlatCat flat in PrivateCategories.Values)
                await collapsedCategoriesMessageParser.AppendStringAsync($"category__{flat.Caption}");

            await collapsedCategoriesMessageParser.AppendStringAsync("recommended");
            await collapsedCategoriesMessageParser.AppendStringAsync("new_ads");
            await collapsedCategoriesMessageParser.AppendStringAsync("staffpicks");
            await collapsedCategoriesMessageParser.AppendStringAsync("official");
            await session.SendMessage(collapsedCategoriesMessageParser);

            var searches = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorSavedSearchesComposer"));
            await searches.AppendIntegerAsync(session.GetHabbo().NavigatorLogs.Count);

            /* TODO CHECK */
            foreach (var navi in session.GetHabbo().NavigatorLogs.Values)
            {
                await searches.AppendIntegerAsync(navi.Id);
                await searches.AppendStringAsync(navi.Value1);
                await searches.AppendStringAsync(navi.Value2);
                await searches.AppendStringAsync("");
            }

            await session.SendMessage(searches);
            //await session.SendMessage(SerlializeNewNavigator("official", "", session));

            var packetName = new ServerMessage(LibraryParser.OutgoingRequest("NewNavigatorSizeMessageComposer"));
            var pref = session.GetHabbo().Preferences;

            await packetName.AppendIntegerAsync(pref.NewnaviX);
            await packetName.AppendIntegerAsync(pref.NewnaviY);
            await packetName.AppendIntegerAsync(pref.NewnaviWidth);
            await packetName.AppendIntegerAsync(pref.NewnaviHeight);
            packetName.AppendBool(false);
            await packetName.AppendIntegerAsync(1);

            await session.SendMessage(packetName);
        }

        /// <summary>
        ///     Serializes the flat categories.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>ServerMessage.</returns>
        internal async Task SerializeFlatCategories(GameClient session)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FlatCategoriesMessageComposer"));

            await serverMessage.AppendIntegerAsync(PrivateCategories.Values.Count);
//            serverMessage.StartArray();

            /* TODO CHECK */
            foreach (FlatCat flatCat in PrivateCategories.Values)
            {
//                serverMessage.Clear();

//                if (flatCat == null)
//                    continue;

                await serverMessage.AppendIntegerAsync(flatCat.Id);
                await serverMessage.AppendStringAsync(flatCat.Caption);
                serverMessage.AppendBool(flatCat.MinRank <= session.GetHabbo().Rank);
                serverMessage.AppendBool(false);
                await serverMessage.AppendStringAsync("");
                await serverMessage.AppendStringAsync("");
                serverMessage.AppendBool(false);

//                serverMessage.SaveArray();
            }

//            serverMessage.EndArray();

            await session.SendMessage(serverMessage);
        }

        /// <summary>
        ///     Gets the name of the flat cat identifier by.
        /// </summary>
        /// <param name="flatName">Name of the flat.</param>
        /// <returns>System.Int32.</returns>
        internal int GetFlatCatIdByName(string flatName)
        {
            /* TODO CHECK */
            foreach (var flat in PrivateCategories.Values.Cast<FlatCat>().Where(flat => flat.Caption == flatName))
                return flat.Id;

            return -1;
        }

        /// <summary>
        ///     Serializes the public rooms.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializePublicRooms()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("OfficialRoomsMessageComposer"));
            var rooms = new List<PublicItem>();
            foreach (var current in _publicItems.Values)
            {
                if (current.ParentId <= 0 && (await current.RoomInfo()) != null) rooms.Add(current);
            }

            await serverMessage.AppendIntegerAsync(rooms.Count);

            /* TODO CHECK */
            foreach (var current in rooms)
            {
                await current.Serialize(serverMessage);

                if (current.ItemType != PublicItemType.Category)
                    continue;

                /* TODO CHECK */
                foreach (var current2 in _publicItems.Values.Where(x => x.ParentId == current.Id))
                    await current2.Serialize(serverMessage);
            }

            if (!_publicItems.Values.Any(current => current.Recommended))
                await serverMessage.AppendIntegerAsync(0);
            else
            {
                var room = _publicItems.Values.First(current => current.Recommended);

                if (room != null)
                {
                    await serverMessage.AppendIntegerAsync(1);
                    await room.Serialize(serverMessage);
                }
                else
                    await serverMessage.AppendIntegerAsync(0);
            }

            await serverMessage.AppendIntegerAsync(0);

            return serverMessage;
        }

        /// <summary>
        ///     Serializes the favorite rooms.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializeFavoriteRooms(GameClient session)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorListingsMessageComposer"));
            await serverMessage.AppendIntegerAsync(6);
            await serverMessage.AppendStringAsync(string.Empty);
            await serverMessage.AppendIntegerAsync(session.GetHabbo().Data.FavouritedRooms.Count);

            var array = session.GetHabbo().Data.FavouritedRooms.ToList();

            /* TODO CHECK */
            foreach (var roomId in array)
            {
                var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
                if (roomData != null) await roomData.Serialize(serverMessage);
            }

            serverMessage.AppendBool(false);

            return serverMessage;
        }

        /// <summary>
        ///     Serializes the recent rooms.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializeRecentRooms(GameClient session)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorListingsMessageComposer"));
            await serverMessage.AppendIntegerAsync(7);
            await serverMessage.AppendStringAsync(string.Empty);

            serverMessage.StartArray();

            /* TODO CHECK */
            foreach (var current in session.GetHabbo().RecentlyVisitedRooms)
            {
                var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(current);
                await roomData.Serialize(serverMessage);
                serverMessage.SaveArray();
            }

            serverMessage.EndArray();
            serverMessage.AppendBool(false);

            return serverMessage;
        }

        /// <summary>
        ///     Serializes the event listing.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeEventListing()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorListingsMessageComposer"));
            serverMessage.AppendInteger(16);
            serverMessage.AppendString(string.Empty);

            var eventRooms = Oblivion.GetGame().GetRoomManager().GetEventRooms();

            serverMessage.AppendInteger(eventRooms.Length);

            var array = eventRooms;

            /* TODO CHECK */
            //foreach (var keyValuePair in array)
                //keyValuePair.Key.Serialize(serverMessage, true);

            return serverMessage;
        }

        internal PublicItem GetPublicItem(uint roomId)
        {
            var search = _publicItems.Where(i => i.Value.RoomId == roomId);

            IEnumerable<KeyValuePair<uint, PublicItem>> keyValuePairs =
                search as KeyValuePair<uint, PublicItem>[] ?? search.ToArray();

            if (!keyValuePairs.Any())
                return null;

            return keyValuePairs.FirstOrDefault().Value;
        }

        /// <summary>
        ///     Serializes the popular room tags.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal async Task<ServerMessage> SerializePopularRoomTags()
        {
            var dictionary = new Dictionary<string, uint>();

            var table = Oblivion.GetGame().GetRoomManager().GetActiveRooms();

            /* TODO CHECK */
            foreach (var room in table)
            {
                var list = room.Key.Tags;

                /* TODO CHECK */
                foreach (var current in list)
                {
                    if (dictionary.ContainsKey(current))
                        dictionary[current] += room.Key.UsersNow;
                    else
                        dictionary.Add(current, room.Key.UsersNow);
                }
            }


            var list2 = new List<KeyValuePair<string, uint>>(dictionary);

            list2.Sort((firstPair, nextPair) => firstPair.Value.CompareTo(nextPair.Value));

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PopularRoomTagsMessageComposer"));
            await serverMessage.AppendIntegerAsync(list2.Count);

            /* TODO CHECK */
            foreach (var current2 in list2)
            {
                await serverMessage.AppendStringAsync(current2.Key);
                await serverMessage.AppendIntegerAsync(current2.Value);
            }

            return serverMessage;
        }

        /// <summary>
        ///     Gets the new length of the navigator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Int32.</returns>
        private static int GetNewNavigatorLength(string value)
        {
            switch (value)
            {
                case "official_view":
                    return 2;

                case "myworld_view":
                    return 5;

                case "hotel_view":
                case "roomads_view":
                    return Oblivion.GetGame().GetNavigator().FlatCatsCount + 1;
            }

            return 1;
        }

        /// <summary>
        ///     Serializes the navigator rooms.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="rooms">The rooms.</param>
        private static async Task SerializeNavigatorRooms(ServerMessage reply, ICollection<RoomData> rooms)
        {
            await reply.AppendStringAsync(string.Empty);

            if (rooms == null || !rooms.Any())
            {
                await reply.AppendIntegerAsync(0);
                reply.AppendBool(false);

                return;
            }

            await reply.AppendIntegerAsync(rooms.Count);

            /* TODO CHECK */
            foreach (var pair in rooms)
                await pair.Serialize(reply);

            reply.AppendBool(false);
        }

        private static async Task SerializeNavigatorRooms(ServerMessage reply, ICollection<uint> rooms)
        {
            await reply.AppendStringAsync(string.Empty);

            if (rooms == null || !rooms.Any())
            {
                await reply.AppendIntegerAsync(0);
                reply.AppendBool(false);

                return;
            }

            await reply.AppendIntegerAsync(rooms.Count);

            /* TODO CHECK */
            foreach (var pair in rooms)
            {
                var current = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(pair);
                await current.Serialize(reply);
            }

            reply.AppendBool(false);
        }

        /// <summary>
        ///     Serializes the navigator popular rooms.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="rooms">The rooms.</param>
        private static async Task<ServerMessage> SerializeNavigatorPopularRooms(ServerMessage reply,
            ICollection<KeyValuePair<RoomData, uint>> rooms)
        {
            if (rooms == null || !rooms.Any())
            {
                await reply.AppendIntegerAsync(0);
                reply.AppendBool(false);
                return reply;
            }

            await reply.AppendIntegerAsync(rooms.Count);

            /* TODO CHECK */
            foreach (var pair in rooms)
                await pair.Key.Serialize(reply);

            reply.AppendBool(false);

            return reply;
        }

        /// <summary>
        ///     Serializes the navigator rooms.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="rooms">The rooms.</param>
        private static async Task SerializeNavigatorRooms(ServerMessage reply,
            ICollection<KeyValuePair<RoomData, int>> rooms)
        {
            await reply.AppendStringAsync(string.Empty);

            if (rooms == null || !rooms.Any())
            {
                await reply.AppendIntegerAsync(0);
                reply.AppendBool(false);

                return;
            }

            await reply.AppendIntegerAsync(rooms.Count);

            /* TODO CHECK */
            foreach (var pair in rooms)
                await pair.Key.Serialize(reply);

            reply.AppendBool(false);
        }

        /// <summary>
        ///     Serializes the promoted.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>ServerMessage.</returns>
        public static async Task<ServerMessage> SerializePromoted(GameClient session, int mode)
        {
            var reply = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorListingsMessageComposer"));
            await reply.AppendIntegerAsync(mode);
            await reply.AppendStringAsync(string.Empty);

            try
            {
                var rooms = Oblivion.GetGame().GetRoomManager().GetEventRooms();

                reply = await SerializeNavigatorPopularRooms(reply, rooms);

                if (rooms != null)
                    Array.Clear(rooms, 0, rooms.Length);
            }
            catch
            {
                await reply.AppendIntegerAsync(0);
                reply.AppendBool(false);
            }

            return reply;
        }

        /// <summary>
        ///     Serializes the search results.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>ServerMessage.</returns>
        public static async Task<ServerMessage> SerializeSearchResults(string searchQuery)
        {
            var containsOwner = false;
            var containsGroup = false;
            var originalQuery = searchQuery;

            if (searchQuery.StartsWith("owner:"))
            {
                searchQuery = searchQuery.Replace("owner:", string.Empty);
                containsOwner = true;
            }
            else if (searchQuery.StartsWith("group:"))
            {
                searchQuery = searchQuery.Replace("group:", string.Empty);
                containsGroup = true;
            }
            else if (searchQuery.StartsWith("roomname:"))
            {
                searchQuery = searchQuery.Replace("roomname:", string.Empty);
            }

            var rooms = new List<RoomData>();

            if (!containsOwner)
            {
                var initforeach = false;

                var activeRooms = Oblivion.GetGame().GetRoomManager().GetActiveRooms();
                try
                {
                    if (activeRooms != null && activeRooms.Any())
                        initforeach = true;
                }
                catch
                {
                    initforeach = false;
                }

                if (initforeach)
                {
                    /* TODO CHECK */
                    var lowerQuery = searchQuery.ToLower();
                    foreach (var rms in activeRooms)
                    {
                        if (rms.Key.Name.ToLower().Contains(lowerQuery) && rooms.Count <= 50)
                            rooms.Add(rms.Key);
                        else
                            break;
                    }
                }
            }

            if (rooms.Count < 50 || containsOwner || containsGroup)
            {
                DataTable dTable;

                using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    if (containsOwner)
                    {
                        dbClient.SetQuery(
                            "SELECT * FROM rooms_data WHERE owner = @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", searchQuery);
                        dTable = dbClient.GetTable();
                    }
                    else if (containsGroup)
                    {
                        dbClient.SetQuery(
                            "SELECT * FROM rooms_data JOIN groups_data ON rooms_data.id = groups_data.room_id WHERE groups_data.name LIKE @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", "%" + searchQuery + "%");
                        dTable = dbClient.GetTable();
                    }
                    else
                    {
                        dbClient.SetQuery(
                            "SELECT * FROM rooms_data WHERE caption LIKE @query AND roomtype = 'private' LIMIT " +
                            (50 - rooms.Count));
                        dbClient.AddParameter("query", $"'%{searchQuery}%'");
                        dTable = dbClient.GetTable();
                    }
                }

                if (dTable != null)
                {
                    /* TODO CHECK */
                    foreach (DataRow row in dTable.Rows)
                    {
                        var rData = await Oblivion.GetGame()
                            .GetRoomManager()
                            .FetchRoomData(Convert.ToUInt32(row["id"]), row);
                        if (!rooms.Contains((RoomData)rData)) rooms.Add(rData);
                    }
                }
            }

            var message = new ServerMessage(LibraryParser.OutgoingRequest("NavigatorListingsMessageComposer"));
            await message.AppendIntegerAsync(8);
            await message.AppendStringAsync(originalQuery);
            await message.AppendIntegerAsync(rooms.Count);

            /* TODO CHECK */
            foreach (var room in rooms)
                await room.Serialize(message);

            message.AppendBool(false);

            return message;
        }
    }
}