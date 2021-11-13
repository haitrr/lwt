namespace Lwt.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// database seeder.
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>
    /// Seed the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SeedData();
}