using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.General.Commands
{
    [Name("General")]
    public class ScoreboardCommand : ReimuBase
    {
        // TODO: scoreboard pages
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
                embed.AddField($"🥇: {CheckUser(ordered[0].Key)}", $"**Total XP:** {ordered[0].Value.Xp}");
                embed.AddField($"🥈: {CheckUser(ordered[1].Key)}", $"**Total XP:** {ordered[1].Value.Xp}");
                embed.AddField($"🥉: {CheckUser(ordered[2].Key)}", $"**Total XP:** {ordered[2].Value.Xp}");

                for (var i = 3; i < ordered.Count; i++)
                    embed.AddField($"[{i + 1}] {CheckUser(ordered[i].Key)}", $"**Total XP:** {ordered[i].Value.Xp}");

                embed.WithFooter(
                    $"Rank: {GetRank(Context.User.Id)} Total XP: {Context.GuildConfig.UserProfiles.GetProfile(Context.User.Id).Xp}",
                    Context.User.GetAvatarUrl());
            }
            else
            {
                foreach (var (key, value) in ordered)
                {
                    embed.AddField(CheckUser(key), $"**Total XP:** {value.Xp}");
                }
            }

            return ReplyAsync(string.Empty, embed.Build());
        }

        private string CheckUser(ulong id)
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
