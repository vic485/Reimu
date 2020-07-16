using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Common.Data.Parts;

namespace Reimu.Moderation
{
    public static class ModerationHelper
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

        public static async Task UpdateCaseAsync(BotContext context, int caseNum, string reason)
        {
            context.GuildConfig.Moderation.Cases[caseNum - 1].Reason = reason;
            context.Database.Save(context.GuildConfig); // Quick save in case the log channel or message were deleted.

            var modCase = context.GuildConfig.Moderation.Cases[caseNum - 1];
            var channel = context.Guild.GetTextChannel(context.GuildConfig.Moderation.LogChannel);
            var message = await channel.GetMessageAsync(modCase.MessageId) as IUserMessage;

            var embed = message.Embeds.FirstOrDefault();
            var user = embed.Fields.FirstOrDefault(x => x.Name == "User").Value;
            var mod = embed.Fields.FirstOrDefault(x => x.Name == "Moderator").Value;
            var newEmbed = new EmbedBuilder()
                .WithColor(embed.Color.Value)
                .WithAuthor(embed.Author.Value.Name, embed.Author.Value.IconUrl)
                .AddField("User", user, true)
                .AddField("Moderator", mod, true)
                .AddField("Reason", reason)
                .WithFooter(embed.Footer.Value.ToString())
                .WithTimestamp(embed.Timestamp.Value)
                .Build();

            await message.ModifyAsync(x => x.Embed = newEmbed);
        }

        /// <summary>
        /// Force downloading of guild users and get a SocketGuildUser by id.
        /// </summary>
        /// <param name="guild">The guild to act on</param>
        /// <param name="userId">User id to search for</param>
        /// <returns>SocketGuildUser with the provided id, or null if not found.</returns>
        public static async Task<SocketGuildUser> ResolveUser(SocketGuild guild, ulong userId)
        {
            await guild.DownloadUsersAsync();
            return guild.GetUser(userId);
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
