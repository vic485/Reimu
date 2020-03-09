using System.Collections.Generic;

namespace Reimu.Database.Models.Parts
{
    public class GuildModeration
    {
        public ulong JoinRole { get; set; }
        public ulong MuteRole { get; set; }

        /// <summary>
        /// Maximum number of warnings per user. User will be autokicked when they reach this number.
        /// Disabled if set to less than 1
        /// </summary>
        public int MaxWarnings { get; set; }

        /// <summary>
        /// Id of channel to log moderation events to
        /// </summary>
        public ulong Channel { get; set; }

        public List<ulong> MutedUsers { get; set; } = new List<ulong>();
        public List<ModerationCase> Cases = new List<ModerationCase>();
    }
}
