using System;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Reimu.Common.Logging;
using Reimu.Common.Data;

namespace Reimu.Database
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
            {
                Logger.LogVerbose("Configuration exists in database.");
            }
            else
            {
                Logger.LogVerbose("No configuration found in database, creating now.");
                Logger.LogForce("Enter bot token: ");
                var token = Console.ReadLine();
                Logger.LogForce("Enter bot prefix: ");
                var prefix = Console.ReadLine();

                Save(new BotConfig
                {
                    Id = "Config",
                    Token = token,
                    Prefix = prefix
                });
            }

            if (session.Advanced.Exists("Status"))
            {
                Logger.LogVerbose("Status data exists in database.");
            }
            else
            {
                Save(new BotStatus
                {
                    Id = "Status"
                });
            }
        }

        /// <summary>
        /// Retrieves an item from the database
        /// </summary>
        /// <param name="id">Unique id of the data</param>
        /// <typeparam name="T">Type deriving from <see cref="DatabaseItem"/></typeparam>
        /// <returns>Data with provided id</returns>
        public T Get<T>(string id) where T : DatabaseItem
        {
            Logger.LogVerbose($"Retrieving from database: {id}.");
            using var session = _store.OpenSession();
            return session.Load<T>(id);
        }

        /// <summary>
        /// Retrieves all items in the database filtered by type and id prefix
        /// </summary>
        /// <param name="prefix">Beginning portion of id to match</param>
        /// <typeparam name="T">Type deriving from <see cref="DatabaseItem"/></typeparam>
        /// <returns>Array of database items matching the provided type and prefix</returns>
        public T[] GetAll<T>(string prefix) where T : DatabaseItem
        {
            Logger.LogVerbose($"Retrieving all {typeof(T)} from database.");
            using var session = _store.OpenSession();
            return session.Advanced.LoadStartingWith<T>(prefix, pageSize: int.MaxValue);
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
            {
                Logger.LogWarning("Received null data, cannot save this.");
                return;
            }

            Logger.LogVerbose($"Saving new or updated info to database: {item.Id}.");
            using var session = _store.OpenSession();
            session.Store(item, item.Id);
            session.SaveChanges();
        }

        /// <summary>
        /// Removes an item from the database
        /// </summary>
        /// <param name="id">unique id of the data</param>
        /// <param name="name">Guild name</param>
        public void Remove(string id)
        {
            using var session = _store.OpenSession();
            session.Delete(id);
            session.SaveChanges();
            Logger.LogInfo($"Removed data {id}.");
        }

        /// <summary>
        /// Returns a database session
        /// </summary>
        public IDocumentSession GetSession()
            => _store.OpenSession();

        public void Dispose()
        {
            _store.Dispose();
        }
    }
}
