using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using Oblivion.Configuration;

namespace Oblivion
{
    internal class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            int uFlags);

        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;

        /// <summary>
        /// Main Void of Oblivion.Emulator
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static async Task Main(string[] args)
        {
            IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
            SetWindowPos(hWnd,
                new IntPtr(HWND_TOPMOST),
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE);
            
            await StartEverything();

            while (Oblivion.IsLive)
            {
                Console.CursorVisible = true;


                await ConsoleCommandHandling.InvokeCommand(Console.ReadLine());
            }
        }

        private static async Task StartEverything()
        {
            StartConsoleWindow();
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), ScClose, 0);
            await InitEnvironment();
        }

        public static void StartConsoleWindow()
        {
//            BringWindowToTop();
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
            Console.WriteLine(@"     " + @"  .NET " + Environment.Version + "     C# 11 Roslyn");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Initialize the Oblivion Environment
        /// </summary>
        public static async Task InitEnvironment()
        {

            if (Oblivion.IsLive)
                return;

            Console.CursorVisible = false;
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += ExceptionHandler;

            await Oblivion.Initialize();
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