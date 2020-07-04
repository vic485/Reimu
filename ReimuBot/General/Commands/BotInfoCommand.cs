using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Core;
using Reimu.Database.Models;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class BotInfoCommand : ReimuBase
    {
        [Command("botinfo")]
        public Task ShowBotInfo()
        {
            var botProcess = Process.GetCurrentProcess();
            var upTime = DateTime.Now - botProcess.StartTime;
            var guildNum = Context.Client.GetShardIdFor(Context.Guild) + 1;
            var embed = CreateEmbed(EmbedColor.Red)
                .WithAuthor("Reimu | 博麗 霊夢", Context.Client.CurrentUser.GetAvatarUrl(), "https://vic485.xyz/Reimu")
                .WithThumbnailUrl(
                    "https://safebooru.org//images/1609/6fee0b9ca6ea990a4de09e3f07822f8b96fab9cb.gif?1685427")
                .AddField("Version", Program.Version, true)
                .AddField("Library", $"Discord.Net\n{DiscordConfig.Version}", true)
                .AddField("GitHub", "[vic485/Reimu](https://github.com/vic485/Reimu)", true)
                .AddField("Shard #", $"{guildNum}/{Context.Client.Shards.Count}", true)
                .AddField("Uptime",
                    $"{(int) Math.Floor(upTime.TotalDays)} day {upTime.Hours} hour {upTime.Minutes} min {upTime.Seconds} sec",
                    true)
                .AddField("Guilds", Context.Client.Guilds.Count, true)
                .AddField("Active Users", $"{Context.Session.Query<UserData>().Statistics(out _).ToArray().Length}",
                    true)
                .AddField("System Info",
                    $"{RuntimeInformation.FrameworkDescription}\n{RuntimeInformation.OSDescription}", true)
                .AddField("Developer", "vic485#0001", true)
                .AddField("​", "**Links:**\n[Support](https://discord.gg/jqpcmev) | " +
                               $"[Invite](https://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=8&scope=bot)")
                .WithFooter(
                    $"Reimu is currently in experimental form. If you would like to help contribute, please visit https://github.com/vic485/Reimu")
                .Build();

            return ReplyAsync(string.Empty, embed);
        }
    }
}
