using System.Collections.Generic;
using Reimu.Database.Models.Parts;

namespace Reimu.Database.Models
{
    /// <summary>
    /// Guild specific configuration
    /// </summary>
    public class GuildConfig : DatabaseItem
    {
        public string Prefix { get; set; }
        public GuildModeration Moderation { get; set; } = new GuildModeration();
    }
}
