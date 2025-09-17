using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attendance.Domain.Entities;

namespace Attendance.Domain.Interfaces;

public interface ITenantService
{
    Task<Tenant> CreateTenantAsync(string name, string server, string database, string? userId, string? password, bool useWindowsAuth, string frontendUrl);
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId);
    Task<Tenant?> GetTenantByFrontendUrlAsync(string frontendUrl);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync();
    Task MigrateTenantDatabaseAsync(string connectionString);
    string BuildConnectionString(Tenant tenant);
}