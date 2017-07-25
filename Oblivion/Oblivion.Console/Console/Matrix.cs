#region

using System;

#endregion

namespace Oblivion
{
    public class Matrix
    {
        public static int Counter;
        private static readonly Random Rand = new Random();

        public const int Interval = 100; // Normal Flowing of Matrix Rain
        private const int FullFlow = Interval + 30; // Fast Flowing of Matrix Rain
        private const int Blacking = FullFlow + 50; // Displaying the Test Alone

        private const ConsoleColor NormalColor = ConsoleColor.DarkGreen;
        private const ConsoleColor GlowColor = ConsoleColor.Green;
        private const ConsoleColor FancyColor = ConsoleColor.White;
        private const String TextInput = "Matrix";

        private static char AsciiCharacter //Randomised Inputs
        {
            get
            {
                var t = Rand.Next(10);
                if (t <= 2) return (char)('0' + Rand.Next(10));
                if (t <= 4) return (char)('a' + Rand.Next(27));
                if (t <= 6) return (char)('A' + Rand.Next(27));
                return (char)(Rand.Next(32, 255));
            }
        }

        public static void UpdateAllColumns(int width, int height, int[] y)
        {
            int x;
            if (Counter < Interval)
            {
                for (x = 0; x < width; ++x)
                {
                    Console.ForegroundColor = x % 10 == 1 ? FancyColor : GlowColor;
                    Console.SetCursorPosition(x, y[x]);
                    Console.Write(AsciiCharacter);

                    Console.ForegroundColor = x % 10 == 9 ? FancyColor : NormalColor;
                    var temp = y[x] - 2;
                    Console.SetCursorPosition(x, InScreenYPosition(temp, height));
                    Console.Write(AsciiCharacter);

                    var temp1 = y[x] - 20;
                    Console.SetCursorPosition(x, InScreenYPosition(temp1, height));
                    Console.Write(' ');
                    y[x] = InScreenYPosition(y[x] + 1, height);
                }
            }
            else if (Counter > Interval && Counter < FullFlow)
            {
                for (x = 0; x < width; ++x)
                {
                    Console.SetCursorPosition(x, y[x]);
                    Console.ForegroundColor = x % 10 == 9 ? FancyColor : NormalColor;

                    Console.Write(AsciiCharacter); //Printing the Character Always at Fixed position

                    y[x] = InScreenYPosition(y[x] + 1, height);
                }
            }
            else if (Counter > FullFlow)
            {
                for (x = 0; x < width; ++x)
                {
                    Console.SetCursorPosition(x, y[x]);
                    Console.Write(' '); //Slowly blacking out the Screen
                    var temp1 = y[x] - 20;
                    Console.SetCursorPosition(x, InScreenYPosition(temp1, height));
                    Console.Write(' ');
                    if (Counter > FullFlow && Counter < Blacking) // Clearing the Entire screen to get the Darkness
                    {
                        Console.ForegroundColor = x % 10 == 9 ? FancyColor : NormalColor;
                        var temp = y[x] - 2;
                        Console.SetCursorPosition(x, InScreenYPosition(temp, height));
                        Console.Write(AsciiCharacter); //The Text is printed Always
                    }
                    Console.SetCursorPosition(width / 2, height / 2);
                    Console.Write(TextInput);
                    y[x] = InScreenYPosition(y[x] + 1, height);
                }
            }
        }

        public static int InScreenYPosition(int yPosition, int height)
        {
            if (yPosition < 0) //When there is negative value
                return yPosition + height;
            return yPosition < height ? yPosition : 0;
        }

        public static void Initialize(out int width, out int height, out int[] y)
        {
            height = Console.WindowHeight;
            width = Console.WindowWidth - 1;
            y = new int[width];
            Console.Clear();

            for (var x = 0; x < width; ++x) //Setting the cursor at random at program startup
                y[x] = Rand.Next(height);
        }
    }
}