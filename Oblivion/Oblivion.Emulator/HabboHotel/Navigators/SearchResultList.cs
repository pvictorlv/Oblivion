using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Navigators.Interfaces;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Navigators
{
    /// <summary>
    ///     Class SearchResultList.
    /// </summary>
    internal class SearchResultList
    {
        /// <summary>
        ///     Serializes the search result list flatcats.
        /// </summary>
        /// <param name="flatCatId">The flat cat identifier.</param>
        /// <param name="direct">if set to <c>true</c> [direct].</param>
        /// <param name="message">The message.</param>
        internal static async Task SerializeSearchResultListFlatcats(int flatCatId, bool direct, ServerMessage message)
        {
            var flatCat = Oblivion.GetGame().GetNavigator().GetFlatCat(flatCatId);

            if (flatCat == null)
                return;

            message.AppendString($"category__{flatCat.Caption}");
            message.AppendString(flatCat.Caption);
            message.AppendInteger(0);
            message.AppendBool(true);
            message.AppendInteger(-1);

            try
            {
                var rooms = Oblivion.GetGame().GetRoomManager().GetActiveRooms();
                message = await Oblivion.GetGame()
                    .GetNavigator()
                    .SerializeNavigatorPopularRoomsNews(message, rooms, flatCatId, direct);
            }
            catch
            {
                message.AppendInteger(0);
            }
        }

        /// <summary>
        ///     Serializes the promotions result list flatcats.
        /// </summary>
        /// <param name="flatCatId">The flat cat identifier.</param>
        /// <param name="direct">if set to <c>true</c> [direct].</param>
        /// <param name="message">The message.</param>
        internal static async Task SerializePromotionsResultListFlatcats(int flatCatId, bool direct, ServerMessage message)
        {
            var flatCat = Oblivion.GetGame().GetNavigator().GetFlatCat(flatCatId);
            message.AppendString("new_ads");
            message.AppendString(flatCat.Caption);
            message.AppendInteger(0);
            message.AppendBool(true);
            message.AppendInteger(-1);
            try
            {
                var rooms = Oblivion.GetGame().GetRoomManager().GetEventRooms();
                message = await Oblivion.GetGame()
                    .GetNavigator()
                    .SerializeNavigatorPopularRoomsNews(message, rooms, flatCatId, direct);
            }
            catch
            {
                message.AppendInteger(0);
            }
        }

        /// <summary>
        ///     Serializes the search result list statics.
        /// </summary>
        /// <param name="staticId">The static identifier.</param>
        /// <param name="direct">if set to <c>true</c> [direct].</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        internal static async Task<bool> SerializeSearchResultListStatics(string staticId, bool direct, ServerMessage message,
            GameClient session)
        {
            if (message == null || session == null) return false;

            if (string.IsNullOrEmpty(staticId) || staticId == "official") staticId = "official_view";
            if (staticId != "hotel_view" && staticId != "roomads_view" && staticId != "myworld_view" &&
                !staticId.StartsWith("category__") && staticId != "official_view")
            {
                message.AppendString(staticId); // code
                message.AppendString(""); // title
                message.AppendInteger(1); // 0 : no button - 1 : Show More - 2 : Show Back button
                message.AppendBool(staticId != "my" && staticId != "popular" && staticId != "official-root");
                // collapsed
                message.AppendInteger(staticId == "official-root" ? 1 : 0); // 0 : list - 1 : thumbnail
            }
            KeyValuePair<RoomData, uint>[] rooms;
            switch (staticId)
            {
                case "hotel_view":
                    {
                        if (!await SerializeSearchResultListStatics("popular", false, message, session)) return false;
                        /* TODO CHECK */
                        foreach (FlatCat flat in Oblivion.GetGame().GetNavigator().PrivateCategories.Values)
                            await SerializeSearchResultListFlatcats(flat.Id, false, message);
                        break;
                    }
                case "myworld_view":
                    {
                        if (!await SerializeSearchResultListStatics("my", false, message, session)) return false;
                        if (!await SerializeSearchResultListStatics("favorites", false, message, session)) return false;
                        if (!await SerializeSearchResultListStatics("my_groups", false, message, session)) return false;
                        if (!await SerializeSearchResultListStatics("history", false, message, session)) return false;
                        if (!await SerializeSearchResultListStatics("friends_rooms", false, message, session)) return false;
                        break;
                    }
                case "roomads_view":
                    {
                        /* TODO CHECK */
                        foreach (FlatCat flat in Oblivion.GetGame().GetNavigator().PrivateCategories.Values)
                            await SerializePromotionsResultListFlatcats(flat.Id, false, message);
                        if (!await SerializeSearchResultListStatics("top_promotions", false, message, session)) return false;
                        break;
                    }
                case "official_view":
                    {
                        if(!await SerializeSearchResultListStatics("official-root", false, message, session)) return false;
                        if (!await SerializeSearchResultListStatics("staffpicks", false, message, session)) return false;
                        break;
                    }
                case "official-root":
                    {
                        message.AppendServerMessage(await Oblivion.GetGame().GetNavigator().SerializeNewPublicRooms());
                        break;
                    }
                case "staffpicks":
                    {
                        message.AppendServerMessage(Oblivion.GetGame().GetNavigator().NewStaffPicks);
                        break;
                    }
                case "my":
                    {
                        var i = 0;
                        if (session?.GetHabbo()?.Data?.Rooms?.Count <= 0)
                        {
                            message.AppendInteger(0);
                            break;
                        }
                        message.StartArray();
                        /* TODO CHECK */
                        foreach (var data in session.GetHabbo().Data.Rooms)
                        {
                            var current = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(data);
                            if (current == null) continue;

                            await current.Serialize(message);
                                message.SaveArray();
                                if (i++ == (direct ? 100 : 20)) break;
                            
                        }

                        message.EndArray();
                        break;
                    }
                case "favorites":
                    {
                        if (session.GetHabbo().Data.FavouritedRooms?.Count <= 0)
                        {
                            message.AppendInteger(0);
                            break;
                        }

                        var i = 0;
                        message.AppendInteger(session.GetHabbo().Data.FavouritedRooms.Count > (direct ? 40 : 8)
                            ? (direct ? 40 : 8)
                            : session.GetHabbo().Data.FavouritedRooms.Count);
                        /* TODO CHECK */
                        foreach (var dataId in session.GetHabbo()
                                     .Data.FavouritedRooms)
                        {
                            var data = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(dataId);
                            if (data != null)
                            {
                                await data.Serialize(message);
                                i++;
                                if (i == (direct ? 40 : 8)) break;
                            }
                        }

                        break;
                    }
                case "friends_rooms":
                    {
                        var i = 0;
                        if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null || session.GetHabbo().GetMessenger().GetActiveFriendsRooms() == null)
                        {
                            message.AppendInteger(0);
                            return false;
                        }
                        var roomsFriends =
                            session.GetHabbo()
                                .GetMessenger()
                                .GetActiveFriendsRooms()
                                .OrderByDescending(p => p.UsersNow)
                                .Take((direct ? 40 : 8))
                                .ToList();
                        message.AppendInteger(roomsFriends.Count);
                        /* TODO CHECK */
                        foreach (var data in roomsFriends.Where(data => data != null))
                        {
                            await data.Serialize(message);

                            i++;
                            if (i == (direct ? 40 : 8)) break;
                        }
                        break;
                    }
                case "recommended":
                    {
                        break;
                    }
                case "popular":
                    {
                        try
                        {
                            rooms = Oblivion.GetGame().GetRoomManager().GetActiveRooms();
                            if (rooms == null)
                            {
                                message.AppendInteger(0);
                                return false;
                            }
                            message.AppendInteger(rooms.Length);
                            /* TODO CHECK */
                            foreach (var room in rooms) await room.Key.Serialize(message);
                        }
                        catch (Exception e)
                        {
                            Writer.Writer.LogException(e.ToString());
                            message.AppendInteger(0);
                            return false;
                        }
                        break;
                    }
                case "top_promotions":
                    {
                        try
                        {
                            rooms = Oblivion.GetGame().GetRoomManager().GetEventRooms();
                            await message.AppendIntegerAsync(rooms.Length);
                            /* TODO CHECK */
                            foreach (var room in rooms) await room.Key.Serialize(message);
                        }
                        catch
                        {
                            await message.AppendIntegerAsync(0);
                        }
                        break;
                    }
                case "my_groups":
                    {
                        var i = 0;
                        message.StartArray();
                        /* TODO CHECK */
                        foreach (var xGroupId in session.GetHabbo().MyGroups)
                        {
                            var xGroup = Oblivion.GetGame().GetGroupManager().GetGroup(xGroupId);
                            if (xGroup == null) continue;
                            var data = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(xGroup.RoomId);
                            if (data == null) continue;
                            await data.Serialize(message);
                            message.SaveArray();
                            if (i++ == (direct ? 40 : 8)) break;
                        }

                        message.EndArray();
                        break;
                    }
                case "history":
                    {
                        var i = 0;
                        message.StartArray();
                        /* TODO CHECK */
                        foreach (var roomId in session.GetHabbo()
                                     .RecentlyVisitedRooms)
                        {
                            var roomData = await Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
                            if (roomData != null)
                            {
                                await roomData.Serialize(message);
                                message.SaveArray();

                                if (i++ == (direct ? 40 : 8)) break;
                            }
                        }

                        message.EndArray();
                        break;
                    }
                default:
                    {
                        if (staticId.StartsWith("category__"))
                        {
                            await SerializeSearchResultListFlatcats(
                                Oblivion.GetGame()
                                    .GetNavigator()
                                    .GetFlatCatIdByName(staticId.Replace("category__", "")), true, message);
                        }
                        else message.AppendInteger(0);
                        break;
                    }
            }

            return true;
        }

        /// <summary>
        ///     Serializes the searches.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        internal static async Task SerializeSearches(string searchQuery, ServerMessage message, GameClient session)
        {
            message.AppendString("");
            message.AppendString(searchQuery);
            message.AppendInteger(2);
            message.AppendBool(false);
            message.AppendInteger(0);
            var containsOwner = false;
            var containsGroup = false;
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
                var activeRooms = Oblivion.GetGame().GetRoomManager().GetActiveRooms();
                var initforeach = false;
                try
                {
                    if (activeRooms != null &&
                        activeRooms.Length > 0)
                        initforeach = true;
                }
                catch
                {
                    initforeach = false;
                }
                if (initforeach)
                {
                    /* TODO CHECK */
                    foreach (var rms in activeRooms)
                    {
                        if (rooms.Count >= 50) break;
                        if (rms.Key.Name.ToLower().Contains(searchQuery.ToLower()))
                        {
                            rooms.Add(rms.Key);
                        }
                    }
                }
            }
            if (rooms.Count < 50 || containsOwner || containsGroup)
            {
                DataTable dTable;
                using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    if (containsOwner)
                    {
                        dbClient.SetQuery(
                            "SELECT * FROM rooms_data WHERE owner LIKE @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", searchQuery);
                        dTable = dbClient.GetTable();
                    }
                    else if (containsGroup)
                    {
                        dbClient.SetQuery(
                            $"SELECT * FROM rooms_data JOIN groups_data ON rooms_data.id = groups_data.room_id WHERE groups_data.name LIKE @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", $"%{searchQuery}%");
                        dTable = dbClient.GetTable();
                    }
                    else
                    {
                        dbClient.SetQuery(
                            $"SELECT * FROM rooms_data WHERE caption LIKE @query AND roomtype = 'private' LIMIT {50 - rooms.Count}");
                        dbClient.AddParameter("query", searchQuery);
                        dTable = dbClient.GetTable();
                    }
                }
                if (dTable != null)
                {
                    /* TODO CHECK */
                    foreach (DataRow row in dTable.Rows)
                    {
                        var rData =await Oblivion.GetGame().GetRoomManager().FetchRoomData(Convert.ToUInt32(row["id"]), row);
                        if (!rooms.Contains((RoomData)rData)) rooms.Add(rData);
                    }
                }
            }
            message.AppendInteger(rooms.Count);
            /* TODO CHECK */ foreach (var data in rooms.Where(data => data != null)) await data.Serialize(message);
        }
    }
}