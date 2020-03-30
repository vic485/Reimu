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
            bool updateGuild = false)
        {
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            _ = Task.Run(() => SaveDocuments(updateConfig, updateGuild));
            return await base.ReplyAsync(message, false, embed, null);
        }

        public async Task<IUserMessage> ReplyFileAsync(string path, string message = null)
        {
            if (message != null)
                await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            return await Context.Channel.SendFileAsync(path, message);
        }

        public async Task<IUserMessage> ReplyDeleteAsync(string message, Embed embed = null, TimeSpan? timeOut = null,
            bool updateConfig = false, bool updateGuild = false)
        {
            timeOut ??= TimeSpan.FromSeconds(9); // バカ
            var msg = await ReplyAsync(message, embed, updateConfig, updateGuild);
            _ = Task.Delay(timeOut.Value).ContinueWith(_ => msg.DeleteAsync().ConfigureAwait(false));
            return msg;
        }

        public async Task<(bool, SocketMessage)> ReplyWaitAsync(string message, Embed embed = null,
            bool sameUser = true, bool sameChannel = true, TimeSpan? timeOut = null, bool handleTimeOut = true)
        {
            await ReplyAsync(message, embed);
            timeOut ??= TimeSpan.FromSeconds(9); // バカじゃないもん
            var interactive = new Interactive<SocketMessage>();

            if (sameUser)
                interactive.AddInteractive(new InteractiveUser());
            if (sameChannel)
                interactive.AddInteractive(new InteractiveChannel());

            var result = await ReplyWaitAsync(interactive, timeOut.Value);
            if (!result.Item1 && handleTimeOut)
                return (result.Item1,
                    await ReplyAsync($"{Context.User.Mention} you did not reply in time.") as SocketMessage);

            return result;
        }

        private async Task<(bool, SocketMessage)> ReplyWaitAsync(IInteractive<SocketMessage> interactive,
            TimeSpan timeOut)
        {
            var trigger = new TaskCompletionSource<SocketMessage>();

            async Task InteractiveHandlerAsync(SocketMessage message)
            {
                if (await interactive.JudgeAsync(Context, message))
                    trigger.SetResult(message);
            }

            Context.Client.MessageReceived += InteractiveHandlerAsync;
            var waitTask = await Task.WhenAny(trigger.Task, Task.Delay(timeOut));
            Context.Client.MessageReceived -= InteractiveHandlerAsync;
            return waitTask == trigger.Task ? (true, await trigger.Task) : (false, null);
        }

        protected static EmbedBuilder CreateEmbed(EmbedColor color)
            => new EmbedBuilder {Color = new Color((uint) color)};

        // TODO: This might be easier/cleaner to use a system to check flags on what was changed by a command
        private void SaveDocuments(bool configChange, bool guildChange)
        {
            if (configChange)
                Context.Database.Save(Context.Config);

            if (guildChange)
                Context.Database.Save(Context.GuildConfig);

            if (Context.Session.Advanced.HasChanges)
                Logger.LogWarning("One or more documents were not saved after a command was run");
        }
    }
}
