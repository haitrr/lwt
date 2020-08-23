namespace Lwt.Test
{
    using System;
    using Lwt.DbContexts;
    using Lwt.Models;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// help use for testing db.
    /// </summary>
    public static class TestDbHelper
    {
        /// <summary>
        /// clean table.
        /// </summary>
        /// <param name="services">services.</param>
        /// <typeparam name="T">entity type.</typeparam>
        public static void CleanTable<T>(IServiceProvider services)
            where T : Entity
        {
            using (IServiceScope scope = services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

                foreach (T t in dbContext.Set<T>())
                {
                    dbContext.Set<T>()
                        .Remove(t);
                }
            }
        }

        /// <summary>
        /// get db context.
        /// </summary>
        /// <param name="factory">factory.</param>
        /// <returns>db context.</returns>
        public static IdentityDbContext GetDbContext(LwtTestWebApplicationFactory factory)
        {
            return factory.Services.CreateScope()
                .ServiceProvider.GetService<IdentityDbContext>();
        }
    }
}