using System;
using System.Text;
using Spectre.Console;

namespace Oblivion.Util
{
    public static class AnsiExtensions
    {
        public static string Emoji(this string emoji) => AnsiConsole.Profile.Capabilities.Legacy ? emoji : "";
    }

    internal static class Out
    {
        private static string ToMarkup(ConsoleColor color) => color switch
        {
            ConsoleColor.Black => "black",
            ConsoleColor.DarkBlue => "darkblue",
            ConsoleColor.DarkGreen => "darkgreen",
            ConsoleColor.DarkCyan => "darkcyan",
            ConsoleColor.DarkRed => "darkred",
            ConsoleColor.DarkMagenta => "darkmagenta",
            ConsoleColor.DarkYellow => "darkyellow",
            ConsoleColor.Gray => "gray",
            ConsoleColor.DarkGray => "darkgray",
            ConsoleColor.Blue => "blue",
            ConsoleColor.Green => "green",
            ConsoleColor.Cyan => "cyan",
            ConsoleColor.Red => "red",
            ConsoleColor.Magenta => "magenta",
            ConsoleColor.Yellow => "yellow",
            ConsoleColor.White => "white",
            _ => "white"
        };

        private static string FormatMessage(string header, string format, string color, bool customMarkup = false)
        {
            // Initialize StringBuilder
            var builder = new StringBuilder();

            // Append initial part with clock emoji
            builder.Append($"[blue]{DateTime.Now:HH:m:s.zzz}[/] ");

            // Conditional append for header with magnifying glass emoji
            if (!string.IsNullOrEmpty(header))
            {
                builder.Append($"[blue dim] {format.EscapeMarkup()}[/]");
            }

            // Append final part with information emoji
            if (string.IsNullOrEmpty(color))
                builder.Append($"[cyan dim] - [/]{format.EscapeMarkup()}");
            builder.Append($"[cyan dim] - [/][{color}]{(customMarkup ? header : header.EscapeMarkup())}[/]");

            return builder.ToString();
        }
        
        public static void WriteLineSimple(string format, string header = "", ConsoleColor color = ConsoleColor.DarkCyan)
        {
            var markupColor = ToMarkup(color);
            var message = FormatMessage(format, header, markupColor);
            AnsiConsole.MarkupLine(message);
        }
        
        public static void WriteLine(string format, string header = "", Color color = default)
        {
            if (color == default) color = Color.DarkCyan;
            var message = FormatMessage(format, header, color.ToMarkup());
            AnsiConsole.MarkupLine(message);
        }

        public static void WriteMarkup(string format, string header = "")
        {
            var message = FormatMessage(format, header, "");
            AnsiConsole.Markup(message);
        }

        public static void Write(string format, string header = "", ConsoleColor color = ConsoleColor.DarkCyan)
        {
            var markupColor = ToMarkup(color);
            var message = FormatMessage(format, header, markupColor);
            AnsiConsole.Markup(message);
        }
        
        public static void Write(string format, string header = "", Color color = default)
        {
            if (color == default) color = Color.DarkCyan;
            var message = FormatMessage(format, header, color.ToMarkup());
            AnsiConsole.Markup(message);
        }
    }
}
