using System.Collections.Generic;

namespace Reimu.Database.Models.Parts
{
    public class UserJoinLeaveSettings
    {
        /// <summary>
        /// Guild channel to send messages to
        /// </summary>
        public ulong Channel { get; set; }

        /// <summary>
        /// Message sent, chosen at random
        /// </summary>
        public List<string> Messages { get; set; } = new List<string>(5);

        /// <summary>
        /// Send to user's DMs instead of to the guild channel
        /// </summary>
        public bool SendToDm { get; set; }
    }
}
