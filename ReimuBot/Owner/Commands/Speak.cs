using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Owner.Commands
{
    public class Speak : ReimuBase
    {
        [Command("speak"), RequireOwner]
        public async Task SpeakAsync(ulong channelId, [Remainder] string message)
        {
            var channel = Context.Client.GetChannel(channelId) as SocketTextChannel;
            await channel.SendMessageAsync(message);
        }
    }
}