namespace Lwt.DbContexts
{
    using Lwt.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    /// <inheritdoc />
    /// <summary>
    /// application identity db context.
    /// </summary>
    public class IdentityDbContext : IdentityDbContext<User, Role, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext" /> class.
        /// </summary>
        /// <param name="options">a.</param>
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public IdentityDbContext()
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
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

            builder.Entity<Term>()
                .HasIndex(t => new { t.Content, t.LanguageCode, t.UserId })
                .IsUnique();
            
            builder.Entity<Text>()
                .ToTable(Text.TableName)
                .Property(t => t.LanguageCode)
                .HasConversion(converter);

            builder.Entity<Text>()
                .HasIndex(t => t.ProcessedTermCount);

            builder.Entity<TextTerm>()
                .ToTable(TextTerm.TableName);
            
            // (TextIdxTermId) index
            builder.Entity<TextTerm>()
                .HasIndex(t => new { t.TextId, t.TermId });
            
            builder.Entity<UserSetting>()
                .ToTable(UserSetting.TableName);
            builder.Entity<LanguageSetting>()
                .ToTable(LanguageSetting.TableName)
                .Property(t => t.LanguageCode)
                .HasConversion(converter);

            builder.Entity<LanguageSetting>()
                .Property(t => t.DictionaryLanguageCode)
                .HasConversion(converter);

            builder.Entity<User>()
                .ToTable("users");
            builder.Entity<Role>()
                .ToTable("roles");

            builder.Entity<IdentityUserRole<int>>(entity => { entity.ToTable("user_roles"); });

            builder.Entity<IdentityUserClaim<int>>(entity => { entity.ToTable("user_claims"); });

            builder.Entity<IdentityUserLogin<int>>(entity => { entity.ToTable("user_logins"); });

            builder.Entity<IdentityRoleClaim<int>>(entity => { entity.ToTable("role_claims"); });

            builder.Entity<IdentityUserToken<int>>(entity => { entity.ToTable("user_tokens"); });
        }
    }
}