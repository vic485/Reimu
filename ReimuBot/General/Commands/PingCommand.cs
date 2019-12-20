using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Core;
using Reimu.Translation;
using Reimu.Preconditions;

namespace Reimu.General.Commands
{
    public class PingCommand : ReimuBase
    {
        [Command("ping")]
        public async Task BaseCommand()
        {
            var netLatency = (await new Ping().SendPingAsync("8.8.8.8")).RoundtripTime;
            var gateLatency = Context.Client.Latency;

            EmbedColor color;
            //var latency = netLatency + gateLatency / 2f;
            var latency = Math.Max(netLatency, gateLatency);
            if (latency < 100)
                color = EmbedColor.Green;
            else if (latency < 250)
                color = EmbedColor.Yellow;
            else
                color = EmbedColor.Red;

            var text = Translator.Get("general", "ping", Context.GuildConfig.Locale);
            await ReplyAsync(string.Empty,
                CreateEmbed(color).WithAuthor(text[0]).AddField(text[1], $"{gateLatency} ms", true)
                    .AddField(text[3], $"{netLatency} ms", true).Build());
        }
    }
}