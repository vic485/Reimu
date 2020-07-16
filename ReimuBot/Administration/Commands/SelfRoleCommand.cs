using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Common.Logging;
using Reimu.Core;
using Reimu.Common.Data.Parts;

namespace Reimu.Administration.Commands
{
    [Name("Administration"), Group("selfrole"), RequireUserPermission(GuildPermission.ManageRoles),
     RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.AddReactions | GuildPermission.ManageMessages)]
    public class SelfRoleCommand : ReimuBase
    {
        #region Commands

        [Command]
        public Task SelfRoleHelp()
        {
            return ReplyAsync();
        }

        [Command("add")]
        public async Task AddSelfRole(string emojiString, SocketRole role)
        {
            await InternalSelfRoleAdd(emojiString, role, Context.GuildConfig.DefaultRoleMenu);
        }

        [Command("add")]
        public async Task AddSelfRoleToList(string emojiString, SocketRole role, string list)
        {
            if (Context.GuildConfig.SelfroleMenus.ContainsKey(list))
            {
                await InternalSelfRoleAdd(emojiString, role, Context.GuildConfig.SelfroleMenus[list], list);
            }
            else
            {
                await ReplyAsync($"{list} self role menu was not found.");
            }
        }

        [Command("list")]
        public async Task ListDefaultMenu()
        {
            await InternalSelfRoleList(Context.GuildConfig.DefaultRoleMenu);
        }

        [Command("list")]
        public async Task ListSelfRoleMenu(string list)
        {
            if (Context.GuildConfig.SelfroleMenus.ContainsKey(list))
            {
                await InternalSelfRoleList(Context.GuildConfig.SelfroleMenus[list], list);
            }
            else
            {
                await ReplyAsync($"There is no {list} self role menu.");
            }
        }

        [Command("list create")]
        public async Task SelfRoleListCreateAsync(string listName)
        {
            if (Context.GuildConfig.SelfroleMenus.ContainsKey(listName))
            {
                await ReplyAsync($"There is already a {listName} self role list.");
            }
            else
            {
                Context.GuildConfig.SelfroleMenus.Add(listName, new SelfRoleMenu());
                await ReplyAsync($"Created {listName} self role menu.", updateGuild: true);
            }
        }

        [Command("list delete")]
        public async Task SelfRoleListDeleteAsync(string listName)
        {
            if (!Context.GuildConfig.SelfroleMenus.ContainsKey(listName))
            {
                await ReplyAsync($"There is no self role menu for {listName}.");
            }
            else
            {
                Context.GuildConfig.SelfroleMenus.Remove(listName);
                await ReplyAsync($"Deleted {listName} self role menu.", updateGuild: true);
            }
        }

        [Command("remove")]
        public async Task RemoveSelfRole(string em)
        {
            await InternalSelfRoleRemove(em, Context.GuildConfig.DefaultRoleMenu);
        }

        [Command("remove")]
        public async Task RemoveSelfRoleFromList(string em, string list)
        {
            if (Context.GuildConfig.SelfroleMenus.ContainsKey(list))
            {
                await InternalSelfRoleRemove(em, Context.GuildConfig.SelfroleMenus[list], list);
            }
            else
            {
                await ReplyAsync($"{list} self role menu was not found.");
            }
        }

        #endregion

        #region Get Methods

        private IEmote GetEmote(string key)
        {
            IEmote result;
            if (Emote.TryParse(key, out var emote))
                result = emote;
            else
                result = new Emoji(key);

            return result;
        }

        private async Task<IUserMessage> GetSelfRoleMessage(SelfRoleMenu rm)
        {
            if (rm.Channel.HasValue && rm.Message.HasValue)
            {
                if (Context.Guild.GetChannel(rm.Channel.Value) is SocketTextChannel channel &&
                    await channel.GetMessageAsync(rm.Message.Value) is IUserMessage message)
                {
                    return message;
                }
            }

            return null;
        }

        #endregion

        #region Internal Methods

        private async Task InternalSelfRoleAdd(string em, SocketRole role, SelfRoleMenu rm, string list = null)
        {
            var emote = GetEmote(em);
            if (rm.SelfRoles.ContainsKey(emote.ToString()))
            {
                rm.SelfRoles[emote.ToString()] = role.Id;
                await UpdateSelfRoleMessage(rm, list);
                await ReplyAsync(
                    $"Self role for {Emote.Parse(emote.ToString())} changed to `{role.Name}` in {list ?? "default"} menu",
                    updateGuild: true);
            }
            else
            {
                rm.SelfRoles.Add(emote.ToString(), role.Id);
                await UpdateSelfRoleMessage(rm, list);
                await ReplyAsync($"Added self role {role.Name} - {emote.ToString()} to {list ?? "default"} menu.",
                    updateGuild: true);
            }
        }

        private async Task InternalSelfRoleList(SelfRoleMenu rm, string list = null)
        {
            if (rm.SelfRoles.Count == 0)
            {
                await ReplyAsync($"Guild has no self assignable roles in {list ?? "default"} menu.");
                return;
            }

            if (rm.Channel.HasValue && rm.Message.HasValue)
            {
                if (Context.Guild.GetChannel(rm.Channel.Value) is SocketTextChannel channel &&
                    await channel.GetMessageAsync(rm.Message.Value) is IUserMessage prevMessage)
                {
                    await prevMessage.DeleteAsync();
                }
            }

            var embed = CreateEmbed(EmbedColor.Purple);
            var sb = new StringBuilder();
            foreach (var key in rm.SelfRoles.Keys)
            {
                sb.AppendLine($"{key} - {Context.Guild.GetRole(rm.SelfRoles[key]).Mention}\n");
            }

            var title = list ?? "Self Roles";
            embed.AddField($"**{title}**", sb.ToString());
            embed.WithFooter("Click a reaction below to receive a role. Remove your reaction to remove the role.");
            var message = await ReplyAsync(string.Empty, embed.Build());

            foreach (var em in rm.SelfRoles.Keys)
            {
                try
                {
                    var emote = GetEmote(em);
                    await message.AddReactionAsync(emote);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    throw;
                }
            }

            rm.Channel = message.Channel.Id;
            rm.Message = message.Id;
            Context.Database.Save(Context.GuildConfig); // Manual save instead of sending another message
            await Context.Message.DeleteAsync(); // Delete command message for cleanliness
        }

        private async Task InternalSelfRoleRemove(string em, SelfRoleMenu rm, string list = null)
        {
            var emote = GetEmote(em);

            if (!rm.SelfRoles.ContainsKey(emote.ToString()))
            {
                await ReplyAsync($"There is not a role assigned to {emote.ToString()} in {list ?? "default"} menu.");
                return;
            }

            rm.SelfRoles.Remove(emote.ToString());
            await UpdateSelfRoleMessage(rm, list);
            await ReplyAsync($"Removed self role assigned to {emote.ToString()} in {list ?? "default"} menu.",
                updateGuild: true);
        }

        #endregion

        // Ynscription's pride requests that even if the Discord(.Net) API improves in the future, this is left alone
        private async Task UpdateSelfRoleMessage(SelfRoleMenu rm, string list)
        {
            var message = await GetSelfRoleMessage(rm);

            if (message == null)
                return;

            var embed = CreateEmbed(EmbedColor.Purple);
            var sb = new StringBuilder();
            var reactions = new List<IEmote>();

            foreach (var key in rm.SelfRoles.Keys)
            {
                sb.AppendLine($"{key} - {Context.Guild.GetRole(rm.SelfRoles[key]).Mention}\n");
                reactions.Add(GetEmote(key));
            }

            if (rm.SelfRoles.Keys.Count > 0)
            {
                var title = list ?? "default";
                embed.AddField($"**{title}**", sb.ToString());
            }
            else
            {
                embed.WithTitle($"{list ?? "Default"} self role list is empty.");
                embed.WithImageUrl("https://vic485.xyz/images/404.png");
            }

            embed.WithFooter("Click a reaction below to receive a role. Remove your reaction to remove the role.");
            await message.ModifyAsync(x => x.Embed = embed.Build());

            // Get the difference between the message reactions and the final reactions
            var toDelete = new List<IEmote>(message.Reactions.Keys.Except(reactions));
            foreach (var emote in toDelete)
            {
                await DiscordApiHelper.DeleteAllReactionsWithEmote(message, emote);
            }

            // Get the difference between the final reactions and the message reactions
            var toAdd = new List<IEmote>(reactions.Except(message.Reactions.Keys));
            await message.AddReactionsAsync(toAdd.ToArray());
        }
    }
}
