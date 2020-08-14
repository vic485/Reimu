using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.Owner.Commands
{
    [Name("Owner"), Group("status"), RequireOwner]
    public class ChangeStatusCommand : ReimuBase
    {
        [Command("stream")]
        public async Task StreamStatus(string url)
        {
            foreach (var shard in Context.Client.Shards)
            {
                await Context.Client.SetGameAsync($"{Context.Config.Prefix}help | Shard [{shard.ShardId + 1}]", url,
                    ActivityType.Streaming);
            }
        }

        [Command("play")]
        public async Task PlayStatus(string game)
        {
            await Context.Client.SetGameAsync(game);
        }

        [Command("reset")]
        public async Task ResetStatus()
        {
            foreach (var shard in Context.Client.Shards)
            {
                await shard.SetGameAsync($"{Context.Config.Prefix}help | Shard [{shard.ShardId + 1}]");
            }
        }
    }
}
