using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Database.Models.Parts;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class WarnCommand : ReimuBase
    {
        [Command("warn"), RequireUserPermission(GuildPermission.KickMembers)]
        public async Task WarnUserAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            var profile = Context.GuildConfig.UserProfiles.GetProfile(user.Id);
            profile.Warnings++;
            Context.GuildConfig.UserProfiles[user.Id] = profile;

            if (Context.GuildConfig.Moderation.MaxWarnings > 0 &&
                profile.Warnings >= Context.GuildConfig.Moderation.MaxWarnings)
            {
                await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                    $"**[Kicked from {Context.Guild.Name}]**\n" +
                    $"Reason: {reason ?? "No reason provided."}");
                await user.KickAsync(reason);
                await ModerationHelper.LogAsync(Context, user, CaseType.Kick, reason);
            }
            else
            {
                await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                    $"**[Warned in {Context.Guild.Name}]**\n" +
                    $"Reason: {reason ?? "No reason provided."}");
                await ModerationHelper.LogAsync(Context, user, CaseType.Warning, reason);
            }
        }
    }
}
