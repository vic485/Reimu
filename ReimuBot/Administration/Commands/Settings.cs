using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Preconditions;

namespace Reimu.Administration.Commands
{
    public class Settings : ReimuBase
    {
        [Command("set joinrole"), UserPermission(GuildPermission.ManageRoles, "manage-roles"), BotPermission(GuildPermission.ManageRoles, "manage-roles")]
        public async Task SetJoinRole(SocketRole role = null)
        {
            if (role == null)
            {
                Context.GuildConfig.Moderation.JoinRole = 0;
                await ReplyAsync("Removed join role", updateGuild: true);
                return;
            }

            Context.GuildConfig.Moderation.JoinRole = role.Id;
            await ReplyAsync($"Set join role to {role.Name}", updateGuild: true);
        }
    }
}