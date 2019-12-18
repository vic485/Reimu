using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Reimu.Common.Logging;

namespace Reimu.Translation
{
    public class Translator
    {
        public static string[] Get(string module, string key, string locale)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "translations", locale.ToLower(), $"{module}.json");
            string[] result;
            try
            {
                Logger.LogVerbose($"Attempting to load {key} from {module} in {locale}.");
                result = JToken.Parse(File.ReadAllText(filePath))[key].ToObject<string[]>();
            }
            catch (Exception e)
            {
                Logger.LogVerbose($"Failed to load (missing key or module), falling back to en-us.");
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "translations", "en-us", $"{module}.json");
                result = JToken.Parse(File.ReadAllText(filePath))[key].ToObject<string[]>();
            }
            return result;
        }
    }
}