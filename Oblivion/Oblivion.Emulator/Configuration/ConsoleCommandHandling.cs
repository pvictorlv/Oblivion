using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.Connection.Connection;
using Oblivion.Encryption.Encryption;
using Oblivion.HabboHotel;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Security;
using Oblivion.Util;

namespace Oblivion.Configuration
{
    /// <summary>
    ///     Class ConsoleCommandHandling.
    /// </summary>
    internal class ConsoleCommandHandling
    {
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket) ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint);

                // Signal that the connection has been made.
            }
            catch (Exception e)
            {
                Out.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     Gets the game.
        /// </summary>
        /// <returns>Game.</returns>
        internal static Game GetGame()
        {
            return Oblivion.GetGame();
        }

        /// <summary>
        ///     Invokes the command.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        internal static async Task InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData) && Logging.DisabledState)
                return;

            Console.WriteLine();

            try
            {
                if (inputData == null)
                    return;

                var strArray = inputData.Split(' ');

                switch (strArray[0])
                {
                    case "shutdown":
                    case "close":
                        Logging.DisablePrimaryWriting(true);
                        Out.WriteLineSimple("Shutdown Initalized", "Oblivion.Life", ConsoleColor.DarkYellow);
                        await Oblivion.PerformShutDown(false);
                        Console.WriteLine();
                        break;

                    case "restart":
                        Logging.LogMessage($"Server Restarting at {DateTime.Now}");
                        Logging.DisablePrimaryWriting(true);
                        Out.WriteLineSimple("Restart Initialized", "Oblivion.Life", ConsoleColor.DarkYellow);
                        await Oblivion.PerformShutDown(true);
                        Console.WriteLine();
                        break;

                    case "flush":
                    case "reload":
                        if (strArray.Length >= 2) break;
                        Console.WriteLine("Please specify parameter. Type 'help' to know more about Console Commands");
                        Console.WriteLine();
                        break;

                    case "debug":
                        Oblivion.DebugMode = !Oblivion.DebugMode;
                        break;

                    case "alert":
                    {
                        var str = inputData.Substring(6);
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                        await message.AppendStringAsync(str);
                        await message.AppendStringAsync(string.Empty);
                        await GetGame().GetClientManager().SendMessageAsync(message);
                        Console.WriteLine("[{0}] was sent!", str);
                        return;
                    }
                    case "clear":
                        Console.Clear();
                        break;

                    case "status":
                        var uptime = DateTime.Now - Oblivion.ServerStarted;

                        Console.WriteLine("Server status:");
                        Console.WriteLine();
                        Console.WriteLine("Uptime:");
                        Console.WriteLine("\tDays:    {0}", uptime.Days);
                        Console.WriteLine("\tHours:   {0}", uptime.Hours);
                        Console.WriteLine("\tMinutes: {0}", uptime.Minutes);
                        Console.WriteLine();
                        Console.WriteLine("Stats:");
                        Console.WriteLine("\tAccepted Connections: {0}",
                            Oblivion.GetConnectionManager().Manager.AcceptedClients);
                        Console.WriteLine("\tActive Threads: {0}", Process.GetCurrentProcess().Threads.Count);
                        Console.WriteLine();
                        Console.WriteLine();
                        break;

                    case "gcinfo":
                    {
                        Console.WriteLine("Mode: " + GCSettings.LatencyMode);
                        Console.WriteLine("Is server GC: " + GCSettings.IsServerGC);

                        break;
                    }

                    case "memstat":
                    {
                        Console.WriteLine("GC status:");
                        Console.WriteLine("\tGeneration supported: " + GC.MaxGeneration);
                        Console.WriteLine("\tLatency mode: " + GCSettings.LatencyMode);
                        Console.WriteLine("\tIs server GC: " + GCSettings.IsServerGC);
                        Console.WriteLine();
                        break;
                    }

                    case "dlog":
                    {
                        Logging.DisabledState = !Logging.DisabledState;
                        break;
                    }
                    case "crypto":
                        {
                            Handler.Initialize(CryptoKeys.N, CryptoKeys.D, CryptoKeys.E);
                            Out.WriteLine("Crypto keys has been reseted.", "Oblivion.Crypto");
                            break;
                        }
                    case "lag":
                        if (Oblivion.DebugMode)
                        {
                            new Task(async () =>
                            {
                                for (uint i = 0; i < 5000; i++)
                                    try
                                    {
                                        await Oblivion.GetGame().GetRoomManager().LoadRoom(i);
                                    }
                                    catch (Exception exception)
                                    {
                                        Out.WriteLine(exception.Message);
                                    }
                            }).Start();
                            var countable = 0;
                            for (uint i = 0; i < 10000; i++)
                            {
                                countable++;
                                new Task(() =>
                                {
                                    var ipAddress = IPAddress.Parse("127.0.0.1");
                                    var remoteEP = new IPEndPoint(ipAddress, Convert.ToInt32(ConfigurationData.Data["game.tcp.port"]));
                                    var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                        ProtocolType.Tcp);

                                    // Connect to the remote endpoint.
                                    client.BeginConnect(remoteEP, ConnectCallback, client);
                                }).Start();
                                if (countable != 150) continue;
                                Thread.Sleep(5000);
                                countable = 0;
                            }
                            Out.WriteLine("Lag Test started");
                        }
                        break;
                    case "memory":
                    {
                        GC.Collect();
                        Console.WriteLine("Memory flushed");

                        break;
                    }

                    case "help":
                        Console.WriteLine("shutdown/close - for safe shutting down OblivionEmulator");
                        Console.WriteLine("clear - Clear all text");
                        Console.WriteLine("memory - Call gargabe collector");
                        Console.WriteLine("memstat - Show memstats");
                        Console.WriteLine("alert (msg) - send alert to Every1!");
                        Console.WriteLine("flush/reload");
                        Console.WriteLine("   - catalog");
                        Console.WriteLine("   - modeldata");
                        Console.WriteLine("   - bans");
                        Console.WriteLine("   - packets (reload packets ids)");
                        Console.WriteLine("   - filter");
                        Console.WriteLine();
                        break;

                    default:
                        UnknownCommand(inputData);
                        break;
                }

                switch (strArray[1])
                {
                    case "database":
                        Oblivion.GetDatabaseManager().Destroy();
                        Console.WriteLine("Database destroyed");
                        Console.WriteLine();
                        break;

                    case "packets":
                        LibraryParser.ReloadDictionarys();
                        Console.WriteLine("> Packets Reloaded Suceffuly...");
                        Console.WriteLine();
                        break;

                    case "catalog":
                    case "shop":
                    case "catalogus":
                        using (var adapter = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            GetGame().GetItemManager().LoadItems(adapter);
                            await GetGame().GetCatalog().Initialize(adapter);
                            await GetGame().ReloadItems();
//                            GetGame().GetCrackableEggHandler().Initialize(adapter);
                        }
                        var msg = new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer"));
                        msg.AppendBool(false);
                        await GetGame()
                            .GetClientManager()
                            .SendMessageAsync(msg);

                        Console.WriteLine("Catalogue was re-loaded.");
                        Console.WriteLine();
                        break;

                    case "modeldata":
//                        using (var adapter2 = Oblivion.GetDatabaseManager().GetQueryReactor()) GetGame().GetRoomManager().LoadModels(adapter2);
                        Console.WriteLine("Room models were re-loaded.");
                        Console.WriteLine();
                        break;

                    case "bans":
                        using (var adapter3 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            await GetGame().GetBanManager().LoadBans(adapter3);
                        }
                        Console.WriteLine("Bans were re-loaded");
                        Console.WriteLine();
                        break;

                    case "filter":
                        BobbaFilter.InitSwearWord();
                        break;

                    default:
                        UnknownCommand(inputData);
                        Console.WriteLine();
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Unknowns the command.
        /// </summary>
        /// <param name="command">The command.</param>
        private static void UnknownCommand(string command)
        {
            Out.WriteLine("Manager", "Undefined Command: " + command);
        }
    }
}