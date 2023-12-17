using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Spectre.Console;
using Color = Spectre.Console.Color;

namespace Oblivion.Writer
{
    public static class Writer
    {
        public static bool DisableWriter { get; set; }

        private static void WriteLine(string line, Color color = default, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "", bool error = false)
        {
            if (color == default) color = Color.DarkCyan;
            if (DisableWriter)
                return;
// Initialize StringBuilder
            var builder = new StringBuilder();

            // Append initial part with clock emoji
            builder.Append($"[blue]{DateTime.Now:HH:m:s.zzz}[/] [{(error ? "red" : "cyan")} dim]{(error ? "Error" : "Info")} [/]");

            // Append final part with information emoji
            
            if (color == default) color = Color.DarkCyan;
            
            builder.Append($"[cyan dim] - [/][{color.ToMarkup()}]{line.EscapeMarkup()}[/]");
            
            AnsiConsole.MarkupLine(builder.ToString());
        }

        [Obsolete("Use HandleException(Exception, string?) instead.")]
        public static void LogException(string logText, Exception? exception = null, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
        {
            WriteToFile(@"Logs\ExceptionsLog.txt", logText);
            WriteLine($"An exception was registered: {logText.EscapeMarkup()}", Color.Red, filePath, callerMemberName, true);
            if (exception is {} ex) HandleException(ex, filePath, callerMemberName);
        }

        /// <summary>
        /// Handles an exception by logging it to a file and displaying its details in a table in the console.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="location">The location where the exception occurred. If null, "unknown location" is used.</param>
        public static void HandleException(Exception exception, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            // Create a StringBuilder to build the log message
            var stringBuilder = new StringBuilder();

            var location = $"{filePath.Split(Path.PathSeparator).Last().Replace(".cs", "")} on {memberName}";
            // Append the log message to the StringBuilder
            stringBuilder.AppendLine(
                $"Exception logged {DateTime.Now} in {location ?? "unknown location"}: {exception}");

            // Write the log message to the ExceptionsLog.txt file
            WriteToFile(@"Logs\ExceptionsLog.txt", stringBuilder.ToString());
            
            // write upmost inner exception to exception with AnsiConsole
            AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything);
        }


        public static void LogCriticalException(string logText, Exception? exception = null, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
        {
            WriteToFile(@"Logs\CriticalExceptionsLog.txt", logText);
            WriteLine($"A critical exception was registered: {logText.EscapeMarkup()}", default, filePath, callerMemberName, true);
            if (exception is {} ex) HandleException(ex, filePath, callerMemberName);
        }

        public static void LogCacheError(string logText, Exception? exception = null, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
        {
            WriteToFile(@"Logs\CacheErrorsLog.txt", logText);
            WriteLine($"A caching error was registered: {logText.EscapeMarkup()}", default, filePath, callerMemberName, true);
            if (exception is {} ex) HandleException(ex, filePath, callerMemberName);
        }

        public static void LogMessage(string logText, bool output = true)
        {
            WriteToFile(@"Logs\CommonLog.txt", logText);

            if (output)
                AnsiConsole.MarkupLine(logText.EscapeMarkup());
        }

        public static void LogThreadException(Exception exceptionText, string threadName, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
            => LogThreadException(exceptionText.ToString(), threadName, exceptionText, filePath, callerMemberName);
        public static void LogThreadException(string exceptionText, string threadName,Exception? exception = null, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
        {
            WriteToFile(@"Logs\ThreadErrorsLog.txt", $"Error en thread {threadName}: \r\n{exceptionText}");
            WriteLine($"An thread error was registered, in thread: {threadName.EscapeMarkup()}", Color.Red, filePath, callerMemberName, true);
            if (exception is {} ex) HandleException(ex, filePath, callerMemberName);
        }

        public static void LogQueryError(Exception exceptionText, string query, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
            => LogQueryError(exceptionText.ToString(), query, exceptionText, filePath, callerMemberName);
        public static void LogQueryError(string exceptionText, string query, Exception? exception = null, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
        {
            WriteToFile(@"Logs\MySQLErrors.txt", $"The query error was in: \r\n{query}\r\n{exceptionText}");
            WriteLine("A MySQL exception was registered.", Color.Red, filePath, callerMemberName, true);
            if (exception is {} ex) HandleException(ex, filePath, callerMemberName);
        }

        public static void LogPacketException(string packet, Exception exceptionText, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
            => LogPacketException(packet, exceptionText.ToString(), exceptionText, filePath, callerMemberName);
        public static void LogPacketException(string packet, string exceptionText, Exception? exception = null, [CallerFilePath] string filePath = "", [CallerMemberName] string callerMemberName = "")
        {
            WriteToFile(@"Logs\PacketLogError.txt", $"Error in packet {packet}: \r\n{exceptionText}");
            WriteLine("A packet exception was registered.", Color.Red, filePath, callerMemberName, true);
            if (exception is {} ex) HandleException(ex, filePath, callerMemberName);
        }

        public static void DisablePrimaryWriting(bool clearConsole)
        {
            DisableWriter = true;
            if (clearConsole)
                AnsiConsole.Clear();
        }

        public static void LogShutdown(StringBuilder builder)
        {
            WriteToFile(@"Logs\shutdownlog.txt", builder.ToString());
        }

        private static readonly Dictionary<string, SemaphoreSlim> FileLocks = new();
        private static void WriteToFile(string path, string content)
        {
            try
            {
                if (DisableWriter) return;
                if (!FileLocks.ContainsKey(path))
                    FileLocks.Add(path, new SemaphoreSlim(1,1));
                FileLocks[path].Wait();
                
                using var writer = new StreamWriter(path, true, Encoding.ASCII);
                writer.WriteLine(content);
                writer.Flush();
                writer.Close();
                FileLocks[path].Release();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
