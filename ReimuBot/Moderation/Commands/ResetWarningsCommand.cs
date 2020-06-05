using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation"), RequireUserPermission(GuildPermission.KickMembers)]
    public class ResetWarningsCommand : ReimuBase
    {
        [Command("resetwarnings"), Alias("resetwarns")]
        public Task ResetWarns(SocketGuildUser user)
        {
            if (user.IsBot || !Context.GuildConfig.UserProfiles.ContainsKey(user.Id))
                return ReplyAsync($"{user.Nickname ?? user.Username} has no warnings.");

            Context.GuildConfig.UserProfiles[user.Id].Warnings = 0;
            return ReplyAsync($"Reset warnings for {user.Nickname ?? user.Username}.", updateGuild: true);
        }
    }
}
