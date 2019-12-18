using System;
using Raven.Client.Documents;
using Reimu.Common.Logging;
using Reimu.Core.Json;

namespace Reimu.Core.Handlers
{
    public class DatabaseHandler
    {
        private readonly IDocumentStore _store;

        public DatabaseHandler(IDocumentStore store) => _store = store;

        /// <summary>
        /// Checks if the configuration exists, and creates it if not
        /// </summary>
        public void Initialize()
        {
            using var session = _store.OpenSession();
            if (session.Advanced.Exists("Config"))
                return;

            Logger.LogForce("Enter bot token: ");
            var token = Console.ReadLine();
            Logger.LogVerbose("Enter bot prefix: ");
            var prefix = Console.ReadLine();

            Save(new BotConfig
            {
                Id = "Config",
                Token = token,
                Prefix = prefix
            });
        }

        /// <summary>
        /// Retrieves an item from the database
        /// </summary>
        /// <param name="id">Unique id of the data</param>
        /// <typeparam name="T">Type deriving from <see cref="DatabaseItem"/></typeparam>
        /// <returns>Data with provided id</returns>
        public T Get<T>(string id) where T : DatabaseItem
        {
            using var session = _store.OpenSession();
            return session.Load<T>(id);
        }

        public void AddGuild(ulong id, string name)
        {
            using var session = _store.OpenSession();
            if (session.Advanced.Exists($"guild-{id}"))
                return;

            Save(new GuildConfig
            {
                Id = $"guild-{id}",
                Prefix = Get<BotConfig>("Config").Prefix
            });

            Logger.LogInfo($"Added config for {name} ({id}).");
        }

        /// <summary>
        /// Save an item or its changes to the database
        /// </summary>
        /// <param name="item">Information to save</param>
        /// <typeparam name="T">Type deriving from <see cref="DatabaseItem"/></typeparam>
        public void Save<T>(T item) where T : DatabaseItem
        {
            if (item == null)
                return;

            using var session = _store.OpenSession();
            session.Store(item, item.Id);
            session.SaveChanges();
        }

        public void RemoveGuild(ulong id, string name)
        {
            // TODO: remove guild model
            Logger.LogInfo($"Removed config for {name} ({id}).");
        }
    }
}