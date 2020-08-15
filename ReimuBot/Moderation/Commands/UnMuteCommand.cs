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
            var name = user.Nickname ?? user.Username;
            if (!DiscordHandler.BlockedContent.Any(name.Contains))
                await ReplyAsync($"{name} was unmuted.", updateGuild: true);
        }
    }
}
