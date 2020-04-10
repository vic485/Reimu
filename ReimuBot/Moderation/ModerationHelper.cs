using System.Threading.Tasks;
using Discord;
using Reimu.Core;
using Reimu.Database.Models.Parts;

namespace Reimu.Moderation
{
    public class ModerationHelper
    {
        public static async Task LogAsync(BotContext context, IUser user, CaseType caseType, string reason)
        {
            var caseNum = context.GuildConfig.Moderation.Cases.Count + 1;
            reason ??=
                $"Responsible moderator please set a reason with {context.GuildConfig.Prefix}reason {caseNum} <reason>";
            var logChannel = context.Guild.GetTextChannel(context.GuildConfig.Moderation.LogChannel);
            if (logChannel == null)
                return;

            var embed = ReimuBase.CreateEmbed(CaseColor(caseType))
                .WithAuthor($"Case #{caseNum} | {caseType} | {user.Username}#{user.Discriminator}", user.GetAvatarUrl())
                .AddField("User", user.Mention, true)
                .AddField("Moderator", context.User.Mention, true)
                .AddField("Reason", reason)
                .WithFooter($"Id: {user.Id}")
                .WithCurrentTimestamp()
                .Build();

            var message = await logChannel.SendMessageAsync(string.Empty, embed: embed);
            context.GuildConfig.Moderation.Cases.Add(new ModCase
            {
                CaseNumber = caseNum,
                CaseType = caseType,
                MessageId = message.Id,
                ModId = context.User.Id,
                Reason = reason,
                UserId = user.Id
            });

            context.Database.Save(context.GuildConfig);
        }

        private static EmbedColor CaseColor(CaseType caseType)
            => caseType switch
            {
                CaseType.Ban => EmbedColor.Red,
                CaseType.Mute => EmbedColor.Yellow,
                CaseType.Warning => EmbedColor.Yellow,
                _ => EmbedColor.Aqua
            };
    }
}
