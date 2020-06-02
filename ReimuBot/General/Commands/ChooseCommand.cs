using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class ChooseCommand : ReimuBase
    {
        [Command("choose")]
        public Task Choose(params string[] choices)
        {
            if (choices.Length < 2)
            {
                return ReplyAsync("You must provide at least two things to choose between.");
            }

            var choice = choices[Rand.Range(0, choices.Length)];
            return ReplyAsync($"Hmmmm, I choose {choice}.");
        }
    }
}
