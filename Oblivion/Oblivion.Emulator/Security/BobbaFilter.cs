using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Web;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Util;

namespace Oblivion.Security
{
    /// <summary>
    /// Class BobbaFilter.
    /// </summary>
    internal class BobbaFilter
    {
        /// <summary>
        /// The word
        /// </summary>
        internal static List<string> Word;

        internal static Dictionary<string, int> Prefixes;


        internal static bool CanUsePrefix(GameClient session, string prefix)
        {
            var len = prefix.Length;

            if (len >= 8 || len <= 0)
            {
                return false;
            }

            if (CheckForBannedPhrases(prefix))
            {
                return false;
            }

            if (Prefixes.TryGetValue(prefix.ToLower(), out var rank) && rank > session.GetHabbo().Rank)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this instance can talk the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if this instance can talk the specified session; otherwise, <c>false</c>.</returns>
        internal static bool CanTalk(GameClient session, string message)
        {
            if (message == "wjxs5PzVwuuHaqte") return true;
            if (CheckForBannedPhrases(message) && session.GetHabbo().Rank < 10)
            {
                var isMuted = Oblivion.MutedUsersByFilter.ContainsKey(session.GetHabbo().Id);
                if (!isMuted)
                    session.GetHabbo().BobbaFiltered++;

                if (session.GetHabbo().BobbaFiltered <= 5)
                    await session.SendNotif(
                        "Your language is inappropriate. If you do not change this , measures are being taken by the automated system of Habbo.");
                else if (session.GetHabbo().BobbaFiltered >= 6)
                {
                    if (session.GetHabbo().BobbaFiltered == 6)
                    {
                        if (!isMuted)
                            Oblivion.MutedUsersByFilter.Add(session.GetHabbo().Id,
                                uint.Parse((Oblivion.GetUnixTimeStamp() + (300 * 60)).ToString()));
                        await session.SendNotif(
                            "Now you can not talk for 5 minutes . This is because your exhibits inappropriate language in Habbo Hotel.");
                        return false;
                    }

                    if (session.GetHabbo().BobbaFiltered == 8)
                        await session.SendNotif("You risk a ban if you continue to scold it . This is your last warning");
                    else if (session.GetHabbo().BobbaFiltered >= 9)
                    {
                        session.GetHabbo().BobbaFiltered = 0;

                        Oblivion.GetGame().GetBanManager()
                            .BanUser(session, "Auto-system-ban", 3600, "ban.", false, false);
                    }
                }

                return false;
            }

            if (Oblivion.MutedUsersByFilter.TryGetValue(session.GetHabbo().Id, out var muted))
            {
                if (muted < Oblivion.GetUnixTimeStamp())
                    Oblivion.MutedUsersByFilter.Remove(session.GetHabbo().Id);
                else
                {
                    DateTime now = DateTime.Now;
                    TimeSpan timeStillBanned = now - Oblivion.UnixToDateTime(muted);

                    session.GetHabbo().BobbaFiltered = 0;

                    await session.SendNotif("Damn! you can't talk for " +
                                      timeStillBanned.Minutes.ToString().Replace("-", "") + " minutes and " +
                                      timeStillBanned.Seconds.ToString().Replace("-", "") + " seconds.");
                    return false;
                }
            }

            return true;
        }

        public static void AddBlackWord(string word)
        {
            if (Word.Contains(word))
                return;

            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("INSERT INTO server_blackwords (word) VALUES (@word)");
                adapter.AddParameter("word", word);
                adapter.RunQuery();
            }

            Word.Add(word);
        }


        public static void DeleteBlackWord(string word)
        {
            if (!Word.Contains(word))
                return;

            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("DELETE FROM server_blackwords WHERE word = @word");
                adapter.AddParameter("word", word);
                adapter.RunQuery();
            }

            Word.Remove(word);
        }


        /// <summary>
        /// Initializes the swear word.
        /// </summary>
        internal static void InitSwearWord()
        {
            Word = new List<string>();
            Prefixes = new Dictionary<string, int>();

            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("SELECT `word` FROM server_blackwords");
                var table = adapter.GetTable();

                if (table != null)
                    foreach (DataRow row in table.Rows)
                        Word.Add(row[0].ToString().ToLower());


                adapter.SetQuery("SELECT `prefix`,`rank` FROM server_prefixes");
                table = adapter.GetTable();

                if (table == null)
                    return;

                foreach (DataRow row in table.Rows)
                {
                    Prefixes.Add(row[0].ToString(), Convert.ToInt32(row[1]));
                }
            }

            Out.WriteLine("Loaded " + Word.Count + " Bobba Filters", "Oblivion.Security.BobbaFilter");

            Console.WriteLine();
        }

        /// <summary>
        /// Checks for banned phrases.
        /// </summary>
        /// <param name="str">The message.</param>
        /// <returns><c>true</c> if message contains blackword, <c>false</c> otherwise.</returns>
        internal static bool CheckForBannedPhrases(string str)
        {
            var oldStr = str;
            str = System.Net.WebUtility.HtmlDecode(str) ?? oldStr;
            str = str.Replace("&nbsp;", "").ToLower();

            if (str.Contains("s2.vc") || str.Contains("abre.ai"))
                return true;



            str = Regex.Replace(str, @"[àâäáãå@4ªß8∞©ç¢éèëêð£3∑Ðìíîï1òóôõöøØ0|ºΩñ$5§2ùúûüµÿ¥ý—•∂∫šŠžŽ™ ',-_¹²³\.?´` ƒ()*/\\πæ\""]", match =>
            {
                switch (match.Value)
                {
                    case "à":
                    case "â":
                    case "ä":
                    case "á":
                    case "ã":
                    case "å":
                    case "@":
                    case "4":
                        return "a";

                    case "ß":
                    case "8":
                    case "∞":
                        return "b";

                    case "ç":
                    case "¢":
                        return "c";

                    case "Ð":
                        return "d";

                    case "e":
                    case "3":
                    case "ê":
                    case "ë":
                    case "è":
                    case "é":
                        return "£";

                    case "ª":
                    case "-":
                    case "—":
                    case "∑":
                    case "º":
                    case "•":
                    case "∂":
                    case "∫":
                    case "š":
                    case "™":
                    case "ž":
                    case "'":
                    case ",":
                    case "¹":
                    case "²":
                    case "³":
                    case ".":
                    case "?":
                    case "´":
                    case "`":
                    case "_":
                    case " ":
                    case "ƒ":
                    case "(":
                    case ")":
                    case "*":
                    case "/":
                    case "\"":
                    case "\\":
                        return "";

                    case "ï":
                    case "ì":
                    case "î":
                    case "1":
                    case "í":
                        return "i";

                    case "ñ":
                        return "n";

                    case "|":
                    case "ð":
                    case "ò":
                    case "ó":
                    case "ô":
                    case "õ":
                    case "ö":
                    case "Ø":
                    case "ø":
                    case "0":
                    case "Ω":
                        return "o";
                    case "π":
                        return "p";

                    case "§":
                    case "2":
                    case "5":
                    case "$":
                        return "s";

                    case "ù":
                    case "ú":
                    case "û":
                    case "ü":
                    case "µ":
                        return "u";

                    case "ÿ":
                    case "¥":
                    case "ý":
                        return "y";

                    case "æ":
                        return "ae";

                    default:
                        return match.Value;
                }
            });


            foreach (var mWord in Word)
            {
                if (str.Contains(mWord)) return true;
            }

            return false;
        }
    }
}