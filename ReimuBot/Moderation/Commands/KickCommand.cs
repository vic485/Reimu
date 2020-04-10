using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Database.Models.Parts;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class KickCommand : ReimuBase
    {
        [Command("kick"), RequireBotPermission(GuildPermission.KickMembers),
         RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUserAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            if (!(Context.User as SocketGuildUser).IsUserHigherThan(user))
            {
                await ReplyAsync("Cannot perform this action on a user that is the same or ranked higher than you.");
                return;
            }

            // TODO: Is there a way to check if the bot is higher than the other user?

            await user.KickAsync(reason);
            await ModerationHelper.LogAsync(Context, user, CaseType.Kick, reason);
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                $"**[Kicked from {Context.Guild.Name}]**\n" +
                $"Reason: {reason ?? "No reason provided."}");
        }
    }
}
