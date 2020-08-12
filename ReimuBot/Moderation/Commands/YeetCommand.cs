using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Common.Data.Parts;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class YeetCommand : ReimuBase
    {
        [Command("yeet"), RequireBotPermission(GuildPermission.BanMembers),
         RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(SocketGuildUser user, [Remainder] string reason = null)
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
            await ModerationHelper.LogAsync(Context, user, CaseType.Ban, reason);
            await ReplyAsync($"{user.Nickname ?? user.Username} was yeeted.");
        }

        [Command("yeet"), RequireBotPermission(GuildPermission.BanMembers),
         RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(ulong id, [Remainder] string reason = null)
        {
            var user = await ModerationHelper.ResolveUser(Context.Guild, id);

            if (user != null)
            {
                await BanUserAsync(await ModerationHelper.ResolveUser(Context.Guild, id), reason);
                return;
            }
            
            var banReason = reason?.Length > 512 ? reason.Substring(0, 512) : reason;
            await Context.Guild.AddBanAsync(id, 1, banReason);
            // TODO: We should still log this to the mod channel (and cases?)
            await ReplyAsync(
                $"The user with id: {id} was not found on the server, but I have added a ban anyways. This is currently not logged by me.");
        }
    }
}
