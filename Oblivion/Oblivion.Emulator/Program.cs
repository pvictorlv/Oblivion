using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Spectre.Console;

namespace Oblivion
{
    internal class Program
    {
        // Importing user32.dll for SetWindowPos function
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

        // Constants for SetWindowPos function
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;

        // Importing user32.dll for DeleteMenu function
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, uint nPosition, int wFlags);

        // Importing user32.dll for GetSystemMenu function
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        // Importing kernel32.dll for GetConsoleWindow function
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        internal const uint ScClose = 0xF060;

        
         /// <summary>
         /// Main method of Oblivion.Emulator.
         /// </summary>
         /// <param name="args">The arguments.</param>
         public static async Task Main(string[] args)
         {
             if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
             {
                 IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
                 SetWindowPos(hWnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
             }
             
             await StartEverything();

             while (Oblivion.IsLive)
             {
                 Console.CursorVisible = true;
                 if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                 {
                     await ConsoleCommandHandling.InvokeCommand(Console.ReadLine());
                 }
             }
         }

         
         /// <summary>
         /// Initialize the Oblivion Environment.
         /// </summary>
         private static async Task InitEnvironment()
         {
             if (Oblivion.IsLive)
                return;

             Console.CursorVisible = false;

             AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
            
             await Oblivion.Initialize();
         }


          /// <summary>
          /// Handles unhandled exceptions.
          /// </summary>
          /// <param name="sender">The sender.</param>
          /// <param name="args">The UnhandledExceptionEventArgs instance containing the event data.</param>
          private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
          {
              try
              {
                  Logging.DisablePrimaryWriting(true);
                  var ex = (Exception)args.ExceptionObject;
                  Logging.LogCriticalException($"SYSTEM CRITICAL EXCEPTION: {ex}");
              }
              catch (Exception ex)
              {
                  Console.WriteLine($"UNSAVED SYSTEM CRITICAL EXCEPTION: {ex}");
              }
          }

          
           /// <summary>
           /// Starts the console window.
           /// </summary>
           private static void StartConsoleWindow()
           {
               Console.Clear();
               PrintLogoAndVersionInfo();
           }

           
            /// <summary>
            /// Prints the logo and version info.
            /// </summary>
            private static void PrintLogoAndVersionInfo()
            {
                AnsiConsole.Write(
                    new FigletText("Oblivion")
                        .LeftJustified()
                        .Color(Color.Red));
                
                AnsiConsole.Write(
                    new FigletText("Habbo")
                        .RightJustified()
                        .Color(Color.DarkCyan));

                var table = new Table().Centered().Border(TableBorder.Rounded).BorderColor(Color.Green);

                table.AddColumn(new TableColumn("[u]Build[/]").Centered());
                table.AddColumn(new TableColumn("[u]Metadata[/]").Centered());
                table.AddRow($"{Oblivion.Version}.{Oblivion.Build}", $".NET {Environment.Version} ~ C# 11 Roslyn");

                AnsiConsole.Write(table);
                // show current OS info
                AnsiConsole.MarkupLine($"[green bold]OS:[/] {RuntimeInformation.OSDescription} [green dim]arch:[/] {RuntimeInformation.OSArchitecture}");
            }


             /// <summary>
             /// Starts everything required for the application.
             /// </summary>
             private static async Task StartEverything()
             {
                 StartConsoleWindow();

                 if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                 {
                     DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), ScClose, 0);
                 }

                 await InitEnvironment();
             }
    }
}