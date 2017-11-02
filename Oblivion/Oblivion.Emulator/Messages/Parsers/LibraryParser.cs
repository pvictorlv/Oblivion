using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Oblivion.Messages.Handlers;
using Oblivion.Util;

namespace Oblivion.Messages.Parsers
{
    internal static class LibraryParser
    {
        internal static Dictionary<int, StaticRequestHandler> Incoming;
        internal static Dictionary<string, string> Library;
        internal static Dictionary<string, int> Outgoing;
        internal static Dictionary<string, string> Config;

        internal static Dictionary<short, short> IncomingAir;
        internal static Dictionary<short, short> OutgoingAir;

        internal static Dictionary<int, string> OutgoingNames;

        private static List<uint> _registeredOutoings;

        internal static int CountReleases;
        internal static string ReleaseName;

        public delegate void ParamLess();

        internal delegate void StaticRequestHandler(GameClientMessageHandler handler);

        public static int OutgoingRequest(string packetName)
        {
            int packetId;

            if (Outgoing.TryGetValue(packetName, out packetId))
                return packetId;

            Writer.Writer.LogMessage("Outgoing " + packetName + " doesn't exist.");

            return -1;
        }

        public static string TryGetOutgoingName(int header)
        {
            if (OutgoingNames.TryGetValue(header, out string incomingName))
                return incomingName;

            return string.Empty;
        }

        public static void RegisterAll()
        {
            Incoming = new Dictionary<int, StaticRequestHandler>();
            Library = new Dictionary<string, string>();
            Outgoing = new Dictionary<string, int>();
            Config = new Dictionary<string, string>();

            IncomingAir = new Dictionary<short, short>();
            OutgoingAir = new Dictionary<short, short>();

            OutgoingNames = new Dictionary<int, string>();

            ReloadDictionarys();
        }

        public static void Initialize()
        {
            Out.WriteLine($"Loaded {CountReleases} Habbo Releases", "Oblivion.Packets");
            Out.WriteLine($"Loaded {Incoming.Count} Event Controllers", "Oblivion.Packets");
        }

        public static void HandlePacket(GameClientMessageHandler handler, ClientMessage message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            if (Incoming.ContainsKey(message.Id))
            {
                if (Oblivion.DebugMode)
                {
                    Console.WriteLine();
                    Console.Write("INCOMING ");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("HANDLED ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(message.Id + Environment.NewLine + message);

                    if (message.Length > 0)
                        Console.WriteLine();

                    Console.WriteLine();
                }
                
                var staticRequestHandler = Incoming[message.Id];
                staticRequestHandler(handler);
            }
            else if (Oblivion.DebugMode)
            {
                Console.WriteLine();
                Console.Write("INCOMING ");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("REFUSED ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(message.Id + Environment.NewLine + message);

                if (message.Length > 0)
                    Console.WriteLine();

                Console.WriteLine();
            }
        }

        internal static void ReloadDictionarys()
        {
            Incoming.Clear();
            Outgoing.Clear();
            Library.Clear();
            Config.Clear();
            IncomingAir.Clear();
            OutgoingAir.Clear();

            RegisterLibrary();
            RegisterConfig();
            RegisterIncoming();
            RegisterOutgoing();
        }

        internal static void RegisterIncoming()
        {
            CountReleases = 0;

            var filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\Packets", "*.incoming");

            /* TODO CHECK */
            foreach (var fileContents in filePaths.Select(currentFile => File.ReadAllLines(currentFile, Encoding.UTF8)))
            {
                CountReleases++;

                /* TODO CHECK */
                foreach (var fields in fileContents.Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("[")).Select(line => line.Replace(" ", string.Empty).Split('=')))
                {
                    var packetName = fields[0];

                    if (fields[1].Contains('/'))
                    {
                        string[] packets = fields[1].Split('/');

                        if (Int16.TryParse(packets[0], out Int16 oldHeader) && Int16.TryParse(packets[1], out Int16 newHeader))
                            IncomingAir.Add(newHeader, oldHeader);

                        fields[1] = packets[0];
                    }

                    var packetId = fields[1].ToLower().Contains('x') ? Convert.ToInt32(fields[1], 16) : Convert.ToInt32(fields[1]);
                    
                    if (!Library.ContainsKey(packetName))
                        continue;

                    var libValue = Library[packetName];

                    var del = (PacketLibrary.GetProperty)Delegate.CreateDelegate(typeof(PacketLibrary.GetProperty), typeof(PacketLibrary), libValue);

                    if (Incoming.ContainsKey(packetId))
                    {
                        if (packetId == -1)
                            continue;

                        Console.WriteLine("> A Incoming Packet with same Id was found: " + packetId);
                    }
                    else
                        Incoming.Add(packetId, new StaticRequestHandler(del));
                }
            }
        }

        internal static void RegisterConfig()
        {
            var filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\Packets", "*.inf");
            /* TODO CHECK */ foreach (var fields in filePaths.Select(File.ReadAllLines).SelectMany(fileContents => fileContents.Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("[")).Select(line => line.Split('='))))
            {
                if (fields[1].Contains('/'))
                    fields[1] = fields[1].Split('/')[0];

                Config.Add(fields[0], fields[1]);
            }
        }

        internal static void RegisterOutgoing()
        {
            _registeredOutoings = new List<uint>();

            var filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\Packets", "*.outgoing");
            /* TODO CHECK */ foreach (var fields in filePaths.Select(File.ReadAllLines).SelectMany(fileContents => fileContents.Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("[")).Select(line => line.Replace(" ", string.Empty).Split('='))))
            {
                if (fields[1].Contains('/'))
                {
                    string[] packets = fields[1].Split('/');

                    if (Int16.TryParse(packets[0], out Int16 oldHeader) && Int16.TryParse(packets[1], out Int16 newHeader))
                        OutgoingAir.Add(oldHeader, newHeader);

                    fields[1] = packets[0];
                }

                var packetName = fields[0];
                var packetId = int.Parse(fields[1]);

                if (packetId != -1)
                {
                    if (_registeredOutoings.Contains((uint)packetId))
                        Writer.Writer.LogMessage("A Outgoing Packet With Same ID Was Encountred. Packet Id: " + packetId, false);
                    else
                        _registeredOutoings.Add((uint)packetId);
                }

                Outgoing.Add(packetName, packetId);
                if (!OutgoingNames.ContainsKey(packetId))
                    OutgoingNames.Add(packetId, packetName);
            }

            _registeredOutoings.Clear();
            _registeredOutoings = null;
        }

        internal static void RegisterLibrary()
        {
            var filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\Packets", "*.library");
            /* TODO CHECK */ foreach (var fields in filePaths.Select(File.ReadAllLines).SelectMany(fileContents => fileContents.Select(line => line.Split('='))))
            {
                if (fields[1].Contains('/'))
                    fields[1] = fields[1].Split('/')[0];

                var incomingName = fields[0];
                var libraryName = fields[1];
                Library.Add(incomingName, libraryName);
            }
        }
    }
}