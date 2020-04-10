using System.Collections.Generic;
using Discord.WebSocket;
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

        /// <summary>
        /// Check if a user is a higher rank than another
        /// </summary>
        /// <param name="user1">The user we are checking</param>
        /// <param name="user2">User to check agains</param>
        /// <returns>true if user1 is higher than user2, false otherwise</returns>
        public static bool IsUserHigherThan(this SocketGuildUser user1, SocketGuildUser user2)
            => user1.Hierarchy > user2.Hierarchy;
    }
}
