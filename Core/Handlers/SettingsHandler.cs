using System.IO;
using Newtonsoft.Json;
using Reimu.Core.JsonModels;

namespace Reimu.Core.Handlers
{
    internal static class SettingsHandler
    {
        // TODO: if we never deal with bot settings outside of startup, it may be best to just load the file each call
        private static ReimuSettings _settings;

        public static ReimuSettings Settings
        {
            get
            {
                if (_settings is null)
                    LoadSettings();

                return _settings;
            }
        }

        private static void LoadSettings()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            if (!File.Exists(configPath))
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new ReimuSettings(), Formatting.Indented));

            _settings = JsonConvert.DeserializeObject<ReimuSettings>(File.ReadAllText(configPath));
        }
    }
}