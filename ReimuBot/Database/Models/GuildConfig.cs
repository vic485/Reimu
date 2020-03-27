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
        /// Guild specific profile data for users
        /// </summary>
        public Dictionary<ulong, GuildProfile> UserProfiles { get; set; } = new Dictionary<ulong, GuildProfile>();
    }
}
