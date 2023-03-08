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
        internal static string DatabaseConnectionType = "MySQL", ServerLanguage = "english";

        /// <summary>
        ///     The build of the server
        /// </summary>
        internal static readonly string Build = "100", Version = "2.0";

        /// <summary>
        ///     The live currency type
        /// </summary>
        internal static int ConsoleTimer = 2000;

        /// <summary>
        ///     The is live
        /// </summary>
        internal static bool IsLive,
            DebugMode,
            ConsoleTimerOn;

        /// <summary>
        /// Multiply current users in rooms
        /// </summary>
        internal static int Multipy = 2;

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
        public static Dictionary<string, IPlugin> Plugins;

        /// <summary>
        ///     The users cached
        /// </summary>
        public static readonly ConcurrentDictionary<uint, Habbo> UsersCached = new ConcurrentDictionary<uint, Habbo>();

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
        private static readonly HashSet<char> AllowedSpecialChars = new HashSet<char>(new[]
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
            var path = AppDomain.CurrentDomain.BaseDirectory + "Plugins";

            if (!Directory.Exists(path))
                return null;

            var files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

            if (files.Length == 0)
                return null;

            var assemblies =
                files.Select(AssemblyName.GetAssemblyName)
                    .Select(Assembly.Load)
                    .Where(assembly => assembly != null)
                    .ToList();

            var pluginType = typeof(IPlugin);
            var pluginTypes = new List<Type>();

            /* TODO CHECK */
            foreach (var types in from assembly in assemblies where assembly != null select assembly.GetTypes())
                pluginTypes.AddRange(types.Where(type => type != null && !type.IsInterface && !type.IsAbstract)
                    .Where(type => type.GetInterface(pluginType.FullName) != null));

            var plugins = new List<IPlugin>(pluginTypes.Count);

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

                var userData = UserDataFactory.GetUserData((int) userId);


                if (userData?.User == null)
                    return null;

                UsersCached.TryAdd(userId, userData.User);
                userData.User.InitInformation(userData);

                return userData.User;
            }
            catch (Exception e)
            {
                Writer.Writer.LogException("Habbo GetHabboForId: " + e);
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
        public static string EscapeJSONString(string str) => str.Replace("\"", "\\\"");


        /// <summary>
        ///     Console Clear Thread
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        internal static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Console.Clear();
            Console.WriteLine();

            Out.WriteLine($"Console Cleared in: {DateTime.Now} Next Time on: {ConsoleTimer} MS ", "Oblivion.Boot",
                ConsoleColor.DarkGreen);

            Console.WriteLine();
            GC.Collect();

            Timer.Start();
        }

        /// <summary>
        ///     Main Void, Initializes the Emulator.
        /// </summary>
        internal static void Initialize()
        {
            Console.Title = "Oblivion Emulator | Loading [...]";
            ServerStarted = DateTime.Now;
            _defaultEncoding = Encoding.Default;
            MutedUsersByFilter = new Dictionary<uint, uint>();
            ChatEmotions.Initialize();

#if !DEBUG
            var ip = GetLocalIPAddress();
            if (ip != "149.56.89.213" && ip!= "147.135.85.92" && ip != "192.95.5.60" && ip != "54.39.202.206" && ip != "10.158.0.2" && ip != "149.56.121.186" &&
                !ip.StartsWith("192.168.") && ip != "147.135.27.245")
            {
                Console.WriteLine($"The ip {ip} is not allowed to use this program.");
                Console.ReadKey();
                return;
            }
#endif

            CultureInfo = CultureInfo.CreateSpecificCulture("en-GB");
            try
            {
                ConfigurationData.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings/main.ini"));
                ConfigurationData.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings/Welcome/settings.ini"), true);

                DatabaseConnectionType = ConfigurationData.Data["db.type"];
                

                Handler.Initialize(CryptoKeys.N, CryptoKeys.D, CryptoKeys.E);

                Manager = new DatabaseManager(ConfigurationData.Data["db.hostname"], uint.Parse(ConfigurationData.Data["db.port"]), ConfigurationData.Data["db.username"]
                , ConfigurationData.Data["db.password"], ConfigurationData.Data["db.name"], uint.Parse(ConfigurationData.Data["db.pool.maxsize"]));

                using (var queryReactor = GetDatabaseManager().GetQueryReactor())
                {
                    ConfigData = new ConfigData(queryReactor);
                    PetCommandHandler.Init(queryReactor);
                    PetLocale.Init(queryReactor);
                    OfflineMessages = new Dictionary<uint, List<OfflineMessage>>();
                    OfflineMessage.InitOfflineMessages(queryReactor);
                }

                ConsoleTimer = int.Parse(ConfigurationData.Data["console.clear.time"]);
                ConsoleTimerOn = bool.Parse(ConfigurationData.Data["console.clear.enabled"]);
                FriendRequestLimit = (uint) int.Parse(ConfigurationData.Data["client.maxrequests"]);

                LibraryParser.RegisterAll();

                Plugins = new Dictionary<string, IPlugin>();
                
                var plugins = LoadPlugins();

                if (plugins != null)
                    foreach (var item in plugins.Where(item => item != null))
                    {
                        Plugins.Add(item.PluginName, item);

                        Out.WriteLine("Loaded Plugin: " + item.PluginName + " Version: " + item.PluginVersion,
                            "Oblivion.Plugins", ConsoleColor.DarkBlue);
                    }
                    
                
                
                ExtraSettings.RunExtraSettings();
                CrossDomainPolicy.Set();

                _game = new Game();
                _game.GetNavigator().LoadNewPublicRooms();
                _game.ContinueLoading();

                ServerLanguage = Convert.ToString(ConfigurationData.Data["system.lang"]);
                _languages = new Languages(ServerLanguage);
                Out.WriteLine("Loaded " + _languages.Count() + " Languages Vars", "Oblivion.Lang");
                
                if (plugins != null)
                    foreach (var itemTwo in plugins)
                        itemTwo?.message_void();

                if (ConsoleTimerOn)
                    Out.WriteLine("Console Clear Timer is Enabled, with " + ConsoleTimer + " Seconds.",
                        "Oblivion.Boot");

                ClientMessageFactory.Init();

                Out.WriteLine(
                    "Starting up asynchronous sockets server for game connections for port " +
                    int.Parse(ConfigurationData.Data["game.tcp.port"]), "Server.AsyncSocketListener");

                                _connectionManager = new ConnectionHandling(int.Parse(ConfigurationData.Data["game.tcp.port"]),
                                    int.Parse(ConfigurationData.Data["game.tcp.conlimit"]),
                                    int.Parse(ConfigurationData.Data["game.tcp.conperip"]),
                                    ConfigurationData.Data["game.tcp.antiddos"].ToLower() == "true",
                                    ConfigurationData.Data["game.tcp.enablenagles"].ToLower() == "true");

                

                Console.WriteLine();

                Out.WriteLine(
                    "Asynchronous sockets server for game connections running on port " +
                    int.Parse(ConfigurationData.Data["game.tcp.port"]) + Environment.NewLine,
                    "Server.AsyncSocketListener");


                new MusSocket(int.Parse(ConfigurationData.Data["mus.tcp.port"]),
                    ConfigurationData.Data["mus.tcp.allowedaddr"]);

                if (ExtraSettings.WebSocketAddr.Length >= 10)
                    _webSocket = new WebSocketManager(ExtraSettings.WebSocketAddr);

                LibraryParser.Initialize();
                Console.WriteLine();

                if (ConsoleTimerOn)
                {
                    Timer = new Timer {Interval = ConsoleTimer};
                    Timer.Elapsed += TimerElapsed;
                    Timer.Start();
                }

                if (ConfigurationData.Data.ContainsKey("StaffAlert.MinRank"))
                    StaffAlertMinRank = uint.Parse(ConfigurationData.Data["StaffAlert.MinRank"]);


                if (ConfigurationData.Data.ContainsKey("Debug"))
                    if (ConfigurationData.Data["Debug"] == "true")
                        DebugMode = true;

                Out.WriteLine("Oblivion Emulator ready. Status: idle", "Oblivion.Boot");
                Console.Beep();
                IsLive = true;
            }
            catch (Exception e)
            {
                Out.WriteLine(
                    "Error loading config.ini: Configuration file is invalid" + Environment.NewLine + e.Message,
                    "Oblivion.Boot", ConsoleColor.Red);
                Out.WriteLine("Please press Y to get more details or press other Key to Exit", "Oblivion.Boot",
                    ConsoleColor.Red);
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    Out.WriteLine(e.ToString());
                    Out.WriteLine(
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
        

        public static string GetLocalIPAddress()
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
        internal static int GetUnixTimeStamp() => (int) (DateTime.UtcNow -
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
        internal static long Now() => (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;

      

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
                    var result = GetHabboById((uint) id);

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
        internal static void PerformShutDown()
        {
            PerformShutDown(false);
        }

        /// <summary>
        ///     Performs the restart.
        /// </summary>
        internal static void PerformRestart()
        {
            PerformShutDown(true);
        }

        /// <summary>
        ///     Shutdown the Emulator
        /// </summary>
        /// <param name="restart">if set to <c>true</c> [restart].</param>
        /// Set a Different Message in Hotel
        internal static void PerformShutDown(bool restart)
        {
            try
            {
                var now = DateTime.Now;

                Cache.StopProcess();

                ShutdownStarted = true;
                Task.Factory.StartNew(() =>
                {
                    var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    serverMessage.AppendString("disconnection");
                    serverMessage.AppendInteger(2);
                    serverMessage.AppendString("title");
                    serverMessage.AppendString("HEY EVERYONE!");
                    serverMessage.AppendString("message");
                    serverMessage.AppendString(
                        restart
                            ? "<b>The hotel is shutting down for a break.<)/b>\nYou may come back later.\r\n<b>So long!</b>"
                            : "<b>The hotel is shutting down for a break.</b><br />You may come back soon. Don't worry, everything's going to be saved..<br /><b>So long!</b>\r\n~ This session was powered by OblivionEmulator");
                    GetGame().GetClientManager().SendMessageAsync(serverMessage);
                });
                Console.Title = "Oblivion Emulator | Shutting down...";

                _game.StopGameLoop();
                _game.GetRoomManager().RemoveAllRooms();
                _game.GetClientManager().CloseAll();

                GetConnectionManager().Destroy();

                /* TODO CHECK */
                foreach (Guild group in _game.GetGroupManager().Groups.Values) group.UpdateForum();

                using (var queryReactor = Manager.GetQueryReactor())
                {
                    queryReactor.RunFastQuery("UPDATE users SET online = '0'");
                }

                _connectionManager.Destroy();
                _game.Destroy();

                try
                {
                    Manager.Destroy();
                    Out.WriteLine("Game Manager destroyed", "Oblivion.GameManager", ConsoleColor.DarkYellow);
                }
                catch (Exception e)
                {
                    Writer.Writer.LogException("Oblivion.cs PerformShutDown GameManager" + e);
                }

                var span = DateTime.Now - now;

                Out.WriteLine("Elapsed " + TimeSpanToString(span) + "ms on Shutdown Proccess", "Oblivion.Life",
                    ConsoleColor.DarkYellow);

                if (!restart)
                    Out.WriteLine("Shutdown Completed. Press Any Key to Continue...", string.Empty,
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