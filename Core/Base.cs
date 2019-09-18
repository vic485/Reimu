using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Reimu.Core
{
    public class Base : ModuleBase<IContext>
    {
        // TODO: Embeds and database docs
        public async Task<IUserMessage> ReplyAsync(string message)
        {
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            return await base.ReplyAsync(message, false, null, null);
        }
    }
}