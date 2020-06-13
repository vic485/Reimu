using System;
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
using Reimu.Core;
using Reimu.Database;
using Reimu.Scheduling;
using VndbSharp;

namespace Reimu
{
    internal static class Program
    {
        public const string Version = "0.1.1";
        private static LocalSettings _settings;

        private static async Task Main(string[] args)
        {
            _settings = SettingsLoader.Load();
            Logger.Initialize(_settings.LogLevel, Path.Combine(Directory.GetCurrentDirectory(), "log.txt"), Version);

            await using var services = SetupServices();
            services.GetRequiredService<DatabaseHandler>().Initialize();
            await services.GetRequiredService<DiscordHandler>().InitializeAsync(services);
            services.GetRequiredService<SchedulerService>().SetupBaseTasks();

            // TODO: Here would be a command line program to manage the bot
            // Mainly so it can be shutdown safely (close/dispose) all services.
            // IDisposables may not be disposed on exit
            await Task.Delay(-1);
        }

        private static ServiceProvider SetupServices()
            => new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(new DiscordSocketConfig
                {
                    //ShardId = _settings.Shard,
                    //TotalShards = _settings.TotalShards,
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
                    Certificate = _settings.Certificate,
                    Database = _settings.DatabaseName,
                    Urls = _settings.DatabaseUrls,
                    Conventions = {ReadBalanceBehavior = ReadBalanceBehavior.FastestNode}
                }.Initialize())
                .AddSingleton<DatabaseHandler>()
                .AddSingleton<DiscordHandler>()
                .AddSingleton<SchedulerService>()
                .AddSingleton(new Vndb(true).WithClientDetails("ReimuBot", Version))
                .BuildServiceProvider();
    }
}
