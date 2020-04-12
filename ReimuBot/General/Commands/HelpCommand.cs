using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class HelpCommand : ReimuBase
    {
        private CommandService _commandService;

        private HelpCommand(CommandService commandService) => _commandService = commandService;
        
        [Command("help")]
        public Task PrintHelp()
        {
            var embed = CreateEmbed(EmbedColor.Red)
                .WithAuthor("List of all commands", Context.Client.CurrentUser.GetAvatarUrl())
                .WithFooter($"Reimu is currently in experimental form. If you would like to help contribute, please visit https://github.com/vic485/Reimu");

            foreach (var commands in _commandService.Commands.GroupBy(x => x.Module.Name).OrderBy(y => y.Key))
            {
                embed.AddField(commands.Key,
                    $"`{string.Join("`, `", commands.Select(x => x.Module.Group ?? x.Name).Distinct())}`");
            }

            return ReplyAsync(string.Empty, embed.Build());
        }

        [Command("help"), Alias("info")]
        public Task ShowCommandHelp([Remainder] string searchCommand)
        {
            // TODO: CommandService.Search() seems to only search command names and not module groups
            return ReplyAsync("Command help currently broken");
        }
    }
}
