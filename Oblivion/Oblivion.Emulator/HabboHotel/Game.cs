﻿using System;
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
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.SoundMachine;
using Oblivion.HabboHotel.Support;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.Helpers;
using Oblivion.Manager;
using Oblivion.Messages.Enums;
using Oblivion.Security;
using Oblivion.Security.BlackWords;
using Oblivion.Util;

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
        private readonly AchievementManager _achievementManager;

        /// <summary>
        ///     The _ban manager
        /// </summary>
        private readonly ModerationBanManager _banManager;

        /// <summary>
        ///     The _bot manager
        /// </summary>
        private readonly BotManager _botManager;

        /// <summary>
        ///     The _catalog
        /// </summary>
        private readonly CatalogManager _catalog;

        /// <summary>
        ///     The _client manager
        /// </summary>
        private readonly GameClientManager _clientManager;

        /// <summary>
        ///     The _clothing manager
        /// </summary>
        private readonly ClothingManager _clothingManager;

        /// <summary>
        ///     The _clothing manager
        /// </summary>
        private readonly CrackableEggHandler _crackableEggHandler;

        /// <summary>
        ///     The _events
        /// </summary>
        private readonly RoomEvents _events;

        /// <summary>
        ///     The _group manager
        /// </summary>
        private readonly GroupManager _groupManager;

        /// <summary>
        ///     The _guide manager
        /// </summary>
        private readonly GuideManager _guideManager;

        private readonly HallOfFame _hallOfFame;

        /// <summary>
        ///     The _hotel view
        /// </summary>
        private readonly HotelView _hotelView;

        /// <summary>
        ///     The _item manager
        /// </summary>
        private readonly ItemManager _itemManager;

        /// <summary>
        ///  The camera Manager
        /// </summary>
        private readonly CameraPhotoManager _cameraManager;

        /// <summary>
        ///     The _moderation tool
        /// </summary>
        private readonly ModerationTool _moderationTool;

        /// <summary>
        ///     The _navigatorManager
        /// </summary>
        private readonly NavigatorManager _navigatorManager;

        /// <summary>
        ///     The _pinata handler
        /// </summary>
        private readonly PinataHandler _pinataHandler;

        /// <summary>
        ///     The _pixel manager
        /// </summary>
        private readonly CoinsManager _pixelManager;

        /// <summary>
        ///     The _poll manager
        /// </summary>
        private readonly PollManager _pollManager;

        /// <summary>
        ///     The _quest manager
        /// </summary>
        private readonly QuestManager _questManager;

        /// <summary>
        ///     The _role manager
        /// </summary>
        private readonly RoleManager _roleManager;

        /// <summary>
        ///     The _room manager
        /// </summary>
        private readonly RoomManager _roomManager;

        /// <summary>
        ///     The _talent manager
        /// </summary>
        private readonly TalentManager _talentManager;

        private readonly TargetedOfferManager _targetedOfferManager;

        /// <summary>
        ///     The _game loop
        /// </summary>
        private Thread _gameLoop;

        /// <summary>
        ///     The client manager cycle ended
        /// </summary>
        internal bool ClientManagerCycleEnded, RoomManagerCycleEnded;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Game" /> class.
        /// </summary>
        internal Game()
        {
            Console.WriteLine();
            Out.WriteLine(@"Starting up Oblivion Emulator for " + Environment.MachineName + "...", @"Oblivion.Boot");
            Console.WriteLine();

            _clientManager = new GameClientManager();
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                AbstractBar bar = new AnimatedBar();
                const int wait = 15, end = 5;

                uint itemsLoaded;
                uint navigatorLoaded;
                uint achievementLoaded;
                uint pollLoaded;

                Progress(bar, wait, end, "Cleaning dirty in database...");
                DatabaseCleanup(queryReactor);

                Progress(bar, wait, end, "Loading Bans...");
                _banManager = new ModerationBanManager();
                _banManager.LoadBans(queryReactor);

                Progress(bar, wait, end, "Loading Roles...");
                _roleManager = new RoleManager();
                _roleManager.LoadRights(queryReactor);

                Progress(bar, wait, end, "Loading Items...");
                _itemManager = new ItemManager();
                _itemManager.LoadItems(queryReactor, out itemsLoaded);

                _cameraManager = new CameraPhotoManager();
                _cameraManager.Init(_itemManager);

                Progress(bar, wait, end, "Loading Catalog...");
                _catalog = new CatalogManager();

                Progress(bar, wait, end, "Loading Targeted Offers...");
                _targetedOfferManager = new TargetedOfferManager();

                Progress(bar, wait, end, "Loading Clothing...");
                _clothingManager = new ClothingManager();
                _clothingManager.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Rooms...");
                _roomManager = new RoomManager();

                Progress(bar, wait, end, "Loading NavigatorManager...");
                _navigatorManager = new NavigatorManager();
                _navigatorManager.Initialize(queryReactor, out navigatorLoaded);

                Progress(bar, wait, end, "Loading Groups...");
                _groupManager = new GroupManager();
                _groupManager.InitGroups();

                Progress(bar, wait, end, "Loading PixelManager...");
                _pixelManager = new CoinsManager();

                Progress(bar, wait, end, "Loading HotelView...");
                _hotelView = new HotelView();

                Progress(bar, wait, end, "Loading Hall Of Fame...");
                _hallOfFame = new HallOfFame();

                Progress(bar, wait, end, "Loading ModerationTool...");
                _moderationTool = new ModerationTool();
                _moderationTool.LoadMessagePresets(queryReactor);
                _moderationTool.LoadPendingTickets(queryReactor);

                Progress(bar, wait, end, "Loading Bots...");
                _botManager = new BotManager();

                Progress(bar, wait, end, "Loading Quests...");
                _questManager = new QuestManager();
                _questManager.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Events...");
                _events = new RoomEvents();

                Progress(bar, wait, end, "Loading Talents...");
                _talentManager = new TalentManager();
                _talentManager.Initialize(queryReactor);

                //this.SnowStormManager = new SnowStormManager();

                Progress(bar, wait, end, "Loading Pinata...");
                _pinataHandler = new PinataHandler();
                _pinataHandler.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Crackable Eggs...");
                _crackableEggHandler = new CrackableEggHandler();
                _crackableEggHandler.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Polls...");
                _pollManager = new PollManager();
                _pollManager.Init(queryReactor, out pollLoaded);

                Progress(bar, wait, end, "Loading Achievements...");
                _achievementManager = new AchievementManager(queryReactor, out achievementLoaded);

                Progress(bar, wait, end, "Loading StaticMessages ...");
                StaticMessagesManager.Load();

                Progress(bar, wait, end, "Loading Guides ...");
                _guideManager = new GuideManager();

                Progress(bar, wait, end, "Loading and Registering Commands...");
                CommandsManager.Register();

                Cache.StartProcess();

                //Progress(bar, wait, end, "Loading AntiMutant...");
                //this.AntiMutant = new AntiMutant();

                Console.Write("\r".PadLeft(Console.WindowWidth - Console.CursorLeft - 1));
            }
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
        internal int GameLoopSleepTimeExt => 25;

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
            dbClient.RunFastQuery("UPDATE rooms_data SET users_now = 0 WHERE users_now <> 0");
            dbClient.RunFastQuery(
                "UPDATE `server_status` SET status = '1', users_online = '0', rooms_loaded = '0', server_ver = 'Oblivion Emulator', stamp = '" +
                Oblivion.GetUnixTimeStamp() + "' LIMIT 1;");
        }

        /*internal AntiMutant GetAntiMutant()
        {
        //return this.AntiMutant;
        }*/

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
        ///     Gets the pixel manager.
        /// </summary>
        /// <returns>CoinsManager.</returns>
        internal CoinsManager GetPixelManager() => _pixelManager;

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
        ///     Gets the bot manager.
        /// </summary>
        /// <returns>BotManager.</returns>
        internal BotManager GetBotManager() => _botManager;

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

        internal CrackableEggHandler GetCrackableEggHandler() => _crackableEggHandler;

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
        internal void ContinueLoading()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                uint catalogPageLoaded;
                PetRace.Init(queryReactor);
                _catalog.Initialize(queryReactor, out catalogPageLoaded);
                Filter.Load();
                BobbaFilter.InitSwearWord();
                BlackWordsManager.Load();
                SoundMachineSongManager.Initialize();
                LowPriorityWorker.Init(queryReactor);
                _roomManager.InitVotedRooms(queryReactor);
                _roomManager.LoadCompetitionManager();
            }
            StartGameLoop();
            _pixelManager.StartTimer();
        }

        /// <summary>
        ///     Starts the game loop.
        /// </summary>
        internal void StartGameLoop()
        {
            const int stackSize = 1024 * 1024 * 256;

            GameLoopActiveExt = true;
            _gameLoop = new Thread(MainGameLoop, stackSize);
            _gameLoop.Start();
        }

        /// <summary>
        ///     Stops the game loop.
        /// </summary>
        internal void StopGameLoop()
        {
            GameLoopActiveExt = false;
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
            Out.WriteLine("Client Manager destroyed", "Oblivion.Game", ConsoleColor.DarkYellow);
        }

        /// <summary>
        ///     Reloaditemses this instance.
        /// </summary>
        internal void ReloadItems()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                _itemManager.LoadItems(queryReactor);
            }
        }

        /// <summary>
        ///     Mains the game loop.
        /// </summary>
        private void MainGameLoop()
        {
            while (GameLoopActiveExt)
            {
                LowPriorityWorker.Process();
                try
                {
                    RoomManagerCycleEnded = false;
                    ClientManagerCycleEnded = false;
                    _roomManager.OnCycle();
                    _clientManager.OnCycle();
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