using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Oblivion.Configuration;
using Oblivion.Database;
using Oblivion.HabboHotel;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Misc;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.UserDataManagement;
using Oblivion.Manager;
using Oblivion.Messages;
using Oblivion.Messages.Factorys;
using Oblivion.Messages.Parsers;
using Oblivion.Util;
using Oblivion.Connection.Net;
using Oblivion.Connection.WebSocket;
using Timer = System.Timers.Timer;
using Oblivion.Encryption.Encryption;
using Spectre.Console;

namespace Oblivion
{
    public class CryptoKeys
    {
        public const string E = "3";

        public const string N =
            "86851dd364d5c5cece3c883171cc6ddc5760779b992482bd1e20dd296888df91b33b936a7b93f06d29e8870f703a216257dec7c81de0058fea4cc5116f75e6efc4e9113513e45357dc3fd43d4efab5963ef178b78bd61e81a14c603b24c8bcce0a12230b320045498edc29282ff0603bc7b7dae8fc1b05b52b2f301a9dc783b7";

        public const string D =
            "59ae13e243392e89ded305764bdd9e92e4eafa67bb6dac7e1415e8c645b0950bccd26246fd0d4af37145af5fa026c0ec3a94853013eaae5ff1888360f4f9449ee023762ec195dff3f30ca0b08b8c947e3859877b5d7dced5c8715c58b53740b84e11fbc71349a27c31745fcefeeea57cff291099205e230e0c7c27e8e1c0512b";
    }

    /// <summary>
    ///     Class Oblivion.
    /// </summary>
    public static class Oblivion
    {
        /// <summary>
        ///     Oblivion Environment: Main Thread of Oblivion Emulator, SetUp's the Emulator
        ///     Contains Initialize: Responsible of the Emulator Loadings
        /// </summary>
        internal static string DatabaseConnectionType = "MySQL";

        /// <summary>
        ///     Oblivion Environment: Main Thread of Oblivion Emulator, SetUp's the Emulator
        ///     Contains Initialize: Responsible of the Emulator Loadings
        /// </summary>
        private static string _serverLanguage = "english";

        /// <summary>
        ///     The build of the server
        /// </summary>
        internal static readonly string Version = "2.3", Build = "100";

        /// <summary>
        ///     The live currency type
        /// </summary>
        private static int _consoleTimer = 2000;

        /// <summary>
        ///     The is live
        /// </summary>
        internal static bool IsLive,
            DebugMode,
            ConsoleTimerOn;

        /// <summary>
        /// Multiply current users in rooms
        /// </summary>
        internal static int Multipy = 1;

        /// <summary>
        ///     The staff alert minimum rank
        /// </summary>
        internal static uint StaffAlertMinRank = 4, FriendRequestLimit = 1000;

        /// <summary>
        ///     Bobba Filter Muted Users by Filter
        /// </summary>
        internal static Dictionary<uint, uint> MutedUsersByFilter;

        /// <summary>
        ///     The manager
        /// </summary>
        internal static DatabaseManager Manager;

        /// <summary>
        ///     The configuration data
        /// </summary>
        internal static ConfigData ConfigData;

        /// <summary>
        ///     The server started
        /// </summary>
        internal static DateTime ServerStarted;

        /// <summary>
        ///     The offline messages
        /// </summary>
        internal static Dictionary<uint, List<OfflineMessage>> OfflineMessages;

        /// <summary>
        ///     The timer
        /// </summary>
        internal static Timer Timer;

        /// <summary>
        ///     The culture information
        /// </summary>
        internal static CultureInfo CultureInfo;

        /// <summary>
        ///     The _plugins
        /// </summary>
        private static Dictionary<string, IPlugin> _plugins;

        /// <summary>
        ///     The users cached
        /// </summary>
        public static readonly ConcurrentDictionary<uint, Habbo> UsersCached = new();

        /// <summary>
        ///     The _connection manager
        /// </summary>
        private static ConnectionHandling _connectionManager;

        /// <summary>
        ///     The _default encoding
        /// </summary>
        private static Encoding _defaultEncoding;

