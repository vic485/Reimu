using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;
using Reimu.Core.Json;
using Reimu.Preconditions;

namespace Reimu.Giveaway.Commands
{
    // TODO: For the love of god, clean up interactions
    public class Giveaway : ReimuBase
    {
        [Command("giveaway allow"),
         UserPermission(GuildPermission.ManageChannels, "This command requires \"manage channels\" permission.")]
        public Task AllowChannel(SocketGuildChannel channel)
        {
            if (Context.GuildConfig.Giveaway == null)
                return ReplyAsync("There is not an active giveaway on this server.");

            if (!Context.GuildConfig.Giveaway.DisabledChannels.Contains(channel.Id))
                return ReplyAsync("This channel does not have xp gain blocked for this giveaway");

            Context.GuildConfig.Giveaway.DisabledChannels.Remove(channel.Id);
            return ReplyAsync($"Allowed xp gain again in {channel.Name} for this giveaway", updateGuild: true);
        }

        [Command("giveaway block"),
         UserPermission(GuildPermission.ManageChannels, "This command requires \"manage channels\" permission.")]
        public Task BlockChannel(SocketGuildChannel channel)
        {
            if (Context.GuildConfig.Giveaway == null)
                return ReplyAsync("There is not an active giveaway on this server.");

            if (Context.GuildConfig.Giveaway.DisabledChannels.Contains(channel.Id))
                return ReplyAsync("This channel already has xp gain blocked for this giveaway");

            Context.GuildConfig.Giveaway.DisabledChannels.Add(channel.Id);
            return ReplyAsync($"Blocked xp gain in {channel.Name} for this giveaway", updateGuild: true);
        }

        [Command("giveaway create"),
         UserPermission(GuildPermission.ManageChannels, "This command requires \"manage channels\" permission.")]
        public async Task CreateAsync()
        {
            // TODO: support multiple
            if (Context.GuildConfig.Giveaway != null)
            {
                await ReplyAsync(
                    $"Looks like there is already an active giveaway. If it has already ended use `{Context.GuildConfig.Prefix}giveaway end` to remove it.");
                return;
            }

            var item = await ReplyWaitAsync("Alright, let's start a giveaway. First, what are we giving away?",
                timeout: TimeSpan.FromMinutes(2));
            if (!item.Item1) return;

            var winnerMessage = await ReplyWaitAsync("Okay, next how many winners will there be?",
                timeout: TimeSpan.FromMinutes(2));
            if (!winnerMessage.Item1) return;

            if (!int.TryParse(winnerMessage.Item2.Content, out var winners))
            {
                await ReplyAsync("I couldn't understand that.").ConfigureAwait(false);
                return;
            }
            
            var timeMessage = await ReplyWaitAsync("Alrighty, lastly how long will the giveaway last? Please type the number of hours as this is all I can understand currently.\nI'll give you 5 minutes to work this out if needed. The giveaway can last up to 49 days.", timeout: TimeSpan.FromMinutes(5));
            if (!timeMessage.Item1) return;

            if (!double.TryParse(timeMessage.Item2.Content, out var time))
            {
                await ReplyAsync("I couldn't understand that.").ConfigureAwait(false);
                return;
            }

            Context.GuildConfig.Giveaway = new GiveawayInfo
            {
                Item = item.Item2.Content,
                EndsAt = DateTime.UtcNow + TimeSpan.FromHours(time),
                NumWinners = winners
            };

            // TODO: embed building
            var embed = new EmbedBuilder().WithAuthor("Giveaway").AddField("Item", item.Item2.Content).WithFooter("Ends at")
                .WithTimestamp(DateTimeOffset.Now + TimeSpan.FromHours(time)).Build();

            await Context.Scheduler.Schedule($"{Context.Guild.Id}-giveaway", TimeSpan.FromHours(time),
                async () => await EndGiveaway());
            await ReplyAsync($"Alright, here is the giveaway info!", embed, updateGuild: true).ConfigureAwait(false);
        }

