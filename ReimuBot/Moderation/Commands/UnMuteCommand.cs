using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Database.Models.Parts;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class UnMuteCommand : ReimuBase
    {
        [Command("unmute"), RequireBotPermission(ChannelPermission.ManageRoles),
         RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task UnMuteUserAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            var role = Context.Guild.GetRole(Context.GuildConfig.Moderation.MuteRole);
            if (role == null)
            {
                await ReplyAsync("Could not find role to unmute user!");
                return;
            }

            await user.RemoveRoleAsync(role);
            Context.GuildConfig.Moderation.MutedUsers.Remove(user.Id);
            await ModerationHelper.LogAsync(Context, user, CaseType.UnMute, reason);
            await ReplyAsync($"{user.Mention} was unmuted.", updateGuild: true);
        }
    }
}
