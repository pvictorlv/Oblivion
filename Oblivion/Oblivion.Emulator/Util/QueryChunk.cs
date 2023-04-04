using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

namespace Oblivion.Util
{
    /// <summary>
    /// Class QueryChunk.
    /// </summary>
    internal class QueryChunk
    {
        /// <summary>
        /// The _ending type
        /// </summary>
        private readonly EndingType _endingType;

        /// <summary>
        /// The _parameters
        /// </summary>
        private Dictionary<string, object> _parameters;

        /// <summary>
        /// The _queries
        /// </summary>
        private StringBuilder _queries;

        /// <summary>
        /// The _query count
        /// </summary>
        private int _queryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryChunk"/> class.
        /// </summary>
        public QueryChunk()
        {
            _parameters = new Dictionary<string, object>();
            _queries = new StringBuilder();
            _queryCount = 0;
            _endingType = EndingType.Sequential;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryChunk"/> class.
        /// </summary>
        /// <param name="startQuery">The start query.</param>
        public QueryChunk(string startQuery)
        {
            _parameters = new Dictionary<string, object>();
            _queries = new StringBuilder(startQuery);
            _endingType = EndingType.Continuous;
            _queryCount = 0;
        }

        /// <summary>
        /// Adds the query.
        /// </summary>
        /// <param name="query">The query.</param>
        internal Task AddQuery(string query)
        {
            {
                _queryCount++;
                _queries.Append(query);

                switch (_endingType)
                {
                    case EndingType.Sequential:
                        _queries.Append(";");
                        return Task.CompletedTask;

                    case EndingType.Continuous:
                        _queries.Append(",");
                        return Task.CompletedTask;

                    default:
                        return Task.CompletedTask;
                }
            }
        }

        /// <summary>
        /// Adds the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        internal Task AddParameter(string parameterName, object value)
        {
            _parameters.Add(parameterName, value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal async Task Execute(IQueryAdapter dbClient)
        {
            if (_queryCount == 0)
                return;

            _queries = _queries.Remove((_queries.Length - 1), 1);

            dbClient.SetQuery(_queries.ToString());

            /* TODO CHECK */ foreach (var current in _parameters)
                dbClient.AddParameter(current.Key, current.Value);

            await dbClient.RunQueryAsync();
        }  /// <summary>
        /// Executes the specified database client with no lock.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal async Task ExecuteNoLock(IQueryAdapter dbClient)
        {
            if (_queryCount == 0)
                return;

            _queries = _queries.Remove((_queries.Length - 1), 1);

            dbClient.SetQuery(_queries.ToString());

            /* TODO CHECK */ foreach (var current in _parameters)
                dbClient.AddParameter(current.Key, current.Value);

            await dbClient.RunQueryAsync();
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        internal void Dispose()
        {
            _parameters.Clear();
            _queries.Clear();
            _parameters = null;
            _queries = null;
        }
    }
}