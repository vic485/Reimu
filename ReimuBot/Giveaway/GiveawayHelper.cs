using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Core.Json;

namespace Reimu.Giveaway
{
    public class GiveawayHelper
    {
        public static Task OnMessage(SocketUser user, GuildConfig config)
        {
            if (config.Giveaway == null)
                return Task.CompletedTask;

            var xp = Rand.Range(config.Giveaway.MinXp, config.Giveaway.MaxXp);
            config.Profiles[user.Id].GiveAwayPoints += xp;
            return Task.CompletedTask;
        }

        public static List<ulong> GetWinners(GiveawayInfo giveaway)
        {
            var totalEntries = giveaway.Entries.Sum(entrant => entrant.Value);
            var winners = new List<ulong>();

            for (var i = 0; i < giveaway.NumWinners; i++)
            {
                var randNum = Rand.Range(0, totalEntries);
                ulong keyCheck = 0;
                foreach (var (key, value) in giveaway.Entries)
                {
                    if (randNum < value)
                    {
                        winners.Add(key);
                        keyCheck = key;
                        break;
                    }

                    randNum -= value;
                }

                if (giveaway.RepeatWinners)
                    giveaway.Entries[keyCheck]--;
                else
                    giveaway.Entries.Remove(keyCheck);
            }

            return winners;
        }
    }
}