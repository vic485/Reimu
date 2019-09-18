using System.Collections.Generic;

namespace Reimu.Core.JsonModels
{
    public class ConfigModel : DatabaseModel
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public List<ulong> UserBlacklist { get; set; } = new List<ulong>();
        public List<ulong> GuildBlacklist { get; set; } = new List<ulong>();
    }
}