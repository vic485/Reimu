using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.Fun.Commands
{
    [Name("Fun")]
    public class StickbugCommand : ReimuBase
    {
        [Command("stickbug")]
        public Task PostStickbug()
        {
            return ReplyAsync(
                "https://cdn.discordapp.com/attachments/701114760187478126/742969823163580496/video0.mp4");
        }
    }
}
