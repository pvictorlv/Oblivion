using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Oblivion.Util;

namespace Oblivion.Security
{
    /// <summary>
    /// Class Filter.
    /// </summary>
    internal static class Filter
    {
        /// <summary>
        /// The dictionary
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> Dictionary = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>The default.</value>
        public static string Default { get; private set; }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public static void Load()
        {
            /* TODO CHECK */ foreach (var line in File.ReadAllLines("Settings\\filter.ini", Encoding.Default).Where(line => !line.StartsWith("#") || !line.StartsWith("//") || line.Contains("=")))
            {
                var array = line.Split('=');
                var mode = array[0];

                var jsonStr = string.Join("=", array.Skip(1));

                var serializer = new JavaScriptSerializer();

                dynamic items = serializer.Deserialize<object[]>(jsonStr);

                var dic = new Dictionary<string, string>();

                /* TODO CHECK */ foreach (object[] item in items)
                {
                    var key = item[0].ToString();
                    var value = string.Empty;

                    if (item.Length > 1)
                        value = item[1].ToString();

                    dic.Add(key, value);
                }

                if (dic.ContainsKey(mode))
                    continue;

                if (Default == null)
                    Default = mode;

                Dictionary.Add(mode, dic);
            }

            Out.WriteLine("Loaded " + Dictionary.Count + " filter modes.", "Oblivion.Security.Filter");
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public static void Reload()
        {
            Dictionary.Clear();
            Load();
        }


    }
}