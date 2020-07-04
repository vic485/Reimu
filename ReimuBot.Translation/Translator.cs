using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ReimuBot.Translation
{
    public static class Translator
    {
        private static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "translation");
        
        /// <summary>
        /// Gets a locale specific string for the given key
        /// </summary>
        /// <param name="file">File the key is stored in</param>
        /// <param name="key">Json key</param>
        /// <returns>Base translation string</returns>
        public static string GetString(string locale, string file, string key)
        {
            try
            {
                return JToken.Parse(File.ReadAllText(Path.Combine(BasePath, locale, $"{file}.json")))[key].ToString();
            }
            catch (Exception e)
            {
                // US english is the main language of the bot, so a string should always exist for the requested file/key
                return JToken.Parse(File.ReadAllText(Path.Combine(BasePath, "en-us", $"{file}.json")))[key]?.ToString();
            }
        }
    }
}
