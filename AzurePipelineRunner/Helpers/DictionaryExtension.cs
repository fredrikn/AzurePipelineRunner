using System;
using System.Collections.Generic;

namespace AzurePipelineRunner.Helpers
{
    /// <summary>
    /// A Extension to help handling Keys with the correct format for Task.
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// Check if a Key alrady exists. This method will make sure the Key is correct formated for Task.
        /// </summary>
        public static bool KeyExists(this Dictionary<string, object> dictionary, string key)
        {
            var newKey = FormatKey(key);
            return dictionary.ContainsKey(newKey);
        }

        /// <summary>
        /// Update a key's value if exists or add a new item.
        /// </summary>
        public static void SetKey(this Dictionary<string, object> dictionary, string key, object value)
        {
            var newKey = FormatKey(key);
            if (dictionary.ContainsKey(newKey))
            {
                dictionary[newKey] = value;
            }
            else
            {
                dictionary.Add(newKey, value);
            }
        }

        /// <summary>
        /// Adds a new item and will make sure the Key is correct formated for Task.
        /// </summary>
        public static void AddKey(this Dictionary<string, object> dictionary, string key, object value)
        {
            var newKey = FormatKey(key);
            dictionary.Add(newKey, value);
        }

        /// <summary>
        /// Gets a value based on a key. This method will make sure the Key is correct formated for Task.
        /// </summary>
        public static object GetValue(
            this Dictionary<string, object> dictionary,
            string key,
            object defaultValue = null,
            bool required = false)
        {
            var newKey = FormatKey(key);

            if (dictionary.ContainsKey(newKey))
            {
                return dictionary[newKey];
            }
            else if (required)
            {
                throw new ArgumentException($"The key '{key}' is required.");
            }
            else if (defaultValue != null)
            {
                return defaultValue;
            }

            throw new ArgumentException($"The key '{key}' has no value.");
        }

        /// <summary>
        /// Gets a value based on a key. This method will make sure the Key is correct formated for Task.
        /// </summary>
        public static string GetValueAsString(
          this Dictionary<string, object> dictionary,
          string key,
          string defaultValue = null,
          bool required = false)
        {
            var value = dictionary.GetValue(key, defaultValue, required);
            return value.ToString();
        }

        /// <summary>
        /// Gets a value as boolean based on a key. This method will make sure the Key is correct formated for Task.
        /// </summary>
        public static bool GetValueAsBool(
          this Dictionary<string, object> dictionary,
          string key,
          bool defaultValue,
          bool required = false)
        {
            var value = dictionary.GetValue(key, defaultValue, required);

            bool newValue;

            if (bool.TryParse(value.ToString(), out newValue))
            {
                return newValue;
            }

            throw new ArgumentException($"The key '{key}' has no value or is not a boolean.");
        }

        private static string FormatKey(string key)
        {
            return key.ToUpperInvariant();
        }
    }
}
