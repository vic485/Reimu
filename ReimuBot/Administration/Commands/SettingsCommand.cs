﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Common.Logging;
using Reimu.Core;

namespace Reimu.Administration.Commands
{
    [Name("Administration"), Group("settings"), Alias("set", "setting")]
    public class SettingsCommand : ReimuBase
    {
        [Command("auditchannel"), RequireUserPermission(GuildPermission.ManageChannels)]
        public Task SetAuditChannel(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                Context.GuildConfig.Moderation.AuditChannel = 0;
                return ReplyAsync("Audit channel removed.", updateGuild: true);
            }

            Context.GuildConfig.Moderation.AuditChannel = channel.Id;
            return ReplyAsync($"Audit channel set to {channel.Mention}.", updateGuild: true);
        }
        
        [Command("joinmessage add"), Alias("jm add", "joinmessage a", "jm a"),
         RequireUserPermission(GuildPermission.ManageChannels)]
        public Task AddJoinMessage([Remainder] string message)
        {
            return ReplyAsync(!Context.GuildConfig.JoinMessages.TryAdd(message)
                ? "You are limited to five (5) join messages."
                : "Join message added.", updateGuild: true);
        }

        [Command("joinmessage remove"), Alias("jm remove", "joinmessage r", "jm r"),
         RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task RemoveJoinMessage()
        {
            if (Context.GuildConfig.JoinMessages.Count == 0)
            {
                await ReplyAsync("Guild has no join messages.");
                return;
            }

            var menu = CreateEmbed(EmbedColor.Red)
                .WithAuthor($"Remove Join Message")
                .WithDescription(BuildMenu(Context.GuildConfig.JoinMessages))
                .WithFooter("Type a **number** from the menu above to remove")
                .Build();

            var message = await ReplyWaitAsync(string.Empty, menu, timeOut: TimeSpan.FromSeconds(15));
            if (!message.Item1)
                return;

            if (!int.TryParse(message.Item2.Content, out var result))
            {
                await ReplyAsync("Invalid input, type a number. e.g. \"0\"");
                return;
            }

            if (result < 0 || result >= Context.GuildConfig.JoinMessages.Count)
            {
                await ReplyAsync("Invalid input, type a number from the menu. e.g. \"0\"");
                return;
            }

            Context.GuildConfig.JoinMessages.RemoveAt(result);
            await ReplyAsync("Join message removed.", updateGuild: true);
        }

        [Command("joinchannel"), RequireUserPermission(GuildPermission.ManageChannels)]
        public Task SetJoinChannel(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                Context.GuildConfig.JoinChannel = 0;
                return ReplyAsync("Join channel removed.", updateGuild: true);
            }

            Context.GuildConfig.JoinChannel = channel.Id;
            return ReplyAsync($"Join channel set to {channel.Mention}.", updateGuild: true);
        }

        [Command("leavemessage add"), Alias("lm add", "leavemessage a", "lm a"),
         RequireUserPermission(GuildPermission.ManageChannels)]
        public Task AddLeaveMessage([Remainder] string message)
        {
            return ReplyAsync(!Context.GuildConfig.LeaveMessages.TryAdd(message)
                ? "You are limited to five (5) leave messages."
                : "Leave message added.", updateGuild: true);
        }

        [Command("leavemessage remove"), Alias("lm remove", "leavemessage r", "lm r"),
         RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task RemoveLeaveMessage()
        {
            if (Context.GuildConfig.LeaveMessages.Count == 0)
            {
                await ReplyAsync("Guild has no leave messages.");
                return;
            }

            var menu = CreateEmbed(EmbedColor.Red)
                .WithAuthor($"Remove Leave Message")
                .WithDescription(BuildMenu(Context.GuildConfig.LeaveMessages))
                .WithFooter("Type a **number** from the menu above to remove")
                .Build();

            var message = await ReplyWaitAsync(string.Empty, menu, timeOut: TimeSpan.FromSeconds(15));
            if (!message.Item1)
                return;

            if (!int.TryParse(message.Item2.Content, out var result))
            {
                await ReplyAsync("Invalid input, type a number. e.g. \"0\"");
                return;
            }

            if (result < 0 || result >= Context.GuildConfig.JoinMessages.Count)
            {
                await ReplyAsync("Invalid input, type a number from the menu. e.g. \"0\"");
                return;
            }

            Context.GuildConfig.JoinMessages.RemoveAt(result);
            await ReplyAsync("Leave message removed.", updateGuild: true);
        }

        [Command("leavechannel"), RequireUserPermission(GuildPermission.ManageChannels)]
        public Task SetLeaveChannel(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                Context.GuildConfig.LeaveChannel = 0;
                return ReplyAsync("Leave channel removed.", updateGuild: true);
            }

            Context.GuildConfig.LeaveChannel = channel.Id;
            return ReplyAsync($"Leave channel set to {channel.Mention}.", updateGuild: true);
        }

