using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Core;
using Reimu.Preconditions;

namespace Reimu.General.Commands
{
    public class Ping : ReimuBase
    {
        [Command("ping")]
        public async Task BaseCommand()
        {
            var netLatency = (await new System.Net.NetworkInformation.Ping().SendPingAsync("8.8.8.8")).RoundtripTime;
            var gateLatency = Context.Client.Latency;

            EmbedColor color;
            var latency = netLatency + gateLatency / 2f;
            if (latency < 100)
                color = EmbedColor.Green;
            else if (latency < 250)
                color = EmbedColor.Yellow;
            else
                color = EmbedColor.Red;

            await ReplyAsync(string.Empty,
                CreateEmbed(color).WithAuthor("Connection Information").AddField("Discord Latency", $"{gateLatency} ms", true)
                    .AddField("Internet Latency", $"{netLatency} ms", true).Build());
        }
    }
}