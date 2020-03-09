using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class Avatar : ReimuBase
    {
        [Command("avatar")]
        public async Task UserAvatarAsync()
        {
            var avatarUrl = Context.User.GetAvatarUrl(size: 1024);
            var embed = CreateEmbed(EmbedColor.Purple)
                .WithAuthor("Your Avatar", url: avatarUrl)
                .WithImageUrl(avatarUrl)
                .Build();

            await ReplyAsync(string.Empty, embed);
        }

        [Command("avatar")]
        public async Task OtherUserAsync(SocketGuildUser user)
        {
            var avatarUrl = user.GetAvatarUrl(size: 1024);
            var name = !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username;
            var embed = CreateEmbed(EmbedColor.Purple)
                .WithAuthor($"{name}'s Avatar", url: avatarUrl)
                .WithImageUrl(avatarUrl)
                .Build();

            await ReplyAsync(string.Empty, embed);
        }
    }
}
