using System;
using System.Collections.Generic;

namespace Reimu.Common.Data.Parts
{
    public class GuildProfile
    {
        /// <summary>
        /// User's points specific to the guild
        /// </summary>
        public int Xp { get; set; }
        
        public int Warnings { get; set; }

        /// <summary>
        /// Last message the user sent that was counted for xp
        /// </summary>
        public DateTime LastMessage { get; set; } = DateTime.UtcNow - TimeSpan.FromMinutes(2);

        /// <summary>
        /// The last time a user ran a specific command (for non global cool-downs)
        /// </summary>
        public Dictionary<string, DateTime> CommandTimes = new Dictionary<string, DateTime>();
    }
}
