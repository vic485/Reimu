using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Reimu.Fun
{
    public static class MessageFun
    {
        public static async Task<bool> RepeatText(SocketMessage socketMessage)
        {
            var reply = socketMessage.Content.ToLower();
            var otherMessages = (await socketMessage.Channel.GetMessagesAsync(3).FlattenAsync()).ToList();

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

            await socketMessage.Channel.SendMessageAsync(reply);
            return true;
        }
    }
}
