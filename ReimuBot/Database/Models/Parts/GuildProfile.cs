using System;
using System.Collections.Generic;

namespace Reimu.Database.Models.Parts
{
    public class GuildProfile
    {
        /// <summary>
        /// The last time a user ran a specific command (for non global cool-downs)
        /// </summary>
        public Dictionary<string, DateTime> CommandTimes = new Dictionary<string, DateTime>();
    }
}
