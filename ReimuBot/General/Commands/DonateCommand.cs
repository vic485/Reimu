using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    //[Name("General")]
    public class DonateCommand : ReimuBase
    {
        //[Command("donate")]
        public Task DonateMessage()
        {
            var embed = CreateEmbed(EmbedColor.Red)
                .WithImageUrl("https://safebooru.org//images/2358/925835b24eef525a6bef4a6c1790c9904e2cdaa4.png?2456163")
                .AddField("Links", "[Buy a coffee](https://www.buymeacoffee.com/vic485)")
                .Build();

            return ReplyAsync(string.Empty, embed);
        }
    }
}
