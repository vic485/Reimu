using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Http;
using Reimu.Core;
using Reimu.Core.Handlers;
using Reimu.Core.Helpers;

namespace Reimu
{
    internal class Program
    {
        /// <summary>
        /// Whether we are running as a console app, or as a service (do we output to console)
        /// </summary>
        internal static bool Headless { get; private set; }

        private static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower().Contains("headless"))
                Headless = true;

            var provider = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    ShardId = SettingsHandler.Settings.Shard,
                    TotalShards = SettingsHandler.Settings.TotalShards,
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
                // TODO: Support for other DB systems
                .AddSingleton(new DocumentStore
                {
                    Certificate = SettingsHandler.Settings.Certificate,
                    Database = SettingsHandler.Settings.DatabaseName,
                    Urls = SettingsHandler.Settings.Urls,
                    Conventions =
                    {
                        ReadBalanceBehavior = ReadBalanceBehavior.RoundRobin
                    }
                }.Initialize())
                .AddSingleton<DiscordHandler>()
                .AddSingleton<RavenHandler>()
                .AddSingleton<GuildHelper>()
                .BuildServiceProvider();
            
            Logger.Initialize();
            // TODO: How to make this work with other DB systems if Raven needs the IDocumentStore
            provider.GetRequiredService<RavenHandler>().Initialize();
            await provider.GetRequiredService<DiscordHandler>().InitializeAsync(provider).ConfigureAwait(false);

            await Task.Delay(-1);
        }
    }
}