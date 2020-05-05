using System.Collections.Generic;

namespace Reimu.Database.Models.Parts
{
    public class GuildModeration
    {
        public ulong LogChannel { get; set; }
        public ulong MuteRole { get; set; }
        public int MaxWarnings { get; set; }
        public bool InviteBlock { get; set; }
        public List<ulong> MutedUsers { get; set; } = new List<ulong>();
        public List<ModCase> Cases { get; set; } = new List<ModCase>();
    }
}
