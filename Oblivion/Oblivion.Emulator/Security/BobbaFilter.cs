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

        /// <summary>
        /// Determines whether this instance can talk the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if this instance can talk the specified session; otherwise, <c>false</c>.</returns>
        internal static bool CanTalk(GameClient session, string message)
        {
            if (CheckForBannedPhrases(message) && session.GetHabbo().Rank < 10)
            {
                var isMuted = Oblivion.MutedUsersByFilter.ContainsKey(session.GetHabbo().Id);
                if (!isMuted)
                    session.GetHabbo().BobbaFiltered++;

                if (session.GetHabbo().BobbaFiltered <= 5)
                    session.SendNotif(
                        "Your language is inappropriate. If you do not change this , measures are being taken by the automated system of Habbo.");
                else if (session.GetHabbo().BobbaFiltered >= 6)
                {
                    if (session.GetHabbo().BobbaFiltered == 6)
                    {
                        if (!isMuted)
                            Oblivion.MutedUsersByFilter.Add(session.GetHabbo().Id,
                                uint.Parse((Oblivion.GetUnixTimeStamp() + (300 * 60)).ToString()));
                        session.SendNotif(
                            "Now you can not talk for 5 minutes . This is because your exhibits inappropriate language in Habbo Hotel.");
                        return false;
                    }

                    if (session.GetHabbo().BobbaFiltered == 8)
                        session.SendNotif("You risk a ban if you continue to scold it . This is your last warning");
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

                    session.SendNotif("Damn! you can't talk for " +
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
                adapter.SetQuery("INSERT INTO server_blackwords VALUES ( @word)");
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

            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("SELECT `word` FROM server_blackwords");
                var table = adapter.GetTable();

                if (table == null)
                    return;

                foreach (DataRow row in table.Rows)
                    Word.Add(row[0].ToString().ToLower());
            }

            Out.WriteLine("Loaded " + Word.Count + " Bobba Filters", "Oblivion.Security.BobbaFilter");

            Console.WriteLine();
        }

        /// <summary>
        /// Checks for banned phrases.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool CheckForBannedPhrases(string str)
        {
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

            foreach (var mWord in Word)
            {
                if (str.Contains(mWord)) return true;
            }

            return false;
        }
    }
}