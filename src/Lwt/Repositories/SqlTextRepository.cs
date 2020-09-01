namespace Lwt.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    /// <inheritdoc cref="Lwt.Repositories.ISqlTextRepository" />
    public class SqlTextRepository : BaseSqlRepository<Text>, ISqlTextRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTextRepository"/> class.
        /// </summary>
        /// <param name="identityDbContext">db context.</param>
        public SqlTextRepository(IdentityDbContext identityDbContext)
            : base(identityDbContext)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Text>> GetByUserAsync(
            int userId,
            TextFilter textFilter,
            PaginationQuery paginationQuery)
        {
            int skip = paginationQuery.ItemPerPage * (paginationQuery.Page - 1);

            IQueryable<Text> query = this.DbSet.Where(t => t.UserId == userId);

            // sort by created time by default
            if (textFilter.LanguageCode != null)
            {
                query = query.Where(t => t.LanguageCode == textFilter.LanguageCode);
            }

            if (textFilter.Title != null)
            {
                query = query.Where(
                    t => t.Title.ToLower()
                        .Contains(textFilter.Title.ToLower()));
            }

            return await query.AsNoTracking()
                .OrderByDescending(t => t.CreatedDate)
                .Select(
                    t => new Text
                    {
                        Id = t.Id,
                        Title = t.Title,
                        LanguageCode = t.LanguageCode,
                        TermCount = t.TermCount,
                        ProcessedTermCount = t.ProcessedTermCount
                    })
                .Skip(skip)
                .Take(paginationQuery.ItemPerPage)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<long> CountByUserAsync(int userId, TextFilter textFilter)
        {
            IQueryable<Text> query = this.DbSet.Where(t => t.UserId == userId);

            // sort by created time by default
            if (textFilter.LanguageCode != null)
            {
                query = query.Where(t => t.LanguageCode == textFilter.LanguageCode);
            }

            if (textFilter.Title != null)
            {
                query = query.Where(
                    t => t.Title.ToLower()
                        .Contains(textFilter.Title.ToLower()));
            }

            return await query.AsNoTracking()
                .CountAsync();
        }

        public void UpdateBookmarkAsync(int id, ulong? bookMark)
        {
            var text = new Text() { Id = id, Bookmark = bookMark };
            this.DbSet.Attach(text)
                .Property(t => t.Bookmark)
                .IsModified = true;
        }

        public void UpdateProcessedTermCount(Text text)
        {
            var updatedText = new Text() { Id = text.Id, ProcessedTermCount = text.ProcessedTermCount };
            this.DbSet.Attach(updatedText)
                .Property(t => t.ProcessedTermCount)
                .IsModified = true;
        }

        public void UpdateTermCountAndProcessedTermCount(Text text)
        {
            var updatedText = new Text()
            {
                Id = text.Id, ProcessedTermCount = text.ProcessedTermCount, TermCount = text.TermCount,
            };
            EntityEntry<Text> attachedText = this.DbSet.Attach(updatedText);
            attachedText.Property(t => t.ProcessedTermCount)
                .IsModified = true;
            attachedText.Property(t => t.TermCount)
                .IsModified = true;
        }
    }
}