namespace Lwt.DbContexts
{
    using System;

    using Lwt.Models;

    using Microsoft.Extensions.Options;

    using MongoDB.Driver;

    /// <summary>
    /// application data db context.
    /// </summary>
    public class LwtDbContext
    {
        private readonly IMongoDatabase database;

        /// <summary>
        /// Initializes a new instance of the <see cref="LwtDbContext"/> class.
        /// </summary>
        /// <param name="settings"> app settings.</param>
        public LwtDbContext(IOptions<AppSettings> settings)
        {
            var client = new MongoClient(settings.Value.MongoConnectionString);

            this.database = client.GetDatabase(settings.Value.MongoDatabase);
        }

        /// <summary>
        /// Gets text collection.
        /// </summary>
        public IMongoCollection<Text> Texts => this.database.GetCollection<Text>("texts");

        /// <summary>
        /// get the collection base on type.
        /// </summary>
        /// <typeparam name="T">type of the collection.</typeparam>
        /// <returns>the collection.</returns>
        /// <exception cref="NotSupportedException">the type is not supported.</exception>
        public IMongoCollection<T> GetCollection<T>()
        {
            Type type = typeof(T);

            if (type == typeof(Text))
            {
                return this.database.GetCollection<T>("texts");
            }

            if (type == typeof(Term))
            {
                return this.database.GetCollection<T>("terms");
            }

            if (type == typeof(Language))
            {
                return this.database.GetCollection<T>("languages");
            }

            throw new NotSupportedException(type.Name);
        }
    }
}