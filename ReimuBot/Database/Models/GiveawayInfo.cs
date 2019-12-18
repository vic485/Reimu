using System;
using System.Collections.Generic;

namespace Reimu.Database.Models
{
    public class GiveawayInfo
    {
        public string Item { get; set; }
        public DateTime EndsAt { get; set; }
        /// <summary>
        /// Giveaway entires.
        /// Key - User Id
        /// Value - Number of entries
        /// </summary>
        public Dictionary<ulong, int> Entries { get; set; } = new Dictionary<ulong, int>();
        public List<ulong> DisabledChannels { get; set; } = new List<ulong>();
        public bool RepeatWinners { get; set; }

        public int NumWinners { get; set; } = 1;

        public int MinXp { get; set; } = 10;
        public int MaxXp { get; set; } = 20;
    }
}