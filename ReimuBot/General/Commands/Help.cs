using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class Help : ReimuBase
    {
        private CommandService _commandService;

        private Help(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Command("help")]
        public async Task ShowHelpAsync()
        {
            // TODO: build help information

            // Notes:
            // When we group commands, i.e. selfrole add, selfrole remove, we should use [Group("selfrole")] on the
            // containing class and [Command("add")] on the tasks.
            // This is accessed via commandInfo.Module.Group and the [Name("General")] is accessed through Module.Name
            // Every command will need the module name attribute for grouping in the help dialogue.
            // Normally all commands would go in the same class so you only put it once, but I do not like that method
            // and find it too messy for my tastes.
        }
    }
}
