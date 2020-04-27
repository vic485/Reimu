using System.Threading.Tasks;
using Discord.Commands;
using Nekos.Net;
using Nekos.Net.Endpoints;
using Reimu.Core;

namespace Reimu.Nsfw.Commands
{
    [Name("Nsfw"), RequireNsfw]
    public class FutanariCommand : ReimuBase
    {
        [Command("futanari"), Alias("futa")]
        public async Task FutaAsync()
        {
            var image = await NekosClient.GetNsfwAsync(NsfwEndpoint.Futanari);
            var embed = CreateEmbed(EmbedColor.Aqua)
                .WithAuthor("Here is the requested image", url: image.FileUrl)
                .WithImageUrl(image.FileUrl)
                .WithFooter("Powered by Nekos.Life")
                .Build();

            await ReplyAsync(string.Empty, embed);
        }
    }
}