        /// <summary>
        ///     The _game
        /// </summary>
        private static Game _game;

        /// <summary>
        ///     The _languages
        /// </summary>
        private static Languages _languages;

        /// <summary>
        ///     The allowed special chars
        /// </summary>
        private static readonly HashSet<char> AllowedSpecialChars = new(new[]
        {
            '-', '.', ' ', 'Ã', '©', '¡', '­', 'º', '³', 'Ã', '‰', '_'
        });

        /// <summary>
        ///     Check's if the Shutdown Has Started
        /// </summary>
        /// <value><c>true</c> if [shutdown started]; otherwise, <c>false</c>.</value>
        internal static bool ShutdownStarted { get; set; }

        public static bool ContainsAny(this string haystack, params string[] needles) => needles.Any(haystack.Contains);

        /// <summary>
        ///     Start the Plugin System
        /// </summary>
        /// <returns>ICollection&lt;IPlugin&gt;.</returns>
        public static ICollection<IPlugin> LoadPlugins()
        {
            // Get the path to the Plugins directory in the application's base directory
            var path = AppDomain.CurrentDomain.BaseDirectory + "Plugins";

// If the Plugins directory does not exist, return null
            if (!Directory.Exists(path))
                return null;

// Get all DLL files in the Plugins directory and its subdirectories
            var files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

// If there are no DLL files, return null
            if (files.Length == 0)
                return null;

// Load each DLL file as an assembly
            var assemblies =
                files.Select(AssemblyName.GetAssemblyName)
                    .Select(Assembly.Load)
                    .Where(assembly => assembly != null)
                    .ToList();

// Get the type of the IPlugin interface
            var pluginType = typeof(IPlugin);
            var pluginTypes = new List<Type>();

// Iterate over each assembly
            foreach (var types in from assembly in assemblies where assembly != null select assembly.GetTypes())
            {
                // For each type in the assembly, if it is not an interface or abstract,
                // and it implements the IPlugin interface, add it to the list of plugin types
                pluginTypes.AddRange(types.Where(type => type != null && !type.IsInterface && !type.IsAbstract)
                    .Where(type => pluginType.FullName != null && type.GetInterface(pluginType.FullName) != null));
            }

// Create a list to hold the plugin instances
            var plugins = new List<IPlugin>(pluginTypes.Count);

// For each plugin type, create an instance and add it to the list of plugins
            plugins.AddRange(pluginTypes.Select(type => (IPlugin) Activator.CreateInstance(type)));

            return plugins;
        }

        /// <summary>
        ///     Get's Habbo By The User Id
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Habbo.</returns>
        /// Table: users.id
        internal static Habbo GetHabboById(uint userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            try
            {
                var clientByUserId = GetGame().GetClientManager().GetClientByUserId(userId);

                if (clientByUserId != null)
                {
                    var habbo = clientByUserId.GetHabbo();

                    return habbo;
                }

                if (UsersCached.TryGetValue(userId, out var user))
                    return user;

                var userData = UserDataFactory.GetUserData((int)userId);


                if (userData?.User == null)
                    return null;

                UsersCached.TryAdd(userId, userData.User);
                userData.User.InitInformation(userData);

                return userData.User;
            }
            catch (Exception e)
            {
                Writer.Writer.HandleException(e);
            }

            return null;
        }

        internal static WebSocketManager GetWebSocket() => _webSocket;


        private static WebSocketManager _webSocket;

        /// <summary>
        /// Escape json
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeJsonString(string str)
        {
            // Create a StringBuilder to build the escaped string
            var stringBuilder = new StringBuilder(str.Length);

            // Iterate over each character in the string
            foreach (char c in str)
            {
                // Switch on the current character
                stringBuilder.Append(c switch
                {
                    // If the character is a quote or a backslash, prepend it with a backslash
                    '"' or '\\' => "\\" + c,
                    // If the character is a control character, append its escaped form
                    '\b' => "\b",
                    '\f' => "\f",
                    '\n' => "\n",
                    '\r' => "\r",
                    '\t' => "\t",
                    // Otherwise, append the character as is
                    _ => c.ToString()
                });
            }

            // Return the built string
            return stringBuilder.ToString();
        }


