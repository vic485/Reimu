using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Common.Logging;

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
