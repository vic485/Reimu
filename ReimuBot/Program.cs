using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Http;
using Reimu.Core.Configuration;
using Reimu.Core.Handlers;
using Reimu.Scheduler;

namespace Reimu
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await using var services = SetupServices();
            services.GetRequiredService<DatabaseHandler>().Initialize();
            await services.GetRequiredService<DiscordHandler>().InitializeAsync(services).ConfigureAwait(false);

            await Task.Delay(-1);
        }

        private static ServiceProvider SetupServices()
            => new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    ShardId = BotConfig.Settings.Shard,
                    TotalShards = BotConfig.Settings.TotalShards,
                    MessageCacheSize = 20,
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Error
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    ThrowOnError = true,
                    IgnoreExtraArgs = false,
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton(new DocumentStore
                {
                    Certificate = BotConfig.Settings.Certificate,
                    Database = BotConfig.Settings.DatabaseName,
                    Urls = BotConfig.Settings.Urls,
                    Conventions =
                    {
                        ReadBalanceBehavior = ReadBalanceBehavior.RoundRobin
                    }
                }.Initialize())
                .AddSingleton<DatabaseHandler>()
                .AddSingleton<DiscordHandler>()
                .AddSingleton<SchedulerService>()
                .BuildServiceProvider();
    }
}