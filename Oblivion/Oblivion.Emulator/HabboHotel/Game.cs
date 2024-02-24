using System;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Achievements;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.Catalogs;
using Oblivion.HabboHotel.Commands;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Handlers;
using Oblivion.HabboHotel.Misc;
using Oblivion.HabboHotel.Navigators;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Polls;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Roles;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.SoundMachine;
using Oblivion.HabboHotel.Support;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.Helpers;
using Oblivion.Manager;
using Oblivion.Messages.Enums;
using Oblivion.Security;
using Oblivion.Util;
using Spectre.Console;

namespace Oblivion.HabboHotel
{
    /// <summary>
    ///     Class Game.
    /// </summary>
    internal class Game
    {
        /// <summary>
        ///     The game loop enabled
        /// </summary>
        internal static bool GameLoopEnabled = true;

        /// <summary>
        ///     The _achievement manager
        /// </summary>
        private AchievementManager _achievementManager;

        /// <summary>
        ///     The _ban manager
        /// </summary>
        private ModerationBanManager _banManager;


        /// <summary>
        ///     The _catalog
        /// </summary>
        private CatalogManager _catalog;

        /// <summary>
        ///     The _client manager
        /// </summary>
        private readonly GameClientManager _clientManager;

        /// <summary>
        ///     The _clothing manager
        /// </summary>
        private ClothingManager _clothingManager;


        private RandomRewardFurniHandler _randomRewardHandler;

        /// <summary>
        ///     The _events
        /// </summary>
        private RoomEvents _events;

        /// <summary>
        ///     The _group manager
        /// </summary>
        private GroupManager _groupManager;

        /// <summary>
        ///     The _guide manager
        /// </summary>
        private GuideManager _guideManager;

        private HallOfFame _hallOfFame;

        /// <summary>
        ///     The _hotel view
        /// </summary>
        private HotelView _hotelView;

        /// <summary>
        ///     The _item manager
        /// </summary>
        private ItemManager _itemManager;

        /// <summary>
        ///  The camera Manager
        /// </summary>
        private CameraPhotoManager _cameraManager;

        /// <summary>
        ///     The _moderation tool
        /// </summary>
        private ModerationTool _moderationTool;

        /// <summary>
        ///     The _navigatorManager
        /// </summary>
        private NavigatorManager _navigatorManager;

        /// <summary>
        ///     The _pinata handler
        /// </summary>
        private PinataHandler _pinataHandler;

        /// <summary>
        ///     The _pixel manager
        /// </summary>
        private CoinsManager _pixelManager;

        /// <summary>
        ///     The _poll manager
        /// </summary>
        private PollManager _pollManager;

        /// <summary>
        ///     The _quest manager
        /// </summary>
        private QuestManager _questManager;

        /// <summary>
        ///     The _role manager
        /// </summary>
        private RoleManager _roleManager;

        /// <summary>
        ///     The _room manager
        /// </summary>
        private RoomManager _roomManager;

        /// <summary>
        ///     The _talent manager
        /// </summary>
        private TalentManager _talentManager;

        private TargetedOfferManager _targetedOfferManager;

        /// <summary>
        ///     The _game loop
        /// </summary>
        private Thread _gameLoop;

        /// <summary>
        ///     The client manager cycle ended
        /// </summary>
        internal bool RoomManagerCycleEnded;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Game" /> class.
        /// </summary>
        internal Game()
        {
            Console.WriteLine();
            Out.WriteLine(@"Starting up Oblivion Emulator for " + Environment.MachineName + "...", @"Oblivion.Boot");
            Console.WriteLine();

            _clientManager = new GameClientManager();
        }

