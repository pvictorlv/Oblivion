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
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
//            Console.SetWindowSize(150, 50);
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(@"     " + @"   ____  _     _ _       _             ");
            Console.WriteLine(@"     " + @"  / __ \| |   | (_)     (_)            ");
            Console.WriteLine(@"     " + @" | |  | | |__ | |___   ___  ___  _ __  ");
            Console.WriteLine(@"     " + @" | |  | | '_ \| | \ \ / / |/ _ \| '_ \ ");
            Console.WriteLine(@"     " + @" | |__| | |_) | | |\ V /| | (_) | | | |");
            Console.WriteLine(@"     " + @"  \____/|_.__/|_|_| \_/ |_|\___/|_| |_|");
            Console.WriteLine();
            Console.WriteLine(@"     " + @"  BUILD " + Oblivion.Version + "." + Oblivion.Build + " RELEASE 63B NO CRYPTO");
            Console.WriteLine(@"     " + @"  .NET Framework " + Environment.Version + "     C# 7 Roslyn");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
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