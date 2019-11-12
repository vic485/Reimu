using System.IO;
using System.Text.Json;

namespace Reimu.Core.Configuration
{
    public class BotConfig
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true
        };
        
        public static SettingData Settings
        {
            get
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                if (!File.Exists(path))
                    File.WriteAllText(path, JsonSerializer.Serialize(new SettingData(), Options));

                return JsonSerializer.Deserialize<SettingData>(File.ReadAllText(path));
            }
        }
    }
}