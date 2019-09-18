using System;
using Raven.Client.Documents;
using Reimu.Core.JsonModels;

namespace Reimu.Core.Handlers
{
    /// <summary>
    /// Handler for RavenDB databases
    /// </summary>
    public class RavenHandler
    {
        private readonly IDocumentStore _store;

        public RavenHandler(IDocumentStore store) => _store = store;

        public void Initialize()
        {
            using (var session = _store.OpenSession())
            {
                if (session.Advanced.Exists("Config")) 
                    return;
                
                Logger.Log("Database", "Enter bot token: ", ConsoleColor.DarkYellow);
                var token = Console.ReadLine();
                Logger.Log("Database", "Enter bot prefix", ConsoleColor.DarkYellow);
                var prefix = Console.ReadLine();
                
                Save(new ConfigModel
                {
                    Id = "Config",
                    Token = token,
                    Prefix = prefix
                });
            }
        }

        /// <summary>
        /// Attempts to add a guild to the database
        /// </summary>
        /// <param name="id">Guild id</param>
        /// <param name="name">Guild name</param>
        public void AddGuild(ulong id, string name)
        {
            using (var session = _store.OpenSession())
            {
                if (session.Advanced.Exists(id.ToString()))
                    return;
                
                Save(new GuildModel
                {
                    Id = id.ToString(),
                    Prefix = Get<ConfigModel>("Config").Prefix
                });
                
                Logger.Log("Database", $"Added config for {name} ({id})", ConsoleColor.DarkYellow);
            }
        }
        
        /// <summary>
        /// Retrieves an item from the database
        /// </summary>
        /// <param name="id">ID of the database item</param>
        /// <typeparam name="T">Type of the database item</typeparam>
        /// <returns>Database item with the provided ID</returns>
        public T Get<T>(string id) where T : DatabaseModel
        {
            using (var session = _store.OpenSession())
                return session.Load<T>(id);
        }

        /// <summary>
        /// Stores data to the database
        /// </summary>
        /// <param name="item">Data to store in the database</param>
        /// <typeparam name="T">A database item type</typeparam>
        public void Save<T>(T item) where T : DatabaseModel
        {
            if (item == null) 
                return;

            using (var session = _store.OpenSession())
            {
                session.Store(item, item.Id);
                session.SaveChanges();
            }
        }

        public void RemoveGuild(ulong id, string name)
        {
            // TODO: Can we add a "global" job to delete the config after time?
            using (var session = _store.OpenSession())
                session.Delete(id.ToString());
            
            Logger.Log("Database", $"Removed config for {name} ({id})", ConsoleColor.DarkYellow);
        }
    }
}