using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reimu.Core;

namespace Reimu.Fun
{
    public static class MessageFun
    {
        public static async Task Process(BotContext context)
        {
            if (context.Message.MentionedUsers.Any(x => x.Id == context.Client.CurrentUser.Id))
            {
                await context.Message.AddReactionAsync(Emote.Parse("<:ReimuWantToDie:501411246906671135>"));
                return;
            }

            if (!PercentChance(context.GuildConfig.FunnyBusiness))
                return;

            if (await Replies(context))
                return;

            await RepeatText(context);
        }

        private static async Task<bool> Replies(SocketCommandContext context)
        {
            var message = context.Message.Content.ToLower();
            switch (message)
            {
                case "lmao":
                    await context.Channel.SendMessageAsync("big草");
                    return true;
                default:
                    return false;
            }
        }

        private static async Task<bool> RepeatText(SocketCommandContext context)
        {
            if (!PercentChance(20))
                return false;

            // Don't promote ping spam
            if (context.Message.MentionedUsers.Any() || context.Message.MentionedRoles.Any())
                return false;

            var reply = context.Message.Content.ToLower();
            var otherMessages = (await context.Channel.GetMessagesAsync(3).FlattenAsync()).ToList();

            if (otherMessages.Count < 3)
                return false;

            // Check message contents
            if (otherMessages.Any(message => message.Content.ToLower() != reply) ||
                otherMessages.Any(x => x.Author.IsBot))
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

            await context.Channel.SendMessageAsync(context.Message.Content);
            return true;
        }

        private static bool PercentChance(int percentage)
            => Rand.Range(0, 100) < percentage;
    }
}
