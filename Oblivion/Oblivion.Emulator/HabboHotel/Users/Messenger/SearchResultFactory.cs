using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Oblivion.HabboHotel.Users.Messenger
{
    /// <summary>
    ///     Class SearchResultFactory.
    /// </summary>
    internal static class SearchResultFactory
    {
        /// <summary>
        /// Cache the last query to prevent spam
        /// </summary>
        private static string _lastQuery;

        /// <summary>
        /// Cache the last query to prevent spam
        /// </summary>
        private static List<SearchResult> _lastResult;

        /// <summary>
        ///     Gets the search result.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List&lt;SearchResult&gt;.</returns>
        internal static List<SearchResult> GetSearchResult(string query)
        {
            if (_lastQuery == query) return _lastResult;

            _lastQuery = query;

            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT id,username,motto,look,last_online FROM users WHERE username LIKE @query LIMIT 50");
                queryReactor.AddParameter("query", $"{query}%");
                table = queryReactor.GetTable();
            }

            _lastResult = (from DataRow dataRow in table.Rows
                let userId = Convert.ToUInt32(dataRow[0])
                let userName = (string) dataRow[1]
                let motto = (string) dataRow[2]
                let look = (string) dataRow[3]
                let lastOnline = dataRow[4].ToString()
                select new SearchResult(userId, userName, motto, look, lastOnline)).ToList();
            return _lastResult;

        }
    }
}