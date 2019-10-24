using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationSectionExtensions
    {
        /// <summary>
        ///     Converts configuration section to Dictionary
        ///     Based on https://stackoverflow.com/questions/50007146/bind-netcore-iconfigurationsection-to-a-dynamic-object
        /// </summary>
        /// <param name="configurationSection">ConfigurationSection to convert</param>
        /// <returns>Null or Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this IConfigurationSection configurationSection)
        {
            var result = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            try
            {
                // retrieve all keys from your settings
                var configs = configurationSection.AsEnumerable().Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x =>
                    new KeyValuePair<string, string>(GetPropertyNameFromKey(configurationSection, x), x.Value)).ToList();

                if (configs?.Any() != true)
                {
                    return null;
                }

                foreach (var kvp in configs)
                {
                    var parent = result;
                    var path = kvp.Key.Split(':');

                    // create or retrieve the hierarchy (keep last path item for later)
                    var i = 0;

                    for (i = 0; i < path.Length - 1; i++)
                    {
                        if (!parent.ContainsKey(path[i]))
                        {
                            parent.Add(path[i], new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase));
                        }

                        parent = (Dictionary<string, object>) parent[path[i]];
                    }

                    if (kvp.Value == null)
                    {
                        continue;
                    }

                    // add the value to the parent
                    // note: in case of an array, key will be an integer and will be dealt with later
                    var key = path[i];
                    parent.Add(key, kvp.Value);
                }

                // at this stage, all arrays are seen as dictionaries with integer keys
                ReplaceWithArray(null, null, result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return null;
            }

            return result;
        }

        private static string GetPropertyNameFromKey(IConfigurationSection configurationSection, KeyValuePair<string, string> x)
        {
            var result = x.Key.Replace(configurationSection.Path, "").TrimStart(':').Trim();

            return result;
        }

        private static void ReplaceWithArray(Dictionary<string, object> parent, string key, Dictionary<string, object> input)
        {
            if (input == null)
            {
                return;
            }

            var dict = input;
            var keys = dict.Keys.ToArray();

            // it's an array if all keys are integers
            if (keys.All(k => int.TryParse(k, out var dummy)))
            {
                var array = new object[keys.Length];

                foreach (var kvp in dict)
                {
                    array[int.Parse(kvp.Key)] = kvp.Value;
                }

                var parentDict = parent;
                parentDict.Remove(key);
                parentDict.Add(key, array);
            }
            else
            {
                foreach (var childKey in dict.Keys.ToList())
                {
                    ReplaceWithArray(input, childKey, dict[childKey] as Dictionary<string, object>);
                }
            }
        }
    }
}
