using System.Threading.Tasks;
using Discord.WebSocket;

namespace Reimu.Core.Interaction
{
    public class InteractiveChannel : IInteractive<SocketMessage>
    {
        public Task<bool> JudgeAsync(BotContext context, SocketMessage message)
            => Task.FromResult(context.Channel.Id == message.Channel.Id);
    }
}
