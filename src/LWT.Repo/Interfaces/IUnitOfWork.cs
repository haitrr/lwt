using System;

namespace LWT.Repo.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int Commit();
    }
}