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
using Reimu.Database.Models.Parts;
using Reimu.Fun;
using Reimu.Moderation;

namespace Reimu.Core
{
    public class DiscordHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly DatabaseHandler _database;

        private IServiceProvider _serviceProvider;

        public DiscordHandler(DiscordShardedClient client, CommandService commandService,
            DatabaseHandler databaseHandler)
        {
            _client = client;
            _commandService = commandService;
            _database = databaseHandler;
        }

        ~DiscordHandler()
        {
            _client.LogoutAsync();
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _client.ShardConnected += Connected;
            _client.ShardDisconnected += Disconnected;
            _client.ShardReady += ReadyAsync;
            _client.ShardLatencyUpdated += LatencyUpdated;
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
            _client.MessageDeleted += MessageDeletedAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.MessageUpdated += MessageUpdated;
            _client.ReactionAdded += ReactionAddedAsync;
            _client.ReactionRemoved += ReactionRemovedAsync;
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
        private static Task Connected(DiscordSocketClient client)
        {
            Logger.LogInfo($"Shard {client.ShardId + 1} has connected.");
            return Task.CompletedTask;
        }

        // Called when disconnected from Discord
        private static Task Disconnected(Exception error, DiscordSocketClient client)
        {
            // Treat this as only a warning since the reasons for a loss of connection can vary in severity
            Logger.LogWarning($"Shard {client.ShardId + 1} disconnected from Discord: {error.Message}");
            return Task.CompletedTask;
        }

        // Called after connected to discord, and user data downloaded
        private async Task ReadyAsync(DiscordSocketClient client)
        {
            Logger.LogInfo($"Shard {client.ShardId + 1} is ready.");
            await client.SetGameAsync(
                $"{_database.Get<BotConfig>("Config").Prefix}help | Shard [{client.ShardId + 1}]");
        }

        // TODO: Instead of latency use this for statuses/errors?
        // Called when the bot's latency changes
        private static Task LatencyUpdated(int old, int newer, DiscordSocketClient client) => client.SetStatusAsync(
            client.ConnectionState == ConnectionState.Disconnected || newer > 500 ? UserStatus.DoNotDisturb
            : client.ConnectionState == ConnectionState.Connecting || newer > 250 ? UserStatus.Idle
            : client.ConnectionState == ConnectionState.Connected || newer < 100 ? UserStatus.Online
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
                var channel =
                    guild.TextChannels.FirstOrDefault(x =>
                        x.Name.Contains("general") || x.Name.Contains("chat") || x.Id == guild.Id) ??
                    guild.DefaultChannel;
                await channel.SendMessageAsync(
                    $"Thank you for inviting me to your server. Guild prefix is `{defaultPrefix}`. Type `{defaultPrefix}help` for commands.");
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

        private async Task MessageDeletedAsync(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel channel)
        {
            var downloadMessage = await cacheable.GetOrDownloadAsync();

            if (!(downloadMessage is SocketUserMessage userMessage) || downloadMessage.Author.IsBot)
                return;

            var context = new BotContext(_client, userMessage, _serviceProvider);
            var auditChannel = context.Guild.GetTextChannel(context.GuildConfig.Moderation.AuditChannel);

            if (auditChannel == null)
                return;

            var embed = new EmbedBuilder()
                .WithColor((uint) EmbedColor.Red)
                .WithAuthor(userMessage.Author.Username, userMessage.Author.GetAvatarUrl())
                .WithDescription(
                    $"Message deleted in {(context.Channel as SocketTextChannel).Mention}\n{context.Message.Content}")
                .Build();

            await auditChannel.SendMessageAsync(string.Empty, embed: embed);
        }

        private async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage userMessage) || socketMessage.Author.IsBot)
                return;

            var context = new BotContext(_client, userMessage, _serviceProvider);
            if (context.Config.UserBlacklist.Contains(context.User.Id) ||
                context.Config.GuildBlacklist.Contains(context.Guild.Id))
                return;

            // Automod, if enabled
            if (context.GuildConfig.Moderation.InviteBlock)
            {
                if (await AutoModerator.CheckForInvite(userMessage, context.User as SocketGuildUser))
                    return;
            }

            if (await AutoModerator.CheckForBlacklistedWord(context))
                return;

            await MessageFun.RepeatText(context);

            // Global xp
            if ((DateTime.UtcNow - context.UserData.LastMessage).TotalMinutes >= 2)
            {
                context.UserData.Xp += Rand.Range(10, 21);
                context.UserData.LastMessage = DateTime.UtcNow;
                _database.Save(context.UserData);
            }

            // Guild xp
            var guildProfile = context.GuildConfig.UserProfiles.GetProfile(context.User.Id);
            if (context.GuildConfig.XpSettings.Enabled &&
                !(context.GuildConfig.XpSettings.BlockedChannels.Contains(context.Channel.Id) ||
                  (context.User as SocketGuildUser).Roles.Any(x =>
                      context.GuildConfig.XpSettings.BlockedRoles.Contains(x.Id))) &&
                (DateTime.UtcNow - guildProfile.LastMessage).TotalMinutes >= 2)
            {
                guildProfile.Xp += Rand.Range(context.GuildConfig.XpSettings.Min,
                    context.GuildConfig.XpSettings.Max + 1);
                guildProfile.LastMessage = DateTime.UtcNow;
                context.GuildConfig.UserProfiles[context.User.Id] = guildProfile;
                _database.Save(context.GuildConfig);
            }

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