        [Command("maxwarns"), RequireUserPermission(GuildPermission.KickMembers)]
        public Task SetMaxWarns(int warnings = 0)
        {
            if (warnings <= 0)
            {
                Context.GuildConfig.Moderation.MaxWarnings = 0;
                return ReplyAsync($"Kicking for maxing out warnings disabled.", updateGuild: true);
            }
            else
            {
                Context.GuildConfig.Moderation.MaxWarnings = warnings;
                return ReplyAsync($"Will automatically kick users when they receive {warnings} warnings.",
                    updateGuild: true);
            }
        }

        [Command("modchannel"), RequireUserPermission(GuildPermission.ManageChannels)]
        public Task SetModChannel(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                Context.GuildConfig.Moderation.LogChannel = 0;
                return ReplyAsync("Moderation channel removed.", updateGuild: true);
            }

            Context.GuildConfig.Moderation.LogChannel = channel.Id;
            return ReplyAsync($"Moderation channel set to {channel.Mention}.", updateGuild: true);
        }

        [Command("muterole"), RequireUserPermission(GuildPermission.ManageRoles),
         RequireBotPermission(GuildPermission.ManageRoles)]
        public Task SetMuteRole(SocketRole role = null)
        {
            if (role == null)
            {
                Context.GuildConfig.Moderation.MuteRole = 0;
                return ReplyAsync("Mute role removed.", updateGuild: true);
            }

            Context.GuildConfig.Moderation.MuteRole = role.Id;
            return ReplyAsync($"Mute role set to `{role.Name}`.", updateGuild: true);
        }

        [Command("verifymessage"), RequireUserPermission(GuildPermission.ManageMessages)]
        public Task SetVerifyMessage(ulong messageId = 0)
        {
            Context.GuildConfig.VerificationMessage = messageId;
            return ReplyAsync(
                messageId == 0
                    ? "Verification message removed."
                    : "Verification message set. If the role has been set members can verify by reacting with ✅.",
                updateGuild: true);
        }

        [Command("verifyrole"), RequireUserPermission(GuildPermission.ManageRoles)]
        public Task SetVerifyRole(SocketRole role = null)
        {
            if (role == null)
            {
                Context.GuildConfig.VerificationRole = 0;
                return ReplyAsync("Verification role removed.", updateGuild: true);
            }

            Context.GuildConfig.VerificationRole = role.Id;
            return ReplyAsync($"Verification role set to `{role.Name}`.", updateGuild: true);
        }

        [Command("wordbl add"), RequireUserPermission(GuildPermission.ManageMessages)]
        public Task WordBlacklistAdd(params string[] words)
        {
            foreach (var word in words)
            {
                if (Context.GuildConfig.Moderation.WordBlacklist.Contains(word))
                    continue;
                
                Context.GuildConfig.Moderation.WordBlacklist.Add(word);
            }
            
            return ReplyAsync("Word(s) added to blacklist.", updateGuild: true);
        }

        [Command("wordbl remove"), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WordBlacklistRemove()
        {
            if (!Context.GuildConfig.Moderation.WordBlacklist.Any())
            {
                await ReplyAsync("Guild has no blacklisted words.");
                return;
            }

            var menu = CreateEmbed(EmbedColor.Red)
                .WithTitle("Remove Blacklisted Word")
                .WithDescription(BuildMenu(Context.GuildConfig.Moderation.WordBlacklist))
                .WithFooter("Type a **number** from the above menu to remove.")
                .Build();
            
            var message = await ReplyWaitAsync(string.Empty, menu, timeOut: TimeSpan.FromSeconds(15));
            if (!message.Item1)
                return;

            if (!int.TryParse(message.Item2.Content, out var result))
            {
                await ReplyAsync("Invalid input, type a number. e.g. \"0\"");
                return;
            }

            if (result < 0 || result >= Context.GuildConfig.Moderation.WordBlacklist.Count)
            {
                await ReplyAsync("Invalid input, type a number from the menu. e.g. \"0\"");
                return;
            }

            Context.GuildConfig.Moderation.WordBlacklist.RemoveAt(result);
            await ReplyAsync("Word removed.", updateGuild: true);
        }

        private static string BuildMenu(List<string> menuItems)
        {
            var menu = "```\n";

            for (var i = 0; i < menuItems.Count; i++)
            {
                menu += $"[{i}] {menuItems[i]}\n";
            }

            menu += "```";
            return menu;
        }
    }
}
