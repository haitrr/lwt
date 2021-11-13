namespace Lwt.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Exceptions;
using Lwt.Models;
using Microsoft.EntityFrameworkCore;

/// <inheritdoc />
public class BaseSqlRepository<T> : ISqlRepository<T>
    where T : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSqlRepository{T}"/> class.
    /// </summary>
    /// <param name="identityDbContext">db context.</param>
    public BaseSqlRepository(IdentityDbContext identityDbContext)
    {
        this.DbSet = identityDbContext.Set<T>();
    }

    /// <summary>
    ///    Gets dbset.
    /// </summary>
    protected DbSet<T> DbSet { get; }

    /// <inheritdoc />
    public void Add(T entity)
    {
        this.DbSet.Add(entity);
    }

    /// <inheritdoc />
    public void BulkAdd(IEnumerable<T> entities)
    {
        this.DbSet.AddRange(entities);
    }

    /// <inheritdoc/>
    public void DeleteById(T entity)
    {
        this.DbSet.Remove(entity);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> filter, PaginationQuery paginationQuery)
    {
        return await this.DbSet.Where(filter)
            .Skip((paginationQuery.Page - 1) * paginationQuery.ItemPerPage)
            .Take(paginationQuery.ItemPerPage)
            .ToArrayAsync();
    }

    /// <inheritdoc/>
    public void Update(T entity)
    {
        this.DbSet.Update(entity);
    }

    /// <inheritdoc/>
    public Task<T?> TryGetByIdAsync(int id)
    {
        return this.DbSet.SingleOrDefaultAsync(e => e.Id == id) !;
    }

    /// <inheritdoc />
    public async Task<T> GetByIdAsync(int id)
    {
        try
        {
            return await this.DbSet.SingleAsync(e => e.Id == id);
        }
        catch
        {
            throw new NotFoundException("Entity not found");
        }
    }

    /// <inheritdoc />
    public Task<bool> IsExistAsync(int id)
    {
        return this.DbSet.AnyAsync(e => e.Id == id);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
    {
        return this.DbSet.CountAsync();
    }

    public IQueryable<T> Queryable()
    {
        return this.DbSet;
    }

    /// <inheritdoc />
    public void Delete(T entity)
    {
        this.DbSet.Remove(entity);
    }

    public void BulkInsert(IEnumerable<T> entities)
    {
        this.DbSet.BulkInsert(entities);
    }
}