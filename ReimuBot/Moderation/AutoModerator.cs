using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Moderation
{
    public static class AutoModerator
    {
        private static readonly string[] Invites = {"discord.gg/", "discordapp.com/invite", "discord.com/invite"};

        public static async Task<bool> CheckForInvite(SocketUserMessage message, SocketGuildUser user)
        {
            // We will consider those with permission to manage messages moderators who can post invite links
            if (user.GuildPermissions.Has(GuildPermission.ManageMessages) || !Invites.Any(message.Content.Contains))
                return false;

            await message.DeleteAsync();
            var dm = await message.Author.GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync(
                $"Your message was deleted because it contains a discord invite link, which is not allowed.\n"
                + "Below is your original message so you may edit and resend it", false,
                new EmbedBuilder().WithColor((uint) EmbedColor.Red).WithDescription(message.Content).Build());
            return true;
        }

        public static async Task<bool> CheckForBlacklistedWord(BotContext context)
        {
            if ((context.User as SocketGuildUser).GuildPermissions.Has(GuildPermission.ManageMessages) ||
                !context.GuildConfig.Moderation.WordBlacklist.Any(x =>
                    context.Message.Content.ToLower().Contains(x.ToLower())))
            {
                return false;
            }

            await context.Message.DeleteAsync();
            await context.Channel.SendMessageAsync($"Your message was deleted as it contains a blacklisted word.");
            return true;
        }

        public static async Task<bool> CheckMentions(BotContext context)
        {
            var user = context.User as SocketGuildUser;

            if (context.GuildConfig.Moderation.MaxMentions <= 0)
                return false;

            if (user.GuildPermissions.Has(GuildPermission.ManageMessages) || context.Message.MentionedUsers.Count <=
                context.GuildConfig.Moderation.MaxMentions &&
                context.Message.MentionedRoles.Count <= context.GuildConfig.Moderation.MaxMentions)
                return false;

            await context.Message.DeleteAsync();

            var profile = context.GuildConfig.UserProfiles.GetProfile(user.Id);
            profile.Warnings++;
            context.GuildConfig.UserProfiles[user.Id] = profile;

            if (context.GuildConfig.Moderation.MaxWarnings > 0 &&
                profile.Warnings >= context.GuildConfig.Moderation.MaxWarnings)
            {
                await (await context.User.GetOrCreateDMChannelAsync()).SendMessageAsync(
                    $"**[Kicked from {context.Guild.Name}]**\n" +
                    $"Reason: Too many mentions in message.");
                await user.KickAsync("Too many mentions in message.");
                // TODO: Log system needs another method or adjustments to work with automoderator
                //await ModerationHelper.LogAsync(Context, user, CaseType.Kick, reason);
                var name = user.Nickname ?? user.Username;

                // Do it silent if their name tries to ping things, since we can't guarantee they don't play with markdown
                if (!DiscordHandler.BlockedContent.Any(name.Contains))
                {
                    await context.Channel.SendMessageAsync(
                        $"`{name}` maxed out warnings and was kicked.");
                }
            }
            else
            {
                await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(
                    $"**[Warned in {context.Guild.Name}]**\n" +
                    $"Reason: Too many mentions in message.");
                //await ModerationHelper.LogAsync(Context, user, CaseType.Warning, reason);
                // TODO: No warning for first infraction?
                await context.Channel.SendMessageAsync(
                    $"Your message was deleted as it contains too many user/role mentions.");
            }

            context.Database.Save(context.GuildConfig);
            return true;
        }
    }
}
