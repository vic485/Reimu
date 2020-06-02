using System.Threading.Tasks;
using Discord;

namespace Reimu.Core
{
    public class DiscordApiHelper
    {
        public static async Task DeleteAllReactionsWithEmote(IUserMessage message, IEmote emote)
        {
            if (message.Reactions.ContainsKey(emote))
            {
                await using var userEnumerator = message.GetReactionUsersAsync(emote, int.MaxValue).GetAsyncEnumerator();
                while (await userEnumerator.MoveNextAsync())
                {
                    foreach (var user in userEnumerator.Current)
                    {
                        await message.RemoveReactionAsync(emote, user);
                    }
                }
            }
        }
    }
}
