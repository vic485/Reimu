using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Administration.Commands
{
    [Name("Administration"), Group("settings"), Alias("set", "setting")]
    public class SettingsCommand : ReimuBase
    {
        [Command("joinmessage add"), Alias("jm add", "joinmessage a", "jm a"),
         RequireUserPermission(GuildPermission.ManageChannels)]
        public Task AddJoinMessage([Remainder] string message)
        {
            return ReplyAsync(!Context.GuildConfig.JoinMessages.TryAdd(message)
                ? "You are limited to five (5) join messages."
                : "Join message added.");
        }

        [Command("joinchannel")]
        public Task SetJoinChannel(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                Context.GuildConfig.JoinChannel = 0;
                return ReplyAsync("Join channel removed.");
            }

            Context.GuildConfig.JoinChannel = channel.Id;
            return ReplyAsync($"Join channel set to {channel.Mention}.");
        }
    }
}
