using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Common.Data.Parts;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class WarnCommand : ReimuBase
    {
        [Command("warn"), RequireUserPermission(GuildPermission.KickMembers)]
        public async Task WarnUserAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("no.");
                return;
            }

            var profile = Context.GuildConfig.UserProfiles.GetProfile(user.Id);
            profile.Warnings++;
            Context.GuildConfig.UserProfiles[user.Id] = profile;

            var name = user.Nickname ?? user.Username;

            if (Context.GuildConfig.Moderation.MaxWarnings > 0 &&
                profile.Warnings >= Context.GuildConfig.Moderation.MaxWarnings)
            {
                await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                    $"**[Kicked from {Context.Guild.Name}]**\n" +
                    $"Reason: {reason ?? "No reason provided. -- Maxed out warnings."}");
                await user.KickAsync(reason);
                await ModerationHelper.LogAsync(Context, user, CaseType.Kick, reason);
                if (!DiscordHandler.BlockedContent.Any(name.Contains))
                    await ReplyAsync($"{name} maxed out warnings and was kicked.");
            }
            else
            {
                await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                    $"**[Warned in {Context.Guild.Name}]**\n" +
                    $"Reason: {reason ?? "No reason provided."}");
                await ModerationHelper.LogAsync(Context, user, CaseType.Warning, reason);

                if (!DiscordHandler.BlockedContent.Any(name.Contains))
                    await ReplyAsync($"{name} was warned.");
            }
        }

        [Command("warn"), RequireUserPermission(GuildPermission.KickMembers)]
        public async Task WarnUserAsync(ulong id, [Remainder] string reason = null)
            => await WarnUserAsync(await ModerationHelper.ResolveUser(Context.Guild, id), reason);
    }
}
