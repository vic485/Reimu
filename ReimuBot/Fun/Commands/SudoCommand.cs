using System;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.Fun.Commands
{
    // Realitätsverlust#8416 wanted this
    [Name("Fun")]
    public class SudoCommand : ReimuBase
    {
        [Command("sudo")]
        public async Task BaseCommand([Remainder] string text = null)
        {
            await ReplyAsync($"{Context.User.Mention} is not in the sudoers file. This incident will be reported.");
        }
    }
}
