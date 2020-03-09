using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Reimu.Database;
using Reimu.Database.Models;

namespace Reimu.Core
{
    public class BotContext : SocketCommandContext
    {
        public IDocumentSession Session { get; }
        public DatabaseHandler Database { get; }
        public BotConfig Config { get; }
        public GuildConfig GuildConfig { get; }
        public GlobalUser UserData { get; }

        public BotContext(DiscordSocketClient client, SocketUserMessage msg, IServiceProvider provider) : base(client,
            msg)
        {
            Database = provider.GetRequiredService<DatabaseHandler>();
            Session = Database.GetSession();
            Config = Database.Get<BotConfig>("Config");
            UserData = Database.Get<GlobalUser>($"user-{User.Id}") ?? new GlobalUser {Id = $"user-{User.Id}"};
            if (Guild != null)
                GuildConfig = Database.Get<GuildConfig>($"guild-{Guild.Id}");
        }
    }
}
