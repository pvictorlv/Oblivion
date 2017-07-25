using System;
using System.Text;

namespace Oblivion.Configuration
{
    /// <summary>
    /// Class Logging.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Gets or sets a value indicating whether [disabled state].
        /// </summary>
        /// <value><c>true</c> if [disabled state]; otherwise, <c>false</c>.</value>
        internal static bool DisabledState
        {
            get { return Writer.Writer.DisabledState; }
            set { Writer.Writer.DisabledState = value; }
        }

        /// <summary>
        /// Logs the query error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="query">The query.</param>
        public static void LogQueryError(Exception exception, string query)
        {
            Writer.Writer.LogQueryError(exception, query);
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="logText">The log text.</param>
        internal static void LogException(string logText)
        {
            Writer.Writer.LogException($"{Environment.NewLine}{logText}{Environment.NewLine}");
        }

        /// <summary>
        /// Logs the critical exception.
        /// </summary>
        /// <param name="logText">The log text.</param>
        internal static void LogCriticalException(string logText)
        {
            Writer.Writer.LogCriticalException(logText);
        }

        /// <summary>
        /// Logs the cache error.
        /// </summary>
        /// <param name="logText">The log text.</param>
        internal static void LogCacheError(string logText)
        {
            Writer.Writer.LogCacheError(logText);
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="logText">The log text.</param>
        internal static void LogMessage(string logText)
        {
            Writer.Writer.LogMessage(logText);
        }

        /// <summary>
        /// Logs the thread exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="threadname">The threadname.</param>
        internal static void LogThreadException(string exception, string threadname)
        {
            Writer.Writer.LogThreadException(exception, threadname);
        }

        /// <summary>
        /// Logs the packet exception.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="exception">The exception.</param>
        internal static void LogPacketException(string packet, string exception)
        {
            Writer.Writer.LogPacketException(packet, exception);
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="pException">The p exception.</param>
        /// <param name="pLocation">The p location.</param>
        internal static void HandleException(Exception pException, string pLocation)
        {
            Writer.Writer.HandleException(pException, pLocation);
        }

        /// <summary>
        /// Disables the primary writing.
        /// </summary>
        /// <param name="clearConsole">if set to <c>true</c> [clear console].</param>
        internal static void DisablePrimaryWriting(bool clearConsole)
        {
            Writer.Writer.DisablePrimaryWriting(clearConsole);
        }

        /// <summary>
        /// Logs the shutdown.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal static void LogShutdown(StringBuilder builder)
        {
            Writer.Writer.LogShutdown(builder);
        }
    }
}