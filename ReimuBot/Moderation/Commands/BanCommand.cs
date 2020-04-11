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

            if (!Context.Guild.CurrentUser.IsUserHigherThan(user))
            {
                await ReplyAsync("Cannot perform this action on a user that is the same or ranked higher than me.");
                return;
            }

            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                $"**[Banned from {Context.Guild.Name}]**\n" +
                $"Reason: {reason ?? "No reason provided."}");
            await user.BanAsync(1, reason);
            await ModerationHelper.LogAsync(Context, user, CaseType.Ban, reason);
        }
    }
}
