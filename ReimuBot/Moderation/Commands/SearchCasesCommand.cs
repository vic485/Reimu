using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation"), RequireUserPermission(GuildPermission.KickMembers)]
    public class SearchCasesCommand : ReimuBase
    {
        [Command("searchcase")]
        public Task SearchCases(SocketGuildUser user)
            => SearchCases(user.Id);

        [Command("searchcase")]
        public Task SearchCases(ulong id)
        {
            var matchingCases = Context.GuildConfig.Moderation.Cases.Where(x => x.UserId == id).ToList();

            if (matchingCases.Count == 0)
            {
                return ReplyAsync("No cases were found for this user.");
            }

            var message = $"Found {matchingCases.Count} cases for this user.\n";
            foreach (var modCase in matchingCases)
            {
                message +=
                    $"<https://discordapp.com/channels/{Context.Guild.Id}/{Context.GuildConfig.Moderation.LogChannel}/{modCase.MessageId}>\n";
            }

            return ReplyAsync(message);
        }
    }
}
