using System.Collections.Generic;
using Reimu.Database.Models.Parts;

namespace Reimu
{
    public static class Extensions
    {
        // Needed because TryGetValue() returns null instead of a new profile
        public static GuildProfile GetProfile(this Dictionary<ulong, GuildProfile> profiles, ulong id)
            => profiles.ContainsKey(id) ? profiles[id] : new GuildProfile();
    }
}
