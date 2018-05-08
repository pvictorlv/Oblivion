using System;
using System.Collections.Generic;
using System.Data;

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
            if (_lastQuery == query)
                return _lastResult;

            _lastQuery = query;

            DataTable table;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT id,username,motto,look,last_online FROM users WHERE username LIKE @query LIMIT 50");
                queryReactor.AddParameter("query", $"%{query}%");
                table = queryReactor.GetTable();
            }

            if (table == null) return new List<SearchResult>();

            List<SearchResult> list = new List<SearchResult>();
            foreach (DataRow dataRow in table.Rows)
            {
                uint userId = Convert.ToUInt32(dataRow[0]);
                string userName = dataRow[1].ToString();
                string motto = dataRow[2].ToString();
                string look = dataRow[3].ToString();
                string lastOnline = dataRow[4].ToString();
                list.Add(new SearchResult(userId, userName, motto, look, lastOnline));
            }

            _lastResult = list;

            return _lastResult;
        }
    }
}