        /// <summary>
        ///     Console Clear Thread
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            AnsiConsole.Clear();
            Console.WriteLine();
            Out.WriteLineSimple($"Console Cleared in: {DateTime.Now} Next Time on: {_consoleTimer} ms", "Oblivion.Boot",
                ConsoleColor.DarkGreen);
            Console.WriteLine();
            GC.Collect();
            Timer.Start();
        }

        /// <summary>
        ///     Main Void, Initializes the Emulator.
        /// </summary>
        internal static async Task Initialize()
        {
            Console.Title = "Oblivion Emulator | Loading [...]";
            ServerStarted = DateTime.Now;
            _defaultEncoding = Encoding.Default;
            MutedUsersByFilter = new Dictionary<uint, uint>();
            ChatEmotions.Initialize();

            CultureInfo = CultureInfo.CreateSpecificCulture("en-GB");
            try
            {
                ConfigurationData.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings/main.ini"));
                ConfigurationData.Load(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings/Welcome/settings.ini"), true);

                DatabaseConnectionType = ConfigurationData.Data["db.type"];
                
                Handler.Initialize(CryptoKeys.N, CryptoKeys.D, CryptoKeys.E);

                Manager = new DatabaseManager(ConfigurationData.Data["db.hostname"],
                    uint.Parse(ConfigurationData.Data["db.port"]), ConfigurationData.Data["db.username"]
                    , ConfigurationData.Data["db.password"], ConfigurationData.Data["db.name"],
                    uint.Parse(ConfigurationData.Data["db.pool.maxsize"]));

                using (var queryReactor = await GetDatabaseManager().GetQueryReactorAsync())
                {
                    ConfigData = new ConfigData(queryReactor);
                    PetCommandHandler.Init(queryReactor);
                    PetLocale.Init(queryReactor);
                    OfflineMessages = new Dictionary<uint, List<OfflineMessage>>();
                    OfflineMessage.InitOfflineMessages(queryReactor);
                }

                _consoleTimer = int.Parse(ConfigurationData.Data["console.clear.time"]);
                ConsoleTimerOn = bool.Parse(ConfigurationData.Data["console.clear.enabled"]);
                FriendRequestLimit = (uint)int.Parse(ConfigurationData.Data["client.maxrequests"]);

                LibraryParser.RegisterAll();

                _plugins = new Dictionary<string, IPlugin>();

                var plugins = LoadPlugins();

                if (plugins != null)
                    foreach (var item in plugins.Where(item => item != null))
                    {
                        _plugins.Add(item.PluginName, item);

                        Out.WriteLineSimple("Loaded Plugin: " + item.PluginName + " Version: " + item.PluginVersion,
                            "Oblivion.Plugins", ConsoleColor.DarkBlue);
                    }


                ExtraSettings.RunExtraSettings();
                CrossDomainPolicy.Set();

                _game = new Game();
                await _game.Init();
                await _game.GetNavigator().LoadNewPublicRooms();
                await _game.ContinueLoading();

                _serverLanguage = Convert.ToString(ConfigurationData.Data["system.lang"]);
                _languages = new Languages(_serverLanguage);
                Out.WriteLine("Loaded " + _languages.Count() + " Languages Vars", "Oblivion.Lang");

                if (plugins != null)
                    foreach (var itemTwo in plugins)
                        itemTwo?.message_void();

                if (ConsoleTimerOn)
                    Out.WriteLine("Console Clear Timer is Enabled, with " + _consoleTimer + " Seconds.",
                        "Oblivion.Boot");

                ClientMessageFactory.Init();

                Out.WriteLine(
                    $"Starting up asynchronous sockets server for game connections for port {int.Parse(ConfigurationData.Data["game.tcp.port"])}", "Server.AsyncSocketListener");

                _connectionManager = new ConnectionHandling(int.Parse(ConfigurationData.Data["game.tcp.port"]),
                    int.Parse(ConfigurationData.Data["game.tcp.conlimit"]),
                    int.Parse(ConfigurationData.Data["game.tcp.conperip"]),
                    ConfigurationData.Data["game.tcp.antiddos"].ToLower() == "true",
                    ConfigurationData.Data["game.tcp.enablenagles"].ToLower() == "true");

                await _connectionManager.StartServer();

                Console.WriteLine();

                Out.WriteLine(
                    $"Asynchronous sockets server for game connections running on port {int.Parse(ConfigurationData.Data["game.tcp.port"])}{Environment.NewLine}",
                    "Server.AsyncSocketListener");


                _ = new MusSocket(int.Parse(ConfigurationData.Data["mus.tcp.port"]),
                    ConfigurationData.Data["mus.tcp.allowedaddr"]);

                if (ExtraSettings.WebSocketAddr.Length >= 10)
                    _webSocket = new WebSocketManager(ExtraSettings.WebSocketAddr);

                LibraryParser.Initialize();
                Console.WriteLine();

                if (ConsoleTimerOn)
                {
                    Timer = new Timer { Interval = _consoleTimer };
                    Timer.Elapsed += TimerElapsed;
                    Timer.Start();
                }

                if (ConfigurationData.Data.TryGetValue("StaffAlert.MinRank", out var value))
                    StaffAlertMinRank = uint.Parse(value);


                if (ConfigurationData.Data.TryGetValue("Debug", out var isDebug))
                    if (isDebug == "true")
                        DebugMode = true;

                Out.WriteLine("Oblivion Emulator ready. Status: idle", "Oblivion.Boot");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    Console.Beep();
                IsLive = true;
            }
            catch (Exception e)
            {
                Out.WriteLineSimple(
                    "Error loading config.ini: Configuration file is invalid" + Environment.NewLine + e.Message,
                    "Oblivion.Boot", ConsoleColor.Red);
                
                Out.WriteLineSimple(e.StackTrace.ToString(), "Oblivion.Boot", ConsoleColor.DarkRed);
                Out.WriteLineSimple("Please press Y to get more details or press other Key to Exit", "Oblivion.Boot",
                    ConsoleColor.Red);
                    
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    Environment.Exit(1);
                }
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    Out.WriteLine(e.ToString());
                    Out.WriteLineSimple(
                        Environment.NewLine + "[Message] Error Details: " + Environment.NewLine + e.StackTrace +
                        Environment.NewLine + e.InnerException + Environment.NewLine + e.TargetSite +
                        Environment.NewLine + "[Message]Press Any Key To Exit", "Oblivion.Boot", ConsoleColor.Red);
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                else
                {
                    Environment.Exit(1);
                }
            }
        }


        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        /// <summary>
        ///     Convert's Enum to Boolean
        /// </summary>
        /// <param name="theEnum">The theEnum.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool EnumToBool(string theEnum) => theEnum == "1";

