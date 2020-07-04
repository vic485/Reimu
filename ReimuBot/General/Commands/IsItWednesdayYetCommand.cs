using System;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;
using ReimuBot.Translation;

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
                ? Translator.GetString(Context.GuildConfig.Locale, "WednesdayCommand", "wednesday")
                : Translator.GetString(Context.GuildConfig.Locale, "WednesdayCommand", today.DayOfWeek.ToString().ToLower()));
        }
    }
}
