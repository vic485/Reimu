using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Core;
using Reimu.Core.Preconditions;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Character;

namespace Reimu.VisualNovels.Commands
{
    [Name("Visual Novels"), Group("vndb")]
    public class VndbCommand : ReimuBase
    {
        [Command]
        public Task TempCommandInfo()
        {
            return ReplyAsync(
                "**Visual Novel Database Search**\nThis command is currently in a very early state.\nImplemented functions: `r!vndb character <character name>`");
        }
        
        [Command("character"), Alias("char", "c"), RequireCoolDown(15)]
        public async Task SearchCharacterAsync(params string[] nameArray)
        {
            var name = string.Join(' ', nameArray);
            var charater =
                await Context.VndbClient.GetCharacterAsync(VndbFilters.Search.Fuzzy(string.Join(' ', name)),
                    VndbFlags.FullCharacter);

            if (charater.Count == 0)
            {
                name = string.Join(' ', nameArray.Reverse().ToArray());
                charater = await Context.VndbClient.GetCharacterAsync(VndbFilters.Search.Fuzzy(string.Join(' ', name)),
                    VndbFlags.FullCharacter);

                if (charater.Count == 0)
                {
                    await ReplyAsync($"No results for {name}");
                    return;
                }
            }

            var myChar = charater.FirstOrDefault();

            await ReplyAsync(string.Empty, BuildCharacterEmbed(myChar));
        }

        private static Embed BuildCharacterEmbed(Character character)
        {
            var embed = CreateEmbed(EmbedColor.Aqua)
                .WithAuthor($"{character.OriginalName} | {character.Name}", url: $"https://vndb.org/c{character.Id}")
                .WithImageUrl(character.Image);

            // TODO: Replace Vndb bb code where possible, and remove tags if not
            if (!string.IsNullOrWhiteSpace(character.Description))
                embed.WithDescription(character.Description.Replace("[spoiler]", "||").Replace("[/spoiler]", "||"));

            if (character.Bust.HasValue) // We assuming having one means all three sizes are in
                embed.AddField("Three Sizes", $"{character.Bust}-{character.Waist}-{character.Hip} cm");

            embed.WithFooter($"https://vndb.org/c{character.Id}", "https://i.imgur.com/4pcNLEZ.png");

            return embed.Build();
        }
    }
}
