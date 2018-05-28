using System;
using Lwt.Models;
using LWT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lwt.DbContexts
{
    public class LwtDbContext : IdentityDbContext<User, Role, Guid>
    {
        public LwtDbContext(DbContextOptions<LwtDbContext> options) : base(options)
        {
        }

        public DbSet<Text> Texts {get;set;}
    }
}
