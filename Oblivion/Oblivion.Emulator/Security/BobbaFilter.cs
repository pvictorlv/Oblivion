﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            if (CheckForBannedPhrases(message) && session.GetHabbo().Rank < 4)
            {
                if (!Oblivion.MutedUsersByFilter.ContainsKey(session.GetHabbo().Id))
                    session.GetHabbo().BobbaFiltered++;

                if (session.GetHabbo().BobbaFiltered < 3)
                    session.SendNotif("Your language is inappropriate. If you do not change this , measures are being taken by the automated system of Habbo.");
                else if (session.GetHabbo().BobbaFiltered >= 3)
                {
                    if (session.GetHabbo().BobbaFiltered == 3)
                    {
                        session.GetHabbo().BobbaFiltered = 4;
                        Oblivion.MutedUsersByFilter.Add(session.GetHabbo().Id, uint.Parse((Oblivion.GetUnixTimeStamp() + (300 * 60)).ToString()));

                        return false;
                    }

                    if (session.GetHabbo().BobbaFiltered == 4)
                        session.SendNotif("Now you can not talk for 5 minutes . This is because your exhibits inappropriate language in Habbo Hotel.");
                    else if (session.GetHabbo().BobbaFiltered == 5)
                        session.SendNotif("You risk a ban if you continue to scold it . This is your last warning");
                    else if (session.GetHabbo().BobbaFiltered >= 7)
                    {
                        session.GetHabbo().BobbaFiltered = 0;

                        Oblivion.GetGame().GetBanManager().BanUser(session, "Auto-system-ban", 3600, "ban.", false, false);
                    }
                }
            }

            if (Oblivion.MutedUsersByFilter.ContainsKey(session.GetHabbo().Id))
            {
                if (Oblivion.MutedUsersByFilter[session.GetHabbo().Id] < Oblivion.GetUnixTimeStamp())
                    Oblivion.MutedUsersByFilter.Remove(session.GetHabbo().Id);
                else
                {
                    DateTime now = DateTime.Now;
                    TimeSpan timeStillBanned = now - Oblivion.UnixToDateTime(Oblivion.MutedUsersByFilter[session.GetHabbo().Id]);

                    session.SendNotif("Damn! you can't talk for " + timeStillBanned.Minutes.ToString().Replace("-", "") + " minutes and " + timeStillBanned.Seconds.ToString().Replace("-", "") + " seconds.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes the swear word.
        /// </summary>
        internal static void InitSwearWord()
        {
            Word = new List<string>();
            Word.Clear();

            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("SELECT `word` FROM wordfilter");
                var table = adapter.GetTable();

                if (table == null)
                    return;

                /* TODO CHECK */ foreach (DataRow row in table.Rows)
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
        internal static bool CheckForBannedPhrases(string message)
        {
            message = message.ToLower();

            message = message.Replace(".", "");
            message = message.Replace(" ", "");
            message = message.Replace("-", "");
            message = message.Replace(",", "");

            return Word.Any(mWord => message.Contains(mWord.ToLower()));
        }
    }
}