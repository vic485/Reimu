using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core.Helpers;
using Reimu.Core.JsonModels;

namespace Reimu.Core.Handlers
{
    /// <summary>
    /// Handles Discord events
    /// </summary>
    public class DiscordHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly RavenHandler _database;
        private readonly CommandService _commandService;
        private readonly GuildHelper _guildHelper;

        private IServiceProvider _serviceProvider;

        public DiscordHandler(DiscordSocketClient client, RavenHandler database, GuildHelper guildHelper,
            CommandService commandService)
        {
            _client = client;
            _database = database;
            _guildHelper = guildHelper;
            _commandService = commandService;
        }

        public async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // TODO: add remaining necessary events
            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.Disconnected += Disconnected;
            _client.LatencyUpdated += LatencyUpdated;
            _client.LeftGuild += LeftGuild;
            _client.GuildAvailable += GuildAvailable;
            _client.JoinedGuild += JoinedGuildAsync;
            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.MessageReceived += CommandHandlerAsync;

            await _client.LoginAsync(TokenType.Bot, _database.Get<ConfigModel>("Config").Token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            _serviceProvider = serviceProvider;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        /// <summary>
        /// Sends discord log messages to Reimu's logger
        /// </summary>
        /// <param name="logMessage">Discord message</param>
        private static Task Log(LogMessage logMessage)
        {
            Logger.Log("Log", logMessage.Message ?? logMessage.Exception.Message, ConsoleColor.Green);
            return Task.CompletedTask;
        }

        #region Connections

        /// <summary>
        /// Made a successful connection to discord, and Reimu is ready to run
        /// </summary>
        private async Task ReadyAsync()
        {
            Logger.Log("Discord", "Successfully connected to Discord!", ConsoleColor.Blue);
            await _client.SetGameAsync($"{_database.Get<ConfigModel>("Config").Prefix}help");
        }

        /// <summary>
        /// Informs the log that we have been disconnected from Discord
        /// </summary>
        /// <param name="error">Error message</param>
        private static Task Disconnected(Exception error)
        {
            Logger.Log("Discord", $"Disconnected from Discord: {error.Message}", ConsoleColor.DarkBlue);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets Reimu's status based on her Discord connection speed
        /// </summary>
        /// <param name="oldLatency">Previous latency value (only included for event purposes)</param>
        /// <param name="newLatency">Current latency value</param>
        private Task LatencyUpdated(int oldLatency, int newLatency) // Yes I included this just to piss people off
            => _client.SetStatusAsync(_client.ConnectionState == ConnectionState.Disconnected || newLatency > 500
                ? UserStatus.DoNotDisturb
                : _client.ConnectionState == ConnectionState.Connecting || newLatency > 250
                    ? UserStatus.Idle
                    : _client.ConnectionState == ConnectionState.Connected || newLatency < 100
                        ? UserStatus.Online
                        : UserStatus.AFK);

        #endregion

        #region Guilds

        private Task LeftGuild(SocketGuild guild)
        {
            _database.RemoveGuild(guild.Id, guild.Name);
            return Task.CompletedTask;
        }

        private Task GuildAvailable(SocketGuild guild)
        {
            if (!_database.Get<ConfigModel>("Config").GuildBlacklist.Contains(guild.Id))
                _database.AddGuild(guild.Id, guild.Name);
            else
                guild.LeaveAsync();

            return Task.CompletedTask;
        }

        private async Task JoinedGuildAsync(SocketGuild guild)
        {
            var config = _database.Get<ConfigModel>("Config");
            if (!config.GuildBlacklist.Contains(guild.Id))
            {
                _database.AddGuild(guild.Id, guild.Name);
                var defaultPrefix = config.Prefix;
                await _guildHelper.DefaultChannel(guild.Id)
                    .SendMessageAsync(
                        $"Thank you for inviting me to your guild. Default prefix is `{defaultPrefix}` type `{defaultPrefix}help` for a list of commands.")
                    .ConfigureAwait(false);
            }
            else
            {
                await guild.LeaveAsync();
            }
        }

        #endregion

        #region User

        private async Task UserJoinedAsync(SocketGuildUser user)
        {
            // TODO: Join message, join role, re-mute user
            await Task.Delay(1);
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            // TODO: leave message
            await Task.Delay(1);
        }

        #endregion

        #region Messages

        private async Task CommandHandlerAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage)) 
                return;

            var argPos = 0;
            var context = new IContext(_client, userMessage, _serviceProvider);
            if (!(userMessage.HasStringPrefix(context.Config.Prefix, ref argPos) ||
                  userMessage.HasStringPrefix(context.Server.Prefix, ref argPos)))
                return;
            // TODO: Guild blacklist check

            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider, MultiMatchHandling.Best);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (result.Error)
            {
                case CommandError.UnmetPrecondition:
                    // TODO: DM error if we can't send a message?
                    if (!result.ErrorReason.Contains("SendMessages"))
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                    return;
            }
            // TODO: Record command for cool downs
        }

        #endregion
    }
}