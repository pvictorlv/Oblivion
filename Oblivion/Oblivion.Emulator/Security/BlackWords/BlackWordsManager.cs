using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Oblivion.Security.BlackWords.Enums;
using Oblivion.Security.BlackWords.Structs;
using Oblivion.Util;

namespace Oblivion.Security.BlackWords
{
    /// <summary>
    /// Class BlackWordsManager.
    /// </summary>
    internal static class BlackWordsManager
    {
        /// <summary>
        /// The words
        /// </summary>
        private static readonly List<BlackWord> Words = new List<BlackWord>();

        /// <summary>
        /// The replaces
        /// </summary>
        private static readonly Dictionary<BlackWordType, BlackWordTypeSettings> Replaces = new Dictionary<BlackWordType, BlackWordTypeSettings>();

        private static readonly BlackWord Empty = new BlackWord(string.Empty, BlackWordType.All);

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public static void Load()
        {
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("SELECT * FROM server_blackwords");
                var table = adapter.GetTable();

                if (table == null)
                    return;

                /* TODO CHECK */ foreach (DataRow row in table.Rows)
                {
                    var word = row["word"].ToString();
                    var typeStr = row["type"].ToString();

                    AddPrivateBlackWord(typeStr, word);
                }
            }

            Out.WriteLine("Loaded " + Words.Count + " BlackWords", "Oblivion.Security.BlackWords");
            Console.WriteLine();
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public static void Reload()
        {
            Words.Clear();
            Replaces.Clear();

            Load();
        }

        public static void AddBlackWord(string typeStr, string word)
        {
            if (!Enum.TryParse(typeStr, true, out BlackWordType type))
                return;

            if (Words.Any(wordStruct => wordStruct.Type == type && word.Contains(wordStruct.Word)))
                return;

            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("INSERT INTO server_blackwords VALUES (null, @word, @type)");
                adapter.AddParameter("word", word);
                adapter.AddParameter("type", typeStr);
                adapter.RunQuery();
            }

            AddPrivateBlackWord(typeStr, word);
        }

        public static void DeleteBlackWord(string typeStr, string word)
        {
            if (!Enum.TryParse(typeStr, true, out BlackWordType type))
                return;

            var wordStruct = Words.FirstOrDefault(wordS => wordS.Type == type && word.Contains(wordS.Word));

            if (string.IsNullOrEmpty(wordStruct.Word))
                return;

            Words.Remove(wordStruct);

            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("DELETE FROM server_blackwords WHERE word = @word AND type = @type");
                adapter.AddParameter("word", word);
                adapter.AddParameter("type", typeStr);
                adapter.RunQuery();
            }
        }

        private static void AddPrivateBlackWord(string typeStr, string word)
        {
            BlackWordType type;

            switch (typeStr)
            {
                case "hotel":
                    type = BlackWordType.Hotel;
                    break;

                case "insult":
                    type = BlackWordType.Insult;
                    break;

                case "all":
                    Out.WriteLine("Word type [all] it's reserved for system. Word: " + word, "Oblivion.Security.BlackWords", ConsoleColor.DarkRed);
                    return;

                default:
                    Out.WriteLine("Undefined type [" + typeStr + "] of word: " + word, "Oblivion.Security.BlackWords", ConsoleColor.DarkRed);
                    return;
            }

            Words.Add(new BlackWord(word, type));

            if (Replaces.ContainsKey(type))
                return;

            string filter = Filter.Default, alert = "User [{0}] with Id: {1} has said a blackword. Word: {2}. Type: {3}. Message: {4}", imageAlert = "bobba";

            var maxAdvices = 7u;
            bool autoBan = true, showMessage = true;

            if (File.Exists("Settings\\BlackWords\\" + typeStr + ".ini"))
            {
                /* TODO CHECK */ foreach (var array in File.ReadAllLines("Settings\\BlackWords\\" + typeStr + ".ini").Where(line => !line.StartsWith("#") || !line.StartsWith("//") || line.Contains("=")).Select(line => line.Split('=')))
                {
                    if (array[0] == "filterType") filter = array[1];
                    if (array[0] == "maxAdvices") maxAdvices = uint.Parse(array[1]);
                    if (array[0] == "alertImage") imageAlert = array[1];
                    if (array[0] == "autoBan") autoBan = array[1] == "true";
                    if (array[0] == "showMessage") showMessage = array[1] == "true";
                }
            }

            if (File.Exists("Settings\\BlackWords\\" + typeStr + ".alert.txt"))
                alert = File.ReadAllText("Settings\\BlackWords\\" + typeStr + ".alert.txt");

            Replaces.Add(type, new BlackWordTypeSettings(filter, alert, maxAdvices, imageAlert, autoBan, showMessage));
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>BlackWordTypeSettings.</returns>
        public static BlackWordTypeSettings? GetSettings(BlackWordType type)
        {
            if (Replaces.TryGetValue(type, out var replace))
                return replace;
            return null;
        }

        /// <summary>
        /// Checks the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="type">The type.</param>
        /// <param name="word">The word.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Check(string str, BlackWordType type, out BlackWord word)
        {
            word = Empty;
            

            var oldStr = str;
            str = HttpUtility.HtmlDecode(str) ?? oldStr;
            str = str.Replace("&nbsp;", "").ToLower();

            if (str.Contains("s2.vc") || str.Contains("abre.ai"))
                return true;

            str = Regex.Replace(str, "[àâäàáâãäåÀÁÂÃÄÅ@4ª]", "a");
            str = Regex.Replace(str, "[ß8∞]", "b");
            str = Regex.Replace(str, "[©çÇ¢]", "c");
            str = Regex.Replace(str, "[éèëêðÉÈËÊ£3∑]", "e");
            str = Regex.Replace(str, "[ìíîïÌÍÎÏ1]", "i");
            str = Regex.Replace(str, "[ñÑ]", "n");
            str = Regex.Replace(str, "[òóôõöøÒÓÔÕÖØ0|ºΩ]", "o");
            str = Regex.Replace(str, "[$5§2]", "s");
            str = Regex.Replace(str, "[ùúûüµÙÚÛÜ]", "u");
            str = Regex.Replace(str, "[ÿ¥]", "y");
            str = Regex.Replace(str, @"[—•∂∫šŠŸžŽ™ ',-_¹²³.?´` ƒ()*/\\]", "");
            str = str.Replace("æ", "ae");
            str = str.Replace("π", "p");
            str = str.Replace("Ð", "d");
            str = str.Replace("\"", "");

//            str = Filter.Replace(data.Filter, str);

            var wordFirst = new BlackWord();
            foreach (var wordStruct in Words)
            {
                if (wordStruct.Type == type && str.Contains(wordStruct.Word))
                {
                    wordFirst = wordStruct;
                    break;
                }
            }
            
            word = wordFirst;

            return !string.IsNullOrEmpty(wordFirst.Word);
        }
    }
}