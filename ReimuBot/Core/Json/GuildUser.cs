using System;

namespace Reimu.Core.Json
{
    public class GuildUser
    {
        public int Points { get; set; }
        public int GiveAwayPoints { get; set; }
        public DateTime LastMessage { get; set; } = DateTime.MinValue;
    }
}