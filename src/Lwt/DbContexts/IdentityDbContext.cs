namespace Lwt.DbContexts
{
    using System;
    using Lwt.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var converter = new ValueConverter<LanguageCode, string>(
                languageCode => languageCode.Value,
                code => LanguageCode.GetFromString(code));

            builder.Entity<Term>()
                .ToTable(Term.TableName)
                .Property(t => t.LanguageCode)
                .HasConversion(converter);

            var learningLevelConverter = new ValueConverter<LearningLevel, string>(
                learningLevel => learningLevel.Value,
                learningLevel => LearningLevel.GetFromString(learningLevel));
            builder.Entity<Term>()
                .Property(t => t.LearningLevel)
                .HasConversion(learningLevelConverter);
            base.OnModelCreating(builder);
        }
    }
}