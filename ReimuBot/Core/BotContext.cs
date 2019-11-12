using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Reimu.Core.Handlers;
using Reimu.Core.Json;
using Reimu.Scheduler;

namespace Reimu.Core
{
    public class BotContext : SocketCommandContext
    {
        // TODO: Do we need a database session?
        public IDocumentSession Session { get; }
        public DatabaseHandler Database { get; }
        public BotConfig Config { get; }
        public GuildConfig GuildConfig { get; }
        public SchedulerService Scheduler { get; }

        public BotContext(DiscordSocketClient client, SocketUserMessage msg, IServiceProvider provider) : base(client,
            msg)
        {
            Session = provider.GetRequiredService<IDocumentStore>().OpenSession();
            Scheduler = provider.GetRequiredService<SchedulerService>();
            Database = provider.GetRequiredService<DatabaseHandler>();
            Config = Database.Get<BotConfig>("Config");
            if (Guild != null)
                GuildConfig = Database.Get<GuildConfig>($"guild-{Guild.Id}");
        }
    }
}