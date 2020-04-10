using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Database.Models.Parts;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class BanCommand : ReimuBase
    {
        [Command("ban"), RequireBotPermission(GuildPermission.BanMembers),
         RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            if (!(Context.User as SocketGuildUser).IsUserHigherThan(user))
            {
                await ReplyAsync("Cannot perform this action on a user that is the same or ranked higher than you.");
                return;
            }

            // TODO: Is there a way to check if the bot is higher than the other user?

            await user.BanAsync(1, reason);
            await ModerationHelper.LogAsync(Context, user, CaseType.Ban, reason);
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                $"**[Banned from {Context.Guild.Name}]**\n" +
                $"Reason: {reason ?? "No reason provided."}");
        }
    }
}
