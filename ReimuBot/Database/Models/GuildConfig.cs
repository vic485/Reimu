using System.Collections.Generic;

namespace Reimu.Database.Models
{
    /// <summary>
    /// Guild specific configuration
    /// </summary>
    public class GuildConfig : DatabaseItem
    {
        public string Prefix { get; set; }
        public GiveawayInfo Giveaway { get; set; } = null;
        public Dictionary<ulong, GuildUser> Profiles { get; set; } = new Dictionary<ulong, GuildUser>();
    }
}