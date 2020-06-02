using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Fun
{
    public static class MessageFun
    {
        public static async Task<bool> RepeatText(BotContext context)
        {
            var reply = context.Message.Content.ToLower();
            var otherMessages = (await context.Channel.GetMessagesAsync(3).FlattenAsync()).ToList();

            if (otherMessages.Count < 3)
                return false;

            // Check message contents
            if (otherMessages.Any(message => message.Content.ToLower() != reply) ||
                otherMessages.Any(x => string.IsNullOrWhiteSpace(x.Content)))
            {
                return false;
            }

            // Check that all authors are different
            if (otherMessages[0].Author == otherMessages[1].Author ||
                otherMessages[1].Author == otherMessages[2].Author ||
                otherMessages[0].Author == otherMessages[2].Author)
            {
                return false;
            }

            // Don't react if bot is in the messages
            if (otherMessages.Any(x => x.Author.Id == context.Client.CurrentUser.Id))
            {
                return false;
            }

            if (!PercentChance(20))
                return false;

            await context.Channel.SendMessageAsync(context.Message.Content);
            return true;
        }

        private static bool PercentChance(int percentage)
            => Rand.Range(0, 100) < percentage;
    }
}
