using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.GameClients.Interfaces;
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
        internal static void SerializeSearchResultListFlatcats(int flatCatId, bool direct, ServerMessage message)
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
                var rooms = Oblivion.GetGame().GetRoomManager().LoadedRoomData.Where(x => x.Value.UsersNow > 0)
                    .OrderBy(x => x.Key);
                Oblivion.GetGame()
                    .GetNavigator()
                    .SerializeNavigatorPopularRoomsNews(ref message, rooms, flatCatId, direct);
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
        internal static void SerializePromotionsResultListFlatcats(int flatCatId, bool direct, ServerMessage message)
        {
            var flatCat = Oblivion.GetGame().GetNavigator().GetFlatCat(flatCatId);
            message.AppendString("new_ads");
            message.AppendString(flatCat.Caption);
            message.AppendInteger(0);
            message.AppendBool(true);
            message.AppendInteger(-1);
            try
            {
                var rooms = Oblivion.GetGame().GetRoomManager().LoadedRoomData.Where(x => x.Value.UsersNow > 0 && x.Value.HasEvent)
                    .OrderBy(x => x.Key);
                Oblivion.GetGame()
                    .GetNavigator()
                    .SerializeNavigatorPopularRoomsNews(ref message, rooms, flatCatId, direct);
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
        internal static bool SerializeSearchResultListStatics(string staticId, bool direct, ServerMessage message,
            GameClient session)
        {
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

            List<KeyValuePair<uint, RoomData>> rooms;
            switch (staticId)
            {
                case "hotel_view":
                {
                    if (!SerializeSearchResultListStatics("popular", false, message, session)) return false;
                    /* TODO CHECK */
                    foreach (FlatCat flat in Oblivion.GetGame().GetNavigator().PrivateCategories.Values)
                        SerializeSearchResultListFlatcats(flat.Id, false, message);
                    break;
                }
                case "myworld_view":
                {
                    if (!SerializeSearchResultListStatics("my", false, message, session)) return false;
                    if (!SerializeSearchResultListStatics("favorites", false, message, session)) return false;
                    if (!SerializeSearchResultListStatics("my_groups", false, message, session)) return false;
                    if (!SerializeSearchResultListStatics("history", false, message, session)) return false;
                    if (!SerializeSearchResultListStatics("friends_rooms", false, message, session)) return false;
                    break;
                }
                case "roomads_view":
                {
                    /* TODO CHECK */
                    foreach (FlatCat flat in Oblivion.GetGame().GetNavigator().PrivateCategories.Values)
                        SerializePromotionsResultListFlatcats(flat.Id, false, message);
                    if (!SerializeSearchResultListStatics("top_promotions", false, message, session)) return false;
                    break;
                }
                case "official_view":
                {
                    if (!SerializeSearchResultListStatics("official-root", false, message, session)) return false;
                    if (!SerializeSearchResultListStatics("staffpicks", false, message, session)) return false;
                    break;
                }
                case "official-root":
                {
                    message.AppendServerMessage(Oblivion.GetGame().GetNavigator().SerializeNewPublicRooms());
                    break;
                }
                case "staffpicks":
                {
                    message.AppendServerMessage(Oblivion.GetGame().GetNavigator().NewStaffPicks);
                    break;
                }
                case "my":
                {
                    if (session?.GetHabbo()?.Data?.Rooms == null)
                    {
                        message.AppendInteger(0);
                        return false;
                    }

                    var myRooms = session.GetHabbo().Data.Rooms;

                    var count = myRooms.Count;
                    if (count > (direct ? 100 : 20))
                    {
                        count = (direct ? 100 : 20);
                    }

                    message.StartArray();
                    foreach (var room in myRooms.Take(count))
                    {
                        var data = Oblivion.GetGame().GetRoomManager().GenerateRoomData(room);
                        if (data == null)
                            continue;
                        if (!data.Serialize(message))
                            continue;
                        message.SaveArray();
                    }

                    message.EndArray();
                    break;
                }
                case "favorites":
                {
                    if (session?.GetHabbo()?.Data?.FavouritedRooms == null)
                    {
                        message.AppendInteger(0);
                        return false;
                    }

                    var i = 0;

                    message.StartArray();

                    foreach (var dataId in session.GetHabbo().Data.FavouritedRooms)
                    {
                        var data = Oblivion.GetGame().GetRoomManager().GenerateRoomData(dataId);
                        if (data == null) continue;
                        if (!data.Serialize(message)) continue;
                        message.EndArray();
                        i++;
                        if (i == (direct ? 40 : 8)) break;
                    }

                    message.EndArray();

                    break;
                }
                case "friends_rooms":
                {
                    var i = 0;
                    if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null ||
                        session.GetHabbo().GetMessenger().GetActiveFriendsRooms() == null)
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
                    message.StartArray();
                    foreach (var data in roomsFriends.Where(data => data != null))
                    {
                        if (!data.Serialize(message)) continue;

                        i++;

                        message.SaveArray();
                        if (i == (direct ? 40 : 8)) break;
                    }

                    message.EndArray();
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
                        rooms = Oblivion.GetGame().GetRoomManager().LoadedRoomData.OrderByDescending(x => x.Value.UsersNow).Take(50)
                            .ToList();


                        message.AppendInteger(rooms.Count);
                        /* TODO CHECK */
                        foreach (var room in rooms)
                        {
                            room.Value?.Serialize(message);
                        }
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
                        rooms = Oblivion.GetGame().GetRoomManager().LoadedRoomData.Where(x => x.Value.Event != null)
                            .OrderByDescending(x => x.Value.Id).ToList();
                        message.StartArray();
                        /* TODO CHECK */
                        foreach (var room in rooms)
                        {
                            if (room.Value == null) continue;
                            if (!room.Value.Serialize(message)) continue;
                            message.SaveArray();
                        }

                        message.EndArray();
                    }
                    catch (Exception e)
                    {
                        Writer.Writer.LogException(e.ToString());
                        message.AppendInteger(0);
                        return false;
                    }

                    break;
                }
                case "my_groups":
                {
                    if (session?.GetHabbo()?.MyGroups == null)
                    {
                        message.AppendInteger(0);
                        return false;
                    }

                    var i = 0;
                    message.StartArray();
                    /* TODO CHECK */
                    foreach (var xGroupId in session.GetHabbo().MyGroups)
                    {
                        var xGroup = Oblivion.GetGame().GetGroupManager().GetGroup(xGroupId);
                        if (xGroup == null) continue;
                        var data = Oblivion.GetGame().GetRoomManager().GenerateRoomData(xGroup.RoomId);
                        if (data == null) continue;
                        if (!data.Serialize(message)) continue;
                        message.SaveArray();
                        if (i++ == (direct ? 40 : 8)) break;
                    }

                    message.EndArray();
                    break;
                }
                case "history":
                {
                    if (session?.GetHabbo()?
                            .RecentlyVisitedRooms == null)
                    {
                        message.AppendInteger(0);
                        return false;
                    }

                    var i = 0;
                    message.StartArray();
                    /* TODO CHECK */
                    foreach (var roomId in session.GetHabbo()
                        .RecentlyVisitedRooms)
                    {
                        var roomData = Oblivion.GetGame().GetRoomManager().GenerateRoomData(roomId);
                        if (roomData != null)
                        {
                            if (!roomData.Serialize(message)) continue;
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
                        SerializeSearchResultListFlatcats(
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
        internal static void SerializeSearches(string searchQuery, ServerMessage message, GameClient session)
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
                var initforeach = false;
                try
                {
                    var loadedRooms = Oblivion.GetGame().GetRoomManager().LoadedRooms;
                    if (loadedRooms != null && loadedRooms.Count > 0)
                        initforeach = true;
                }
                catch
                {
                    initforeach = false;
                }

                if (initforeach)
                {
                    /* TODO CHECK */
                    foreach (var rms in Oblivion.GetGame().GetRoomManager().LoadedRoomData)
                    {
                        if (rms.Value.Name.ToLower().Contains(searchQuery.ToLower()) && rooms.Count <= 50)
                            rooms.Add(rms.Value);
                        else break;
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
                            "SELECT * FROM rooms_data WHERE owner = @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", searchQuery);
                        dTable = dbClient.GetTable();
                    }
                    else if (containsGroup)
                    {
                        dbClient.SetQuery(
                            "SELECT * FROM rooms_data JOIN groups_data ON rooms_data.id = groups_data.room_id WHERE groups_data.name LIKE @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", $"%{searchQuery}%");
                        dTable = dbClient.GetTable();
                    }
                    else
                    {
                        dbClient.SetQuery(
                            $"SELECT * FROM rooms_data WHERE caption LIKE @query AND roomtype = 'private' LIMIT {50 - rooms.Count}");
                        dbClient.AddParameter("query", $"%{searchQuery}%");
                        dTable = dbClient.GetTable();
                    }
                }

                if (dTable != null)
                {
                    /* TODO CHECK */
                    foreach (
                        var rData in
                        dTable.Rows.Cast<DataRow>()
                            .Select(
                                row =>
                                    Oblivion.GetGame().GetRoomManager().FetchRoomData(Convert.ToUInt32(row["id"]), row))
                            .Where(rData => !rooms.Contains(rData)))
                        rooms.Add(rData);
                }
            }

            message.AppendInteger(rooms.Count);
            /* TODO CHECK */
            foreach (var data in rooms.Where(data => data != null)) data.Serialize(message);
        }
    }
}