using System;
using System.Threading.Tasks;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    Task<int> SaveChangesAsync();
}