        internal async Task Init()
        {
            var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync();
            var status = AnsiConsole.Status(); // Changed from Progress to Status
            status.Spinner(Spinner.Known.CircleQuarters);
            // Define all tasks
            var tasks = new (string Description, Func<Task> Action)[]
            {
                ("Cleaning dirty in database...", () => { DatabaseCleanup(queryReactor); return Task.CompletedTask; }),
                ("Loading Bans...", async () => { _banManager = new ModerationBanManager(); await _banManager.LoadBans(queryReactor); }),
                ("Loading Roles...", () => { _roleManager = new RoleManager(); _roleManager.LoadRights(queryReactor); return Task.CompletedTask; }),
                ("Loading Items...", async () => { _itemManager = new ItemManager(); await _itemManager.LoadItems(queryReactor, 0); }),
                ("Loading Catalog...", () => { _catalog = new CatalogManager(); return Task.CompletedTask; }),
                ("Loading Targeted Offers...", () => { _targetedOfferManager = new TargetedOfferManager(); return Task.CompletedTask; }),
                ("Loading Clothing...", () => { _clothingManager = new ClothingManager(); _clothingManager.Initialize(queryReactor); return Task.CompletedTask; }),
                ("Loading Rooms...", () => { _roomManager = new RoomManager(); return Task.CompletedTask; }),
                ("Loading NavigatorManager...", () => { _navigatorManager = new NavigatorManager(); _navigatorManager.Initialize(queryReactor, out _); return Task.CompletedTask; }),
                ("Loading Groups...", () => { _groupManager = new GroupManager(); _groupManager.InitGroups(); return Task.CompletedTask; }),
                ("Loading PixelManager...", () => { _pixelManager = new CoinsManager(); return Task.CompletedTask; }),
                ("Loading HotelView...", () => { _hotelView = new HotelView(); return Task.CompletedTask; }),
                ("Loading Hall Of Fame...", () => { _hallOfFame = new HallOfFame(); return Task.CompletedTask; }),
                ("Loading ModerationTool...", async () => { _moderationTool = new ModerationTool(); await _moderationTool.LoadMessagePresets(queryReactor); }),
                ("Loading Quests...", () => { _questManager = new QuestManager(); _questManager.Initialize(queryReactor); return Task.CompletedTask; }),
                ("Loading Events...", () => { _events = new RoomEvents(); return Task.CompletedTask; }),
                ("Loading Camera Photo Manager...", async () => { _cameraManager = new CameraPhotoManager(); await _cameraManager.Init(_itemManager); }),
                ("Loading Talents...", () => { _talentManager = new TalentManager(); _talentManager.Initialize(queryReactor); return Task.CompletedTask; }),
                ("Loading Pinata...", async () => { _pinataHandler = new PinataHandler(); await _pinataHandler.Initialize(queryReactor); }),
                ("Loading Random Rewards...", () => { _randomRewardHandler = new RandomRewardFurniHandler(); return Task.CompletedTask; }),
                ("Loading Polls...", async () => { _pollManager = new PollManager(); await _pollManager.Init(queryReactor, 0); }),
                ("Loading Achievements...", async () => { _achievementManager = new AchievementManager(); await _achievementManager.LoadAchievements(queryReactor); }),
                ("Loading StaticMessages ...", () => { StaticMessagesManager.Load(); return Task.CompletedTask; }),
                ("Loading Guides ...", () => { _guideManager = new GuideManager(); return Task.CompletedTask; }),
                ("Loading and Registering Commands...", () => { CommandsManager.Register(); return Task.CompletedTask; }),
                ("Loading AntiMutant...", () => { this.AntiMutant = new AntiMutant(); return Task.CompletedTask; })
            };

            foreach (var (description, action) in tasks)
            {
                await status.StartAsync(description, ctx => action());
            }

            queryReactor.Dispose();
            
        }
/// <summary>
        ///     Gets a value indicating whether [game loop enabled ext].
        /// </summary>
        /// <value><c>true</c> if [game loop enabled ext]; otherwise, <c>false</c>.</value>
        internal bool GameLoopEnabledExt => GameLoopEnabled;

        /// <summary>
        ///     Gets a value indicating whether [game loop active ext].
        /// </summary>
        /// <value><c>true</c> if [game loop active ext]; otherwise, <c>false</c>.</value>
        internal bool GameLoopActiveExt { get; private set; }

        /// <summary>
        ///     Gets the game loop sleep time ext.
        /// </summary>
        /// <value>The game loop sleep time ext.</value>
        internal int GameLoopSleepTimeExt => 100;

        /// <summary>
        ///     Progresses the specified bar.
        /// </summary>
        /// <param name="bar">The bar.</param>
        /// <param name="wait">The wait.</param>
        /// <param name="end">The end.</param>
        /// <param name="message">The message.</param>
        public static void Progress(AbstractBar bar, int wait, int end, string message)
        {
            bar.PrintMessage(message);
            for (var cont = 0; cont < end; cont++)
                bar.Step();
        }

        /// <summary>
        /// get the camera manager
        /// </summary>
        /// <returns></returns>
        public CameraPhotoManager GetCameraManager() => _cameraManager;


        /// <summary>
        ///     Databases the cleanup.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        private static void DatabaseCleanup(IQueryAdapter dbClient)
        {
            dbClient.RunFastQuery("UPDATE users SET online = '0' WHERE online <> '0'");
            dbClient.RunFastQuery(
                "UPDATE `server_status` SET status = '1', users_online = '0', rooms_loaded = '0', server_ver = 'Oblivion Emulator', stamp = '" +
                Oblivion.GetUnixTimeStamp() + "' LIMIT 1;");
        }

        internal AntiMutant GetAntiMutant()
        {
            return this.AntiMutant;
        }

        public AntiMutant AntiMutant { get; set; }

        /// <summary>
        ///     Gets the client manager.
        /// </summary>
        /// <returns>GameClientManager.</returns>
        internal GameClientManager GetClientManager() => _clientManager;

        /// <summary>
        ///     Gets the ban manager.
        /// </summary>
        /// <returns>ModerationBanManager.</returns>
        internal ModerationBanManager GetBanManager() => _banManager;

        /// <summary>
        ///     Gets the role manager.
        /// </summary>
        /// <returns>RoleManager.</returns>
        internal RoleManager GetRoleManager() => _roleManager;

