﻿using System;
using System.Collections.Generic;
using System.Data;

namespace Oblivion.Configuration
{
    /// <summary>
    /// Class Languages.
    /// </summary>
    internal class Languages
    {
        /// <summary>
        /// The texts
        /// </summary>
        internal Dictionary<string,string> Texts;

        /// <summary>
        /// Initializes a new instance of the <see cref="Languages" /> class.
        /// </summary>
        /// <param name="language">The language.</param>
        internal Languages(string language)
        {
            Texts = new Dictionary<string, string>();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT * FROM server_langs WHERE lang = '{language}' ORDER BY id DESC");

                var table = queryReactor.GetTable();

                if (table == null)
                    return;

                /* TODO CHECK */ foreach (DataRow dataRow in table.Rows)
                {
                    var name = dataRow["name"].ToString();
                    var text = dataRow["text"].ToString();
                    Texts.Add(name, text);
                }
            }
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <param name="var">The variable.</param>
        /// <returns>System.String.</returns>
        internal string GetVar(string var)
        {
            if (Texts.TryGetValue(var, out string value))
                return value;

            Console.WriteLine("[Language] Not Found: " + var);

            return "Language var not Found: " + var;
        }

        /// <summary>
        /// Counts this instance.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int Count() => Texts.Count;
    }
}