        /// <summary>
        /// When message is edited
        /// </summary>
        /// <param name="cacheable">Contains the original message</param>
        /// <param name="socketMessage">The updated message</param>
        /// <param name="channel">Channel the message was in</param>
        /// <returns></returns>
        public async Task MessageUpdated(Cacheable<IMessage, ulong> cacheable, SocketMessage socketMessage,
            ISocketMessageChannel channel)
        {
            var message1 = await cacheable.GetOrDownloadAsync();
            if (!(socketMessage is SocketUserMessage userMessage) || socketMessage.Author.IsBot)
                return;

            var context = new BotContext(_client, userMessage, _serviceProvider);
            if (context.Config.GuildBlacklist.Contains(context.Guild.Id))
                return;

            // Automod, if enabled
            if (context.GuildConfig.Moderation.InviteBlock)
            {
                if (await AutoModerator.CheckForInvite(userMessage, context.User as SocketGuildUser))
                    return;
            }
        }

        private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var message = await cacheable.GetOrDownloadAsync();
            if (!(reaction.Channel is SocketGuildChannel guildChannel && reaction.User.Value is SocketGuildUser user))
                return;

            if (user.IsBot)
                return;

            var guild = guildChannel.Guild;
            var config = _database.Get<GuildConfig>($"guild-{guild.Id}");

            // Check for gateway reactions
            if (config.VerificationMessage != 0 && message.Id == config.VerificationMessage &&
                reaction.Emote.Name == "✅")
            {
                var role = guild.GetRole(config.VerificationRole);
                if (role != null && !user.Roles.Contains(role))
                    await user.AddRoleAsync(role);

                return;
            }

            // TODO: Other big uses for reactions

            // Check if message is a self role menu
            var rm = FindRoleMenu(config, reaction.Channel.Id, message.Id);
            if (rm == null)
                return;

            var final = reaction.Emote.ToString();
            // Animated emotes come in without the 'a' prefix for some reason
            if (!rm.SelfRoles.ContainsKey(final))
            {
                var index = final.IndexOf("<") + 1;
                if (index != 1)
                    final = final.Insert(index, "a");

                if (!rm.SelfRoles.ContainsKey(final))
                    return;
            }

            var selfRole = guild.GetRole(rm.SelfRoles[final]);
            await user.AddRoleAsync(selfRole);
        }

        private async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            // This will probably only be needed for self roles
            var message = await cacheable.GetOrDownloadAsync();

            if (!(reaction.Channel is SocketGuildChannel guildChannel && reaction.User.Value is SocketGuildUser user))
                return;

            if (user.IsBot)
                return;

            var guild = guildChannel.Guild;
            var config = _database.Get<GuildConfig>($"guild-{guild.Id}");

            var rm = FindRoleMenu(config, reaction.Channel.Id, message.Id);
            if (rm == null)
                return;

            var final = reaction.Emote.ToString();
            // Animated emotes come in without the 'a' prefix for some reason
            if (!rm.SelfRoles.ContainsKey(final))
            {
                var index = final.IndexOf("<") + 1;
                if (index != 1)
                    final = final.Insert(index, "a");

                if (!rm.SelfRoles.ContainsKey(final))
                    return;
            }

            var role = guild.GetRole(rm.SelfRoles[final]);
            if (!user.Roles.Contains(role))
                return;

            await user.RemoveRoleAsync(role);
        }

        #endregion

        #region User

        private async Task UserJoinedAsync(SocketGuildUser user)
        {
            if (user.IsBot)
                return;

            var config = _database.Get<GuildConfig>($"guild-{user.Guild.Id}");
            if (config.JoinSettings.Messages.Count != 0)
            {
                if (config.JoinSettings.SendToDm)
                {
                    var channel = await user.GetOrCreateDMChannelAsync();
                    var message = config.JoinSettings.Messages[Rand.Range(0, config.JoinSettings.Messages.Count)]
                        .Replace("{user}", user.Mention).Replace("{guild}", $"**{user.Guild.Name}**");
                    await channel.SendMessageAsync(message);
                }
                else
                {
                    var channel = user.Guild.GetTextChannel(config.JoinSettings.Channel);
                    var message = config.JoinSettings.Messages[Rand.Range(0, config.JoinSettings.Messages.Count)]
                        .Replace("{user}", user.Mention).Replace("{guild}", $"**{user.Guild.Name}**");
                    await channel.SendMessageAsync(message);
                }
            }
            // TODO: Join role

            if (config.Moderation.MutedUsers.Contains(user.Id))
            {
                var muteRole = user.Guild.GetRole(config.Moderation.MuteRole);
                if (muteRole != null)
                    await user.AddRoleAsync(muteRole);
            }
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            if (user.IsBot)
                return;

            var config = _database.Get<GuildConfig>($"guild-{user.Guild.Id}");
            if (config.LeaveSettings.Messages.Count == 0)
                return;

            if (config.LeaveSettings.SendToDm)
            {
                var channel = await user.GetOrCreateDMChannelAsync();
                var message = config.LeaveSettings.Messages[Rand.Range(0, config.LeaveSettings.Messages.Count)]
                    .Replace("{user}", user.Nickname ?? user.Username).Replace("{guild}", $"**{user.Guild.Name}**");
                await channel.SendMessageAsync(message);
            }
            else
            {
                var channel = user.Guild.GetTextChannel(config.LeaveSettings.Channel);
                var message = config.LeaveSettings.Messages[Rand.Range(0, config.LeaveSettings.Messages.Count)]
                    .Replace("{user}", user.Nickname ?? user.Username).Replace("{guild}", $"**{user.Guild.Name}**");
                await channel.SendMessageAsync(message);
            }
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

        private SelfRoleMenu FindRoleMenu(GuildConfig config, ulong channelId, ulong messageId)
        {
            foreach (var rm in config.SelfroleMenus.Values)
            {
                if (rm.Channel.HasValue && rm.Channel.Value == channelId && rm.Message.HasValue &&
                    rm.Message.Value == messageId)
                    return rm;
            }

            return null;
        }
    }
}
