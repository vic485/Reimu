using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.Fun.Commands
{
    // Realitätsverlust#8416 wanted this
    public class Sudo : ReimuBase
    {
        [Command("sudo")]
        public async Task BaseCommand()
        {
            await ReplyAsync($"{Context.User.Mention} is not in the sudoers file. This incident will be reported.");
        }
    }
}
