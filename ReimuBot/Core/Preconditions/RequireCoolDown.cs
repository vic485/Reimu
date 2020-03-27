using System;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Common.Logging;

namespace Reimu.Core.Preconditions
{
    public class RequireCoolDown : PreconditionAttribute
    {
        private TimeSpan _coolDownLength;

        public RequireCoolDown(int seconds) => _coolDownLength = TimeSpan.FromSeconds(seconds);

        // TODO: Could use some clean up
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext commandContext, CommandInfo command,
            IServiceProvider services)
        {
            if (!(commandContext is BotContext context))
            {
                var exception = new ArgumentException("Received a bad command context, could not convert to bot context");
                Logger.LogException(exception);
                throw exception;
            }
            
            var commandToCheck = command.Module.Group ?? command.Name;
            var profile = context.GuildConfig.UserProfiles.GetProfile(context.User.Id);

            if (!profile.CommandTimes.ContainsKey(commandToCheck))
                return Task.FromResult(PreconditionResult.FromSuccess());

            var timePassed = DateTime.UtcNow - profile.CommandTimes[commandToCheck];
            if (timePassed >= _coolDownLength)
                return Task.FromResult(PreconditionResult.FromSuccess());

            var timeLeft = _coolDownLength - timePassed;
            // Ignore milliseconds
            if (timeLeft.Seconds == 0)
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(
                PreconditionResult.FromError($"This command is on cool down. Time left: {timeLeft.Seconds}s."));
        }
    }
}
