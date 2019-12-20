using System;
using Raven.Client.Documents;
using Reimu.Common.Logging;
using Reimu.Database.Models;

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
                return;
            }

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

        public void AddGuild(ulong id, string name, string voiceRegionId)
        {
            using var session = _store.OpenSession();
            if (session.Advanced.Exists($"guild-{id}"))
                return;

            Save(new GuildConfig
            {
                Id = $"guild-{id}",
                Prefix = Get<BotConfig>("Config").Prefix,
                Locale = GetLocale(voiceRegionId)
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
        /// Removes a guild from the database
        /// </summary>
        /// <param name="id">Guild id</param>
        /// <param name="name">Guild name</param>
        public void RemoveGuild(ulong id, string name)
        {
            using var session = _store.OpenSession();
            session.Delete($"guild-{id}");
            Logger.LogInfo($"Removed config for {name} ({id}).");
        }

        private static string GetLocale(string discordRegionId)
            => discordRegionId switch
            {
                "japan" => "ja-jp",
                _ => "en-us"
            };
    }
}