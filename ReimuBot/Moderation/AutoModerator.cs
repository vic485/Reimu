using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Moderation
{
    public static class AutoModerator
    {
        private static readonly string[] Invites = {"discord.gg/", "discordapp.com/invite"};

        public static async Task<bool> CheckForInvite(SocketUserMessage message, SocketGuildUser user)
        {
            // We will consider those with permission to manage messages moderators who can post invite links
            if (!Invites.Any(message.Content.Contains) || user.GuildPermissions.Has(GuildPermission.ManageMessages))
                return false;

            await message.DeleteAsync();
            var dm = await message.Author.GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync(
                $"Your message was deleted because it contains a discord invite link, which is not allowed.\n"
                + "Below is your original message so you may edit and resend it", false,
                new EmbedBuilder().WithColor((uint) EmbedColor.Red).WithDescription(message.Content).Build());
            return true;
        }
    }
}
