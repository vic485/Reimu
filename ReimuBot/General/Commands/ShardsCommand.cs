using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class ShardsCommand : ReimuBase
    {
        [Command("shards")]
        public Task ShowShardStatusAsync()
        {
            var embed = CreateEmbed(EmbedColor.Red)
                .WithAuthor(
                    $"Shard Status | You are on shard {Context.Client.GetShardFor(Context.Guild).ShardId}/{Context.Client.Shards.Count}");

            foreach (var shard in Context.Client.Shards)
            {
                embed.AddField($"Shard {shard.ShardId}", shard.ConnectionState);
            }

            return ReplyAsync(string.Empty, embed.Build());
        }
    }
}
