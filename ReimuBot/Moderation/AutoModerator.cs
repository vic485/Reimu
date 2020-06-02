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
                !context.GuildConfig.Moderation.WordBlacklist.Any(x => context.Message.Content.ToLower().Contains(x.ToLower())))
            {
                return false;
            }

            await context.Message.DeleteAsync();
            await context.Channel.SendMessageAsync($"Your message was deleted as it contains a blacklisted word.");
            return true;
        }
    }
}
