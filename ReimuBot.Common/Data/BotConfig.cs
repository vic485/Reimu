using System.Collections.Generic;

namespace Reimu.Common.Data
{
    /// <summary>
    /// Shared bot configuration settings
    /// </summary>
    public class BotConfig : DatabaseItem
    {
        /// <summary>
        /// Token to connect to discord
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Default prefix for commands
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// List of guilds prohibited from using bot functions
        /// </summary>
        public List<ulong> GuildBlacklist { get; set; } = new List<ulong>();
        
        /// <summary>
        /// List of users prohibited from using bot functions
        /// </summary>
        public List<ulong> UserBlacklist { get; set; } = new List<ulong>();
    }
}
