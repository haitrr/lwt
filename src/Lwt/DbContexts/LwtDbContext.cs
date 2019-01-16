namespace Lwt.DbContexts
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
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
        /// get the collection base on type.
        /// </summary>
        /// <typeparam name="T">type of the collection.</typeparam>
        /// <returns>the collection.</returns>
        /// <exception cref="NotSupportedException">the type is not supported.</exception>
        [SuppressMessage("StyleCop", "CA1308", Justification = "The collection name should be lower case.")]
        public IMongoCollection<T> GetCollection<T>()
        {
            Type type = typeof(T);

            return this.database.GetCollection<T>(type.Name.ToLower(CultureInfo.InvariantCulture));
        }
    }
}