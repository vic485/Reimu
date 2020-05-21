using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class PurgeCommand : ReimuBase
    {
        [Command("purge"), RequireUserPermission(GuildPermission.ManageMessages),
         RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(int amount = 10)
        {
            var messages = (await Context.Channel.GetMessagesAsync(amount).FlattenAsync()).ToList();

            if (amount <= 100)
            {
                await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            }
            else
            {
                for (var i = 0; i < messages.Count; i += 100)
                    await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages.Skip(i).Take(100));
            }

            await ReplyDeleteAsync($"Purged **{amount}** messages.");
        }
    }
}
