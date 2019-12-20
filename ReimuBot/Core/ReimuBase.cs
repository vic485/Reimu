using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Common.Logging;
using Reimu.Core.Interaction;

namespace Reimu.Core
{
    public class ReimuBase : ModuleBase<BotContext>
    {
        public async Task<IUserMessage> ReplyAsync(string message, Embed embed = null, bool updateConfig = false,
            bool updateGuild = false, bool updateUser = false)
        {
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            _ = Task.Run(() => SaveDocuments(updateConfig, updateGuild, updateUser));
            return await base.ReplyAsync(message, false, embed, null);
        }

        public async Task<IUserMessage> ReplyFile(string path, string message = null)
        {
            if (message != null)
                await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            return await Context.Channel.SendFileAsync(path, message);
        }

        public async Task<IUserMessage> ReplyDeleteAsync(string message, Embed embed = null, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(7);
            var msg = await ReplyAsync(message, embed).ConfigureAwait(false);
            _ = Task.Delay(timeout.Value).ContinueWith(_ => msg.DeleteAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            return msg;
        }

        public async Task<(bool, SocketMessage)> ReplyWaitAsync(string message, Embed embed = null,
            bool sameUser = true, bool sameChannel = true,
            TimeSpan? timeout = null, bool handleTimeout = true)
        {
            await ReplyAsync(message, embed);
            timeout ??= TimeSpan.FromSeconds(15);
            var interactive = new Interactive<SocketMessage>();
            if (sameUser)
                interactive.AddInteractive(new InteractiveUser());
            if (sameChannel)
                interactive.AddInteractive(new InteractiveChannel());

            var result = await ReplyWaitAsync(interactive, timeout.Value);
            if (!result.Item1 && handleTimeout)
                return (result.Item1,
                    await ReplyAsync($"{Context.User.Mention}, you did not reply in time") as SocketMessage);
            return result;
        }

        private async Task<(bool, SocketMessage)> ReplyWaitAsync(IInteractive<SocketMessage> interactive,
            TimeSpan timeout)
        {
            var trigger = new TaskCompletionSource<SocketMessage>();

            async Task InteractiveHandlerAsync(SocketMessage message)
            {
                var result = await interactive.JudgeAsync(Context, message).ConfigureAwait(false);
                if (result)
                    trigger.SetResult(message);
            }

            Context.Client.MessageReceived += InteractiveHandlerAsync;
            var waitTask = await Task.WhenAny(trigger.Task, Task.Delay(timeout)).ConfigureAwait(false);
            Context.Client.MessageReceived -= InteractiveHandlerAsync;
            return waitTask == trigger.Task ? (true, await trigger.Task.ConfigureAwait(false)) : (false, null);
        }

        public EmbedBuilder CreateEmbed(EmbedColor color)
            => new EmbedBuilder
            {
                Color = color switch
                {
                    EmbedColor.Aqua => new Color(138, 247, 252), // Part of Tenshi's dress
                    EmbedColor.Green => new Color(0, 109, 56), // Sanae's eyes
                    EmbedColor.Purple => new Color(172, 130, 220), // Reisen's hair
                    EmbedColor.Red => new Color(255, 70, 44), // Reimu's outfit
                    EmbedColor.Yellow => new Color(255, 241, 3), // Marisa's hair
                    _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
                }
            };

        // TODO: This might be easier/cleaner to use a system to check flags on what was changed by a command
        private void SaveDocuments(bool configChange, bool guildChange, bool userChange)
        {
            if (configChange)
            {
                Logger.LogVerbose("Bot configuration update requested.");
                Context.Database.Save(Context.Config);
            }

            if (guildChange)
            {
                Logger.LogVerbose($"Guild configuration update requested for {Context.GuildConfig.Id}.");
                Context.Database.Save(Context.GuildConfig);
            }

            if (userChange)
            {
                Logger.LogVerbose($"Global user data update requested for {Context.UserData.Id}.");
                Context.Database.Save(Context.UserData);
            }

            if (Context.Session.Advanced.HasChanges)
                Logger.LogError("One or more documents were not saved after a command was run");
        }
    }
}