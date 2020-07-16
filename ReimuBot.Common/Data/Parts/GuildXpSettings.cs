using System.Collections.Generic;

namespace Reimu.Common.Data.Parts
{
    public class GuildXpSettings
    {
        /// <summary>
        /// Whether guild xp gain is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Min amount of xp to be given per message
        /// </summary>
        public int Min { get; set; } = 10;

        /// <summary>
        /// Max amount of xp to be given per message
        /// </summary>
        public int Max { get; set; } = 20;

        /// <summary>
        /// Channels where xp gain is disabled
        /// </summary>
        public List<ulong> BlockedChannels { get; set; } = new List<ulong>();

        /// <summary>
        /// Roles blocked from gaining xp
        /// </summary>
        public List<ulong> BlockedRoles { get; set; } = new List<ulong>();
    }
}
