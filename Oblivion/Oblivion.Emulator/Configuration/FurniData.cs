/*using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using Oblivion.Util;

namespace Oblivion.Configuration
{
    /// <summary>
    /// Struct FurniData
    /// </summary>
    public struct FurniData
    {
        /// <summary>
        /// The identifier
        /// </summary>
        public int Id;

        /// <summary>
        /// The name
        /// </summary>
        public string Name;

        /// <summary>
        /// The x
        /// </summary>
        public ushort X, Y;

        /// <summary>
        /// The can sit
        /// </summary>
        public bool CanSit, CanWalk;

        /// <summary>
        /// Initializes a new instance of the <see cref="FurniData" /> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="canSit">if set to <c>true</c> [can sit].</param>
        /// <param name="canWalk">if set to <c>true</c> [can walk].</param>
        public FurniData(int id, string name, ushort x = 0, ushort y = 0, bool canSit = false, bool canWalk = false)
        {
            Id = id;
            Name = name;
            X = x;
            Y = y;
            CanSit = canSit;
            CanWalk = canWalk;
        }
    }

    /// <summary>
    /// Class FurniDataParser.
    /// </summary>
    internal static class FurniDataParser
    {
        /// <summary>
        /// The floor items
        /// </summary>
        public static Dictionary<string, FurniData> FloorItems;

        /// <summary>
        /// The wall items
        /// </summary>
        public static Dictionary<string, FurniData> WallItems;

        /// <summary>
        /// Sets the cache.
        /// </summary>
        public static void SetCache()
        {
            var xmlParser = new XmlDocument();
            var wC = new WebClient();

            try
            {
                string xmlFileContent;
                string cacheDirectory = $"{Environment.CurrentDirectory}\\Cache";
                Directory.CreateDirectory(cacheDirectory);
                if (File.Exists($"{cacheDirectory}\\FurniDataCache.xml"))
                    xmlFileContent = File.ReadAllText($"{cacheDirectory}\\FurniDataCache.xml");
                else
                    File.WriteAllText($"{cacheDirectory}\\FurniDataCache.xml",
                        xmlFileContent = wC.DownloadString(ExtraSettings.FurniDataUrl));

                xmlParser.LoadXml(xmlFileContent);

                FloorItems = new Dictionary<string, FurniData>();

                XmlNodeList xmlNodeList = xmlParser.DocumentElement?.SelectNodes("/furnidata/roomitemtypes/furnitype");

                if (xmlNodeList != null)
                {
                    foreach (XmlNode node in xmlNodeList)
                    {
                        try
                        {
                            if (!FloorItems.ContainsKey(node?.Attributes?["classname"]?.Value))
                                FloorItems.Add(node?.Attributes?["classname"]?.Value,
                                    new FurniData(int.Parse(node.Attributes["id"].Value),
                                        node.SelectSingleNode("name").InnerText,
                                        ushort.Parse(node.SelectSingleNode("xdim").InnerText),
                                        ushort.Parse(node.SelectSingleNode("ydim").InnerText),
                                        node.SelectSingleNode("cansiton").InnerText == "1",
                                        node.SelectSingleNode("canstandon").InnerText == "1"));
                        }
                        catch (Exception e)
                        {
                            if (!string.IsNullOrEmpty(node?.Attributes?["classname"]?.Value))
                            {
                                Console.WriteLine("Errror parsing furnidata by {0} with exception: {1}",
                                    node.Attributes["classname"].Value, e);
                            }
                        }
                    }
                }

                WallItems = new Dictionary<string, FurniData>();

                 foreach (XmlNode node in xmlParser.DocumentElement.SelectNodes("/furnidata/wallitemtypes/furnitype"))
                    WallItems.Add(node.Attributes["classname"].Value,
                        new FurniData(int.Parse(node.Attributes["id"].Value), node.SelectSingleNode("name").InnerText));
            }
            catch (WebException e)
            {
                Out.WriteLine($"Error downloading furnidata.xml: {Environment.NewLine + e}", "Oblivion.FurniData",
                    ConsoleColor.Red);
                Out.WriteLine("Type a key to close");
                Console.ReadKey();
                Environment.Exit(e.HResult);
            }
            catch (XmlException e)
            {
                Out.WriteLine($"Error parsing furnidata.xml: {Environment.NewLine + e}", "Oblivion.FurniData",
                    ConsoleColor.Red);
                Out.WriteLine("Type a key to close");
                Console.ReadKey();
                Environment.Exit(e.HResult);
            }
            catch (NullReferenceException e)
            {
                Out.WriteLine($"Error parsing value null of furnidata.xml: {Environment.NewLine + e}",
                    "Oblivion.FurniData", ConsoleColor.Red);
                Out.WriteLine("Type a key to close");
                Console.ReadKey();
                Environment.Exit(e.HResult);
            }

            wC.Dispose();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public static void Clear()
        {
            FloorItems.Clear();
            WallItems.Clear();
            FloorItems = null;
            WallItems = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}*/