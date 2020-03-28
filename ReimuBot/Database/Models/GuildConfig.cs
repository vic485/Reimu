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
        
        public ulong JoinChannel { get; set; }
        
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        
        public ulong LeaveChannel { get; set; }
        
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        
        /// <summary>
        /// Guild specific profile data for users
        /// </summary>
        public Dictionary<ulong, GuildProfile> UserProfiles { get; set; } = new Dictionary<ulong, GuildProfile>();
    }
}
