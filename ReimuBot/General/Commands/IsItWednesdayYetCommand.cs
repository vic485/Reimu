using System;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class IsItWednesdayYetCommand : ReimuBase
    {
        [Command("isitwednesdayyet"), Alias("iiwy")]
        public Task CheckForWednesday()
        {
            var today = DateTime.UtcNow;
            return ReplyAsync(today.DayOfWeek == DayOfWeek.Wednesday
                ? "Yes, it is Wednesday!"
                : $"No :( it is only {today.DayOfWeek}...");
        }
    }
}
