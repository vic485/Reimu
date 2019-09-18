using System.Linq;
using Discord.WebSocket;
using Reimu.Core.Handlers;

namespace Reimu.Core.Helpers
{
    public class GuildHelper
    {
        private readonly RavenHandler _database;
        private readonly DiscordSocketClient _client;

        public GuildHelper(RavenHandler database, DiscordSocketClient client)
        {
            _client = client;
            _database = database;
        }

        public SocketTextChannel DefaultChannel(ulong guildId)
        {
            var guild = _client.GetGuild(guildId);
            return guild.TextChannels.FirstOrDefault(x =>
                       x.Name.Contains("general") || x.Name.Contains("chat") || x.Id == guildId) ??
                   guild.DefaultChannel;
        }
    }
}