using System;
using System.Collections.Generic;

namespace Primes.Common.Files.Settings
{
    public class SettingsDocument
    {
        private readonly Dictionary<string, string> settings;



        public SettingsDocument()
        {
            settings = new Dictionary<string, string>();
        }
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



        public bool TryGetValue(string key, out string value)
        {
            return settings.TryGetValue(key, out value);
        }
        public string GetValue(string key) => settings[key];
        public bool ContainsEntry(string key)
        {
            return settings.ContainsKey(key);
        }
        public bool ContainsEntries(string[] keys)
        {
            foreach (string key in keys)
                if (!ContainsEntry(key))
                    return false;

            return true;
        }
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
        public void AddEntry(string key, string value)
        {
            settings.Add(key, value);
        }



        public override string ToString()
        {
            string outp = string.Empty;

            foreach (KeyValuePair<string, string> pair in settings)
                outp += $"\"{pair.Key}\"=\"{pair.Value}\"\n";

            return outp.TrimEnd('\n');
        }
    }
}

/*public bool GetByte(string key, out byte value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!byte.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetSByte(string key, out sbyte value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!sbyte.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetShort(string key, out short value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!short.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetUShort(string key, out ushort value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!ushort.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetInt(string key, out int value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!int.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetUInt(string key, out uint value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!uint.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetLong(string key, out long value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!long.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetULong(string key, out ulong value)
        {
            value = 0;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!ulong.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetBool(string key, out bool value)
        {
            value = false;

            if (!settings.ContainsKey(key))
                return false;

            string val = settings[key];

            if (!bool.TryParse(val, out value))
                return false;

            return true;
        }
        public bool GetString(string key, out string value)
        {
            value = string.Empty;

            if (!settings.ContainsKey(key))
                return false;

            value = settings[key];

            return true;
        }*/
