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
        internal static Dictionary<ushort, string> OutgoingAirNames;

        private static List<uint> _registeredOutoings;

        internal static int CountReleases;

        internal delegate void StaticRequestHandler(GameClientMessageHandler handler);

        public static int OutgoingRequest(string packetName)
        {
            if (Outgoing.TryGetValue(packetName, out var packetId))
                return packetId;

            Writer.Writer.LogMessage("Outgoing " + packetName + " doesn't exist.");

            return -1;
        }

        public static string TryGetOutgoingName(int header) => OutgoingNames.TryGetValue(header, out var incomingName) ? incomingName : string.Empty;

        public static string TryGetOutgoingAirName(ushort header) => OutgoingAirNames.TryGetValue(header, out var incomingName) ? incomingName : string.Empty;

        public static void RegisterAll()
        {
            Incoming = new Dictionary<int, StaticRequestHandler>();
            Library = new Dictionary<string, string>();
            Outgoing = new Dictionary<string, int>();
            Config = new Dictionary<string, string>();

            IncomingAir = new Dictionary<short, short>();
            OutgoingAir = new Dictionary<short, short>();
            OutgoingAirNames = new Dictionary<ushort, string>();

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

            if (Incoming.TryGetValue(message.Id, out var staticRequestHandler))
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

                if (handler.GetResponse() == null) return;

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
            OutgoingAirNames.Clear();

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
                    
                    if (!Library.TryGetValue(packetName, out var libValue))
                        continue;

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
            if (File.Exists("outgoing_air.ini"))
            {
                foreach (string line in File.ReadAllLines("outgoing_air.ini"))
                {
                    if (string.IsNullOrWhiteSpace(line) && !line.StartsWith("#") && line.Contains("="))
                    {
                        string[] @params = line.Split('=');
                        string id = @params[0];
                        string @namespace = @params[1];

                        if (ushort.TryParse(id, out ushort idUShort))
                            OutgoingAirNames.Add(idUShort, @namespace);
                    }
                }
            }

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

                if (Outgoing.ContainsKey(packetName))
                {
                    Out.WriteLine($"{packetName} is already registered!");
                    continue;
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