        /// <summary>
        ///     Gets the catalog.
        /// </summary>
        /// <returns>Catalog.</returns>
        internal CatalogManager GetCatalog() => _catalog;

        /// <summary>
        ///     Gets the room events.
        /// </summary>
        /// <returns>RoomEvents.</returns>
        internal RoomEvents GetRoomEvents() => _events;

        /// <summary>
        ///     Gets the guide manager.
        /// </summary>
        /// <returns>GuideManager.</returns>
        internal GuideManager GetGuideManager() => _guideManager;

        /// <summary>
        ///     Gets the navigator.
        /// </summary>
        /// <returns>NavigatorManager.</returns>
        internal NavigatorManager GetNavigator() => _navigatorManager;

        /// <summary>
        ///     Gets the item manager.
        /// </summary>
        /// <returns>ItemManager.</returns>
        internal ItemManager GetItemManager() => _itemManager;

        /// <summary>
        ///     Gets the room manager.
        /// </summary>
        /// <returns>RoomManager.</returns>
        internal RoomManager GetRoomManager() => _roomManager;

        /// <summary>
        ///     Gets the hotel view.
        /// </summary>
        /// <returns>HotelView.</returns>
        internal HotelView GetHotelView() => _hotelView;

        internal HallOfFame GetHallOfFame() => _hallOfFame;

        internal TargetedOfferManager GetTargetedOfferManager() => _targetedOfferManager;

        /// <summary>
        ///     Gets the achievement manager.
        /// </summary>
        /// <returns>AchievementManager.</returns>
        internal AchievementManager GetAchievementManager() => _achievementManager;

        /// <summary>
        ///     Gets the moderation tool.
        /// </summary>
        /// <returns>ModerationTool.</returns>
        internal ModerationTool GetModerationTool() => _moderationTool;

        /// <summary>
        ///     Gets the quest manager.
        /// </summary>
        /// <returns>QuestManager.</returns>
        internal QuestManager GetQuestManager() => _questManager;

        /// <summary>
        ///     Gets the group manager.
        /// </summary>
        /// <returns>GroupManager.</returns>
        internal GroupManager GetGroupManager() => _groupManager;

        /// <summary>
        ///     Gets the talent manager.
        /// </summary>
        /// <returns>TalentManager.</returns>
        internal TalentManager GetTalentManager() => _talentManager;

        /// <summary>
        ///     Gets the pinata handler.
        /// </summary>
        /// <returns>PinataHandler.</returns>
        internal PinataHandler GetPinataHandler() => _pinataHandler;

        internal RandomRewardFurniHandler GetRandomRewardFurniHandler() => _randomRewardHandler;

        /// <summary>
        ///     Gets the poll manager.
        /// </summary>
        /// <returns>PollManager.</returns>
        internal PollManager GetPollManager() => _pollManager;

        /// <summary>
        ///     Gets the clothing manager.
        /// </summary>
        /// <returns>ClothingManager.</returns>
        internal ClothingManager GetClothingManager() => _clothingManager;

        /// <summary>
        ///     Continues the loading.
        /// </summary>
        internal async Task ContinueLoading()
        {
            using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                PetRace.Init(queryReactor);
                await _catalog.Initialize(queryReactor);
                BobbaFilter.InitSwearWord();
                SoundMachineSongManager.Initialize();
                LowPriorityWorker.Init(queryReactor);
                await _roomManager.LoadCompetitionManager();
            }

            StartGameLoop();
            _pixelManager.StartTimer();
        }

        /// <summary>
        ///     Starts the game loop.
        /// </summary>
        internal void StartGameLoop()
        {
            GameLoopActiveExt = true;
            _gameLoop = new Thread(MainGameLoop);
            _gameLoop.Start();
        }

        /// <summary>
        ///     Stops the game loop.
        /// </summary>
        internal void StopGameLoop()
        {
            GameLoopActiveExt = false;
            RoomManagerCycleEnded = true;
            /* while (!RoomManagerCycleEnded || !ClientManagerCycleEnded)
                 Thread.Sleep(25);*/
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                DatabaseCleanup(queryReactor);
            GetClientManager();
            Out.WriteLineSimple("Client Manager destroyed", "Oblivion.Game", ConsoleColor.DarkYellow);
        }

        /// <summary>
        ///     Reloaditemses this instance.
        /// </summary>
        internal Task ReloadItems()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                _itemManager.LoadItems(queryReactor);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Mains the game loop.
        /// </summary>
        private async void MainGameLoop()
        {
            while (GameLoopActiveExt)
            {
                try
                {
                    await LowPriorityWorker.Process();
                    RoomManagerCycleEnded = false;
                    _roomManager.OnCycle();
                }
                catch (Exception ex)
                {
                    Logging.LogCriticalException($"Exception in Game Loop!: {ex}");
                }

                Thread.Sleep(GameLoopSleepTimeExt);
            }
        }
    }
}