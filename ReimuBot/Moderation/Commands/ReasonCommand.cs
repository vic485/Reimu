using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Reimu.Core;

namespace Reimu.Moderation.Commands
{
    [Name("Moderation")]
    public class ReasonCommand : ReimuBase
    {
        [Command("reason"),
         RequireUserPermission(GuildPermission.KickMembers)] // TODO: lower requirements or something?
        public async Task UpdateCaseReasonAsync(int caseNum, [Remainder] string reason)
        {
            if (caseNum > Context.GuildConfig.Moderation.Cases.Count || caseNum <= 0)
            {
                await ReplyAsync("Invalid case number");
                return;
            }

            await ModerationHelper.UpdateCaseAsync(Context, caseNum, reason);
        }
    }
}
