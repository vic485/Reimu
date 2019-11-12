using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    public class Ping : ReimuBase
    {
        [Command("ping")]
        public async Task BaseCommand()
        {
            var netLatency = (await new System.Net.NetworkInformation.Ping().SendPingAsync("8.8.8.8")).RoundtripTime;
            var gateLatency = Context.Client.Latency;

            await ReplyAsync($"Pong!\nCurrent network latency: {netLatency}\nCurrent discord latency {gateLatency}");
        }
    }
}