using System.Collections.Generic;

namespace Reimu.Database.Models
{
    /// <summary>
    /// Guild specific configuration
    /// </summary>
    public class GuildConfig : DatabaseItem
    {
        public string Prefix { get; set; }
        public string Locale { get; set; } = "en-us";
        public ulong JoinChannel { get; set; }
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public ulong LeaveChannel { get; set; }
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public GuildModeration Moderation { get; set; } = new GuildModeration();
        public GiveawayInfo Giveaway { get; set; } = null;
        public Dictionary<ulong, GuildUser> Profiles { get; set; } = new Dictionary<ulong, GuildUser>();
    }
}