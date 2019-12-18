using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Http;
using Reimu.Common.Configuration;
using Reimu.Common.Logging;
using Reimu.Core.Handlers;
using Reimu.Database;
using Reimu.Scheduler;

namespace Reimu
{
    internal static class Program
    {
        public const string Version = "0.0.1";
        public static SettingData Configuration { get; private set; }

        private static async Task Main(string[] args)
        {
            Configuration = SettingsLoader.Load();
            Logger.Initialize(Configuration.LogLevel, Path.Combine(Directory.GetCurrentDirectory(), "log.txt"),
                Version);
            await using var services = SetupServices();
            services.GetRequiredService<DatabaseHandler>().Initialize();
            await services.GetRequiredService<DiscordHandler>().InitializeAsync(services).ConfigureAwait(false);

            await Task.Delay(-1);
        }

        private static ServiceProvider SetupServices()
            => new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    ShardId = Configuration.Shard,
                    TotalShards = Configuration.TotalShards,
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
                    Certificate = Configuration.Certificate,
                    Database = Configuration.DatabaseName,
                    Urls = Configuration.DatabaseUrls,
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