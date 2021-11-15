using System;
using System.Collections.Generic;

namespace Primes.Common.Files.Settings
{
    /// <summary>
    /// Class that represents a settings file and contains useful methods for interaction with such files.
    /// </summary>
    public class SettingsDocument
    {
        private readonly Dictionary<string, string> settings;



        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public SettingsDocument()
        {
            settings = new Dictionary<string, string>();
        }
        /// <summary>
        /// Constructor that parses a string source.
        /// </summary>
        /// <param name="source"></param>
        public SettingsDocument(string source)
        {
            settings = new Dictionary<string, string>();

            string[] lines = source.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;

                int[] indexes = lines[i].GetIndexesOf("\"");

                if (indexes.Length != 4)
                    throw new FormatException();

                string key, value;

                key = lines[i].Substring(indexes[0] + 1, indexes[1] - indexes[0] - 1);
                value = lines[i].Substring(indexes[2] + 1, indexes[3] - indexes[2] - 1);

                settings.Add(key, value);
            }
        }



        /// <summary>
        /// Attempts to get a setting from the document.
        /// </summary>
        /// <param name="key">The name of the setting.</param>
        /// <param name="value">The value, if it exists.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out string value)
        {
            return settings.TryGetValue(key, out value);
        }
        /// <summary>
        /// Gets the setting from the document, throwing an exception if the setting is not present.
        /// </summary>
        /// <param name="key">The name of the setting.</param>
        /// <returns></returns>
        public string GetValue(string key) => settings[key];
        /// <summary>
        /// Checks if the document contains a given setting.
        /// </summary>
        /// <param name="key">The name of the setting.</param>
        /// <returns></returns>
        public bool ContainsEntry(string key)
        {
            return settings.ContainsKey(key);
        }
        /// <summary>
        /// Checks if the document contains all of the given settings.
        /// </summary>
        /// <param name="keys">Array with the setting names.</param>
        /// <returns></returns>
        public bool ContainsEntries(string[] keys)
        {
            foreach (string key in keys)
                if (!ContainsEntry(key))
                    return false;

            return true;
        }
        /// <summary>
        /// Applies the current document to a given scheme, adding missing settings and optionally resetting all to default values.
        /// </summary>
        /// <param name="scheme">The scheme to apply.</param>
        /// <param name="resetAllToDefaults">Wether or not to reset to default values.</param>
        public void ApplySettingsScheme(Dictionary<string, string> scheme, bool resetAllToDefaults)
        {
            foreach (KeyValuePair<string, string> pair in scheme)
            {
                if (settings.ContainsKey(pair.Key))
                {
                    if (resetAllToDefaults)
                        settings[pair.Key] = pair.Value;
                }
                else
                    settings.Add(pair);
            }
        }
        /// <summary>
        /// Adds a setting to the document.
        /// </summary>
        /// <param name="key">The number of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public void AddEntry(string key, string value)
        {
            settings.Add(key, value);
        }



        /// <summary>
        /// Converts the settings document to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string outp = string.Empty;

            foreach (KeyValuePair<string, string> pair in settings)
                outp += $"\"{pair.Key}\"=\"{pair.Value}\"\n";

            return outp.TrimEnd('\n');
        }
    }
}
