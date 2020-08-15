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
    public class MuteCommand : ReimuBase
    {
        [Command("mute"), RequireUserPermission(GuildPermission.ManageRoles),
         RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteUserAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("no.");
                return;
            }

            var role = Context.Guild.GetRole(Context.GuildConfig.Moderation.MuteRole);
            if (role == null)
            {
                await ReplyAsync("Could not find role to mute user!");
                return;
            }

            await user.AddRoleAsync(role);
            Context.GuildConfig.Moderation.MutedUsers.Add(user.Id);
            await ModerationHelper.LogAsync(Context, user, CaseType.Mute, reason);
            var name = user.Nickname ?? user.Username;
            if (!DiscordHandler.BlockedContent.Any(name.Contains))
                await ReplyAsync($"{name} was muted.", updateGuild: true);
        }
    }
}
