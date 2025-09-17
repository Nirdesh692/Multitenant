using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Attendance.Domain.Entities;
using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.Services;
using System;

namespace Attendance.Infrastructure.Identity
{
    public class TenantUserStore : UserStore<User, IdentityRole<Guid>, TenantDbContext, Guid>
    {
        private readonly ITenantContextService _tenantContextService;

        public TenantUserStore(ITenantContextService tenantContextService) 
            : base(GetTenantContext(tenantContextService))
        {
            _tenantContextService = tenantContextService;
        }

        private static TenantDbContext GetTenantContext(ITenantContextService tenantContextService)
        {
            var context = tenantContextService.GetTenantDbContext();
            if (context == null)
                throw new InvalidOperationException("Tenant context is required for user operations");
            return context;
        }
    }
}