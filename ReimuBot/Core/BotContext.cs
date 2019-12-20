using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Reimu.Core.Handlers;
using Reimu.Database;
using Reimu.Database.Models;
using Reimu.Scheduler;

namespace Reimu.Core
{
    public class BotContext : SocketCommandContext
    {
        public IDocumentSession Session { get; }
        public DatabaseHandler Database { get; }
        public BotConfig Config { get; }
        public GuildConfig GuildConfig { get; }
        public SchedulerService Scheduler { get; }
        public GlobalUser UserData { get; }

        public BotContext(DiscordSocketClient client, SocketUserMessage msg, IServiceProvider provider) : base(client,
            msg)
        {
            Session = provider.GetRequiredService<IDocumentStore>().OpenSession();
            Scheduler = provider.GetRequiredService<SchedulerService>();
            Database = provider.GetRequiredService<DatabaseHandler>();
            Config = Database.Get<BotConfig>("Config");
            UserData = Database.Get<GlobalUser>($"user-{User.Id}") ?? new GlobalUser {Id = $"user-{User.Id}"};
            // TODO: This will cause the message handler to throw if we are not in a guild. The message handler will need to be adjusted later if we support dm commands
            //if (Guild != null)
            GuildConfig = Database.Get<GuildConfig>($"guild-{Guild.Id}");
        }
    }
}