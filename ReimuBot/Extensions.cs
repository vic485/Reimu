using System.Collections.Generic;
using Reimu.Database.Models.Parts;

namespace Reimu
{
    public static class Extensions
    {
        // Needed because TryGetValue() returns null instead of a new profile
        public static GuildProfile GetProfile(this Dictionary<ulong, GuildProfile> profiles, ulong id)
            => profiles.ContainsKey(id) ? profiles[id] : new GuildProfile();
        
        // Keep us from going over list capacities.
        // Only use when the capacity was set on initialization!
        public static bool TryAdd<T>(this List<T> list, T item)
        {
            if (list.Count >= list.Capacity)
                return false;
            
            list.Add(item);
            return true;
        }
    }
}
