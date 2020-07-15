using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;
using Reimu.Common.Data;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class ScoreboardCommand : ReimuBase
    {
        [Command("scoreboard"), Alias("top")]
        public Task ShowScoreboardAsync()
        {
            if (Context.GuildConfig.UserProfiles.Count == 0)
                return ReplyAsync("No guild profiles available");

            var embed = CreateEmbed(EmbedColor.Aqua).WithTitle($"Top 10: {Context.Guild.Name}");
            var ordered = Context.GuildConfig.UserProfiles.OrderByDescending(x => x.Value.Xp)
                .Where(y => y.Value.Xp != 0).Take(10).ToList();
            if (ordered.Count > 3)
            {
                embed.AddField($"🥇: {CheckGuildUser(ordered[0].Key)}", $"**Total XP:** {ordered[0].Value.Xp}");
                embed.AddField($"🥈: {CheckGuildUser(ordered[1].Key)}", $"**Total XP:** {ordered[1].Value.Xp}");
                embed.AddField($"🥉: {CheckGuildUser(ordered[2].Key)}", $"**Total XP:** {ordered[2].Value.Xp}");

                for (var i = 3; i < ordered.Count; i++)
                    embed.AddField($"[{i + 1}] {CheckGuildUser(ordered[i].Key)}",
                        $"**Total XP:** {ordered[i].Value.Xp}");

                embed.WithFooter(
                    $"Rank: {GetRank(Context.User.Id)} Total XP: {Context.GuildConfig.UserProfiles.GetProfile(Context.User.Id).Xp}",
                    Context.User.GetAvatarUrl());
            }
            else
            {
                foreach (var (key, value) in ordered)
                {
                    embed.AddField(CheckGuildUser(key), $"**Total XP:** {value.Xp}");
                }
            }

            return ReplyAsync(string.Empty, embed.Build());
        }

        [Command("scoreboard"), Alias("top")]
        public Task ScoreboardPage(int page)
        {
            if (Context.GuildConfig.UserProfiles.Count == 0)
                return ReplyAsync("No guild profiles available");

            if (page <= 1)
            {
                return ShowScoreboardAsync();
            }

            var pageOffset = 10 * (page - 1);
            var embed = CreateEmbed(EmbedColor.Aqua).WithTitle($"Guild Leaderboard: {Context.Guild.Name}");
            var ordered = Context.GuildConfig.UserProfiles.OrderByDescending(x => x.Value.Xp)
                .Where(y => y.Value.Xp != 0).Skip(pageOffset).Take(10).ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                embed.AddField($"[{i + 1 + pageOffset}] {CheckGuildUser(ordered[i].Key)}",
                    $"**Total XP:** {ordered[i].Value.Xp}");
            }

            embed.WithFooter(
                $"Rank: {GetRank(Context.User.Id)} Total XP: {Context.GuildConfig.UserProfiles.GetProfile(Context.User.Id).Xp}",
                Context.User.GetAvatarUrl());

            return ReplyAsync(string.Empty, embed.Build());
        }

        /*[Command("scoreboard global"), Alias("top global")]
        public Task ShowGlobalScoreboard()
        {
            var profiles = Context.Database.GetAll<UserData>("user-");

            var embed = CreateEmbed(EmbedColor.Aqua).WithTitle("Global Leaderboard");
            var ordered = profiles.OrderByDescending(x => x.Xp).Where(y => y.Xp != 0).Take(10).ToList();

            foreach (var userData in ordered)
            {
                embed.AddField(CheckUser(ulong.Parse(userData.Id.Replace("user-", ""))), userData.Xp.ToString());
            }

            return ReplyAsync(string.Empty, embed.Build());
        }*/

        [Command("scoreboard global"), Alias("top global")]
        public Task ShowGlobalScoreboardPage(int page = 1)
        {
            if (page <= 1)
                page = 1;

            var profiles = Context.Database.GetAll<UserData>("user-");
            var pageOffset = 10 * (page - 1);
            var embed = CreateEmbed(EmbedColor.Aqua).WithTitle($"Global Leaderboard");
            var ordered = profiles.OrderByDescending(x => x.Xp)
                .Where(y => y.Xp != 0).Skip(pageOffset).Take(10).ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                embed.AddField($"[{i + 1 + pageOffset}] {CheckUser(ulong.Parse(ordered[i].Id.Replace("user-", "")))}",
                    $"**Total XP:** {ordered[i].Xp}");
            }

            embed.WithFooter(
                $"Total XP: {Context.UserData.Xp}",
                Context.User.GetAvatarUrl());

            return ReplyAsync(string.Empty, embed.Build());
        }

        private string CheckUser(ulong id)
        {
            var user = Context.Client.GetUser(id);
            return user == null ? "Deleted User" : user.Username;
        }

        private string CheckGuildUser(ulong id)
        {
            var user = Context.Guild.GetUser(id);
            return user == null ? "Unknown User" : user.Nickname ?? user.Username;
        }

        private int GetRank(ulong id)
        {
            var profileList = Context.GuildConfig.UserProfiles.OrderByDescending(x => x.Value.Xp).ToList();
            var profile = profileList.FirstOrDefault(x => x.Key == id);
            return profileList.IndexOf(profile) + 1;
        }
    }
}
