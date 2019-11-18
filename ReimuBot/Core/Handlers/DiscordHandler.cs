using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core.Json;
using Reimu.Giveaway;

namespace Reimu.Core.Handlers
{
    public class DiscordHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseHandler _database;
        private readonly CommandService _commandService;

        private IServiceProvider _serviceProvider;

        public DiscordHandler(DiscordSocketClient client, DatabaseHandler database, CommandService commandService)
        {
            _client = client;
            _database = database;
            _commandService = commandService;
        }

        // Connect events and logs into discord
        public async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // TODO: Add the rest of the events we will handle here
            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.Disconnected += Disconnected;
            _client.Connected += Connected;
            _client.LatencyUpdated += LatencyUpdated;
            _client.LeftGuild += LeftGuild;
            _client.GuildAvailable += GuildAvailable;
            _client.JoinedGuild += JoinedGuildAsync;
            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.MessageReceived += HandleMessageAsync;

            await _client.LoginAsync(TokenType.Bot, _database.Get<BotConfig>("Config").Token).ConfigureAwait(false);
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
        /// <returns></returns>
        private async Task ReadyAsync()
        {
            Logger.Log("Discord", "Connected and ready to run", ConsoleColor.Blue);
            // TODO: Change this to custom status when we can
            await _client.SetGameAsync($"{_database.Get<BotConfig>("Config").Prefix}help");
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

        private static Task Connected()
        {
            Logger.Log("Discord", "Connected to Discord", ConsoleColor.DarkBlue);
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

        #region Guild Events

        private Task LeftGuild(SocketGuild guild)
        {
            _database.RemoveGuild(guild.Id, guild.Name);
            return Task.CompletedTask;
        }

        private Task GuildAvailable(SocketGuild guild)
        {
            if (!_database.Get<BotConfig>("Config").GuildBlacklist.Contains(guild.Id))
                _database.AddGuild(guild.Id, guild.Name);
            else
                guild.LeaveAsync();

            return Task.CompletedTask;
        }

        private async Task JoinedGuildAsync(SocketGuild guild)
        {
            var config = _database.Get<BotConfig>("Config");
            if (!config.GuildBlacklist.Contains(guild.Id))
            {
                _database.AddGuild(guild.Id, guild.Name);
                var defaultPrefix = config.Prefix;
                // TODO: Send message to default channel
            }
            else
            {
                await guild.LeaveAsync();
            }
        }

        #endregion

        #region User Events

        private async Task UserJoinedAsync(SocketGuildUser user)
        {
            // TODO: Join role, join message, re-mute user
            await Task.Delay(1);
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            // TODO: Leave message
            await Task.Delay(1);
        }

        #endregion

        #region Messages

        private async Task HandleMessageAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage) || message.Author.IsBot)
                return;

            // Why contains not work????
            if (userMessage.MentionedUsers.Any(x => x.Id == _client.CurrentUser.Id))
            {
                await userMessage.AddReactionAsync(Emote.Parse("<:ReimuWantToDie:501411246906671135>"));
                return;
            }

            var context = new BotContext(_client, userMessage, _serviceProvider);
            
            // Prevent xp gain in dms, guild xp would cause this to throw an error as well, blocking dm commands
            if (!(context.Channel is IDMChannel))
            {
                // Globals
                if (DateTime.UtcNow - context.UserData.LastMessage > TimeSpan.FromMinutes(2))
                {
                    context.UserData.Points += Rand.Range(10, 20);
                    context.UserData.LastMessage = DateTime.UtcNow;
                    _database.Save(context.UserData);
                }

                // Guild
                if (!context.GuildConfig.Profiles.ContainsKey(context.User.Id))
                    context.GuildConfig.Profiles.Add(context.User.Id, new GuildUser());

                if (DateTime.UtcNow - context.GuildConfig.Profiles[context.User.Id].LastMessage >
                    TimeSpan.FromMinutes(2))
                {
                    await GiveawayHelper.OnMessage(context.User, context.GuildConfig);

                    context.GuildConfig.Profiles[context.User.Id].LastMessage = DateTime.UtcNow;
                    // TODO: Allow for changing of guild score rewarding
                    context.GuildConfig.Profiles[context.User.Id].Points = Rand.Range(10, 20);
                    _database.Save(context.GuildConfig);
                }
            }

            var argPos = 0;
            if (!(userMessage.HasStringPrefix(context.Config.Prefix, ref argPos) ||
                  userMessage.HasStringPrefix("r!", ref argPos)))
                return;
            // TODO: Blacklist checks

            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider, MultiMatchHandling.Best);
            switch (result.Error)
            {
                case CommandError.UnmetPrecondition:
                    // TODO: DM error if we can't send messages?
                    if (!result.ErrorReason.Contains("SendMessages"))
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                    return;
            }

            // TODO: Record command for cooldown
        }

        #endregion
    }
}