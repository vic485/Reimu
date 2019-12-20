using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Translation;

namespace Reimu.Preconditions
{
    public class UserPermission : PreconditionAttribute
    {
        private GuildPermission _guildPermission;
        private string _error;

        public UserPermission(GuildPermission guildPermission, string error)
        {
            _guildPermission = guildPermission;
            _error = error;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext commandContext,
            CommandInfo command, IServiceProvider services)
        {
            if (!(commandContext is BotContext context && context.User is SocketGuildUser user))
                throw new ArgumentException("Received incorrect context.");

            var special =
                context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id == context.User.Id ||
                context.User.Id == context.Guild.OwnerId;

            return user.GuildPermissions.Has(_guildPermission) || special
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(
                    PreconditionResult.FromError(Translator.Get("preconditions", _error,
                        context.GuildConfig.Locale)[0]));
        }
    }
}