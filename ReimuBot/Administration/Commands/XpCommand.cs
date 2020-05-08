using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Administration.Commands
{
    [Name("Administration"), Group("xp")]
    public class XpCommand : ReimuBase
    {
        [Command("block"), RequireUserPermission(GuildPermission.ManageRoles)]
        public Task BlockRole(params SocketRole[] roles)
        {
            foreach (var role in roles)
            {
                if (Context.GuildConfig.XpSettings.BlockedRoles.Contains(role.Id))
                    continue;

                Context.GuildConfig.XpSettings.BlockedRoles.Add(role.Id);
            }

            return ReplyAsync("The roles are now blocked from gaining guild xp.", updateGuild: true);
        }

        [Command("block"), RequireUserPermission(GuildPermission.ManageRoles)]
        public Task BlockRole(params SocketChannel[] channels)
        {
            foreach (var channel in channels)
            {
                if (Context.GuildConfig.XpSettings.BlockedChannels.Contains(channel.Id))
                    continue;

                Context.GuildConfig.XpSettings.BlockedChannels.Add(channel.Id);
            }

            return ReplyAsync("The channels are now blocked from gaining guild xp.", updateGuild: true);
        }

        [Command("toggle"), RequireUserPermission(GuildPermission.ManageRoles)]
        public Task ToggleXp()
        {
            Context.GuildConfig.XpSettings.Enabled = !Context.GuildConfig.XpSettings.Enabled;

            var status = Context.GuildConfig.XpSettings.Enabled ? "enabled" : "disabled";
            return ReplyAsync($"Guild xp has been {status}.", updateGuild: true);
        }

        [Command("unblock"), RequireUserPermission(GuildPermission.ManageRoles)]
        public Task UnBlockRole(params SocketRole[] roles)
        {
            foreach (var role in roles)
            {
                if (!Context.GuildConfig.XpSettings.BlockedRoles.Contains(role.Id))
                    continue;

                Context.GuildConfig.XpSettings.BlockedRoles.Remove(role.Id);
            }

            return ReplyAsync("The roles are no longer blocked from gaining guild xp.", updateGuild: true);
        }

        [Command("unblock"), RequireUserPermission(GuildPermission.ManageRoles)]
        public Task UnBlockRole(params SocketChannel[] channels)
        {
            foreach (var channel in channels)
            {
                if (!Context.GuildConfig.XpSettings.BlockedChannels.Contains(channel.Id))
                    continue;

                Context.GuildConfig.XpSettings.BlockedChannels.Remove(channel.Id);
            }

            return ReplyAsync("The channels are no longer blocked from gaining guild xp.", updateGuild: true);
        }

        [Command]
        public Task ShowXpSettings()
        {
            var roles = Context.GuildConfig.XpSettings.BlockedRoles.Aggregate(string.Empty,
                (current, roleId) => current + $"{Context.Guild.GetRole(roleId)?.Mention}\n");

            var channels = Context.GuildConfig.XpSettings.BlockedChannels.Aggregate(string.Empty,
                (current, channelId) => current + $"{Context.Guild.GetTextChannel(channelId)?.Mention}\n");

            var embed = CreateEmbed(EmbedColor.Purple)
                .WithTitle($"{Context.Guild.Name} XP Settings")
                .AddField("Status", Context.GuildConfig.XpSettings.Enabled ? "Enabled" : "Disabled")
                .AddField("Blocked Roles", string.IsNullOrWhiteSpace(roles) ? "None" : roles)
                .AddField("Blocked Channels", string.IsNullOrWhiteSpace(channels) ? "None" : channels)
                .Build();

            return ReplyAsync(string.Empty, embed);
        }
    }
}
