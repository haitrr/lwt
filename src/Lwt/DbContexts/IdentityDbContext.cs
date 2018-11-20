namespace Lwt.DbContexts
{
    using System;

    using Lwt.Models;

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc />
    /// <summary>
    /// application identity db context.
    /// </summary>
    public class IdentityDbContext : IdentityDbContext<User, Role, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext" /> class.
        /// </summary>
        /// <param name="options">a.</param>
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }
    }
}