        /// <summary>
        ///     Convert's Boolean to Integer
        /// </summary>
        /// <param name="theBool">if set to <c>true</c> [theBool].</param>
        /// <returns>System.Int32.</returns>
        internal static int BoolToInteger(bool theBool) => theBool ? 1 : 0;

        /// <summary>
        ///     Convert's Boolean to Enum
        /// </summary>
        /// <param name="theBool">if set to <c>true</c> [theBool].</param>
        /// <returns>System.String.</returns>
        internal static string BoolToEnum(bool theBool) => theBool ? "1" : "0";

        /// <summary>
        ///     Generates a Random Number in the Interval Min,Max
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetRandomNumber(int min, int max) => RandomNumber.Get(min, max);

        /// <summary>
        ///     Get's the Actual Timestamp in Unix Format
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal static int GetUnixTimeStamp() => (int)(DateTime.UtcNow -
                                                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .TotalSeconds;

        /// <summary>
        ///     Convert's a Unix TimeStamp to DateTime
        /// </summary>
        /// <param name="unixTimeStamp">The unix time stamp.</param>
        /// <returns>DateTime.</returns>
        internal static DateTime UnixToDateTime(double unixTimeStamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0,
            DateTimeKind.Local).AddSeconds(unixTimeStamp).ToLocalTime();

        internal static DateTime UnixToDateTime(int unixTimeStamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0,
            DateTimeKind.Local).AddSeconds(unixTimeStamp).ToLocalTime();

        /// <summary>
        ///     Convert timestamp to GroupJoin String
        /// </summary>
        /// <param name="timeStamp">The target.</param>
        /// <returns>System.String.</returns>
        public static string GetGroupDateJoinString(long timeStamp)
        {
            var time = UnixToDateTime(timeStamp).ToString("MMMM/dd/yyyy", CultureInfo).Split('/');

            return $"{time[0].Substring(0, 3)} {time[1]}, {time[2]}";
        }

        /// <summary>
        ///     Convert's a DateTime to Unix TimeStamp
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>System.Int32.</returns>
        internal static int DateTimeToUnix(DateTime target) => Convert.ToInt32(
            (target - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

        /// <summary>
        ///     Convert's a String to Unix TimeStamp
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>System.Int32.</returns>
        internal static int DateToUnix(string target)
        {
            var time = Convert.ToDateTime(target);
            return Convert.ToInt32(
                (time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }

        /// <summary>
        ///     Get the Actual Time
        /// </summary>
        /// <returns>System.Int64.</returns>
        internal static long Now() => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;


        /// <summary>
        ///     Filter's the Habbo Avatars Figure
        /// </summary>
        /// <param name="figure">The figure.</param>
        /// <returns>System.String.</returns>
        internal static string FilterFigure(string figure) => figure.Any(character => !IsValid(character))
            ? "lg-3023-1335.hr-828-45.sh-295-1332.hd-180-4.ea-3168-89.ca-1813-62.ch-235-1332"
            : figure;

        /// <summary>
        ///     Check if is a Valid AlphaNumeric String
        /// </summary>
        /// <param name="inputStr">The input string.</param>
        /// <returns><c>true</c> if [is valid alpha numeric] [the specified input string]; otherwise, <c>false</c>.</returns>
        internal static bool IsValidAlphaNumeric(string inputStr) => !string.IsNullOrEmpty(inputStr.ToLower()) &&
                                                                     inputStr.ToLower().All(IsValid);

        /// <summary>
        ///     Get a Habbo With the Habbo's Username
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Habbo.</returns>
        /// Table: users.username
        internal static Habbo GetHabboForName(string userName)
        {
            try
            {
                int id;
                using (var queryReactor = GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("SELECT id FROM users WHERE username = @user");

                    queryReactor.AddParameter("user", userName);

                    id = queryReactor.GetInteger();
                }

                if (id > 0)
                {
                    var result = GetHabboById((uint)id);

                    return result;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        /// <summary>
        ///     Check if the Input String is a Integer
        /// </summary>
        /// <param name="theNum">The theNum.</param>
        /// <returns><c>true</c> if the specified theNum is number; otherwise, <c>false</c>.</returns>
        internal static bool IsNum(string theNum) => double.TryParse(theNum, out _);

        /// <summary>
        ///     Get the Database Configuration Data
        /// </summary>
        /// <returns>ConfigData.</returns>
        internal static ConfigData GetDbConfig() => ConfigData;

        /// <summary>
        ///     Get's the Default Emulator Encoding
        /// </summary>
        /// <returns>Encoding.</returns>
        internal static Encoding GetDefaultEncoding() => _defaultEncoding;

        /// <summary>
        ///     Get's the Game Connection Manager Handler
        /// </summary>
        /// <returns>ConnectionHandling.</returns>
        internal static ConnectionHandling GetConnectionManager() => _connectionManager;

        /// <summary>
        ///     Get's the Game Environment Handler
        /// </summary>
        /// <returns>Game.</returns>
        internal static Game GetGame() => _game;

        /// <summary>
        ///     Gets the language.
        /// </summary>
        /// <returns>Languages.</returns>
        internal static Languages GetLanguage() => _languages;

        /// <summary>
        ///     Filter's SQL Injection Characters
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        internal static string FilterInjectionChars(string input)
        {
            input = input.Replace('\u0001', ' ');
            input = input.Replace('\u0002', ' ');
            input = input.Replace('\u0003', ' ');
            input = input.Replace('\t', ' ');

            return input;
        }

        /// <summary>
        ///     Get's the Database Manager Handler
        /// </summary>
        /// <returns>DatabaseManager.</returns>
        internal static DatabaseManager GetDatabaseManager() => Manager;

        /// <summary>
        ///     Perform's the Emulator Shutdown
        /// </summary>
        internal static async void PerformShutDown()
        {
            await PerformShutDown(false);
        }

        /// <summary>
        ///     Performs the restart.
        /// </summary>
        internal static async void PerformRestart()
        {
            await PerformShutDown(true);
        }

        /// <summary>
        ///     Shutdown the Emulator
        /// </summary>
        /// <param name="restart">if set to <c>true</c> [restart].</param>
        /// Set a Different Message in Hotel
        internal static async Task PerformShutDown(bool restart)
        {
            try
            {
                var now = DateTime.Now;

                Cache.StopProcess();

                ShutdownStarted = true;
                await Task.Run(async () =>
                {
                    var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    await serverMessage.AppendStringAsync("disconnection");
                    await serverMessage.AppendIntegerAsync(2);
                    await serverMessage.AppendStringAsync("title");
                    await serverMessage.AppendStringAsync("HEY EVERYONE!");
                    await serverMessage.AppendStringAsync("message");
                    await serverMessage.AppendStringAsync(
                        restart
                            ? "<b>The hotel is shutting down for a break.<)/b>\nYou may come back later.\r\n<b>So long!</b>"
                            : "<b>The hotel is shutting down for a break.</b><br />You may come back soon. Don't worry, everything's going to be saved..<br /><b>So long!</b>\r\n~ This session was powered by OblivionEmulator");
                   await GetGame().GetClientManager().SendMessageAsync(serverMessage);
                });
                Console.Title = "Oblivion Emulator | Shutting down...";

                _game.StopGameLoop();
                await _game.GetRoomManager().RemoveAllRooms();
                await _game.GetClientManager().CloseAll();

                GetConnectionManager().Destroy();

                /* TODO CHECK */
                foreach (Guild group in _game.GetGroupManager().Groups.Values) group.UpdateForum();

                using (var queryReactor = await Manager.GetQueryReactorAsync())
                {
                    await queryReactor.RunFastQueryAsync("UPDATE users SET online = '0'");
                }

                _connectionManager.Destroy();
                _game.Destroy();

                try
                {
                    Manager.Destroy();
                    Out.WriteLineSimple("Game Manager destroyed", "Oblivion.GameManager", ConsoleColor.DarkYellow);
                }
                catch (Exception e)
                {
                    Writer.Writer.LogException("Oblivion.cs PerformShutDown GameManager" + e);
                }

                var span = DateTime.Now - now;

                Out.WriteLineSimple("Elapsed " + TimeSpanToString(span) + "ms on Shutdown Proccess", "Oblivion.Life",
                    ConsoleColor.DarkYellow);

                if (!restart)
                    Out.WriteLineSimple("Shutdown Completed. Press Any Key to Continue...", string.Empty,
                        ConsoleColor.DarkRed);

                if (!restart)
                    Console.ReadKey();

                IsLive = false;

                if (restart)
                    Process.Start(Assembly.GetEntryAssembly().Location);

                Console.WriteLine("Closing...");
                Environment.Exit(0);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Convert's a Unix TimeSpan to A String
        /// </summary>
        /// <param name="span">The span.</param>
        /// <returns>System.String.</returns>
        internal static string TimeSpanToString(TimeSpan span) => string.Concat(span.Seconds, " s, ", span.Milliseconds,
            " ms");

        /// <summary>
        ///     Check's if Input Data is a Valid AlphaNumeric Character
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns><c>true</c> if the specified c is valid; otherwise, <c>false</c>.</returns>
        private static bool IsValid(char c) => char.IsLetterOrDigit(c) || AllowedSpecialChars.Contains(c);
    }
}