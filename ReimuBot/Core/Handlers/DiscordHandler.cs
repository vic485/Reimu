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
            //_client.ReactionAdded
            //_client.ReactionRemoved
            //_client.ReactionsCleared
            //_client.RecipientAdded
            //_client.RecipientRemoved
            //_client.RoleCreated
            //_client.RoleDeleted
            //_client.RoleUpdated
            //_client.UserBanned
            _client.UserJoined += UserJoinedAsync;
            //_client.UserLeft
            //_client.UserUnbanned
            //_client.UserUpdated
            //_client.CurrentUserUpdated
            //_client.GuildMembersDownloaded
            //_client.GuildMemberUpdated
            //_client.UserIsTyping
            //_client.VoiceServerUpdated
            //_client.UserVoiceStateUpdated

            Logger.LogVerbose("Logging in to Discord.");
            await _client.LoginAsync(TokenType.Bot, _database.Get<BotConfig>("Config").Token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            var recommendShard = await _client.GetRecommendedShardCountAsync().ConfigureAwait(false);
            if (Program.Configuration.TotalShards < recommendShard)
                Logger.LogWarning($"Running on less than the recommended number of shards. Set to {Program.Configuration.TotalShards} but recommended is {recommendShard}.");

            _serviceProvider = serviceProvider;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            Logger.LogVerbose($"Commands registered: {_commandService.Commands.Count()}.");
        }

        /// <summary>
        /// Sends discord log messages to Reimu's logger
        /// </summary>
        /// <param name="logMessage">Discord message</param>
        private static Task Log(LogMessage logMessage)
        {
            Logger.LogInfo(logMessage.Message ?? logMessage.Exception.Message);
            return Task.CompletedTask;
        }

        #region Connections

        /// <summary>
        /// Made a successful connection to discord, and Reimu is ready to run
        /// </summary>
        /// <returns></returns>
        private async Task ReadyAsync()
        {
            Logger.LogInfo("Everything ready to run.");
            // TODO: Change this to custom status when we can
            await _client.SetGameAsync($"{_database.Get<BotConfig>("Config").Prefix}help");
        }

        /// <summary>
        /// Informs the log that we have been disconnected from Discord
        /// </summary>
        /// <param name="error">Error message</param>
        private static Task Disconnected(Exception error)
        {
            Logger.LogInfo($"Disconnected from Discord: {error.Message}.");
            return Task.CompletedTask;
        }

        private static Task Connected()
        {
            Logger.LogInfo("Connected to Discord gateway.");
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

        private Task GuildAvailable(SocketGuild guild)
        {
            Logger.LogVerbose($"Guild {guild.Name} is available.");
            if (_database.Get<BotConfig>("Config").GuildBlacklist.Contains(guild.Id))
            {
                Logger.LogVerbose($"Guild {guild.Name} is blacklisted, leaving...");
                guild.LeaveAsync();
            }
            else
            {
                _database.AddGuild(guild.Id, guild.Name, guild.VoiceRegionId);
            }

            return Task.CompletedTask;
        }

        private Task GuildUnavailable(SocketGuild guild)
        {
            Logger.LogVerbose($"Guild {guild.Name} is unavailable.");
            return Task.CompletedTask;
        }

        private async Task JoinedGuildAsync(SocketGuild guild)
        {
            Logger.LogVerbose($"Joined guild {guild.Name}.");
            var config = _database.Get<BotConfig>("Config");
            if (config.GuildBlacklist.Contains(guild.Id))
            {
                Logger.LogVerbose($"Guild {guild.Name} is blacklisted, leaving...");
                await guild.LeaveAsync();
                return;
            }
            
            _database.AddGuild(guild.Id, guild.Name, guild.VoiceRegionId);
            // TODO: Send message to default channel
        }

        private Task LeftGuild(SocketGuild guild)
        {
            Logger.LogVerbose($"Removed from, or left guild {guild.Name}");
            _database.RemoveGuild(guild.Id, guild.Name);
            return Task.CompletedTask;
        }

        #endregion

        #region User Events

        private async Task UserJoinedAsync(SocketGuildUser user)
        {
            // TODO: Join message, re-mute user
            var config = _database.Get<GuildConfig>($"guild-{user.Guild.Id}");

            var role = user.Guild.GetRole(config.Moderation.JoinRole);
            if (role != null)
                await user.AddRoleAsync(role);
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            // TODO: Leave message
            await Task.Delay(1);
        }

        #endregion

        #region Messages

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // Ignore system, webhook, and other bot messages
            if (!(message is SocketUserMessage userMessage) || message.Author.IsBot)
                return;

            // Why contains not work????
            if (userMessage.MentionedUsers.Any(x => x.Id == _client.CurrentUser.Id))
            {
                // TODO: Have fun with this
                await userMessage.AddReactionAsync(Emote.Parse("<:ReimuWantToDie:501411246906671135>"));
                return;
            }

            var context = new BotContext(_client, userMessage, _serviceProvider);
            
            // Prevent xp gain in dms, guild xp would cause this to throw an error as well, blocking dm commands
            // TODO: Disabling this for now, needs cleaning
            /*if (!(context.Channel is IDMChannel))
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
            }*/

            var argPos = 0;
            if (!(userMessage.HasStringPrefix(context.Config.Prefix, ref argPos) ||
                  userMessage.HasStringPrefix(context.GuildConfig.Prefix, ref argPos)))
                return;
            
            // TODO: Guilds can blacklist users locally?
            if (context.Config.GuildBlacklist.Contains(context.Guild.Id) ||
                context.Config.UserBlacklist.Contains(context.User.Id))
                return;

            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider, MultiMatchHandling.Best);
            if (!result.Error.HasValue)
            {
                // TODO: Log commands for cooldown
                return;
            }
            
            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    break;
                case CommandError.ParseFailed:
                    Logger.LogVerbose($"Failed to parse command: {result.ErrorReason}.");
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
                    break;
                case CommandError.Exception:
                    break;
                case CommandError.Unsuccessful:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}