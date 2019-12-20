using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Core;
using Reimu.Translation;

namespace Reimu.Preconditions
{
    public class BotPermission : PreconditionAttribute
    {
        private GuildPermission _guildPermission;
        private string _errorKey;

        public BotPermission(GuildPermission guildPermission, string errorKey)
        {
            _guildPermission = guildPermission;
            _errorKey = errorKey;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext commandContext, CommandInfo command, IServiceProvider services)
        {
            if (!(commandContext is BotContext context))
                throw new ArgumentException("Received incorrect context");

            return context.Guild.CurrentUser.GuildPermissions.Has(_guildPermission)
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError(
                    Translator.Get("preconditions", _errorKey, context.GuildConfig.Locale)[1]));
        }
    }
}