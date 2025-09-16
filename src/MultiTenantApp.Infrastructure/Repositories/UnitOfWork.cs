using System.Threading.Tasks;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;

namespace MultiTenantApp.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TenantDbContext _context;
    private IRepository<User>? _users;

    public UnitOfWork(TenantDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}