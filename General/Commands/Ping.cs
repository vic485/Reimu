using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.General.Commands
{
    public class Ping : Base
    {
        [Command("ping")]
        public async Task BaseCommand()
        {
            var networkLatency =
                (await new System.Net.NetworkInformation.Ping().SendPingAsync("8.8.8.8")).RoundtripTime;
            var gatewayLatency = (Context.Client as DiscordSocketClient).Latency;

            await ReplyAsync(
                $"Pong!\nCurrent gateway latency: {gatewayLatency} ms\nCurrent internet latency: {networkLatency} ms");
        }
    }
}