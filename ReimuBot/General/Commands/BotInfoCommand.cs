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
            var embed = CreateEmbed(EmbedColor.Red)
                .WithAuthor("Reimu Information")
                .WithThumbnailUrl(
                    "https://safebooru.org//images/1609/6fee0b9ca6ea990a4de09e3f07822f8b96fab9cb.gif?1685427")
                .AddField("Bot",
                    "```ini\n" +
                    "[Developer]\n" +
                    "vic485#0001\n" +
                    "[Runtime]\n" +
                    $"{RuntimeInformation.FrameworkDescription}\n" +
                    "[Framework]\n" +
                    $"{DiscordConfig.Version}\n" +
                    "[Servers]\n" +
                    $"{Context.Client.Guilds.Count}\n" +
                    "[Users]\n" +
                    $"{Context.Session.Query<UserData>().Statistics(out _).ToArray().Length}" +
                    "```")
                .AddField("System",
                    "```ini\n" +
                    "[Operating System]\n" +
                    $"{RuntimeInformation.OSDescription}\n" +
                    "[Uptime]\n" +
                    $"{botProcess.TotalProcessorTime.ToString()}\n" +
                    "[Memory Usage]\n" +
                    $"{botProcess.WorkingSet64 / (1024 * 1024)} MB\n" +
                    "```")
                .Build();

            return ReplyAsync(string.Empty, embed);
        }
    }
}
