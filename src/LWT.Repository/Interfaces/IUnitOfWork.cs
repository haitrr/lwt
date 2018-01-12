using System;

namespace LWT.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int Commit();
    }
}