        [Command("giveaway end"),
         UserPermission(GuildPermission.ManageChannels, "This command requires \"manage channels\" permission.")]
        public async Task EndAsync()
        {
            if (Context.GuildConfig.Giveaway == null)
            {
                await ReplyAsync("There is not an active giveaway on this server.").ConfigureAwait(false);
                return;
            }

            if (Context.GuildConfig.Giveaway.EndsAt > DateTime.UtcNow)
            {
                var confirmResponse = await ReplyWaitAsync("The giveaway time period has not ended yet. I can end it now and figure out the winners from the current entries. Would you like me to do so?", timeout: TimeSpan.FromMinutes(1), handleTimeout: false).ConfigureAwait(false);
                if (!confirmResponse.Item1)
                {
                    await ReplyAsync("You did not respond, I will leave the giveaway up.").ConfigureAwait(false);
                    return;
                }

                if (Responses.Deny.Any(x => x == confirmResponse.Item2.Content))
                {
                    await ReplyAsync("Very well, I will leave the giveaway up.").ConfigureAwait(false);
                    return;
                }

                if (Responses.Confirm.All(x => x != confirmResponse.Item2.Content))
                {
                    await ReplyAsync("I'm not sure what that means. I will leave the giveaway up then.")
                        .ConfigureAwait(false);
                    return;
                }
            }

            var winners = GiveawayHelper.GetWinners(Context.GuildConfig.Giveaway);
            Context.GuildConfig.Giveaway = null;
            var winnerString = winners.Aggregate("",
                (current, winner) => current + Context.Guild.GetUser(winner).Mention + "\n");
            if (string.IsNullOrWhiteSpace(winnerString))
                winnerString = "none";

            var embed = new EmbedBuilder().WithAuthor("Giveaway results").AddField("Winners", winnerString).Build();
            await ReplyAsync("", embed, updateGuild: true);
        }

        [Command("giveaway enter")]
        public async Task EnterAsync()
        {
            if (Context.GuildConfig.Giveaway == null)
            {
                await ReplyAsync("There is not an active giveaway on this server.");
                return;
            }

            var points = Context.GuildConfig.Profiles[Context.User.Id].GiveAwayPoints;
            if (points < 100)
            {
                await ReplyAsync(
                    $"{Context.User.Mention} you need at least 100 points to add an entry to the giveaway. You have {points}");
                return;
            }

            var entries = points / 100;
            var left = points % 100;

            if (!Context.GuildConfig.Giveaway.Entries.ContainsKey(Context.User.Id))
            {
                Context.GuildConfig.Giveaway.Entries.Add(Context.User.Id, 0);
            }

            var total = Context.GuildConfig.Giveaway.Entries[Context.User.Id] += entries;
            Context.GuildConfig.Profiles[Context.User.Id].GiveAwayPoints = left;
            await ReplyAsync(
                $"{Context.User.Mention} you have added {entries} entries to the giveaway for a total of {total}.\nYou now have {left} points left",
                updateGuild: true);
        }

        [Command("giveaway set max"),
         UserPermission(GuildPermission.ManageChannels, "This command requires \"manage channels\" permission.")]
        public Task SetMax(int value)
        {
            if (Context.GuildConfig.Giveaway == null)
                return ReplyAsync("There is not an active giveaway on this server.");

            Context.GuildConfig.Giveaway.MaxXp = value;
            return ReplyAsync($"Set the maximum points per message to {value}", updateGuild: true);
        }

        [Command("giveaway set min"),
         UserPermission(GuildPermission.ManageChannels, "This command requires \"manage channels\" permission.")]
        public Task SetMin(int value)
        {
            if (Context.GuildConfig.Giveaway == null)
                return ReplyAsync("There is not an active giveaway on this server.");

            Context.GuildConfig.Giveaway.MinXp = value;
            return ReplyAsync($"Set the minimum points per message to {value}", updateGuild: true);
        }

        [Command("giveaway toggle repeats"),
         UserPermission(GuildPermission.ManageChannels, "This command requires \"manage channels\" permission.")]
        public Task ToggleMultiWin()
        {
            if (Context.GuildConfig.Giveaway == null)
                return ReplyAsync("There is not an active giveaway on this server.");

            var val = Context.GuildConfig.Giveaway.RepeatWinners = !Context.GuildConfig.Giveaway.RepeatWinners;
            string response;
            if (val)
                response = "Repeated winners has been enabled";
            else
                response = "Repeated winners has been disabled";

            return ReplyAsync(response, updateGuild: true);
        }

        private async Task EndGiveaway()
        {
            var newData = Context.Database.Get<GuildConfig>($"guild-{Context.Guild.Id}");
            var winners = GiveawayHelper.GetWinners(newData.Giveaway);
            Context.GuildConfig.Giveaway = null;
            var winnerString = winners.Aggregate("",
                (current, winner) => current + Context.Guild.GetUser(winner).Mention + "\n");
            if (string.IsNullOrWhiteSpace(winnerString))
                winnerString = "none";

            var embed = new EmbedBuilder().WithAuthor("Giveaway results").AddField("Winners", winnerString).Build();
            await ReplyAsync("", embed, updateGuild: true);
        }
    }
}