using System;
using System.Threading.Tasks;
using Reimu.Core;

namespace Reimu.Economy.Commands
{
    public class Daily : ReimuBase
    {
        public Task CollectCredits()
        {
            var elapsed = DateTime.UtcNow - Context.UserData.CreditsCollected;

            if (elapsed.Days < 1)
            {
                return ReplyAsync("");
            }

            var points = Rand.Range(30, 60);
            Context.UserData.Credits += points;
            Context.UserData.CreditsCollected += TimeSpan.FromDays(elapsed.Days);

            return ReplyAsync($"Collected {points} points", updateUser: true);
        }
    }
}