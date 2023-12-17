using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Oblivion.Configuration
{
    /// <summary>
    /// Class ConfigurationData.
    /// </summary>
    internal static class ConfigurationData
    {
        /// <summary>
        /// A dictionary that holds the configuration data as key-value pairs.
        /// </summary>
        internal static readonly Dictionary<string, string> Data = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationData"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="mayNotExist">if set to <c>true</c> [may not exist].</param>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        internal static void Load(string filePath, bool mayNotExist = false)
        {
            if (!File.Exists(filePath) && !mayNotExist)
            {
                throw new ArgumentException($"Configuration file not found in '{filePath}'.");
            }

            try
            {
                using var streamReader = new StreamReader(filePath ?? throw new ArgumentNullException(nameof(filePath)));
                while (streamReader.ReadLine() is { } text)
                {
                    if (string.IsNullOrWhiteSpace(text) || text.StartsWith("#"))
                        continue;

                    var num = text.IndexOf('=');

                    if (num == -1)
                        continue;

                    var key = text[..num];
                    var value = text[(num + 1)..];

                    Data[key] = value;
                }
                
                foreach (DictionaryEntry envVar in Environment.GetEnvironmentVariables())
                {
                    if (envVar.Key is not string key || envVar.Value is not string value)
                        continue;

                    var newKey = key.ToLower().Replace("_", ".");
                    Data[newKey] = value;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Could not process configuration file: {ex.Message}", ex);
            }
        }
    }
}
