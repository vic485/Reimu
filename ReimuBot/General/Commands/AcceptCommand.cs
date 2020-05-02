using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("Owner")] // This is not an owner command, but we are putting it in the module to hide it from help
    public class AcceptCommand : ReimuBase
    {
        // Use channel permission so we can use this even if reimu doesn't have perms in guild for self roles
        [Command("accept"), RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task AcceptAsync()
        {
            if (Context.GuildConfig.VerificationRole == 0)
                return;

            var role = Context.Guild.GetRole(Context.GuildConfig.VerificationRole);
            if (role != null) 
                // ReSharper disable once PossibleNullReferenceException
                await (Context.User as SocketGuildUser)?.AddRoleAsync(role);
            // If this is somehow still null it will throw which is most likely desired
        }
    }
}
