﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Common.Data.Parts;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class KickCommand : ReimuBase
    {
        [Command("kick"), RequireBotPermission(GuildPermission.KickMembers),
         RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUserAsync(SocketGuildUser user, [Remainder] string reason = null)
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

            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                $"**[Kicked from {Context.Guild.Name}]**\n" +
                $"Reason: {reason ?? "No reason provided."}");
            await user.KickAsync(/*reason*/);
            await ModerationHelper.LogAsync(Context, user, CaseType.Kick, reason);
            var name = user.Nickname ?? user.Username;
            if (!DiscordHandler.BlockedContent.Any(name.Contains))
                await ReplyAsync($"{name} was kicked.");
        }

        [Command("kick"), RequireBotPermission(GuildPermission.KickMembers),
         RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUserAsync(ulong id, [Remainder] string reason = null)
            => await KickUserAsync(await ModerationHelper.ResolveUser(Context.Guild, id), reason);
    }
}
