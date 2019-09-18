using System;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents.Session;
using Reimu.Core.Handlers;
using Reimu.Core.JsonModels;

namespace Reimu.Core
{
    public class IContext : ICommandContext
    {
        public IUser User => Message.Author;
        public IGuild Guild { get; }
        public IDiscordClient Client { get; }
        public IUserMessage Message { get; }

        public IMessageChannel Channel => Message.Channel;
        // TODO: Do we need a database session?
        public IDocumentSession Session { get; }
        public RavenHandler Database { get; }
        public ConfigModel Config { get; }
        public GuildModel Server { get; }

        public IContext(IDiscordClient client, IUserMessage message, IServiceProvider provider)
        {
            Client = client;
            Message = message;
            //User = Message.Author;
            //Channel = Message.Channel;
            // TODO: Do something to allow DM commands?
            Guild = (message.Channel as IGuildChannel).Guild;
            Database = provider.GetRequiredService<RavenHandler>();
            Config = Database.Get<ConfigModel>("Config");
            Server = Database.Get<GuildModel>(Guild.Id.ToString());
        }
    }
}