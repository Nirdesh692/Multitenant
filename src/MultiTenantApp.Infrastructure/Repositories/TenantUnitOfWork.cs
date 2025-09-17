using System;
using System.Threading.Tasks;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;
using MultiTenantApp.Infrastructure.Services;

namespace MultiTenantApp.Infrastructure.Repositories
{
    public class TenantUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ITenantContextService _tenantContextService;
        private TenantDbContext? _context;
        private IRepository<User>? _users;
        private bool _disposed = false;

        public TenantUnitOfWork(ITenantContextService tenantContextService)
        {
            _tenantContextService = tenantContextService;
        }

        private TenantDbContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = _tenantContextService.GetTenantDbContext();
                    if (_context == null)
                        throw new InvalidOperationException("Tenant context is required");
                }
                return _context;
            }
        }

        public IRepository<User> Users => _users ??= new Repository<User>(Context);

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
                _disposed = true;
            }
        }
    }
}