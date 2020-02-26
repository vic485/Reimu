using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Common.Logging;

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
        
        // TODO: This might be easier/cleaner to use a system to check flags on what was changed by a command
        private void SaveDocuments(bool configChange, bool guildChange, bool userChange)
        {
            if (configChange)
                Context.Database.Save(Context.Config);

            if (guildChange)
                Context.Database.Save(Context.GuildConfig);

            //if (userChange)
                //Context.Database.Save(Context.UserData);

            if (Context.Session.Advanced.HasChanges)
                Logger.LogWarning("One or more documents were not saved after a command was run");
        }
    }
}
