namespace Lwt.DbContexts
{
    using System;

    using Lwt.Models;

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc />
    /// <summary>
    /// a.
    /// </summary>
    public class LwtDbContext : IdentityDbContext<User, Role, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LwtDbContext" /> class.
        /// </summary>
        /// <param name="options">a.</param>
        public LwtDbContext(DbContextOptions<LwtDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets texts.
        /// </summary>
        public DbSet<Text> Texts { get; set; }
    }
}