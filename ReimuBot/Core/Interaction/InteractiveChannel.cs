using System.Threading.Tasks;
using Discord.WebSocket;

namespace Reimu.Core.Interaction
{
    /// <summary>
    /// For checking that the message is from the same channel as the command
    /// </summary>
    public class InteractiveChannel : IInteractive<SocketMessage>
    {
        public Task<bool> JudgeAsync(BotContext context, SocketMessage message)
            => Task.FromResult(context.Channel.Id == message.Channel.Id);
    }
}