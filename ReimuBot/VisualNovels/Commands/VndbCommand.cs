using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;
using Reimu.Core.Preconditions;
using VndbSharp;
using VndbSharp.Models;

namespace Reimu.VisualNovels.Commands
{
    [Name("Visual Novels"), Group("vndb")]
    public class VndbCommand : ReimuBase
    {
        [Command("character"), Alias("char", "c"), RequireCoolDown(15)]
        public async Task SearchCharacterAsync([Remainder] string name)
        {
            var charater = await Context.VndbClient.GetCharacterAsync(VndbFilters.Name.Fuzzy(name), VndbFlags.FullCharacter);
            var myChar = charater.FirstOrDefault();

            var embed = CreateEmbed(EmbedColor.Aqua)
                .WithAuthor(myChar.OriginalName)
                // TODO: Replace Vndb bb code where possible, and remove tags if not
                .WithDescription(myChar.Description.Replace("[spoiler]", "||").Replace("[/spoiler]", "||"))
                .WithThumbnailUrl(myChar.Image)
                .Build();

            await ReplyAsync(string.Empty, embed);
        }
    }
}
