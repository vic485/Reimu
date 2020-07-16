using System;

namespace Reimu.Common.Data
{
    /// <summary>
    /// Globally accessed user information
    /// </summary>
    public class UserData : DatabaseItem
    {
        /// <summary>
        /// Global xp
        /// </summary>
        public int Xp { get; set; }

        /// <summary>
        /// Last message the user sent that was counted for xp
        /// </summary>
        public DateTime LastMessage { get; set; } = DateTime.UtcNow - TimeSpan.FromMinutes(2);
    }
}
