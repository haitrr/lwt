namespace Lwt.Models
{
    /// <summary>
    /// application setting.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the application secret key.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Gets or sets connection string to mongodb server.
        /// </summary>
        public string MongoConnectionString { get; set; }

        /// <summary>
        /// Gets or sets application database name.
        /// </summary>
        public string MongoDatabase { get; set; }
    }
}