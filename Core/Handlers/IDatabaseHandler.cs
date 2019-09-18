using Reimu.Core.JsonModels;

namespace Reimu.Core.Handlers
{
    /// <summary>
    /// Necessary methods for all database systems to implement
    /// </summary>
    public interface IDatabaseHandler
    {
        /// <summary>
        /// Attempts to add a new guild's config to the database
        /// </summary>
        /// <param name="id">Guild id</param>
        /// <param name="name">Guild name</param>
        void AddGuild(ulong id, string name);

        /// <summary>
        /// Retrieves an item from the database
        /// </summary>
        /// <param name="id">ID of the database item</param>
        /// <typeparam name="T">Type of the database item</typeparam>
        /// <returns>Database item with the provided ID</returns>
        T Get<T>(string id) where T : DatabaseModel;

        /// <summary>
        /// Removes a guild's config from the database 
        /// </summary>
        /// <param name="id">Guild id</param>
        /// <param name="name">Guild name</param>
        void RemoveGuild(ulong id, string name);

        /// <summary>
        /// Stores data to the database
        /// </summary>
        /// <param name="item">Data to store in the database</param>
        /// <typeparam name="T">A database item type</typeparam>
        void Save<T>(T item) where T : DatabaseModel;
    }
}