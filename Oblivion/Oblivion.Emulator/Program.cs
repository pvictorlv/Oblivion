using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Oblivion.Configuration;

namespace Oblivion
{
    internal class Program
    {
        /// <summary>
        /// Main Void of Oblivion.Emulator
        /// </summary>
        /// <param name="args">The arguments.</param>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        [STAThread]
        public static void Main(string[] args)
        {
            StartEverything();

            while (Oblivion.IsLive)
            {
                Console.CursorVisible = true;
                ConsoleCommandHandling.InvokeCommand(Console.ReadLine());
            }
        }

        private static void StartEverything()
        {
            StartConsoleWindow();
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), ScClose, 0);
            InitEnvironment();
        }

        public static void StartConsoleWindow()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Console.SetWindowSize(Console.LargestWindowWidth > 149 ? 150 : Console.WindowWidth, Console.LargestWindowHeight > 49 ? 50 : Console.WindowHeight);
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.WriteLine(@"     " + @"                                            |         |              ");
            Console.WriteLine(@"     " + @",---.,---,.   .,---.,---.    ,---.,-.-..   .|    ,---.|--- ,---.,---.");
            Console.WriteLine(@"     " + @",---| .-' |   ||    |---'    |---'| | ||   ||    ,---||    |   ||    ");
            Console.WriteLine(@"     " + @"`---^'---'`---'`    `---'    `---'` ' '`---'`---'`---^`---'`---'`    ");
            Console.WriteLine();
            Console.WriteLine(@"     " + @"  BUILD " + Oblivion.Version + "." + Oblivion.Build + " RELEASE 63B CRYPTO BOTH SIDE");
            Console.WriteLine(@"     " + @"  .NET Framework " + Environment.Version + "     C# 6 Roslyn");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(Console.LargestWindowWidth > 149 ? "---------------------------------------------------------------------------------------------------------------------------------------------------" : "-------------------------------------------------------------------------");
        }

        /// <summary>
        /// Initialize the Oblivion Environment
        /// </summary>
        public static void InitEnvironment()
        {
            if (Oblivion.IsLive)
                return;

            Console.CursorVisible = false;
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += ExceptionHandler;
            Oblivion.Initialize();
        }

        /// <summary>
        /// Mies the handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logging.DisablePrimaryWriting(true);
            var ex = (Exception)args.ExceptionObject;
            Logging.LogCriticalException($"SYSTEM CRITICAL EXCEPTION: {ex}");
        }

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, uint nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        internal const uint ScClose = 0xF060;
    }
}