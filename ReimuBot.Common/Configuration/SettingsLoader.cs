using System.IO;
using System.Text.Json;

namespace Reimu.Common.Configuration
{
    public static class SettingsLoader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true
        };

        public static LocalSettings Load()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            if (!File.Exists(path))
                File.WriteAllText(path, JsonSerializer.Serialize(new LocalSettings(), Options));

            return JsonSerializer.Deserialize<LocalSettings>(File.ReadAllText(path));
        }
    }
}
