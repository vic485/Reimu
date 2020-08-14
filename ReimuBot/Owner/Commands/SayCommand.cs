using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Owner.Commands
{
    [Name("Owner"), RequireOwner]
    public class SayCommand : ReimuBase
    {
        [Command("say")]
        public Task Say(ulong channelId, [Remainder] string message)
        {
            if (!(Context.Client.GetChannel(channelId) is SocketTextChannel channel))
                return ReplyAsync("Channel not found.");

            return channel.SendMessageAsync(message);
        }
    }
}
