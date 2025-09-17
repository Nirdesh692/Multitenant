using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;
using System;
using System.Linq;

namespace MultiTenantApp.Infrastructure.Services
{
    public interface ITenantContextService
    {
        string? GetCurrentTenantId();
        TenantDbContext? GetTenantDbContext();
    }

    public class TenantContextService : ITenantContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MasterDbContext _masterContext;
        private readonly ITenantService _tenantService;

        public TenantContextService(
            IHttpContextAccessor httpContextAccessor, 
            MasterDbContext masterContext, 
            ITenantService tenantService)
        {
            _httpContextAccessor = httpContextAccessor;
            _masterContext = masterContext;
            _tenantService = tenantService;
        }

        public string? GetCurrentTenantId()
        {
            return _httpContextAccessor.HttpContext?.Items["TenantId"]?.ToString();
        }

        public TenantDbContext? GetTenantDbContext()
        {
            var tenantId = GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return null;

            var tenant = _masterContext.Tenants.FirstOrDefault(t => t.Id.ToString() == tenantId);
            if (tenant == null)
                return null;

            var connectionString = _tenantService.BuildConnectionString(tenant);
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new TenantDbContext(optionsBuilder.Options);
        }
    }
}