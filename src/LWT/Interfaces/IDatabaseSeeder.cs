namespace Lwt.Interfaces
{
    /// <summary>
    /// database seeder.
    /// </summary>
    public interface IDatabaseSeeder
    {
        /// <summary>
        /// Seed the database.
        /// </summary>
        void SeedData();
    }
}