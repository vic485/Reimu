using System.Collections.Generic;
using Reimu.Database.Models.Parts;

namespace Reimu.Database.Models
{
    /// <summary>
    /// Guild specific configuration
    /// </summary>
    public class GuildConfig : DatabaseItem
    {
        /// <summary>
        /// Command prefix for the guild
        /// </summary>
        public string Prefix { get; set; }
        
        /// <summary>
        /// Id of channel to send user join messages to
        /// </summary>
        public ulong JoinChannel { get; set; }
        
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        
        /// <summary>
        /// Id of channel to send user leave messages to
        /// </summary>
        public ulong LeaveChannel { get; set; }
        
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        
        /// <summary>
        /// Moderation settings and data
        /// </summary>
        public GuildModeration Moderation { get; set; } = new GuildModeration();
        
        /// <summary>
        /// Guild specific profile data for users
        /// </summary>
        public Dictionary<ulong, GuildProfile> UserProfiles { get; set; } = new Dictionary<ulong, GuildProfile>();
    }
}
