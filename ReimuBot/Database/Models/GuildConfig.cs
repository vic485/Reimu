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
    }
}
