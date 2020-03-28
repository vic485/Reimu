using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Common.Logging;
using Reimu.Database;
using Reimu.Database.Models;

namespace Reimu.Core
{
    public class DiscordHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly DatabaseHandler _database;

        private IServiceProvider _serviceProvider;

        public DiscordHandler(DiscordSocketClient client, CommandService commandService,
            DatabaseHandler databaseHandler)
        {
            _client = client;
            _commandService = commandService;
            _database = databaseHandler;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _client.Connected += Connected;
            _client.Disconnected += Disconnected;
            _client.Ready += ReadyAsync;
            _client.LatencyUpdated += LatencyUpdated;
            _client.Log += Log;
            //_client.ChannelCreated
            //_client.ChannelDestroyed
            //_client.ChannelUpdated
            _client.GuildAvailable += GuildAvailable;
            _client.GuildUnavailable += GuildUnavailable;
            //_client.GuildUpdated
            _client.JoinedGuild += JoinedGuildAsync;
            _client.LeftGuild += LeftGuild;
            //_client.LoggedIn
            //_client.LoggedOut
            //_client.MessageDeleted
            _client.MessageReceived += MessageReceivedAsync;
            //_client.MessageUpdated
            //_client.ReactionAdded += ReactionAddedAsync;
            //_client.ReactionRemoved += ReactionRemovedAsync;
            //_client.ReactionsCleared
            //_client.RecipientAdded
            //_client.RecipientRemoved
            //_client.RoleCreated
            //_client.RoleDeleted
            //_client.RoleUpdated
            //_client.UserBanned
            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            //_client.UserUnbanned
            //_client.UserUpdated
            //_client.CurrentUserUpdated
            //_client.GuildMembersDownloaded
            //_client.GuildMemberUpdated
            //_client.UserIsTyping
            //_client.VoiceServerUpdated
            //_client.UserVoiceStateUpdated

            Logger.LogVerbose("Logging into Discord.");
            await _client.LoginAsync(TokenType.Bot, _database.Get<BotConfig>("Config").Token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            _serviceProvider = provider;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            Logger.LogVerbose($"Total commands registered: {_commandService.Commands.Count()}");
        }

        #region Connections

        // Called when (re)connected to Discord
        private static Task Connected()
        {
            Logger.LogInfo("Connected to Discord gateway.");
            return Task.CompletedTask;
        }

        // Called when disconnected from Discord
        private static Task Disconnected(Exception error)
        {
            // Treat this as only a warning since the reasons for a loss of connection can vary in severity
            Logger.LogWarning($"Disconnected from Discord: {error.Message}");
            return Task.CompletedTask;
        }

        // Called after connected to discord, and user data downloaded
        private async Task ReadyAsync()
        {
            Logger.LogInfo("Everything ready to run.");
            // TODO: Change this to custom status if/when we can
            await _client.SetGameAsync($"{_database.Get<BotConfig>("Config").Prefix}help");
        }

        // TODO: Instead of latency use this for statuses/errors?
        // Called when the bot's latency changes
        private Task LatencyUpdated(int old, int newer) => _client.SetStatusAsync(
            _client.ConnectionState == ConnectionState.Disconnected || newer > 500 ? UserStatus.DoNotDisturb
            : _client.ConnectionState == ConnectionState.Connecting || newer > 250 ? UserStatus.Idle
            : _client.ConnectionState == ConnectionState.Connected || newer < 100 ? UserStatus.Online
            : UserStatus.AFK);

        #endregion

        /// <summary>
        /// Sends Discord.Net log messages to Reimu's logger
        /// </summary>
        /// <param name="message">Discord message</param>
        /// <exception cref="ArgumentOutOfRangeException">If LogMessage.Severity is malformed</exception>
        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                // TODO: Which cases need message.Exception.Message ?
                case LogSeverity.Critical:
                    Logger.LogCritical(message.Message);
                    break;
                case LogSeverity.Error:
                    Logger.LogError(message.Message);
                    break;
                case LogSeverity.Warning:
                    Logger.LogWarning(message.Message);
                    break;
                case LogSeverity.Info:
                    Logger.LogInfo(message.Message);
                    break;
                case LogSeverity.Verbose:
                    Logger.LogInfo(message.Message);
                    break;
                case LogSeverity.Debug:
                    Logger.LogDebug(message.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        #region Guild

        private Task GuildAvailable(SocketGuild guild)
        {
            if (!_database.Get<BotConfig>("Config").GuildBlacklist.Contains(guild.Id))
                _database.AddGuild(guild.Id, guild.Name);
            else
                guild.LeaveAsync();

            return Task.CompletedTask;
        }

        private Task GuildUnavailable(SocketGuild guild)
        {
            Logger.LogInfo($"Guild {guild.Name} has become unavailable.");
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

        private Task LeftGuild(SocketGuild guild)
        {
            _database.Remove($"guild-{guild.Id}");
            return Task.CompletedTask;
        }

        #endregion

        #region Messages

        private async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage userMessage) || socketMessage.Author.IsBot)
                return;

            var context = new BotContext(_client, userMessage, _serviceProvider);
            if (context.Config.UserBlacklist.Contains(context.User.Id) ||
                context.Config.GuildBlacklist.Contains(context.Guild.Id))
                return;

            var argPos = 0;
            if (!(userMessage.HasStringPrefix(context.Config.Prefix, ref argPos) ||
                  userMessage.HasStringPrefix(context.GuildConfig.Prefix, ref argPos)))
                return;

            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider, MultiMatchHandling.Best);
            if (result.Error == null)
            {
                RecordCommand(context, argPos);
                return;
            }

            switch (result.Error.Value)
            {
                case CommandError.UnknownCommand:
                    break;
                case CommandError.ParseFailed:
                    break;
                case CommandError.BadArgCount:
                    break;
                case CommandError.ObjectNotFound:
                    break;
                case CommandError.MultipleMatches:
                    break;
                case CommandError.UnmetPrecondition:
                    // TODO: DM error if we can't send messages?
                    if (!result.ErrorReason.Contains("SendMessages"))
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                    return;
                case CommandError.Exception:
                    break;
                case CommandError.Unsuccessful:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region User

        private async Task UserJoinedAsync(SocketGuildUser user)
        {
            var config = _database.Get<GuildConfig>($"guild-{user.Guild.Id}");
            var channel = user.Guild.GetTextChannel(config.JoinChannel);
            if (!(channel == null || config.JoinMessages.Count == 0))
            {
                var message = config.JoinMessages[Rand.Range(0, config.JoinMessages.Count)]
                    .Replace("{user}", user.Mention);
                await channel.SendMessageAsync(message);
            }
            // TODO: Join role, re-mute user
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            var config = _database.Get<GuildConfig>($"guild-{user.Guild.Id}");
            var channel = user.Guild.GetTextChannel(config.LeaveChannel);
            if (channel == null || config.LeaveMessages.Count == 0)
                return;

            var message = config.LeaveMessages[Rand.Range(0, config.LeaveMessages.Count)]
                .Replace("{user}", user.Nickname ?? user.Username);
            await channel.SendMessageAsync(message);
        }

        #endregion

        private void RecordCommand(BotContext context, int argPos)
        {
            var search = _commandService.Search(context, argPos);
            if (!search.IsSuccess) return;
            var command = search.Commands.FirstOrDefault().Command;
            var profile = context.GuildConfig.UserProfiles.GetProfile(context.User.Id);
            profile.CommandTimes[command.Module.Group ?? command.Name] = DateTime.UtcNow;
            context.GuildConfig.UserProfiles[context.User.Id] = profile;
            _database.Save(context.GuildConfig);
        }
    }
}
