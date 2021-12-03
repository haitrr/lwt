using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Models;

namespace Lwt.Repositories;

/// <inheritdoc cref="Lwt.Interfaces.ILogRepository" />
public class LogRepository : BaseSqlRepository<Log>, ILogRepository
{
    /// <inheritdoc />
    public LogRepository(IdentityDbContext identityDbContext) : base(identityDbContext)
    {
    }
}