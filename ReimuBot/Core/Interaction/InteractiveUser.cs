using System.Threading.Tasks;
using Discord.WebSocket;

namespace Reimu.Core.Interaction
{
    public class InteractiveUser : IInteractive<SocketMessage>
    {
        public Task<bool> JudgeAsync(BotContext context, SocketMessage message)
            => Task.FromResult(context.User.Id == message.Author.Id);
    }
}