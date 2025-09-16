using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;

namespace MultiTenantApp.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly MasterDbContext _masterContext;
    private readonly IConfiguration _configuration;

    public TenantService(MasterDbContext masterContext, IConfiguration configuration)
    {
        _masterContext = masterContext;
        _configuration = configuration;
    }

    public async Task<Tenant> CreateTenantAsync(string name, string server, string database, string? userId, string? password, bool useWindowsAuth, string frontendUrl)
    {
        var tenant = new Tenant
        {
            Name = name,
            Server = server,
            Database = database,
            UserId = userId,
            Password = password,
            UseWindowsAuth = useWindowsAuth,
            FrontendUrl = frontendUrl
        };

        _masterContext.Tenants.Add(tenant);
        await _masterContext.SaveChangesAsync();

        return tenant;
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
    {
        return await _masterContext.Tenants.FindAsync(tenantId);
    }

    public async Task<Tenant?> GetTenantByFrontendUrlAsync(string frontendUrl)
    {
        return await _masterContext.Tenants.FirstOrDefaultAsync(t => t.FrontendUrl == frontendUrl && t.IsActive);
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
    {
        return await _masterContext.Tenants.Where(t => t.IsActive).ToListAsync();
    }

    public async Task MigrateTenantDatabaseAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new TenantDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        // Create default admin user
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Email = "admin@tenant.com",
                FirstName = "Admin",
                LastName = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }

    public string BuildConnectionString(Tenant tenant)
    {
        var templateKey = tenant.UseWindowsAuth ? "TenantTemplateWindows" : "TenantTemplateSql";
        var template = _configuration.GetConnectionString(templateKey) ?? string.Empty;
        
        var connectionString = template
            .Replace("{Server}", tenant.Server)
            .Replace("{Database}", tenant.Database);
            
        if (!tenant.UseWindowsAuth)
        {
            connectionString = connectionString
                .Replace("{UserId}", tenant.UserId ?? string.Empty)
                .Replace("{Password}", tenant.Password ?? string.Empty);
        }
        
        return connectionString;
    }
}