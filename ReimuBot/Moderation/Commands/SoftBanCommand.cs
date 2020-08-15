using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Common.Data.Parts;
using Reimu.Core;

namespace Reimu.Moderation.Commands
{
    // This is technically an advanced kick, as we only use the ban to remove the user's messages, and unban immediately following
    [Name("Moderation"), RequireUserPermission(GuildPermission.KickMembers),
     RequireBotPermission(GuildPermission.BanMembers)]
    public class SoftBanCommand : ReimuBase
    {
        [Command("softban"), Alias("hardkick")]
        public async Task SoftBanAsync(SocketGuildUser user, [Remainder] string reason)
        {
            if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("no.");
                return;
            }

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

            var banReason = reason?.Length > 512 ? reason.Substring(0, 512) : reason;
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                $"**[Banned from {Context.Guild.Name}]**\n" +
                $"Reason: {reason ?? "No reason provided."}");
            await user.BanAsync(1, banReason);
            await ModerationHelper.LogAsync(Context, user, CaseType.SoftBan, reason);
            var name = user.Nickname ?? user.Username;
            if (!DiscordHandler.BlockedContent.Any(name.Contains))
                await ReplyAsync($"{user.Nickname ?? user.Username} was soft-banned.");
            await Context.Guild.RemoveBanAsync(user);
        }

        // TODO: This is most likely only going to be run on an active user, meaning Reimu should always be able to
        // resolve them. On the off chance that this is not the case, we would need id soft banning here as well.
    }
}
